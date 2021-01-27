using DomainDrivenDesign.MongoDB.Persistence;

namespace DomainDriveDesignTestProject.Mongo
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