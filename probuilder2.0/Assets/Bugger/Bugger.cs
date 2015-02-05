// #if BUGGER

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Parabox.Debug {
public class Bugger
{
#region Data Structs

	public class LogEntry
	{
		public LogEntry(string _message)
		{
			message = _message;
			date = DateTime.Now;
			stackTrace = new StackTrace(true);
			StackFrame sf = stackTrace.GetFrame(2);
			formattedStackTrace = Path.GetFileNameWithoutExtension(sf.GetFileName()) + "::" + sf.GetMethod().Name + " - " + sf.GetFileLineNumber();
			logType = LogType.Log;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			
			sb.AppendLine(message);
			sb.AppendLine(date.ToString());
			sb.AppendLine(formattedStackTrace);
			sb.AppendLine();
			sb.Append(stackTrace.ToString());

			return sb.ToString();
		}

		public string message;
		public DateTime date;
		public StackTrace stackTrace;
		public string formattedStackTrace;
		public LogType logType;
	}

	public class TempLogEntry
	{
		public TempLogEntry(string _message, double _life)
		{
			message = _message;
			life = _life;
			#if UNITY_EDITOR
			logTime = EditorApplication.timeSinceStartup;
			#else
			logTime = Time.time;
			#endif
		}
		
		public string message;
		public double life;
		public double logTime;
	}
#endregion

#region Members

	public static Dictionary<string, LogEntry> keyedLogs = new Dictionary<string, LogEntry>();
	public static Dictionary<string, TempLogEntry> tempLog = new Dictionary<string, TempLogEntry>();

	public static string LogPath { get { return Directory.GetParent(Application.dataPath) + "/Temp/BuggerLog.txt"; } }
	public static string LogSplitString { get { return "---"; } }

	public static double lastLogEntryTime { get; internal set; }
#endregion

#region Logging Methods

	public static void DebugLogHandler(string logString, string stackTrace, LogType type)
	{
		#if !UNITY_WEBPLAYER

		string json = UnityLogToJSON(logString, stackTrace, type);

		if(!File.Exists(Bugger.LogPath))
			File.WriteAllText(LogPath, "{ \"entries\" : [\n");
		
		using (StreamWriter sw = File.AppendText(LogPath)) 
		{
			sw.WriteLine( json );	
			#if UNITY_EDITOR
			lastLogEntryTime = EditorApplication.timeSinceStartup;
			#endif
		}	
		#endif
	}

	public static void SetKey<T>(string key, T value)
	{
		if(keyedLogs.ContainsKey(key))
			keyedLogs[key] = new LogEntry(value.ToString());
		else
			keyedLogs.Add(key, new LogEntry(value.ToString()));
	}

	public static void LogFrame<T>(T value)
	{
		if(tempLog.ContainsKey("Log"))
			tempLog["Log"] = new TempLogEntry(value.ToString(), 1f);
		else
			tempLog.Add("Log", new TempLogEntry(value.ToString(), 1f));
	}

	public static void LogFrame<T>(T value, float life)
	{
		if(tempLog.ContainsKey("Log"))
			tempLog["Log"] = new TempLogEntry(value.ToString(), life);
		else
			tempLog.Add("Log", new TempLogEntry(value.ToString(), life));
	}

	public static void LogFrame<T>(string key, T value)
	{
		if(tempLog.ContainsKey(key))
			tempLog[key] = new TempLogEntry(value.ToString(), 1f);
		else
			tempLog.Add(key, new TempLogEntry(value.ToString(), 1f));
	}

	public static void LogFrame<T>(string key, T value, float life)
	{
		if(tempLog.ContainsKey(key))
			tempLog[key] = new TempLogEntry(value.ToString(), life);
		else
			tempLog.Add(key, new TempLogEntry(value.ToString(), life));
	}

	public static void Log<T>(T value)
	{
		Log(value, 3);
	}

	public static void Log<T>(T value, int offset) { Log(value, offset, LogType.Log); }

	public static void Log<T>(T value, int offset, LogType logType)
	{
		#if !UNITY_WEBPLAYER

		StackTrace trace = new StackTrace(true);
		
		string json = LogToJSON(value.ToString(), trace, offset, logType);

		if(!File.Exists(Bugger.LogPath))
			File.WriteAllText(LogPath, "{ \"entries\" : [\n");
		
		using (StreamWriter sw = File.AppendText(LogPath)) 
		{
			sw.WriteLine( json );	
			#if UNITY_EDITOR
			lastLogEntryTime = EditorApplication.timeSinceStartup;
			#endif
		}	
		#endif
	}

	public static void ClearLog()
	{
		// UnityEngine.Debug.ClearDeveloperConsole();

		#if !UNITY_WEBPLAYER && UNITY_EDITOR
		File.WriteAllText(LogPath, "{ \"entries\" : [\n");
		lastLogEntryTime = EditorApplication.timeSinceStartup;
		#endif
	}
#endregion

#region Utility

	private static string UnityLogToJSON(string msg, string stackTrace, LogType logType)
	{
		// TODO
		StringBuilder sb = new StringBuilder();
		StringWriter sw = new StringWriter(sb);

		// errors and logs
		// string LogAndErrorStackPattern = @"(.*( \(.*\))) (\(at )(.*)(:[0-9]{0,9})";
		
		string[] stackSplit = stackTrace.Split('\n');

		using (JsonWriter writer = new JsonTextWriter(sw))
		{
			writer.Formatting = Formatting.Indented;

			// Message
			writer.WriteStartObject();
			writer.WritePropertyName("message");

			writer.WriteValue(msg);

			// log type			
			writer.WritePropertyName("logtype");
			writer.WriteValue((int)logType);

			// Stack Trace
			writer.WritePropertyName("stacktrace");
			writer.WriteStartArray();


			if(stackTrace == "")
			{
				string method = "Method Name";
				string filename = "FileName.cs";

				int firstLeftParenthesisIndex = msg.IndexOf("(");
				if(firstLeftParenthesisIndex >= 0)
					filename = msg.Substring(0, firstLeftParenthesisIndex );

				int firstOpenTicMarkIndex = msg.IndexOf("`");
	
				if(firstOpenTicMarkIndex >= 0)
				{
					int nextOpenTicMarkIndex = msg.IndexOf("'", firstOpenTicMarkIndex);
	
					if(nextOpenTicMarkIndex > firstOpenTicMarkIndex)
					{
						method = msg.Substring(firstOpenTicMarkIndex+1, nextOpenTicMarkIndex- firstOpenTicMarkIndex-1);
					}
				}

				int lineNumber = 0;

				int firstCommaIndex = msg.IndexOf(",", firstLeftParenthesisIndex);
				string lineNumberSubstring = msg.Substring(firstLeftParenthesisIndex+1, firstCommaIndex - firstLeftParenthesisIndex-1 );
				int.TryParse(lineNumberSubstring, out lineNumber);
					
				writer.WriteStartObject();
					writer.WritePropertyName("method");
					writer.WriteValue(method);

					writer.WritePropertyName("path");
					writer.WriteValue(filename);
					
					writer.WritePropertyName("lineNumber");
					writer.WriteValue(lineNumber);
				writer.WriteEndObject();
			}
			else
			{
				for(int i = 0; i < stackSplit.Length; i++)
				{
					string split = stackSplit[i];

					int firstLeftParenthesisIndex = split.IndexOf("(");

					string method = "Method Name";
					if(firstLeftParenthesisIndex > 0)
						method = split.Substring(0, firstLeftParenthesisIndex);
					else
						continue;

					string filename = "";
					int fileNameIndex = split.IndexOf("(at ");
					int lastColonIndex = split.LastIndexOf(":");

					if(fileNameIndex >= 0 && lastColonIndex > fileNameIndex)
						filename = split.Substring(fileNameIndex, lastColonIndex-fileNameIndex);
					else
						continue;
					
					int lineNumber = 0;
					string lineNumberSubstring = split.Substring(lastColonIndex+1, split.Length-lastColonIndex-2);
					int.TryParse(lineNumberSubstring, out lineNumber);

					writer.WriteStartObject();
						writer.WritePropertyName("method");
						writer.WriteValue(method);

						writer.WritePropertyName("path");
						writer.WriteValue(filename);
						
						writer.WritePropertyName("lineNumber");
						writer.WriteValue(lineNumber);
					writer.WriteEndObject();
				}
			}

			writer.WriteEnd();
			writer.WriteEndObject();

			sb.Append(",\n");
		}

		return sb.ToString();		
	}


	private static string LogToJSON(string msg, StackTrace stack, int offset, LogType logType)
	{
		StackFrame[] frames = stack.GetFrames();
		int frameLength = frames.Length;

		StringBuilder sb = new StringBuilder();
		StringWriter sw = new StringWriter(sb);
		
		using (JsonWriter writer = new JsonTextWriter(sw))
		{
			writer.Formatting = Formatting.Indented;

			writer.WriteStartObject();
			writer.WritePropertyName("message");
			writer.WriteValue(msg);
			
			writer.WritePropertyName("logtype");
			writer.WriteValue((int)logType);

			writer.WritePropertyName("stacktrace");
			writer.WriteStartArray();

			for(int i = offset; i < frameLength && frames[i].GetFileName() != null; i++)
			{

				writer.WriteStartObject();
					writer.WritePropertyName("method");
					writer.WriteValue(frames[i].GetMethod().ToString());
					writer.WritePropertyName("path");
					writer.WriteValue(frames[i].GetFileName().ToString());
					writer.WritePropertyName("lineNumber");
					writer.WriteValue(frames[i].GetFileLineNumber());
				writer.WriteEndObject();
			}

			writer.WriteEnd();
			writer.WriteEndObject();

			sb.Append(",\n");
		}

		return sb.ToString();
	}

	public static bool RelativeFilePath(string filePath, out string relPath)
	{
		relPath = "";

		if(filePath == null)
			return false;

		int pathLen = filePath.Length;
		int ind = filePath.IndexOf("Assets");

		if(ind < 0)
			return false;

		relPath = filePath.Substring(ind, pathLen-ind);

		return true;
	}
#endregion
}
}
// #endif