using System;
using System.Linq;
using System.Reflection;
using UnityEngine.ProBuilder;
using UnityEditor;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Addons that rely on scripting define symbols can be enabled / disabled here. This class is separated from the add-on scripts themselves and bundled in the DLL so that restarting Unity will unload scripting defines that are no longer available.
	/// </summary>
	[InitializeOnLoad]
	static class ScriptingSymbolManager
	{
		static ScriptingSymbolManager()
		{
			if( FbxTypesExist() )
				AddScriptingDefine("PROBUILDER_FBX_PLUGIN_ENABLED");
			else
				RemoveScriptingDefine("PROBUILDER_FBX_PLUGIN_ENABLED");
		}

		static bool FbxTypesExist()
		{
#if UNITY_2017_1_OR_NEWER
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type fbxExporterType = ReflectionUtility.GetType("FbxExporters.Editor.ModelExporter");
			return fbxExporterType != null && assemblies.Any(x => x.FullName.Contains("FbxSdk"));
#else
			return false;
#endif
		}

		static bool IsObsolete(BuildTargetGroup group)
		{
			var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
			return attrs.Length > 0;
		}

		/// <summary>
		/// Add a define to the scripting define symbols for every build target.
		/// </summary>
		/// <param name="define"></param>
		public static void AddScriptingDefine(string define)
		{
			foreach(BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if( targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup) )
					continue;

				string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

				if( !defineSymbols.Contains(define) )
				{
					if(defineSymbols.Length < 1)
						defineSymbols = define;
					else if(defineSymbols.EndsWith(";"))
						defineSymbols = string.Format("{0}{1}", defineSymbols, define);
					else
						defineSymbols = string.Format("{0};{1}", defineSymbols, define);

					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
				}
			}
		}

		/// <summary>
		/// Remove a define from the scripting define symbols for every build target.
		/// </summary>
		/// <param name="define"></param>
		public static void RemoveScriptingDefine(string define)
		{
			foreach(BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
			{
				if( targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup) )
					continue;

				string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

				if( defineSymbols.Contains(define) )
				{
					defineSymbols = defineSymbols.Replace(string.Format("{0};", define), "");
					defineSymbols = defineSymbols.Replace(define, "");

					PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
				}
			}
		}
	}
}
