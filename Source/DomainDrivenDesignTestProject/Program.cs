using System;
using System.Collections.Generic;
using System.Text;

namespace DomainDrivenDesignTestProject
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			while (true)
			{
				Console.Clear();
				Console.WriteLine("B: Benchmark");
				Console.WriteLine("R: Run sample");
				Console.WriteLine("X: Exit");
				string choice = Console.ReadLine().ToUpperInvariant();
				Console.WriteLine();
				switch (choice)
				{
					case "B":
						Benchmark.Run();
						break;

					case "R":
						SampleApp.CreateAndRun();
						break;

					case "X":
						return;

					default:
						Console.WriteLine("Invalid option");
						break;
				}
				Console.WriteLine("Press enter");
				Console.ReadLine();
			}
		}
	}
}
