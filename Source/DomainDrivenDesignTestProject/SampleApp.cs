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
		private readonly IUnitOfWork<ApplicationDbContext, ValidationError> UnitOfWork;
		private readonly IIncomingFileTransactionRepository Repository;

		public SampleApp(
			IUnitOfWork<ApplicationDbContext, ValidationError> unitOfWork,
			IIncomingFileTransactionRepository repository)
		{
			UnitOfWork = unitOfWork;
			Repository = repository;
		}

		public async Task RunAsync()
		{
			var newObject = new IncomingFileTransaction();
			newObject.Id = Guid.Parse("10dd8f53-f383-49e1-9dae-e9f22011a97d");
			Repository.AddOrUpdate(newObject);
			await UnitOfWork.CommitAsync().ConfigureAwait(false);

			// Retrieving the same ID should give the same instance
			var retrievedObject1 = await Repository.GetAsync(newObject.Id).ConfigureAwait(false);
			Console.WriteLine("Same == " + (newObject == retrievedObject1));

			// Altering the ConcurrencyVersion should have no effect
			retrievedObject1.ConcurrencyVersion = 999;
			Repository.AddOrUpdate(retrievedObject1);
			await UnitOfWork.CommitAsync().ConfigureAwait(false);

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
			services.AddScoped<IUnitOfWork<ApplicationDbContext,ValidationError>, UnitOfWork<ApplicationDbContext, ValidationError>>();
			services.AddScoped<IIncomingFileTransactionRepository, IncomingFileTransactionRepository>();
			services.AddScoped<
					IValidationService<ApplicationDbContext, ValidationError>,
					NullValidationService<ApplicationDbContext>
				> ();

			IServiceProvider sp = services.BuildServiceProvider();
			var instance = sp.GetService<SampleApp>();
			instance.RunAsync().Wait();
		}
	}
}
