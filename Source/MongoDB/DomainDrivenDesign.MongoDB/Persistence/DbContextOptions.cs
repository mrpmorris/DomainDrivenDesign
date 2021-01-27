namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract class DbContextOptions
	{
		public string? ConnectionString { get; set; }
		public string? DatabaseName { get; set; }
	}

	public class DbContextOptions<TDatabase> : DbContextOptions
		where TDatabase: DbContext
	{
	}
}
