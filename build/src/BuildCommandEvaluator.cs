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
			if(command.Command.Equals(BuildCommand.COPY))
				Copy(command.Arguments);
			else if(command.Command.Equals(BuildCommand.MKDIR))
				CreateDirectory(command.Arguments);
			else if(command.Command.Equals(BuildCommand.DELETE))
				Delete(command.Arguments);
		}

		private static void CreateDirectory(List<string> arguments)
		{
			if( arguments == null || arguments.Count < 1 )
			{
				Console.WriteLine(string.Format("CreateDirectory command requires at least 1 argument."));
				return;
			}

			foreach(string path in arguments)
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch(System.Exception e)
				{
					Console.WriteLine(string.Format("mkdir {0} failed.\n{2}", path, e.ToString()));
				}
			}
		}

		private static void Copy(List<string> arguments)
		{
			if( arguments == null || arguments.Count != 2 )
			{
				Console.WriteLine(string.Format("Copy command requires 2 arguments: Copy(string source, string destination)"));
				return;
			}

			string source = arguments[0];
			string destination = arguments[1];

			try
			{
				File.Copy(source, destination, true);
			}
			catch(System.Exception e)
			{
				Console.WriteLine(string.Format("cp {0} {1} failed.\n{2}", source, destination, e.ToString()));
			}
		}

		private static void Delete(List<string> arguments)
		{
			if( arguments == null || arguments.Count < 1)
			{
				Console.WriteLine(string.Format("Delete command requires at least one argument."));
				return;
			}

			foreach(string arg in arguments)
			{
				try 
				{
					File.Delete(arg);
				}
				catch(System.Exception e)
				{
					Console.WriteLine(string.Format("rm {0} failed.\n{2}", arg, e.ToString()));
				}
			}
		}
	}
}
