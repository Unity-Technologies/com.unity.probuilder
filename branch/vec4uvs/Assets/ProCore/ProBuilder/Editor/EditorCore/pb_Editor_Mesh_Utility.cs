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
			pb_MeshUtility.CollapseSharedVertices(InObject);	///< Merge compatible shared vertices to a single vertex.	

			float time = Time.realtimeSinceStartup;
			InObject.GenerateUV2();

			// If GenerateUV2() takes longer than 3 seconds (!), show a warning prompting user
			// to disable auto-uv2 generation.
			if( (Time.realtimeSinceStartup - time) > 3f )
				Debug.LogWarning(string.Format("Generate UV2 for \"{0}\" took {1} seconds!  You may want to consider disabling Auto-UV2 generation in the `Preferences > ProBuilder` tab.", InObject.name, (Time.realtimeSinceStartup - time).ToString("F2")));
		}
	}
}