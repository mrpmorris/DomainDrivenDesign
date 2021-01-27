using DomainDrivenDesignTestProject.Mongo.DomainClasses;
using DomainDrivenDesign.MongoDB.Persistence;

namespace DomainDrivenDesignTestProject.Mongo.Repositories
{
	public class IncomingFileTransactionRepository : RepositoryBase<IncomingFileTransaction>
	{
		protected override string GetCollectionName() => "IncomingFileTransaction";

		public IncomingFileTransactionRepository(DbContext dbContext)
			: base(dbContext)
		{
		}
	}
}
