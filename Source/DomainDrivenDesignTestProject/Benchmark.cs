using DomainDrivenDesign.MongoDB.Persistence;
using System;
using System.Diagnostics;
using System.Linq;

namespace DomainDrivenDesignTestProject
{
	class Benchmark
	{
		private const int NumberOfParentObjects = 1000;

		public static void Run()
		{
			Console.Write("Warming up databases ");
			SaveDataToMongoDB();
			Console.Write('.');
			SaveDataToEFCoreInMemory();
			Console.Write('.');
			SaveDataToEFCoreSqlServer();
			Console.Write('.');
			SaveDataToEFCoreSqlServerBatched();
			Console.WriteLine('.');

			long mongoTotal = 0;
			long efInMemoryTotal = 0;
			long efSqlServerBatchedTotal = 0;
			long efSqlServerTotal = 0;
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine();
				Console.WriteLine("----------");
				Console.WriteLine("Round " + (i + 1));
				Console.WriteLine("----------");
				mongoTotal += Time("MongoDB", SaveDataToMongoDB);
				efInMemoryTotal += Time("EFCoreInMemory", SaveDataToEFCoreInMemory);
				efSqlServerBatchedTotal += Time("EFCoreSQLBatched", SaveDataToEFCoreSqlServerBatched);
				efSqlServerTotal += Time("EFCoreSQL", SaveDataToEFCoreSqlServer);
			}
			Console.WriteLine();
			Console.WriteLine("Totals");
			Console.WriteLine("==========");
			Console.WriteLine($"MongoDB total = {mongoTotal} ms (av {mongoTotal / 10m} ms)");
			Console.WriteLine($"EFCoreInMemory total = {efInMemoryTotal} ms (av {efInMemoryTotal / 10m} ms)");
			Console.WriteLine($"EFCoreSQLBatched total = {efSqlServerBatchedTotal} ms (av {efSqlServerBatchedTotal / 10m} ms)");
			Console.WriteLine($"EFCoreSQL total = {efSqlServerTotal} ms (av {efSqlServerTotal / 10m} ms)");
			Console.ReadLine();
		}

		static long Time(string name, Action action)
		{
			Stopwatch sw = Stopwatch.StartNew();
			action();
			sw.Stop();
			Console.WriteLine($"{name} took {sw.ElapsedMilliseconds} ms");
			return sw.ElapsedMilliseconds;
		}

		static void SaveDataToEFCoreInMemory()
		{
			var data = Enumerable.Range(1, NumberOfParentObjects).Select(x => new EFCore.DomainClasses.IncomingFileTransaction());
			using (var db = new EFCore.InMemoryApplicationDbContext())
			{
				db.IncomingFileTransaction.AddRange(data);
				db.SaveChanges();
			}
		}

		static void SaveDataToEFCoreSqlServer()
		{
			var data = Enumerable.Range(1, NumberOfParentObjects).Select(x => new EFCore.DomainClasses.IncomingFileTransaction());
			using (var db = new EFCore.ApplicationDbContext())
			{
				db.IncomingFileTransaction.AddRange(data);
				db.SaveChanges();
			}
		}

		static void SaveDataToEFCoreSqlServerBatched()
		{
			var data = Enumerable.Range(1, NumberOfParentObjects).Select(x => new EFCore.DomainClasses.IncomingFileTransaction());
			using (var db = new EFCore.ApplicationDbContext())
			{
				db.IncomingFileTransaction.AddRange(data);
				db.BulkSaveChangesAsync(options => options.BatchSize = 100_000).Wait();
			}
		}


		static void SaveDataToMongoDB()
		{
			var data = Enumerable.Range(1, NumberOfParentObjects).Select(x => new Mongo.DomainClasses.IncomingFileTransaction());

			var options = new DbContextOptions<DomainDrivenDesignTestProject.Mongo.ApplicationDbContext>()
			{
				ConnectionString = "mongodb://localhost:27017",
				DatabaseName = "DomainDrivenMongo"
			};
			var db = new Mongo.ApplicationDbContext(options);
			var repo = new DomainDrivenDesignTestProject.Mongo.Repositories.IncomingFileTransactionRepository(db);

			foreach (Mongo.DomainClasses.IncomingFileTransaction ift in data)
				repo.AddOrUpdate(ift);

			IUnitOfWork uow = new UnitOfWork(db);
			uow.SaveChangesAsync().Wait();
		}
	}
}
