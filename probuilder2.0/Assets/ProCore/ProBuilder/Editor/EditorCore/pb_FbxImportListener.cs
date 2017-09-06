using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;


namespace ProBuilder2.Common
{
	[InitializeOnLoad]
	static class pb_FbxImportListener
	{
		static pb_FbxImportListener()
		{
			FbxExporters.FbxPrefab.OnUpdate += OnFbxUpdate;
		}

		static void OnFbxUpdate(FbxExporters.FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			pb_Log.Debug("boogers");
		}
	}
}