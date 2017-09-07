using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
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

		[MenuItem("Tools/Debug/ProBuilder/Reset with MeshFilter")]
		static void ResetSelection()
		{
			OnFbxUpdate(null, Selection.gameObjects);
		}

		static void OnFbxUpdate(FbxExporters.FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			foreach(GameObject go in updatedObjects)
			{
				pb_Object pb = go.GetComponent<pb_Object>();

				if(pb == null)
					continue;

				pbMeshOps.ResetPbObjectWithMeshFilter(pb, true);

				// @todo Rebuild()
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				pb_Log.Info("reset: " + go.name + " with MeshFilter");
			}
		}
	}
}
