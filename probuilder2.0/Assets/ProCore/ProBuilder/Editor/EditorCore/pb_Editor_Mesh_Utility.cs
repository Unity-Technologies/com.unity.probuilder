using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.Collections;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Helper functions that are only available in the Editor.
	 */
	public static class pb_Editor_Mesh_Utility
	{
		/**
		 * Optmizes the mesh geometry, and generates a UV2 channel (if automatic lightmap generation is enabled).
		 * Also sets the pb_Object to 'Dirty' so that changes are stored.
		 */
		public static void Optimize(this pb_Object InObject)
		{
			EditorUtility.SetDirty(InObject);

			profiler.Begin("Optimize");
			profiler.Begin("GeneratePerTriangleMesh");
			int[][] triangles;
			pb_Vertex[] vertices = pb_MeshUtility.GeneratePerTriangleMesh(InObject.msh);
			profiler.End();

			profiler.Begin("GeneratePerTriangleUV");
			InObject.msh.uv2 = Unwrapping.GeneratePerTriangleUV(InObject.msh);
			profiler.End();

			// profiler.Begin("Apply UV2 to vertices");
			// for(int i = 0; i < uv2.Length; i++)
			// {
			// 	vertices[i].uv2 = uv2[i];
			// 	vertices[i].hasUv2 = true;
			// }
			// profiler.End();

			// profiler.Begin("Merge and Apply");
			// pb_MeshUtility.MergeVerticesAndApply(vertices, InObject.msh);
			// profiler.End();

			profiler.Begin("CollapseSharedVertices");
			// Merge compatible shared vertices to a single vertex.	
			pb_MeshUtility.CollapseSharedVertices(vertices, InObject.msh);
			profiler.End();

			profiler.End();

			profiler.Print();

			float time = Time.realtimeSinceStartup;
			// InObject.GenerateUV2();

			// If GenerateUV2() takes longer than 3 seconds (!), show a warning prompting user
			// to disable auto-uv2 generation.
			if( (Time.realtimeSinceStartup - time) > 3f )
				Debug.LogWarning(string.Format("Generate UV2 for \"{0}\" took {1} seconds!  You may want to consider disabling Auto-UV2 generation in the `Preferences > ProBuilder` tab.", InObject.name, (Time.realtimeSinceStartup - time).ToString("F2")));
		}
	}
}
