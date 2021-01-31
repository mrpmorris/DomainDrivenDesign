using MongoDB.Bson;
using MongoDB.Driver;
using DomainDrivenDesign.MongoDB.DomainClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using DomainDrivenDesign.MongoDB.Interception;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract partial class DbContext
	{
		private int IsSaving;
		private readonly IMongoClient MongoClient;
		private readonly IMongoDatabase MongoDatabase;
		private readonly ConcurrentDictionary<string, IDbSet> DbSetLookup;
		private readonly ConcurrentDictionary<CollectionNameAndEntityId, EntityEntry> EntityEntryLookup;

		public DbContext(DbContextOptions options)
		{
			MongoClient = CreateMongoClient(options);
			MongoDatabase = CreateMongoDatabase(options);
			DbSetLookup = new ConcurrentDictionary<string, IDbSet>(StringComparer.Ordinal);
			EntityEntryLookup = new ConcurrentDictionary<CollectionNameAndEntityId, EntityEntry>();
		}

		public EntityEntry GetEntry(string collectionName, AggregateRoot entity)
		{
			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			if (EntityEntryLookup.TryGetValue(key, out EntityEntry entry))
				return entry;
			return new EntityEntry(collectionName, entity, EntityState.Unknown, entity.ConcurrencyVersion);
		}

		internal EntityEntry[] GetEntries() => EntityEntryLookup.Values.ToArray();

		internal IQueryable<TEntity?> GetQueryable<TEntity>(string collectionName)
			where TEntity : AggregateRoot
		=>
			new AggregateRootQueryableInterceptor<TEntity>(
				source: ((IMongoCollection<TEntity>)DbSetLookup[collectionName].Collection).AsQueryable(),
				dbContext: this,
				collectionName,
				interceptValue: x => Attach(
					type: typeof(TEntity),
					collectionName: collectionName,
					entity: x));

		internal object Attach(Type type, string collectionName, object entity)
		{
			AggregateRoot? aggregateRoot = entity as AggregateRoot;
			if (aggregateRoot is null)
				throw new ArgumentException($"'{entity.GetType().FullName}'" +
					$" must be of type '{typeof(AggregateRoot).FullName}'");

			var key = new CollectionNameAndEntityId(collectionName, aggregateRoot.Id);
			EntityEntry entry = EntityEntryLookup.GetOrAdd(
				key: key,
				valueFactory: _ => new EntityEntry(
					collectionName,
					aggregateRoot,
					EntityState.Unmodified,
					aggregateRoot.ConcurrencyVersion));
			return entry.Entity;
		}

		internal TEntity Attach<TEntity>(string collectionName, TEntity entity)
			where TEntity : AggregateRoot
		{
			return (TEntity)Attach(typeof(TEntity), collectionName, entity);
		}

		internal void AddOrUpdate<TEntity>(string collectionName, TEntity entity)
			where TEntity : AggregateRoot
		{
			if (entity.Id == default)
				throw new ArgumentException("Id has not been set", nameof(entity));

			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			EntityEntryLookup.AddOrUpdate(
				key: key,
				addValueFactory: _ => new EntityEntry(
					collectionName,
					entity,
					EntityState.Created,
					entity.ConcurrencyVersion),
				updateValueFactory: (_, entry) =>
				{
					if (entry.State == EntityState.Deleted)
						throw new StateException(
							$"Cannot call {nameof(AddOrUpdate)} on entity with state {nameof(EntityState.Deleted)}" +
							$", Entity.Id={entry.Entity.Id}, EntityType={entity.GetType().FullName}",
							state: EntityState.Deleted);
					return new EntityEntry(
						entry.CollectionName,
						entry.Entity,
						EntityState.Modified,
						entry.OriginalEntityConcurrencyVersion);
				});
		}

		internal void Delete<TEntity>(string collectionName, TEntity entity)
			where TEntity : AggregateRoot
		{
			if (entity.Id == default(ObjectId))
				throw new ArgumentException("Id has not been set", nameof(entity));

			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			_ = EntityEntryLookup.AddOrUpdate(
				key: key,
				addValueFactory: _ => new EntityEntry(
					collectionName,
					entity,
					EntityState.Deleted,
					entity.ConcurrencyVersion),
				updateValueFactory: (_, entry) => new EntityEntry(
					entry.CollectionName,
					entry.Entity,
					EntityState.Deleted,
					entry.OriginalEntityConcurrencyVersion));
		}

		internal async Task SaveChangesAsync()
		{
			if (Interlocked.Increment(ref IsSaving) != 1)
				throw new InvalidOperationException($"Cannot call {nameof(SaveChangesAsync)} from multiple threads");
			var collectionNameToChangesLookup = EntityEntryLookup
				.Select(x => x.Value)
				.GroupBy(x => x.CollectionName)
				.ToDictionary(x => x.Key, x => x.Select(entityEntry => entityEntry));

			//TODO: Start transaction
			try
			{
				var saveChangesTasks = new List<Task>();
				foreach (KeyValuePair<string, IEnumerable<EntityEntry>> changesWithinCollection in collectionNameToChangesLookup)
				{
					if (!DbSetLookup.TryGetValue(changesWithinCollection.Key, out IDbSet dbSet))
						throw new KeyNotFoundException($"DbSet has not been created {changesWithinCollection.Key}");
					var saveChangesTast = dbSet.SaveCollectionChangesAsync(changesWithinCollection.Value);
					saveChangesTasks.Add(saveChangesTast);
				}
				await Task.WhenAll(saveChangesTasks).ConfigureAwait(false);
				//TODO: Commit transaction
				UpdateEntityEntryStatesAfterSave();
			}
			catch
			{
				//TODO: Rollback transaction
				throw;
			}
			finally
			{
				Interlocked.Decrement(ref IsSaving);
			}
		}

		internal async Task<TEntity?> GetAsync<TEntity>(string collectionName, ObjectId id)
			where TEntity : AggregateRoot
		{
			var key = new CollectionNameAndEntityId(collectionName, id);
			if (EntityEntryLookup.TryGetValue(key, out EntityEntry entry))
				return (TEntity)entry.Entity;

			FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;
			FilterDefinition<TEntity> filter = filterBuilder.Where(x => x.Id == id);

			var collection = MongoDatabase.GetCollection<TEntity>(collectionName);

			IAsyncCursor<TEntity> cursor = await collection.FindAsync<TEntity>(filter).ConfigureAwait(false);

			TEntity? retrievedEntity = null;
			if (await cursor.MoveNextAsync().ConfigureAwait(false))
				retrievedEntity = cursor.Current.First();

			if (retrievedEntity is not null)
				EntityEntryLookup[key] = new EntityEntry(
					collectionName,
					retrievedEntity,
					EntityState.Unmodified,
					retrievedEntity.ConcurrencyVersion);

			return retrievedEntity;
		}

		internal Task<TEntity[]> GetManyAsync<TEntity>(
			string collectionName,
			IEnumerable<ObjectId> ids)
			where TEntity : AggregateRoot
		{
			if (!ids.Any())
				return Task.FromResult(Array.Empty<TEntity>());

			throw new NotImplementedException();
		}


		protected DbSet<TEntity> CreateDbSet<TEntity>(string collectionName)
			where TEntity : AggregateRoot
		{
			var dbSet = new DbSet<TEntity>(
				collectionName: collectionName,
				mongoDatabase: MongoDatabase);

			_ = DbSetLookup.AddOrUpdate(
				key: collectionName,
				addValueFactory: _ => dbSet,
				updateValueFactory: (_, _) =>
					throw new ArgumentException($"Duplicate collection name {collectionName}", nameof(collectionName)));

			return dbSet;
		}

		protected virtual void ConfigureMongoClientSettings(MongoClientSettings mongoClientSettings)
		{
		}

		protected virtual void ConfigureMongoDatabaseSettings(MongoDatabaseSettings mongoDatabaseSettings)
		{
		}

		private MongoClient CreateMongoClient(DbContextOptions options)
		{
			if (options.ConnectionString is null)
				throw new NullReferenceException($"{nameof(options)}.{nameof(options.ConnectionString)} cannot be null");

			MongoClientSettings mongoClientSettings =
				MongoClientSettings.FromConnectionString(options.ConnectionString);
			ConfigureMongoClientSettings(mongoClientSettings);
			return new MongoClient(mongoClientSettings);
		}

		private IMongoDatabase CreateMongoDatabase(DbContextOptions options)
		{
			if (options.DatabaseName is null)
				throw new NullReferenceException($"{nameof(options)}.{nameof(options.DatabaseName)} cannot be null");

			MongoDatabaseSettings mongoDatabaseSettings = new MongoDatabaseSettings();
			ConfigureMongoDatabaseSettings(mongoDatabaseSettings);
			return MongoClient.GetDatabase(name: options.DatabaseName, mongoDatabaseSettings);
		}

		private void UpdateEntityEntryStatesAfterSave()
		{
			var entityEntryLookupKeysAndValues = EntityEntryLookup.Select(x => x).ToArray();
			foreach (var kvp in entityEntryLookupKeysAndValues)
			{
				switch (kvp.Value.State)
				{
					case EntityState.Created:
					case EntityState.Modified:
						EntityEntryLookup[kvp.Key] = new EntityEntry(
							collectionName: kvp.Value.CollectionName,
							entity: kvp.Value.Entity,
							state: EntityState.Unmodified,
							originalEntityConcurrencyVersion: kvp.Value.OriginalEntityConcurrencyVersion + 1);
						break;

					case EntityState.Deleted:
						EntityEntryLookup.TryRemove(kvp.Key, out _);
						break;

					case EntityState.Unmodified:
						break;

					default:
						throw new NotImplementedException(kvp.Value.State.ToString());
				}
			}
		}
	}
}
