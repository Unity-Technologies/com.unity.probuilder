using System;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build;
#endif

namespace UnityEditor.ProBuilder
{
    static class ScriptingSymbolManager
    {
#if !UNITY_2021_2_OR_NEWER
        static bool IsObsolete(BuildTargetGroup group)
        {
            var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
            return attrs.Length > 0;
        }
#endif

        internal static bool ContainsDefine(string define)
        {
#if UNITY_2021_2_OR_NEWER
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
#else
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                    return false;
            }

            return true;
#endif
        }

        /// <summary>
        /// Add a define to the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void AddScriptingDefine(string define)
        {
#if UNITY_2021_2_OR_NEWER
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
#else
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                {
                    if (defineSymbols.Length < 1)
                        defineSymbols = define;
                    else if (defineSymbols.EndsWith(";"))
                        defineSymbols = string.Format("{0}{1}", defineSymbols, define);
                    else
                        defineSymbols = string.Format("{0};{1}", defineSymbols, define);

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
                }
            }
#endif
        }

        /// <summary>
        /// Remove a define from the scripting define symbols for every build target.
        /// </summary>
        /// <param name="define"></param>
        public static void RemoveScriptingDefine(string define)
        {
#if UNITY_2021_2_OR_NEWER
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
#else
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (defineSymbols.Contains(define))
                {
                    defineSymbols = defineSymbols.Replace(string.Format("{0};", define), "");
                    defineSymbols = defineSymbols.Replace(define, "");

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
                }
            }
#endif
        }
    }
}
