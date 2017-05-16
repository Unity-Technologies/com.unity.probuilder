using System;

namespace ProBuilder.BuildSystem
{
	internal static class HelpText
	{
		public const string Contents = @"pb-build
============

This command line utility is used to compile ProBuilder projects & packages from source code.

Usage:

  mono pb-build.exe target.json

Switches:

 -debug
   Compile assemblies with debug symbols.

 -silent
   pb-build will not emit any console messages.

 -verbose
   pb-build will emit all console messages. This is handy for debugging build targets.

 -unity=path/to/unity
   Overrides the ""UnityPath"" property on build target. Path should point to the Unity directory
   Ex, on Windows: C:\Program Files\Unity or Mac: /Applications/Unity/Unity.app
   ";
	}
}
