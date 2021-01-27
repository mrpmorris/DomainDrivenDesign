using DomainDrivenDesignTestProject.Mongo.DomainClasses;
using DomainDrivenDesign.MongoDB.Persistence;
using System.Linq;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace DomainDrivenDesignTestProject.Mongo.Repositories
{
	public interface IIncomingFileTransactionRepository
	{
		IQueryable<IncomingFileTransaction> Query();
		void AddOrUpdate(IncomingFileTransaction instance);
		Task<IncomingFileTransaction?> GetAsync(ObjectId id);
	}

	public class IncomingFileTransactionRepository :
		RepositoryBase<ApplicationDbContext, IncomingFileTransaction>,
		IIncomingFileTransactionRepository
	{
		protected override string GetCollectionName() => "IncomingFileTransaction";

		public IncomingFileTransactionRepository(ApplicationDbContext dbContext)
			: base(dbContext)
		{
		}
	}
}
