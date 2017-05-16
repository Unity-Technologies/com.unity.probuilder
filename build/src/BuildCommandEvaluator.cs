using System;
using System.IO;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	/**
	 *	Responsible for defining BuildCommand keys and implementations.
	 */
	public static class BuildCommandEvaluator
	{
		/**
		 *	Execute a BuildCommand.
		 */
		public static void Execute(BuildCommand command)
		{
			string res = null;

			if(command.Command.Equals(BuildCommand.COPY))
				res = Copy(command.Arguments);
			else if(command.Command.Equals(BuildCommand.MKDIR))
				res = CreateDirectory(command.Arguments);
			else if(command.Command.Equals(BuildCommand.DELETE))
				res = Delete(command.Arguments);

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

		private static void CommandFailed(string command, string warning)
		{
			Log.Info("Failed: " + command);
			Log.Warning(warning);
		}
	}
}
