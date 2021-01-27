using DomainDrivenDesign.MongoDB.Persistence;
using DomainDrivenDesignTestProject.Mongo;
using DomainDrivenDesignTestProject.Mongo.DomainClasses;
using DomainDrivenDesignTestProject.Mongo.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DomainDrivenDesignTestProject
{
	public class SampleApp
	{
		private readonly IUnitOfWork<ApplicationDbContext> UnitOfWork;
		private readonly IIncomingFileTransactionRepository Repository;

		public SampleApp(
			IUnitOfWork<ApplicationDbContext> unitOfWork,
			IIncomingFileTransactionRepository repository)
		{
			UnitOfWork = unitOfWork;
			Repository = repository;
		}

		public async Task RunAsync()
		{
			var newObject = new IncomingFileTransaction();
			Repository.AddOrUpdate(newObject);
			await UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
		}

		public static void CreateAndRun()
		{
			var services = new ServiceCollection();
			services.AddScoped<SampleApp>();
			services.AddScoped(_ =>
				new DbContextOptions<ApplicationDbContext>(
					connectionString: "mongodb://localhost:27017",
					databaseName: "DomainDrivenMongo"));
			services.AddScoped<ApplicationDbContext>();
			services.AddScoped<IUnitOfWork<ApplicationDbContext>, UnitOfWork<ApplicationDbContext>>();
			services.AddScoped<IIncomingFileTransactionRepository, IncomingFileTransactionRepository>();

			IServiceProvider sp = services.BuildServiceProvider();
			var instance = sp.GetService<SampleApp>();
			instance.RunAsync().Wait();
		}
	}
}
