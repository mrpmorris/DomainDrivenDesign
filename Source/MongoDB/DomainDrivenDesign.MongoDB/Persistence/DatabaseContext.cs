using MongoDB.Bson;
using MongoDB.Driver;
using DomainDrivenDesign.MongoDB.DomainClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract partial class DatabaseContext
	{
		protected readonly IMongoClient MongoClient;
		protected readonly IMongoDatabase MongoDatabase;
		protected readonly ConcurrentDictionary<CollectionNameAndEntityId, EntityEntry> EntityEntryLookup;

		private int IsLocked;

		public DatabaseContext(DatabaseContextOptions options)
		{
			MongoClient = CreateMongoClient(options);
			MongoDatabase = CreateMongoDatabase(options);
			EntityEntryLookup = new ConcurrentDictionary<CollectionNameAndEntityId, EntityEntry>();
		}

		public EntityEntry GetEntry(string collectionName, AggregateRoot entity)
		{
			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			if (EntityEntryLookup.TryGetValue(key, out EntityEntry entry))
				return entry;
			return new EntityEntry(collectionName, entity, EntityState.Unknown);
		}

		public void AddOrUpdate<TEntity>(string collectionName, TEntity entity)
			where TEntity: AggregateRoot
		{
			if (entity.Id == default(ObjectId))
				throw new ArgumentException("Id has not been set", nameof(entity));

			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			EntityEntryLookup.AddOrUpdate(
				key: key,
				addValueFactory: _ => new EntityEntry(collectionName, entity, EntityState.Created),
				updateValueFactory: (_, entry) =>
				{
					if (entry.State == EntityState.Deleted)
						throw new StateException(
							$"Cannot call {nameof(AddOrUpdate)} on entity with state {nameof(EntityState.Deleted)}" +
							$", Entity.Id={entry.Entity.Id}, EntityType={entity.GetType().FullName}",
							state: EntityState.Deleted);
					return new EntityEntry(collectionName, entity, EntityState.Modified);
				});
		}

		public void Delete<TEntity>(string collectionName, TEntity entity)
			where TEntity: AggregateRoot
		{
			if (entity.Id == default(ObjectId))
				throw new ArgumentException("Id has not been set", nameof(entity));

			if (GetEntry(collectionName, entity).State == EntityState.Unknown)
				throw new InvalidOperationException($"Cannot delete an unknown object");

			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			EntityEntryLookup[key] = new EntityEntry(collectionName, entity, EntityState.Deleted);
		}

		public async Task SaveChangesAsync()
		{
			if (Interlocked.Increment(ref IsLocked) != 1)
				throw new InvalidOperationException($"Cannot call {nameof(SaveChangesAsync)} from multiple threads");
			var collectionNameToChangesLookup = EntityEntryLookup
				.Select(x => x.Value)
				.GroupBy(x => x.CollectionName)
				.ToDictionary(x => x.Key, x => x.Select(entityEntry => entityEntry));

			//TODO: Start transaction
			try
			{
				foreach (KeyValuePair<string, IEnumerable<EntityEntry>> changesWithinCollection in collectionNameToChangesLookup)
				{
					await SaveCollectionChangesAsync(changesWithinCollection.Value).ConfigureAwait(false);
				}
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
				Interlocked.Decrement(ref IsLocked);
			}
		}

		public async Task<TEntity?> GetAsync<TEntity>(string collectionName, ObjectId id)
			where TEntity: AggregateRoot
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
				EntityEntryLookup[key] = new EntityEntry(collectionName, retrievedEntity, EntityState.Unmodified);

			return retrievedEntity;
		}


		protected virtual void ConfigureMongoClientSettings(MongoClientSettings mongoClientSettings)
		{
		}

		protected virtual void ConfigureMongoDatabaseSettings(MongoDatabaseSettings mongoDatabaseSettings)
		{
		}

		private MongoClient CreateMongoClient(DatabaseContextOptions options)
		{
			if (options.ConnectionString is null)
				throw new NullReferenceException($"{nameof(options)}.{nameof(options.ConnectionString)} cannot be null");

			MongoClientSettings mongoClientSettings =
				MongoClientSettings.FromConnectionString(options.ConnectionString);
			ConfigureMongoClientSettings(mongoClientSettings);
			return new MongoClient(mongoClientSettings);
		}

		private IMongoDatabase CreateMongoDatabase(DatabaseContextOptions options)
		{
			if (options.DatabaseName is null)
				throw new NullReferenceException($"{nameof(options)}.{nameof(options.DatabaseName)} cannot be null");

			MongoDatabaseSettings mongoDatabaseSettings = new MongoDatabaseSettings();
			ConfigureMongoDatabaseSettings(mongoDatabaseSettings);
			return MongoClient.GetDatabase(name: options.DatabaseName, mongoDatabaseSettings);
		}

		private async Task SaveCollectionChangesAsync(IEnumerable<EntityEntry> entityEntries)
		{
			string collectionName = entityEntries.First().CollectionName;

			int expectedInsertedCount = 0;
			int expectedModifiedCount = 0;
			int expectedDeletedCount = 0;

			var updates = new List<WriteModel<AggregateRoot>>();
			FilterDefinitionBuilder<AggregateRoot> filterBuilder = Builders<AggregateRoot>.Filter;
			foreach (EntityEntry entityEntry in entityEntries)
			{
				switch (entityEntry.State)
				{
					case EntityState.Created:
						expectedInsertedCount++;
						updates.Add(CreateInsertAction(entityEntry.Entity));
						break;

					case EntityState.Modified:
						expectedModifiedCount++;
						updates.Add(CreateReplaceAction(entityEntry.Entity, filterBuilder));
						break;

					case EntityState.Deleted:
						expectedDeletedCount++;
						updates.Add(CreateDeleteAction(entityEntry.Entity, filterBuilder));
						break;

					case EntityState.Unmodified:
						break;

					default:
						throw new NotImplementedException(entityEntry.State.ToString());
				}
			}

			if (!updates.Any())
				return;

			var collection = MongoDatabase.GetCollection<AggregateRoot>(collectionName);
			BulkWriteResult<AggregateRoot> result = await collection.BulkWriteAsync(updates).ConfigureAwait(false);
			if (result.InsertedCount != expectedInsertedCount
				|| result.ModifiedCount != expectedModifiedCount
				|| result.DeletedCount != expectedDeletedCount)
			{
				throw new PersistenceConflictException();
			}
		}

		private WriteModel<AggregateRoot> CreateInsertAction(AggregateRoot createdEntity) =>
			new InsertOneModel<AggregateRoot>(createdEntity);

		private WriteModel<AggregateRoot> CreateReplaceAction(
			AggregateRoot updatedEntity,
			FilterDefinitionBuilder<AggregateRoot> filterBuilder)
			=>
				new ReplaceOneModel<AggregateRoot>(
					filter: filterBuilder.Where(x => x.Id == updatedEntity.Id),
					replacement: updatedEntity);

		private WriteModel<AggregateRoot> CreateDeleteAction(
			AggregateRoot deletedEntity,
			FilterDefinitionBuilder<AggregateRoot> filterBuilder)
			=>
				new DeleteOneModel<AggregateRoot>(filterBuilder.Where(x => x.Id == deletedEntity.Id));

		private void UpdateEntityEntryStatesAfterSave()
		{
			var entityEntryLookupKeysAndValues = EntityEntryLookup.Select(x => x).ToArray();
			foreach(var kvp in entityEntryLookupKeysAndValues)
			{
				switch (kvp.Value.State)
				{
					case EntityState.Created:
					case EntityState.Modified:
						EntityEntryLookup[kvp.Key] = new EntityEntry(
							collectionName: kvp.Value.CollectionName,
							entity: kvp.Value.Entity,
							state: EntityState.Unmodified);
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
