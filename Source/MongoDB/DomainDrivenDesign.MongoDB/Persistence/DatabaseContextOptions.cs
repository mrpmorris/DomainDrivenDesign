namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract class DatabaseContextOptions
	{
		public string? ConnectionString { get; set; }
		public string? DatabaseName { get; set; }
	}

	public class DatabaseContextOptions<TDatabase> : DatabaseContextOptions
		where TDatabase: DatabaseContext
	{
	}
}
