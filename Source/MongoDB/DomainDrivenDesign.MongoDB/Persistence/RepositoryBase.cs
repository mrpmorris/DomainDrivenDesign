using DomainDrivenDesign.MongoDB.DomainClasses;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract class RepositoryBase<TEntity>
		where TEntity : AggregateRoot
	{
		protected abstract string GetCollectionName();
		protected readonly DbContext DbContext;

		protected RepositoryBase(DbContext dbContext)
		{
			DbContext = dbContext;
		}

		public void AddOrUpdate(TEntity entity)
		{
		}

		public void Delete(TEntity entity)
		{
		}

		public Task<TEntity> GetAsync(ObjectId id)
		{
			throw new NotImplementedException();
		}

		public Task<TEntity[]> GetManyAsync(IEnumerable<ObjectId> ids)
		{
			throw new NotImplementedException();
		}

		public IQueryable<TEntity> Query()
		{
			throw new NotImplementedException();
		}
	}
}
