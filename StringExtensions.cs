using System;

namespace ToyRobot
{
	public static class StringExtensions
	{
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