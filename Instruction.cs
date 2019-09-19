namespace ToyRobot
{
	/// <summary>
	/// Abstract class that represents an instruction to be executed by the Robot.
	/// </summary>
	internal abstract class Instruction
	{
		/// <summary>
		/// The type of instruction
		/// </summary>
		public virtual InstructionType Type { get; }
		/// <summary>
		/// The raw string command used to construct this instruction
		/// </summary>
		public string RawCommand { get; protected set; }

		protected Instruction(string rawCommand)
		{
			this.RawCommand = rawCommand;
		}
	}

	/// <summary>
	/// An instruction of unknown type, which does not do anything.
	/// </summary>
	internal class NoopInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Unknown;

		private NoopInstruction(string rawCommand) : base(rawCommand) { }
		public static NoopInstruction Create(string rawCommand = null)
		{
			return new NoopInstruction(rawCommand);
		}
	}

	/// <summary>
	/// Instruction that places, or repositions, the Robot on the board.
	/// </summary>
	internal class PlaceInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Place;
		public int X { get; set; }
		public int Y { get; set; }
		public MovementDirection Direction { get; set; }

		private PlaceInstruction(string rawCommand) : base(rawCommand) { }
		public static PlaceInstruction Create(int x, int y, MovementDirection direction, string rawCommand = null)
		{
			return new PlaceInstruction(rawCommand)
			{
				X = x,
				Y = y,
				Direction = direction
			};
		}
	}

	/// <summary>
	/// Instruction that moves the Robot forward (if the movement is legal).
	/// </summary>
	internal class MoveInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Move;

		private MoveInstruction(string rawCommand) : base(rawCommand) { }
		public static MoveInstruction Create(string rawCommand = null)
		{
			return new MoveInstruction(rawCommand);
		}
	}

	/// <summary>
	/// Instruction that rotates the Robot clockwise or counter-clockwise by 90 degrees.
	/// </summary>
	internal class RotateInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Rotate;
		public RotationDirection Direction { get; set; }

		private RotateInstruction(string rawCommand) : base(rawCommand) { }
		public static RotateInstruction Create(RotationDirection direction, string rawCommand = null)
		{
			return new RotateInstruction(rawCommand) { Direction = direction };
		}
	}

	/// <summary>
	/// Instruct the Robot to report its current status.
	/// </summary>
	internal class ReportInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Report;

		private ReportInstruction(string rawCommand) : base(rawCommand) { }
		public static ReportInstruction Create(string rawCommand = null)
		{
			return new ReportInstruction(rawCommand);
		}
	}
}