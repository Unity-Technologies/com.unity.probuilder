using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Subdivide a ProBuilder mesh.
	/// </summary>
	static class Subdivision
	{
		/// <summary>
		/// Subdivide all faces on the mesh.
		/// </summary>
		/// <remarks>More accurately, this inserts a vertex at the center of each face and connects each edge at it's center.</remarks>
		/// <param name="pb"></param>
		/// <returns></returns>
		public static ActionResult Subdivide(this ProBuilderMesh pb)
		{
			Face[] ignore;
			return pb.Subdivide(pb.faces, out ignore);
		}

		/// <summary>
		/// Subdivide a pb_Object, optionally restricting to the specified faces.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces">The faces to be affected by subdivision.</param>
		/// <param name="subdividedFaces"></param>
		/// <returns>An result indicating the status of the action.</returns>
		public static ActionResult Subdivide(this ProBuilderMesh pb, IList<Face> faces, out Face[] subdividedFaces)
		{
			ActionResult res = ConnectElements.Connect(pb, faces, out subdividedFaces);
			return res;
		}
	}
}
