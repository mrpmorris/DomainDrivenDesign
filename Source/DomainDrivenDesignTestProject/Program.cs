using DomainDrivenDesign.MongoDB.Persistence;
using System;
using System.Diagnostics;
using System.Linq;

namespace DomainDriveDesignTestProject
{
	class Program
	{
		static void Main(string[] args)
		{
			SaveDataToMongoDB();
			SaveDataToEFCore();

			long efTotal = 0;
			long mongoTotal = 0;
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine();
				Console.WriteLine("----------");
				Console.WriteLine("Round " + (i + 1));
				Console.WriteLine("----------");
				mongoTotal += Time("MongoDB", SaveDataToMongoDB);
				efTotal += Time("EFCore", SaveDataToEFCore);
			}
			Console.WriteLine("==========");
			Console.WriteLine("MongoDB total = " + mongoTotal);
			Console.WriteLine("EFCore total = " + efTotal);
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

		static void SaveDataToEFCore()
		{
			var data = Enumerable.Range(1, 1000).Select(x => new EFCore.DomainClasses.IncomingFileTransaction());
			using (var db = new EFCore.ApplicationDbContext())
			{
				db.IncomingFileTransaction.AddRange(data);
				db.SaveChanges();
			}
		}

		static void SaveDataToMongoDB()
		{
			const string collectionName = Mongo.ApplicationDbContext.CollectionNames.IncomingFileTransaction;

			var data = Enumerable.Range(1, 1000).Select(x => new Mongo.DomainClasses.IncomingFileTransaction());

			var options = new DatabaseContextOptions<DomainDriveDesignTestProject.Mongo.ApplicationDbContext>()
			{
				ConnectionString = "mongodb://localhost:30001",
				DatabaseName = "SwitchStream"
			};
			var db = new Mongo.ApplicationDbContext(options);

			foreach (Mongo.DomainClasses.IncomingFileTransaction ift in data)
				db.AddOrUpdate(collectionName, ift);
			db.SaveChangesAsync().Wait();
		}
	}
}
