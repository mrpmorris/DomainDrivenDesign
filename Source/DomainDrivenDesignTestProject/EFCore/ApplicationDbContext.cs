using Microsoft.EntityFrameworkCore;
using DomainDriveDesignTestProject.EFCore.DomainClasses;

namespace DomainDriveDesignTestProject.EFCore
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<IncomingFileTransaction> IncomingFileTransaction { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=.\\Dev;Database=BenchmarkEFCore;Trusted_Connection=True;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<IncomingFileTransaction>()
				.ToTable("Test_IncomingFileTransaction")
				.HasMany(x => x.Infos);

			modelBuilder.Entity<IncomingFileTransaction>()
				.HasMany(x => x.Events);

			modelBuilder.Entity<IncomingFileTransactionInfo>()
				.ToTable("Test_IncomingFileTransactionInfo");
			modelBuilder.Entity<IncomingFileTransactionEvent>()
				.ToTable("Test_IncomingFileTransactionEvent");
		}
	}

	public class InMemoryApplicationDbContext : ApplicationDbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseInMemoryDatabase("X");
		}
	}
}
