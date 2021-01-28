using DomainDrivenDesign.MongoDB.DomainClasses;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract class RepositoryBase<TDbContext, TEntity>
		where TDbContext : DbContext
		where TEntity : AggregateRoot
	{
		protected abstract string GetCollectionName();
		protected readonly TDbContext DbContext;

		protected RepositoryBase(TDbContext dbContext)
		{
			DbContext = dbContext;
		}

		public void AddOrUpdate(TEntity entity)
		{
			DbContext.AddOrUpdate(GetCollectionName(), entity);
		}

		public void Delete(TEntity entity)
		{
			DbContext.Delete(GetCollectionName(), entity);
		}

		public Task<TEntity?> GetAsync(ObjectId id) =>
			DbContext.GetAsync<TEntity>(GetCollectionName(), id);

		public IQueryable<TEntity> Query() =>
			DbContext.GetQueryable<TEntity>(GetCollectionName());
	}
}
