using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	public static class pb_Subdivide
	{
		/**
		 * Subdivide a pb_Object.
		 */
		public static pb_ActionResult Subdivide(this pb_Object pb)
		{
			pb_Face[] ignore;
			return pb.Subdivide(pb.faces, out ignore);
		}

		/**
		 * Subdivide a pb_Object, optionally restricting to faces.
		 */
		public static pb_ActionResult Subdivide(this pb_Object pb, IList<pb_Face> faces, out pb_Face[] subdividedFaces)
		{
			IEnumerable<pb_Edge> affected = faces.SelectMany(x => x.edges);
			pb_ActionResult res = pb_ConnectEdges.Connect(pb, affected, out subdividedFaces);
			return res;
		}
	}
}
