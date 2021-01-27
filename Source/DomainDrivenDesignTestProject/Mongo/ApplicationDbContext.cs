using DomainDrivenDesign.MongoDB.Persistence;

namespace DomainDrivenDesignTestProject.Mongo
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public static class CollectionNames
		{
			public const string IncomingFileTransaction = "IncomingFileTransaction";
		}
	}
}