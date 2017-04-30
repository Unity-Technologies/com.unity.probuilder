using System;
using System.Collections.Generic;

namespace ProBuilder.BuildSystem
{
	/**
	 *	Describes a set of commands to be executed during a build.
	 */
	public class BuildCommand
	{
		/**
		 *	Copy command.
		 *	Copies files or folders from source to destination.
		 *	JSON: cp
		 *	ARGS: (string source, string destination)
		 */
		public const string COPY = "cp";

		/**
		 *	Delete command.
		 *	Remove files or folders.
		 *	JSON: rm
		 *	ARGS: (string path)
		 */
		public const string DELETE = "rm";

		/**
		 *	Create new directory.
		 */
		public const string MKDIR = "mkdir";

		// The command to execute.
		public string Command;

		// Any arguments needed by the executing command. Ex, `cp` takes source and destination.
		public List<string> Arguments;

		public void Replace(string key, string replace)
		{
			for(int i = 0; i < (Arguments != null ? Arguments.Count : 0); i++)
			{
				Arguments[i] = Arguments[i].Replace(key, replace);
			}
		}
	}
}
