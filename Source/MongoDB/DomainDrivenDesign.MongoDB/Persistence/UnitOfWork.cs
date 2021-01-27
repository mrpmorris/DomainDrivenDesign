using System;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public interface IUnitOfWork
	{
		Task SaveChangesAsync();
	}

	public class UnitOfWork : IUnitOfWork
	{
		protected readonly DbContext DbContext;

		public UnitOfWork(DbContext dbContext)
		{
			DbContext = dbContext;
		}

		public Task SaveChangesAsync()
		{
			throw new NotImplementedException();
		}
	}
}
