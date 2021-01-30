using DomainDrivenDesign.MongoDB.DomainClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	internal interface IDbSet
	{
		object Collection { get;  }
		Task SaveCollectionChangesAsync(IEnumerable<EntityEntry> entityEntries);
	}

	public class DbSet<TEntity> : IDbSet
		where TEntity : AggregateRoot
	{
		private readonly IMongoCollection<TEntity> Collection;

		internal DbSet(string collectionName, IMongoDatabase mongoDatabase)
		{
			Collection = mongoDatabase.GetCollection<TEntity>(collectionName);
		}

		object IDbSet.Collection => Collection;

		async Task IDbSet.SaveCollectionChangesAsync(IEnumerable<EntityEntry> entityEntries)
		{
			if (!entityEntries.Any())
				return;

			int expectedInsertedCount = 0;
			int expectedModifiedCount = 0;
			int expectedDeletedCount = 0;

			var updates = new List<WriteModel<TEntity>>();
			FilterDefinitionBuilder<TEntity> filterBuilder = Builders<TEntity>.Filter;
			foreach (EntityEntry entityEntry in entityEntries)
			{
				unchecked
				{
					entityEntry.Entity.ConcurrencyVersion = entityEntry.OriginalEntityConcurrencyVersion + 1;
				}

				switch (entityEntry.State)
				{
					case EntityState.Created:
						expectedInsertedCount++;
						updates.Add(CreateInsertAction((TEntity)entityEntry.Entity));
						break;

					case EntityState.Modified:
						expectedModifiedCount++;
						updates.Add(
							CreateReplaceAction(
								(TEntity)entityEntry.Entity,
								entityEntry.OriginalEntityConcurrencyVersion,
								filterBuilder));
						break;

					case EntityState.Deleted:
						expectedDeletedCount++;
						updates.Add(
							CreateDeleteAction(
								(TEntity)entityEntry.Entity,
								entityEntry.OriginalEntityConcurrencyVersion,
								filterBuilder));
						break;

					case EntityState.Unmodified:
						break;

					default:
						throw new NotImplementedException(entityEntry.State.ToString());
				}
			}

			BulkWriteResult<TEntity> result = await Collection.BulkWriteAsync(updates).ConfigureAwait(false);
			if (result.InsertedCount != expectedInsertedCount
				|| result.ModifiedCount != expectedModifiedCount
				|| result.DeletedCount != expectedDeletedCount)
			{
				throw new PersistenceConflictException();
			}
		}

		private WriteModel<TEntity> CreateInsertAction(TEntity createdEntity) =>
			new InsertOneModel<TEntity>(createdEntity);

		private WriteModel<TEntity> CreateReplaceAction(
			TEntity updatedEntity,
			int originalConcurrencyVersion,
			FilterDefinitionBuilder<TEntity> filterBuilder)
		=>
			new ReplaceOneModel<TEntity>(
				filter: filterBuilder.Where(x => 
					x.Id == updatedEntity.Id
					&& x.ConcurrencyVersion == originalConcurrencyVersion),
				replacement: updatedEntity);

		private WriteModel<TEntity> CreateDeleteAction(
			TEntity deletedEntity,
			int originalConcurrencyVersion,
			FilterDefinitionBuilder<TEntity> filterBuilder)
		=>
			new DeleteOneModel<TEntity>(
				filterBuilder.Where(x =>
					x.Id == deletedEntity.Id
					&& x.ConcurrencyVersion == originalConcurrencyVersion));
	}
}
