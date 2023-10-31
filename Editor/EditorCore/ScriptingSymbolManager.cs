using System;
using UnityEditor.Build;

namespace UnityEditor.ProBuilder
{
    static class ScriptingSymbolManager
    {
        internal static bool ContainsDefine(string define)
        {
            var validPlatforms = BuildPlatforms.instance.GetValidPlatforms(true);
            foreach (BuildPlatform targetPlatform in validPlatforms)
            {
                if (targetPlatform.namedBuildTarget == NamedBuildTarget.Unknown)
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbols(targetPlatform.namedBuildTarget);

                if (!defineSymbols.Contains(define))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Add a define to the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void AddScriptingDefine(string define)
        {
            var validPlatforms = BuildPlatforms.instance.GetValidPlatforms(true);
            foreach (BuildPlatform targetPlatform in validPlatforms)
            {
                if (targetPlatform.namedBuildTarget == NamedBuildTarget.Unknown)
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbols(targetPlatform.namedBuildTarget);

                if (!defineSymbols.Contains(define))
                {
                    if (defineSymbols.Length < 1)
                        defineSymbols = define;
                    else if (defineSymbols.EndsWith(";"))
                        defineSymbols = string.Format("{0}{1}", defineSymbols, define);
                    else
                        defineSymbols = string.Format("{0};{1}", defineSymbols, define);

                    PlayerSettings.SetScriptingDefineSymbols(targetPlatform.namedBuildTarget, defineSymbols);
                }
            }
        }

        /// <summary>
        /// Remove a define from the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void RemoveScriptingDefine(string define)
        {
            var validPlatforms = BuildPlatforms.instance.GetValidPlatforms(true);
            foreach (BuildPlatform targetPlatform in validPlatforms)
            {
                if (targetPlatform.namedBuildTarget == NamedBuildTarget.Unknown)
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbols(targetPlatform.namedBuildTarget);

                if (defineSymbols.Contains(define))
                {
                    defineSymbols = defineSymbols.Replace(string.Format("{0};", define), "");
                    defineSymbols = defineSymbols.Replace(define, "");

                    PlayerSettings.SetScriptingDefineSymbols(targetPlatform.namedBuildTarget, defineSymbols);
                }
            }
        }
    }
}
