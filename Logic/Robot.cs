using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToyRobot.Extensions;
using ToyRobot.Model;

namespace ToyRobot.Logic
{
	/// <summary>
	/// The Robot.
	/// </summary>
	internal class Robot
	{
		/// <summary>
		/// Describes the internal state of the Robot.
		/// </summary>
		private class RobotState
		{
			/// <summary>
			/// Whether is has been legally placed on the board
			/// </summary>
			public bool Placed { get; set; } = false;
			/// <summary>
			/// Current horizontal position
			/// </summary>
			public int CurrentX { get; set; } = 0;
			/// <summary>
			/// Current vertical position
			/// </summary>
			public int CurrentY { get; set; } = 0;
			/// <summary>
			/// Current direction the Robot is facing towards
			/// </summary>
			public MovementDirection CurrentDirection { get; set; } = MovementDirection.North;
			/// <summary>
			/// Current movement velocity (1 cell/move is the default)
			/// </summary>
			public int Velocity { get; set; } = 1;
		}

		private readonly bool _autoReport;
		private readonly IEnumerable<string> _commands;
		private readonly Dictionary<InstructionType, Regex> Regexes = new Dictionary<InstructionType, Regex>();
		private const int BoardWidth = 5;
		private const int BoardHeight = 5;
		private readonly RobotState state = new RobotState();

