using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ToyRobot
{
	internal class Robot
	{
		private class RobotState
		{
			public bool Placed { get; set; } = false;
			public int CurrentX { get; set; } = 0;
			public int CurrentY { get; set; } = 0;
			public MovementDirection CurrentDirection { get; set; } = MovementDirection.North;
			public int Velocity { get; set; } = 1;
		}

		private readonly bool _autoReport;
		private readonly IEnumerable<string> _commands;
		private readonly Dictionary<InstructionType, Regex> Regexes = new Dictionary<InstructionType, Regex>();
		private const int BoardWidth = 5;
		private const int BoardHeight = 5;
		private readonly RobotState state = new RobotState();

		public Robot()
		{
			this.Regexes.Add(InstructionType.Place, new Regex(@"PLACE\s+(\d)\s*,\s*(\d)\s*,(NORTH|EAST|SOUTH|WEST)", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
			this.Regexes.Add(InstructionType.Move, new Regex(@"MOVE", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
			this.Regexes.Add(InstructionType.Rotate, new Regex(@"(LEFT|RIGHT)", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
			this.Regexes.Add(InstructionType.Report, new Regex(@"REPORT", RegexOptions.ECMAScript | RegexOptions.IgnoreCase));
		}

		public Robot(IEnumerable<string> commands, bool autoReport = false) : this()
		{
			this._autoReport = autoReport;
			this._commands = commands.Select(c => c.Trim()).ToArray();
		}

		public CommandExecutionResult[] Execute()
		{
			var instructions = this._commands.Select(cmd => this.ParseCommand(cmd.Trim()));
			var ret = instructions
				.Select(this.Execute)
				.Select(o => new CommandExecutionResult { Executed = o.executed, LogMessage = o.log })
				.ToArray();
			if (!(instructions.LastOrDefault() is ReportInstruction))
			{
				this.Execute(ReportInstruction.Create());
			}
			return ret;
		}

		private Instruction ParseCommand(string command)
		{
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
						sb.AppendLine("Placement has not occurred yet. Rotate instruction cannot be executed.");
						break;
					}
					this.Report();
					executed = true;
					sb.AppendLine($"Robot status successfully reported.");
					break;
			}
			if (this._autoReport && executed && instruction.Type != InstructionType.Report)
			{
				this.Report(true);
			}

			return (executed, sb.ToString());
		}

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

		private void Report(bool isAutoReport = false)
		{
			if (isAutoReport)
			{
				Console.WriteLine("\nAutomatic report:");
			}

			const string cellTop = "---";
			var lineTop = string.Concat(Enumerable.Repeat(cellTop, Robot.BoardWidth)) + "-";
			var lineBottom = string.Concat(Enumerable.Repeat(cellTop, Robot.BoardWidth)) + "-";
			var lines = new StringBuilder();
			for (var y = Robot.BoardHeight - 1; y >= 0; --y)
			{
				var cells = new StringBuilder();
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

		private bool IsInCell(int x, int y)
		{
			return this.state.CurrentX == x && this.state.CurrentY == y;
		}

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

		private bool IsPositionValid(int x, int y)
		{
			var isXValid = 0 <= x && x < Robot.BoardWidth;
			var isYValid = 0 <= y && y < Robot.BoardHeight;
			return isXValid && isYValid;
		}
	}
}