using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public enum pb_LogLevel
	{
		None 		= 0x0,
		Error 		= 0x1,
		Warning 	= 0x2,
		Info		= 0x4,
		All			= 0xFF
	}

	/**
	 *	Debug log replacement.
	 */
	public static class pb_Log
	{
		public static Stack<pb_LogLevel> m_logStack = new Stack<pb_LogLevel>();
		
		public static pb_LogLevel m_LogLevel = pb_LogLevel.All;

		public static void PushLogLevel(pb_LogLevel level)
		{
			m_logStack.Push(m_LogLevel);
			m_LogLevel = level;
		}

		public static void PopLogLevel()
		{
			m_LogLevel = m_logStack.Pop();
		}

		public static void Log(string message)
		{
			if( (m_LogLevel & pb_LogLevel.Info) > 0 )
				Debug.Log(message);
		}

		public static void LogWarning(string message)
		{
			if( (m_LogLevel & pb_LogLevel.Warning) > 0 )
				Debug.LogWarning(message);
		}

		public static void LogError(string message)
		{
			if( (m_LogLevel & pb_LogLevel.Error) > 0 )
				Debug.LogError(message);
		}
	}
}
