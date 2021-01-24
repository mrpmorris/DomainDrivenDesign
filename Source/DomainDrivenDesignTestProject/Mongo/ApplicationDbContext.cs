using DomainDrivenDesign.MongoDB.Persistence;

namespace DomainDriveDesignTestProject.Mongo
{
	public class ApplicationDbContext : DatabaseContext
	{
		public ApplicationDbContext(DatabaseContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public static class CollectionNames
		{
			public const string IncomingFileTransaction = "IncomingFileTransaction";
		}
	}
}