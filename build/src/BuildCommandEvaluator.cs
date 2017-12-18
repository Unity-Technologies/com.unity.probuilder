using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	/**
	 *	Responsible for defining BuildCommand keys and implementations.
	 */
	public static class BuildCommandEvaluator
	{
		private static Dictionary<string, string> m_NamedRegexMatches = new Dictionary<string, string>();
		private static List<string> m_RegexMatches = new List<string>();

		/**
		 *	Execute a BuildCommand.
		 */
		public static void Execute(BuildCommand command)
		{
			string res = null;

			List<string> args = new List<string>(command.Arguments);

			for(int i = 0; i < args.Count; i++)
			{
				foreach(var kvp in m_NamedRegexMatches)
					args[i] = args[i].Replace("GetFindVar(" + kvp.Key + ")", kvp.Value);

				for(int n = 0; n < m_RegexMatches.Count; n++)
					args[i] = args[i].Replace("GetFindVar(" + n + ")", m_RegexMatches[n]);
			}

			if(command.Command.Equals(BuildCommand.COPY))
				res = Copy(args);
			else if(command.Command.Equals(BuildCommand.MKDIR))
				res = CreateDirectory(args);
			else if(command.Command.Equals(BuildCommand.DELETE))
				res = Delete(args);
			else if(command.Command.Equals(BuildCommand.FIND))
				res = Find(args);
			else if(command.Command.Equals(BuildCommand.REPLACE))
				res = Replace(args);

			if(!string.IsNullOrEmpty(res))
				CommandFailed(command.ToString(), res);
		}

		private static string CreateDirectory(List<string> arguments)
		{
			if( arguments == null || arguments.Count < 1 )
				return "CreateDirectory command requires at least 1 argument.";

			string error = null;

			foreach(string path in arguments)
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch(System.Exception e)
				{
					if(error != null)
						error = string.Format("{0}\n{1}", error, e.ToString());
					else
						error = e.ToString();
				}
			}

			return error;
		}

		private static string Copy(List<string> arguments)
		{
			if( arguments == null || arguments.Count != 2 )
				return "Copy command requires 2 arguments: Copy(string source, string destination";

			string source = arguments[0];
			string destination = arguments[1];
			string error = null;

			try
			{
				if(ReferenceUtility.IsDirectory(source))
				{
					// http://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
					foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
					    Directory.CreateDirectory(dirPath.Replace(source, destination));

					foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
					    File.Copy(newPath, newPath.Replace(source, destination), true);
				}
				else
				{
					File.Copy(source, destination, true);
				}
			}
			catch(System.Exception e)
			{
				if(error != null)
					error = string.Format("{0}\n{1}", error, e.ToString());
				else
					error = e.ToString();
			}

			return error;
		}

		private static string Delete(List<string> arguments)
		{
			if( arguments == null || arguments.Count < 1)
				return "Delete command requires at least one argument.";

			string error = null;

			foreach(string arg in arguments)
			{
				try
				{
					if(ReferenceUtility.IsDirectory(arg))
						Directory.Delete(arg, true);
					else if(File.Exists(arg))
						File.Delete(arg);
				}
				catch(System.Exception e)
				{
					if(error != null)
						error = string.Format("{0}\n{1}", error, e.ToString());
					else
						error = e.ToString();
				}
			}

			return null;
		}

		private static string Find(List<string> arguments)
		{
			if(arguments == null || arguments.Count < 2)
				return "Insufficient arguments for find. `find` expects at least two arguments: file path, regex pattern. An optional third argument names the resulting variable.";


			if(!File.Exists(arguments[0]))
				return "File does not exist at path: " + arguments[0];

			string pattern = arguments[1];
			string contents = File.ReadAllText(arguments[0]);
			Match match = Regex.Match(contents, pattern, RegexOptions.Multiline);

			if(!match.Success)
				return "No match for regex `" + arguments[1] + "` found in file `" + arguments[0] + "`.";

			m_RegexMatches.Add(match.Value);

			if(arguments.Count > 2)
			{
				if(m_NamedRegexMatches.ContainsKey(arguments[2]))
					Log.Warning("Warning, attempted overwrite of named regex variable: " + arguments[2]);
				else
					m_NamedRegexMatches.Add(arguments[2], match.Value);
			}

			Log.Info("Found match: " + match.Value);

			return null;
		}

		private static string Replace(List<string> arguments)
		{
			if(arguments == null || arguments.Count != 3)
				return "Replace accepts 3 arguments: File path, Regex pattern, Replace text";

			try
			{
				string path = arguments[0];
				string contents = File.ReadAllText(path);
				string pattern = arguments[1];
				string replace = arguments[2];
				string replaced = Regex.Replace(contents, pattern, replace);
				Log.Info("=> " + pattern + " (" + replace + ")");
				File.WriteAllText(path, replaced);
			}
			catch(System.Exception e)
			{
				return e.ToString();
			}

			return null;
		}

		private static void CommandFailed(string command, string warning)
		{
			Log.Info("Failed: " + command);
			Log.Warning(warning);
		}
	}
}
