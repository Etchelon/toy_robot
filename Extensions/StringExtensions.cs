namespace ToyRobot.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Capitalizes the first letter of a string, and transforms the rest to lower case
		/// </summary>
		/// <param name="str">The string to transform</param>
		/// <returns>The input string but in Title Case</returns>
		public static string ToTitleCase(this string str)
		{
			if ((str?.Length ?? 0) == 0)
			{
				return str;
			}
			return char.ToUpper(str[0]) + str.Substring(1).ToLower();
		}
	}
}