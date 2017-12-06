using System;
using System.Linq;
using System.Reflection;
using ProBuilder.Core;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Addons that rely on scripting define symbols can be enabled / disabled here. This class is separated from the add-on scripts themselves and bundled in the DLL so that restarting Unity will unload scripting defines that are no longer available.
	/// </summary>
	[InitializeOnLoad]
	static class pb_ScriptingSymbolManager
	{
		static pb_ScriptingSymbolManager()
		{
			if( FbxTypesExist() )
				pb_EditorUtility.AddScriptingDefine("PROBUILDER_FBX_PLUGIN_ENABLED");
			else
				pb_EditorUtility.RemoveScriptingDefine("PROBUILDER_FBX_PLUGIN_ENABLED");
		}

		private static bool FbxTypesExist()
		{
#if UNITY_2017_1_OR_NEWER
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type fbxExporterType = pb_Reflection.GetType("FbxExporters.Editor.ModelExporter");
			return fbxExporterType != null && assemblies.Any(x => x.FullName.Contains("FbxSdk"));
#else
			return false;
#endif
		}
	}
}
