namespace ToyRobot.Model
{
	/// <summary>
	/// Describes the result of the execution of an instruction
	/// </summary>
	internal class InstructionExecutionResult
	{
		/// <summary>
		/// Whether the instruction was executed
		/// </summary>
		public bool Executed { get; set; }
		/// <summary>
		/// The log message associated to the execution
		/// </summary>
		public string LogMessage { get; set; }
	}
}