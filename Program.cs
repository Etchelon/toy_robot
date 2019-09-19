using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToyRobot
{
	class Program
	{
		static async Task Main(string[] args)
		{
			// Try to parse program options
			bool autoReport = args.Contains("--autoReport");
			string logLevel = args.FirstOrDefault(arg => arg.StartsWith("--log"))?.Substring("--log".Length) ?? "None";

			void PrintLines(IEnumerable<string> lines)
			{
				lines
					.Select<string, Action>(c => () => Console.WriteLine(c))
					.ToList()
					.ForEach(f => f());
			}

			Console.WriteLine("Toy Robot starting up...\n");

			// Ask the user how to retrieve the commands
			string mode = null;
			do
			{
				Console.WriteLine("Retrive commands from file [f] or via direct input [c]?");
				mode = Console.ReadLine();
			} while (!CommandsRetriever.AllowedModes.Contains(mode.ToLower()));

			var commandsRetriever = new CommandsRetriever(mode);
			// Retrieve the commands
			var commands = await commandsRetriever.GetCommands();

			// Start your engines...
			var robot = new Robot(commands, autoReport);
			// Go!
			var results = robot.Execute();

			// Log if the user asked to
			if (logLevel != "None")
			{
				var onlyFailed = logLevel == "Failed";
				var filteredResults = onlyFailed ? results.Where(o => !o.Executed) : results;
				var message = onlyFailed ? "The following commands were not executed:" : "The following commands were sent to the robot";
				var logs = filteredResults.Select(o => o.LogMessage);
				Console.WriteLine();
				Console.WriteLine(message);
				PrintLines(logs);
			}

			Console.WriteLine("\nToy Robot shutting down...");
		}
	}
}
