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

 --debug or -d
   Compile assemblies with debug symbols.

 --silent or -q
   pb-build will not emit any console messages.

 --verbose or -v
   pb-build will emit all console messages. This is handy for debugging build targets.

 --clean or -c
   Include the ""Clean"" BuildTarget step. If omitted ""Clean"" will not be run.

 --unity=path/to/unity
   Overrides the ""UnityPath"" property on build target. Path should point to the Unity directory
   Ex, on Windows: C:\Program Files\Unity or Mac: /Applications/Unity/Unity.app

Single char switches may be combined in a single argument (ex, `mono pb-build.exe target.json -dqc`)

Build Targets:

  Build targets are json files. Some pre-defined macros are provided:
    - $UNITY - Path to Unity contents folder (resolves UnityContentsPath or UnityDataPath).
    - $TARGET_DIR - Directory that this build target json file resides in.
    - $USER - The user name of the account currently logged in to this computer.
   ";
    }
}
