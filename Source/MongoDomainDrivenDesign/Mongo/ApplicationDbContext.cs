using MongoDB.Bson;
using MongoDB.Driver;
using PeterLeslieMorris.DomainDrivenDesign.MongoDb.DomainClasses;
using PeterLeslieMorris.DomainDrivenDesign.MongoDb.Persistence;
using System.Threading.Tasks;

namespace MongoDomainDrivenDesign.Mongo
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