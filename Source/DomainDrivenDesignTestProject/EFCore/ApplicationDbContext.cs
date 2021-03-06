﻿using Microsoft.EntityFrameworkCore;
using DomainDrivenDesignTestProject.EFCore.DomainClasses;

namespace DomainDrivenDesignTestProject.EFCore
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
				.HasMany(x => x.Infos);

			modelBuilder.Entity<IncomingFileTransaction>()
				.HasMany(x => x.Events);
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