		/// <summary>
		/// Initializes the Regex used to parse commands.
		/// </summary>
		private Robot()
		{
			this.Regexes.Add(InstructionType.Place, new Regex(@"PLACE\s+(\d)\s*,\s*(\d)\s*,(NORTH|EAST|SOUTH|WEST)", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
			this.Regexes.Add(InstructionType.Move, new Regex(@"MOVE", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
			this.Regexes.Add(InstructionType.Rotate, new Regex(@"(LEFT|RIGHT)", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
			this.Regexes.Add(InstructionType.Report, new Regex(@"REPORT", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
		}

		/// <summary>
		/// Constructs a Robot.
		/// </summary>
		/// <param name="commands">The commands to be executed</param>
		/// <param name="autoReport">Whether to automatically report the status after each command, in addition to normal Report commands</param>
		public Robot(IEnumerable<string> commands, bool autoReport = false) : this()
		{
			this._autoReport = autoReport;
			this._commands = commands.Select(c => c.Trim()).ToArray();
		}

		/// <summary>
		/// Executes the commands that have been given to the Robot.
		/// </summary>
		/// <returns></returns>
		public InstructionExecutionResult[] Execute()
		{
			var instructions = this._commands.Select(cmd => this.ParseCommand(cmd.Trim()));
			var ret = instructions
				.Select(this.Execute)
				.Select(o => new InstructionExecutionResult { Executed = o.executed, LogMessage = o.log })
				.ToArray();

			// Perform a final Report if the user hasn't explicitly issued one himself
			if (!(instructions.LastOrDefault() is ReportInstruction))
			{
				this.Execute(ReportInstruction.Create());
			}
			return ret;
		}

		/// <summary>
		/// Parses a string command into an Instruction.
		/// </summary>
		/// <param name="command">The string command to parse</param>
		/// <returns>An concrete Instruction</returns>
		private Instruction ParseCommand(string command)
		{
			// Check which regex matches the command, and therefore derive the command's type
			var matchingRegex = this.Regexes
				.DefaultIfEmpty(KeyValuePair.Create(InstructionType.Unknown, new Regex("")))
				.FirstOrDefault(kvp => kvp.Value.IsMatch(command));

			var matchedType = matchingRegex.Key;
			var rgx = matchingRegex.Value;
			switch (matchedType)
			{
				default:
				case InstructionType.Unknown:
					return NoopInstruction.Create(command);
				case InstructionType.Place:
					{
						var matches = rgx.Match(command);
						int x = int.Parse(matches.Groups.ElementAt(1).Value);
						int y = int.Parse(matches.Groups.ElementAt(2).Value);
						var direction = Enum.Parse<MovementDirection>(matches.Groups.ElementAt(3).Value.ToTitleCase());
						return PlaceInstruction.Create(x, y, direction, command);
					}
				case InstructionType.Move:
					return MoveInstruction.Create(command);
				case InstructionType.Rotate:
					{
						var matches = rgx.Match(command);
						var rotationDirection = matches.Groups.ElementAt(1).Value.ToLower() == "right"
							? RotationDirection.Clockwise
							: RotationDirection.CounterClockwise;
						return RotateInstruction.Create(rotationDirection, command);
					}
				case InstructionType.Report:
					return ReportInstruction.Create(command);
			}
		}

		/// <summary>
		/// Executes a single instruction.
		/// </summary>
		/// <param name="instruction">The instruction to execute</param>
		/// <returns>A pair of (bool, string) that report the execution of the instruction</returns>
		private (bool executed, string log) Execute(Instruction instruction)
		{
			var executed = false;
			var sb = new StringBuilder($"Execution of instruction type {instruction.GetType().Name} with raw command: {instruction.RawCommand}\n");
			switch (instruction)
			{
				case NoopInstruction noop:
					executed = false;
					break;
				case PlaceInstruction place:
					var isRepositioning = this.state.Placed;
					executed = this.TryPlacement(place.X, place.Y, place.Direction);
					sb.AppendLine(executed
						? $"Placement in ({place.X}, {place.Y}) is legal. Robot {(isRepositioning ? "repositioned" : "placed")} successfully."
						: $"Placement in ({place.X}, {place.Y}) is not legal. Robot not {(isRepositioning ? "repositioned" : "placed")}.");
					break;
				case MoveInstruction move:
					if (!this.state.Placed)
					{
						sb.AppendLine("Placement has not occurred yet. Move instruction cannot be executed.");
						break;
					}
					executed = this.TryMovement();
					var directionName = Enum.GetName(typeof(MovementDirection), this.state.CurrentDirection);
					sb.AppendLine(executed
						? $"Robot successfully moved {directionName} towards ({this.state.CurrentX}, {this.state.CurrentY})"
						: $"Robot could not be moved {directionName} from ({this.state.CurrentX}, {this.state.CurrentY})");
					break;
				case RotateInstruction rotate:
					if (!this.state.Placed)
					{
						sb.AppendLine("Placement has not occurred yet. Rotate instruction cannot be executed.");
						break;
					}
					this.Rotate(rotate.Direction);
					executed = true;
					sb.AppendLine($"Robot successfully rotated. Now facing {this.state.CurrentDirection.ToString()}");
					break;
				case ReportInstruction report:
					if (!this.state.Placed)
					{
						sb.AppendLine("Placement has not occurred yet. Report instruction cannot be executed.");
						break;
					}
					this.Report();
					executed = true;
					sb.AppendLine($"Robot status successfully reported.");
					break;
			}

			// Report the status after a successful non-report instruction, if autoReport is active
			if (this._autoReport && executed && instruction.Type != InstructionType.Report)
			{
				this.Report(true);
			}

			return (executed, sb.ToString());
		}

		/// <summary>
		/// Tries to place the Robot at the specified coordinates.
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <param name="direction">The initial direction of movement</param>
		/// <returns>True if the placement is legal, false otherwise</returns>
		private bool TryPlacement(int x, int y, MovementDirection direction)
		{
			var canPlace = this.IsPositionValid(x, y);
			if (canPlace)
			{
				this.state.CurrentX = x;
				this.state.CurrentY = y;
				this.state.CurrentDirection = direction;
				this.state.Placed = true;
			}
			return canPlace;
		}

		/// <summary>
		/// Tries to move towards the current direction of movement
		/// </summary>
		/// <returns>True if the movement is possible, false otherwise</returns>
		private bool TryMovement()
		{
			var (newX, newY) = this.CalculateNextPosition();
			var canMove = this.IsPositionValid(newX, newY);
			if (canMove)
			{
				this.state.CurrentX = newX;
				this.state.CurrentY = newY;
			}
			return canMove;
		}

		/// <summary>
		/// Rotates the Robot 90 degrees in the specified direction. This command is always successful.
		/// </summary>
		/// <param name="direction">Clockwise or counter-clockwise</param>
		private void Rotate(RotationDirection direction)
		{
			const int nPossibleDirections = 4;
			var dir = (int)this.state.CurrentDirection; // 0 to 3
			var newDir = dir + (direction == RotationDirection.Clockwise ? +1 : -1);
			if (newDir < 0)
			{
				newDir = nPossibleDirections + newDir;
			}
			this.state.CurrentDirection = (MovementDirection)newDir;
		}

		/// <summary>
		/// Reports the current position of the Robot on the board.
		/// </summary>
		/// <param name="isAutoReport">Whether to add an extra line indicating that the report is automatic</param>
		private void Report(bool isAutoReport = false)
		{
			if (isAutoReport)
			{
				Console.WriteLine("\nAutomatic report:");
			}

			const string cellTopSegment = "---";
			var lineTop = "  " + string.Concat(Enumerable.Repeat(cellTopSegment, Robot.BoardWidth)) + "-";
			var lineBottom = "  " + string.Concat(Enumerable.Repeat(cellTopSegment, Robot.BoardWidth)) + "-";
			var colCounters = "  " + string.Concat(Enumerable.Range(0, 5).Select(n => $"  {n}")) + " ";
			var lines = new StringBuilder();
			lines.AppendLine(colCounters);
			for (var y = Robot.BoardHeight - 1; y >= 0; --y)
			{
				var cells = new StringBuilder($"{y} ");
				for (var x = 0; x < Robot.BoardWidth; ++x)
				{
					var directionStr = Enum.GetName(typeof(MovementDirection), this.state.CurrentDirection);
					var cell = this.IsInCell(x, y) ? $"R{directionStr[0]}" : "  ";
					cells.Append($"|{cell}");
				}
				cells.Append("|");
				var line = cells.ToString();
				lines.AppendLine(lineTop);
				lines.AppendLine(cells.ToString());
			}
			lines.AppendLine(lineBottom);
			Console.WriteLine(lines.ToString());
		}

		/// <summary>
		/// Check whether the Robot is currently in the specified cell.
		/// </summary>
		/// <param name="x">The cell's X coordinate</param>
		/// <param name="y">The cell's Y coordinate</param>
		/// <returns>True if the Robot is in the cell, false otherwise</returns>
		private bool IsInCell(int x, int y)
		{
			return this.state.CurrentX == x && this.state.CurrentY == y;
		}

		/// <summary>
		/// Calculates the next position of the Robot if it were to move in the current direction.
		/// </summary>
		/// <returns>The next (x, y) position</returns>
		private (int x, int y) CalculateNextPosition()
		{
			var newX = this.state.CurrentX + (this.state.CurrentDirection == MovementDirection.East
				? this.state.Velocity
				: this.state.CurrentDirection == MovementDirection.West ? -this.state.Velocity
				: 0);
			var newY = this.state.CurrentY + (this.state.CurrentDirection == MovementDirection.North
				? this.state.Velocity
				: this.state.CurrentDirection == MovementDirection.South ? -this.state.Velocity
				: 0);
			return (newX, newY);
		}

		/// <summary>
		/// Checks whether a specified position is legal (ie: inside the board) or not.
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <returns>True if the position is legal, false otherwise</returns>
		private bool IsPositionValid(int x, int y)
		{
			var isXValid = 0 <= x && x < Robot.BoardWidth;
			var isYValid = 0 <= y && y < Robot.BoardHeight;
			return isXValid && isYValid;
		}
	}
}