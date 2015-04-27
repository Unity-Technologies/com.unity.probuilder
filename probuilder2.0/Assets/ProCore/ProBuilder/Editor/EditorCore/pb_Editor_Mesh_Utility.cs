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
			InObject.GenerateUV2();
		}
	}
}