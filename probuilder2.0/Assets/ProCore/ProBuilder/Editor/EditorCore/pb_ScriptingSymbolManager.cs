using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	[InitializeOnLoad]
	static class pb_ScriptingSymbolManager
	{
		static pb_ScriptingSymbolManager()
		{
			if( FbxTypesExist() )
			{
				pb_Log.Debug("Loading FBX support");
				pb_EditorUtility.AddScriptingDefine("PROBUILDER_FBX_ENABLED");
			}
			else
			{
				pb_Log.Debug("Unloading FBX support");
				pb_EditorUtility.RemoveScriptingDefine("PROBUILDER_FBX_ENABLED");
			}
		}

		private static bool FbxTypesExist()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Type fbxExporterType = pb_Reflection.GetType("FbxExporters.Editor.ModelExporter");
			return fbxExporterType != null && assemblies.Any(x => x.FullName.Contains("FbxSdk"));
		}
	}
}