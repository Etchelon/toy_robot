namespace ToyRobot
{
	internal abstract class Instruction
	{
		public virtual InstructionType Type { get; }
		public string RawCommand { get; protected set; }

		protected Instruction(string rawCommand)
		{
			this.RawCommand = rawCommand;
		}
	}

	internal class NoopInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Unknown;

		private NoopInstruction(string rawCommand) : base(rawCommand) { }
		public static NoopInstruction Create(string rawCommand = null)
		{
			return new NoopInstruction(rawCommand);
		}
	}

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

	internal class MoveInstruction : Instruction
	{
		public override InstructionType Type => InstructionType.Move;

		private MoveInstruction(string rawCommand) : base(rawCommand) { }
		public static MoveInstruction Create(string rawCommand = null)
		{
			return new MoveInstruction(rawCommand);
		}
	}

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