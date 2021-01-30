using DomainDrivenDesign.MongoDB.Persistence;
using DomainDrivenDesignTestProject.Mongo.DomainClasses;

namespace DomainDrivenDesignTestProject.Mongo
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<IncomingFileTransaction> IncomingFileTransactions { get; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
			IncomingFileTransactions = CreateDbSet<IncomingFileTransaction>(CollectionNames.IncomingFileTransaction);
		}

		public static class CollectionNames
		{
			public const string IncomingFileTransaction = "IncomingFileTransaction";
		}
	}
}