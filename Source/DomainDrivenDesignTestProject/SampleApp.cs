using DomainDrivenDesign.MongoDB.Persistence;
using DomainDrivenDesign.MongoDB.Validation;
using DomainDrivenDesignTestProject.Mongo;
using DomainDrivenDesignTestProject.Mongo.DomainClasses;
using DomainDrivenDesignTestProject.Mongo.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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

			// Retrieving the same ID should give the same instance
			var retrievedObject1 = await Repository.GetAsync(newObject.Id).ConfigureAwait(false);
			Console.WriteLine("Same == " + (newObject == retrievedObject1));

			// Altering the ConcurrencyVersion should have no effect
			retrievedObject1.ConcurrencyVersion = 999;
			Repository.AddOrUpdate(retrievedObject1);
			await UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

			// Queryable should return the same instance
			retrievedObject1 = Repository.Query()
				.Where(x => x.ConcurrencyVersion >= 0)
				.Where(x => x.ConcurrencyVersion < 10)
				.OrderByDescending(x => x.CreatedUtc)
				.First();
			Console.WriteLine("Same == " + (newObject == retrievedObject1));

			// Should be able to select non-aggregate values
			int concurrencyVersion = Repository.Query().Select(x => x.ConcurrencyVersion).First();
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
			services.AddScoped(typeof(ValidationService<>), typeof(NullValidationService<>));

			IServiceProvider sp = services.BuildServiceProvider();
			var instance = sp.GetService<SampleApp>();
			instance.RunAsync().Wait();
		}
	}
}
