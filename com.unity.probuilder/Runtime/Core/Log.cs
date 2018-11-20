using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Describes the various states of chatty-ness.
    /// </summary>
    [Flags]
    enum LogLevel
    {
        None        = 0x0,
        Error       = 0x1,
        Warning     = 0x2,
        Info        = 0x4,
        Default     = Error | Warning,
        All         = 0xFF
    }

    /// <summary>
    /// Where the pb_Log writes to (default is Unity Console).
    /// </summary>
    /// <remarks>
    /// If logging to a File, the pb_Log.outputFile must be set.
    /// You may log to one or multiple output sources.
    /// </remarks>
    [Flags]
    enum LogOutput
    {
        None = 0x0,
        Console = 0x1,
        File = 0x2,
    }

    /// <summary>
    /// Debug log replacement.
    /// </summary>
    /// <remarks>
    /// IMPORTANT - pb_LogEditor initializes this class from the Editor side (so preferences can be accessed)!
    /// </remarks>
    static class Log
    {
        public const string k_ProBuilderLogFileName = "ProBuilderLog.txt";

        // Retain a stack of previous log levels.
        static Stack<LogLevel> s_logStack = new Stack<LogLevel>();

        // Current log level.
        static LogLevel s_LogLevel = LogLevel.All;

        // Where to write log strings.
        static LogOutput s_Output = LogOutput.Console;

        // Path to the log file.
        static string s_LogFilePath = k_ProBuilderLogFileName;

        /// <summary>
        /// Push the current log level in the stack. See also PopLogLevel.
        /// </summary>
        /// <param name="level"></param>
        public static void PushLogLevel(LogLevel level)
        {
            s_logStack.Push(s_LogLevel);
            s_LogLevel = level;
        }

        /// <summary>
        /// Pop the current log level in the stack. See also PushLogLevel.
        /// </summary>
        public static void PopLogLevel()
        {
            s_LogLevel = s_logStack.Pop();
        }

        /// <summary>
        /// Set the log level without modifying the stack.
        /// </summary>
        /// <param name="level"></param>
        public static void SetLogLevel(LogLevel level)
        {
            s_LogLevel = level;
        }

        /// <summary>
        /// Set the output destination for logs.
        /// If output is file, make sure to also set the log file path (otherwise it defaults to ProBuilderLog.txt in project directory).
        /// </summary>
        /// <param name="output"></param>
        public static void SetOutput(LogOutput output)
        {
            s_Output = output;
        }

        /// <summary>
        /// Set the path of the log file that pb_Log writes messages to.
        /// </summary>
        /// <param name="path"></param>
        public static void SetLogFile(string path)
        {
            s_LogFilePath = path;
        }

        /// <summary>
        /// Output a debug message.
        /// </summary>
        /// <remarks>These should not be committed to trunk.</remarks>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        [Conditional("DEBUG")]
        public static void Debug<T>(T value)
        {
            Debug(value.ToString());
        }

        /// <summary>
        /// Output a debug message.
        /// </summary>
        /// <remarks>
        /// These should not be committed to trunk.
        /// </remarks>
        /// <param name="message"></param>
        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            DoPrint(message, LogType.Log);
        }

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] values)
        {
            Debug(string.Format(format, values));
        }

        /// <summary>
        /// Output an informational message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        public static void Info(string format, params object[] values)
        {
            Info(string.Format(format, values));
        }

        public static void Info(string message)
        {
            if ((s_LogLevel & LogLevel.Info) > 0)
                DoPrint(message, LogType.Log);
        }

        /// <summary>
        /// Output a warning message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        public static void Warning(string format, params object[] values)
        {
            Warning(string.Format(format, values));
        }

        public static void Warning(string message)
        {
            if ((s_LogLevel & LogLevel.Warning) > 0)
                DoPrint(message, LogType.Warning);
        }

        /// <summary>
        /// Output an error message.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="values"></param>
        public static void Error(string format, params object[] values)
        {
            Error(string.Format(format, values));
        }

        public static void Error(string message)
        {
            if ((s_LogLevel & LogLevel.Error) > 0)
                DoPrint(message, LogType.Error);
        }

        /// <summary>
        /// ConsolePro3 specific functionality - update a single log continuously.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        [Conditional("CONSOLE_PRO_ENABLED")]
        internal static void Watch<T, K>(T key, K value)
        {
            UnityEngine.Debug.Log(string.Format("{0} : {1}\nCPAPI:{{\"cmd\":\"Watch\" \"name\":\"{0}\"}}", key.ToString(), value.ToString()));
        }

        static void DoPrint(string message, LogType type)
        {
            if ((s_Output & LogOutput.Console) > 0)
                PrintToConsole(message, type);

            if ((s_Output & LogOutput.File) > 0)
                PrintToFile(message, s_LogFilePath);
        }

        /// <summary>
        /// Print a message to a file.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="path"></param>
        static void PrintToFile(string message, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string full_path = Path.GetFullPath(path);

            if (string.IsNullOrEmpty(full_path))
            {
                Log.PrintToConsole("m_LogFilePath bad: " + full_path);
                return;
            }

            if (!File.Exists(full_path))
            {
                string directory = Path.GetDirectoryName(full_path);

                if (string.IsNullOrEmpty(directory))
                {
                    Log.PrintToConsole("m_LogFilePath bad: " + full_path);
                    return;
                }

                Directory.CreateDirectory(directory);

                using (StreamWriter sw = File.CreateText(full_path))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(full_path))
                {
//                  sw.WriteLine();
                    sw.WriteLine(message);
                }
            }
        }

        /// <summary>
        /// Delete the log file if it exists.
        /// </summary>
        public static void ClearLogFile()
        {
            if (File.Exists(s_LogFilePath))
                File.Delete(s_LogFilePath);
        }

        /// <summary>
        /// Print a message to the Unity console.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        static void PrintToConsole(string message, LogType type = LogType.Log)
        {
            if (type == LogType.Log)
                UnityEngine.Debug.Log(message);
            else if (type == LogType.Warning)
                UnityEngine.Debug.LogWarning(message);
            else if (type == LogType.Error)
                UnityEngine.Debug.LogError(message);
            else if (type == LogType.Assert)
#if UNITY_5_3_OR_NEWER
                UnityEngine.Debug.LogAssertion(message);
#else
                UnityEngine.Debug.LogError(message);
#endif
            else
                UnityEngine.Debug.Log(message);
        }

        internal static void NotNull<T>(T obj, string message)
        {
            if (obj == null)
                throw new ArgumentNullException(message);
        }
    }
}
