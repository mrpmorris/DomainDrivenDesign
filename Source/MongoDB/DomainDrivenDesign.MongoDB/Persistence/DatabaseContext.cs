using MongoDB.Bson;
using MongoDB.Driver;
using DomainDrivenDesign.MongoDB.DomainClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract partial class DatabaseContext
	{
		protected readonly IMongoClient MongoClient;
		protected readonly IMongoDatabase MongoDatabase;
		protected readonly ConcurrentDictionary<CollectionNameAndEntityId, EntityEntry> EntityEntryLookup;

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

			var key = new CollectionNameAndEntityId(collectionName, entity.Id);
			EntityEntryLookup[key] = new EntityEntry(collectionName, entity, EntityState.Deleted);
		}

		public async Task SaveChangesAsync()
		{
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
			}
			catch
			{
				//TODO: Rollback transaction
				throw;
			}
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
			var updates = new List<WriteModel<AggregateRoot>>();

			var stateToEntriesLookup = entityEntries
				.GroupBy(x => x.State)
				.ToDictionary(x => x.Key, x => x.Select(entityEntry => entityEntry.Entity).ToArray());

			int expectedInsertedCount = 0;
			if (stateToEntriesLookup.TryGetValue(EntityState.Created, out AggregateRoot[] createdEntities))
			{
				updates.AddRange(CreateInsertActions(createdEntities));
				expectedInsertedCount = createdEntities.Length;
			}

			int expectedModifiedCount = 0;
			if (stateToEntriesLookup.TryGetValue(EntityState.Modified, out AggregateRoot[] modifiedEntities))
			{
				updates.AddRange(CreateReplaceActions(modifiedEntities));
				expectedModifiedCount = modifiedEntities.Length;
			}

			int expectedDeletedCount = 0;
			if (stateToEntriesLookup.TryGetValue(EntityState.Deleted, out AggregateRoot[] deletedEntities))
			{
				updates.AddRange(CreateReplaceActions(modifiedEntities));
				expectedDeletedCount = deletedEntities.Length;
			}

			var collection = MongoDatabase.GetCollection<AggregateRoot>(collectionName);
			BulkWriteResult<AggregateRoot> result = await collection.BulkWriteAsync(updates).ConfigureAwait(false);
			if (result.InsertedCount != expectedInsertedCount
				|| result.ModifiedCount != expectedModifiedCount
				|| result.DeletedCount != expectedDeletedCount)
			{
				throw new PersistenceConflictException();
			}
		}

		private IEnumerable<WriteModel<AggregateRoot>> CreateInsertActions(IEnumerable<AggregateRoot> createdEntities) =>
			createdEntities
				.Select(x => new InsertOneModel<AggregateRoot>(x));

		private IEnumerable<WriteModel<AggregateRoot>> CreateReplaceActions(IEnumerable<AggregateRoot> updatedEntities)
		{
			FilterDefinitionBuilder<AggregateRoot> filterBuilder = Builders<AggregateRoot>.Filter;
			return updatedEntities
				.Select(entity => new ReplaceOneModel<AggregateRoot>(
					filter: filterBuilder.Where(x => x.Id == entity.Id),
					replacement: entity));
		}

		private IEnumerable<WriteModel<AggregateRoot>> CreateDeleteActions(IEnumerable<AggregateRoot> updatedEntities)
		{
			FilterDefinitionBuilder<AggregateRoot> filterBuilder = Builders<AggregateRoot>.Filter;
			return updatedEntities
				.Select(entity => new DeleteOneModel<AggregateRoot>(filterBuilder.Where(x => x.Id == entity.Id)));
		}

	}
}
