using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ToyRobot
{
	/// <summary>
	/// The entity responsible for retrieving the list of commands to issue to the Robot
	/// </summary>
	internal class CommandsRetriever
	{
		/// <summary>
		/// How to retrieve the commands.
		/// </summary>
		enum RetrievalMode
		{
			/// <summary>
			/// Use hard coded commands
			/// </summary>
			Debug = 0,
			/// <summary>
			/// Get from file
			/// </summary>
			File,
			/// <summary>
			/// Get from standard input
			/// </summary>
			Console
		}

		public static string[] AllowedModes = new[] { "f", "c", "dbg" };
		private const string DebugCommands = @"
PLACE 1,2,EAST
MOVE
MOVE
LEFT
MOVE
REPORT";
		private const string FileName = "commands.txt";
		private readonly RetrievalMode _mode;

		/// <summary>
		/// Constructs the Retriever by specifying how to get the commands.
		/// </summary>
		/// <param name="mode">The retrieval mode: one of "f", "c", "dbg"</param>
		public CommandsRetriever(string mode)
		{
			this._mode = (mode == "dbg"
				? RetrievalMode.Debug
				: mode == "f"
				? RetrievalMode.File
				: mode == "c"
				? RetrievalMode.Console
				: (RetrievalMode?)null) ?? throw new ArgumentOutOfRangeException($"Mode {mode} is not supported");
		}

		/// <summary>
		/// Actually retrieve the commands.
		/// </summary>
		/// <returns>The list of string commands issued to the Robot</returns>
		public async Task<string[]> GetCommands()
		{
			switch (this._mode)
			{
				default:
					return await Task.FromResult(new string[0]);
				case RetrievalMode.Debug:
					{
						var commands = CommandsRetriever.DebugCommands.Split('\n');
						return commands;
					}
				case RetrievalMode.File:
					{
						var fileContent = await File.ReadAllTextAsync(CommandsRetriever.FileName);
						var commands = fileContent.Split('\n');
						return commands;
					}
				case RetrievalMode.Console:
					{
						Func<Task<string>> readAsync = () => Task.Run(() => Console.ReadLine());
						var commands = new List<string>();
						Console.WriteLine("Start entering commands. Use \"END\" to stop the input.");
						while (true)
						{
							var command = await readAsync();
							if (command.ToLower() == "end")
							{
								break;
							}
							commands.Add(command);
						}
						return commands.ToArray();
					}
			}
		}
	}
}