using Microsoft.EntityFrameworkCore;
using MongoDomainDrivenDesign.EFCore.DomainClasses;

namespace MongoDomainDrivenDesign.EFCore
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<IncomingFileTransaction> IncomingFileTransaction { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			optionsBuilder.UseSqlServer("Server=.\\Dev;Database=BenchmarkEFCore;Trusted_Connection=True;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<IncomingFileTransaction>()
				.HasMany(x => x.Infos);

			modelBuilder.Entity<IncomingFileTransaction>()
				.HasMany(x => x.Events);
		}
	}
}
