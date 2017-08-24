using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using FbxExporters.Editor; // @todo reflect this to avoid dependency

namespace ProBuilder2.Common
{
	/*
	 * Register a delegate with the ModelExporter class so that ProBuilder can modify the mesh
	 * prior to conversion to an FBX node.
	 */
	[InitializeOnLoad]
	static class pb_FbxExportListener
	{
		static pb_FbxExportListener()
		{
			ModelExporter.onWillConvertGameObjectToNode += OnWillConvertGameObjectToNode;
			ModelExporter.onDidConvertGameObjectToNode += OnDidConvertGameObjectToNode;
		}

		private static void OnWillConvertGameObjectToNode(GameObject go)
		{
			pb_Log.Debug("OnWillConvertGameObjectToNode: " + go.name);

			pb_Object pb = go != null ? go.GetComponent<pb_Object>() : null;

			if(pb != null)
			{
				pb.ToMesh();
				pb.Refresh();
			}
		}

		private static void OnDidConvertGameObjectToNode(GameObject go)
		{
			pb_Log.Debug("OnDidConvertGameObjectToNode: " + go.name);

			pb_Object pb = go != null ? go.GetComponent<pb_Object>() : null;

			if(pb != null)
				pb.Optimize();
		}
	}
}