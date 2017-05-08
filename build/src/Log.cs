using System;

namespace ProBuilder.BuildSystem
{
	public enum LogLevel
	{
		Critical 	= 0x1,
		Error 		= 0x2,
		Warning 	= 0x4,
		Info 		= 0x8,
		All 		= 0xFF
	}

	public static class Log
	{
		public static LogLevel Level = LogLevel.All;

		public static void Info(string contents)
		{
			if((Level & LogLevel.Info) > 0)
				Console.WriteLine(contents);
		}
	}
}
