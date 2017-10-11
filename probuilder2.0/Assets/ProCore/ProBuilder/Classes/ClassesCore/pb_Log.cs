using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 * Describes the various states of chatty-ness.
	 */
	[Flags]
	public enum pb_LogLevel
	{
		None 		= 0x0,
		Error 		= 0x1,
		Warning 	= 0x2,
		Info		= 0x4,
		All			= 0xFF
	}

	/**
	 * Where the pb_Log writes to (default is Unity Console).
	 * If logging to a File, the pb_Log.outputFile must be set.
	 * You may log to one or multiple output sources.
	 */
	[Flags]
	public enum pb_LogOutput
	{
		None = 0x0,
		Console = 0x1,
		File = 0x2,
	}

	/**
	 * Debug log replacement.
	 *
	 * IMPORTANT - pb_LogEditor initializes this class from the Editor side (so preferences can be accessed)!
	 */
	public static class pb_Log
	{
		public const string DEFAULT_LOG_PATH = "ProBuilderLog.txt";

		// Retain a stack of previous log levels.
		private static Stack<pb_LogLevel> m_logStack = new Stack<pb_LogLevel>();

		// Current log level.
		private static pb_LogLevel m_LogLevel = pb_LogLevel.All;

		// Where to write log strings.
		private static pb_LogOutput m_Output = pb_LogOutput.Console;

		// Path to the log file.
		private static string m_LogFilePath = DEFAULT_LOG_PATH;

		/**
		 * Push the current log level in the stack. See also PopLogLevel.
		 */
		public static void PushLogLevel(pb_LogLevel level)
		{
			m_logStack.Push(m_LogLevel);
			m_LogLevel = level;
		}

		/**
		 * Pop the current log level in the stack. See also PushLogLevel.
		 */
		public static void PopLogLevel()
		{
			m_LogLevel = m_logStack.Pop();
		}

		/**
		 * Set the log level without modifying the stack.
		 */
		public static void SetLogLevel(pb_LogLevel level)
		{
			m_LogLevel = level;
		}

		/**
		 * Set the output destination for logs. If output is file, make sure to
		 * also set the log file path (otherwise it defaults to ProBuilderLog.txt
		 * in project directory).
		 */
		public static void SetOutput(pb_LogOutput output)
		{
			m_Output = output;
		}

		/**
		 * Set the path of the log file that pb_Log writes messages to.
		 */
		public static void SetLogFile(string path)
		{
			m_LogFilePath = path;
		}

		/**
		 * Output a debug message. These should not be committed to trunk.
		 */
		public static void Debug<T>(T value)
		{
			Debug(value.ToString());
		}

		/**
		 * Output a debug message. These should not be committed to trunk.
		 */
		public static void Debug(string message)
		{
			DoPrint(message, LogType.Log);
		}

		public static void Debug(string format, params object[] values)
		{
			Debug(string.Format(format, values));
		}

		/**
		 * Output an informational message.
		 */
		public static void Info(string format, params object[] values)
		{
			Info(string.Format(format, values));
		}

		public static void Info(string message)
		{
			if( (m_LogLevel & pb_LogLevel.Info) > 0 )
				DoPrint(message, LogType.Log);
		}

		/**
		 * Output a warning message.
		 */
		public static void Warning(string format, params object[] values)
		{
			Warning(string.Format(format, values));
		}

		public static void Warning(string message)
		{
			if( (m_LogLevel & pb_LogLevel.Warning) > 0 )
				DoPrint(message, LogType.Warning);
		}

		/**
		 * Output an error message.
		 */
		public static void Error(string format, params object[] values)
		{
			Error(string.Format(format, values));
		}

		public static void Error(string message)
		{
			if( (m_LogLevel & pb_LogLevel.Error) > 0 )
				DoPrint(message, LogType.Error);
		}

		/**
		 * ConsolePro3 specific functionality - update a single log continuously.
		 */
		public static void Watch<T, K>(T key, K value)
		{
			UnityEngine.Debug.Log(string.Format("{0} : {1}\nCPAPI:{{\"cmd\":\"Watch\" \"name\":\"{0}\"}}", key.ToString(), value.ToString()));
		}

		private static void DoPrint(string message, LogType type)
		{
			if((m_Output & pb_LogOutput.Console) > 0)
				PrintToConsole(message, type);

			if((m_Output & pb_LogOutput.File) > 0)
				PrintToFile(message, m_LogFilePath);
		}

		/**
		 * Print a message to a file.
		 */
		public static void PrintToFile(string message, string path)
		{
			if(string.IsNullOrEmpty(path))
				return;

			string full_path = Path.GetFullPath(path);

			if(string.IsNullOrEmpty(full_path))
			{
				pb_Log.PrintToConsole("m_LogFilePath bad: " + full_path);
				return;
			}

			if(!File.Exists(full_path))
			{
				string directory = Path.GetDirectoryName(full_path);

				if(string.IsNullOrEmpty(directory))
				{
					pb_Log.PrintToConsole("m_LogFilePath bad: " + full_path);
					return;
				}

				Directory.CreateDirectory(directory);

				using(StreamWriter sw = File.CreateText(full_path))
				{
					sw.WriteLine(message);
				}
			}
			else
			{
				using(StreamWriter sw = File.AppendText(full_path))
				{
					sw.WriteLine();
					sw.WriteLine(message);
				}
			}
		}

		/**
		 * Print a message to the Unity console.
		 */
		public static void PrintToConsole(string message, LogType type = LogType.Log)
		{
			if(type == LogType.Log)
				UnityEngine.Debug.Log(message);
			else if(type == LogType.Warning)
				UnityEngine.Debug.LogWarning(message);
			else if(type == LogType.Error)
				UnityEngine.Debug.LogError(message);
			else if(type == LogType.Assert)
#if UNITY_5_3_OR_NEWER
				UnityEngine.Debug.LogAssertion(message);
#else
				UnityEngine.Debug.LogError(message);
#endif
			else
				UnityEngine.Debug.Log(message);
		}

	}
}
