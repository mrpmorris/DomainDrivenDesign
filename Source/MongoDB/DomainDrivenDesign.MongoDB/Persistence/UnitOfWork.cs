using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public interface IUnitOfWork<TDbContext>
		where TDbContext: DbContext
	{
		Task SaveChangesAsync();
	}

	public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
		where TDbContext: DbContext
	{
		protected readonly TDbContext DbContext;

		public UnitOfWork(TDbContext dbContext)
		{
			DbContext = dbContext;
		}

		public Task SaveChangesAsync() => DbContext.SaveChangesAsync();
	}
}
