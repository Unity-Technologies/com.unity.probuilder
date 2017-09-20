using UnityEngine;
using ProBuilder2.Common;
using System.Collections.Generic;

// @todo removes
using System.Linq;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Functions for decomposing faces to their base triangles.
	 */
	[System.Obsolete("See pb_MeshTopology")]
	public static class pb_Facetize
	{
		/**
		 *	Break down an object (or some subset of its faces) to triangles.
		 */
		[System.Obsolete("Use pb_MeshTopology.ToTriangles")]
		public static pb_ActionResult Facetize(this pb_Object pb, IList<pb_Face> faces, out pb_Face[] newFaces)
		{
			return pb_MeshTopology.ToTriangles(pb, faces, out newFaces);
		}
	}
}
