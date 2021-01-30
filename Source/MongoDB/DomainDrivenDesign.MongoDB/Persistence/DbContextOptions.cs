using System;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public abstract class DbContextOptions
	{
		public string? ConnectionString { get; set; }
		public string? DatabaseName { get; set; }

		public DbContextOptions() { }

		public DbContextOptions(string? connectionString, string? databaseName)
		{
			if (connectionString is null)
				throw new ArgumentNullException(nameof(connectionString));
			if (databaseName is null)
				throw new ArgumentNullException(nameof(databaseName));

			ConnectionString = connectionString;
			DatabaseName = databaseName;
		}
	}

	public class DbContextOptions<TDatabase> : DbContextOptions
		where TDatabase: DbContext
	{
		public DbContextOptions() : base() { }

		public DbContextOptions(string? connectionString, string? databaseName)
			: base(
					connectionString: connectionString,
					databaseName: databaseName)
		{
		}
	}
}
