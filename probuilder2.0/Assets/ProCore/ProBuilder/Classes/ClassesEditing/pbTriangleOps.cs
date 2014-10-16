using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;

namespace ProBuilder2.MeshOperations
{
	public static class pbTriangleOps
	{
		/**
		 *	\brief Reverse the winding order for each passed #pb_Face.
		 *	@param faces The faces to apply normal flippin' to.
		 *	\returns Nothing.  No soup for you.
		 *	\sa SelectedFaces pb_Face
		 */
		public static void ReverseWindingOrder(this pb_Object pb, pb_Face[] faces)
		{
			for(int i = 0; i < faces.Length; i++)
				faces[i].ReverseIndices();

			pb.ToMesh();
			pb.Refresh();
		}	

		/**
		 * Attempt to figure out the winding order the passed face.  Note that 
		 * this may return WindingOrder.Unknown.
		 */
		public static WindingOrder GetWindingOrder(this pb_Object pb, pb_Face face)
		{
			Vector2[] p = pb_Math.PlanarProject(pb.GetVertices( face.edges.AllTriangles() ), pb_Math.Normal(pb, face));

			float sum = 0f;

			// http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
			for(int i = 0; i < p.Length; i++)
			{
				Vector2 a = p[i];
				Vector2 b = i < p.Length - 1 ? p[i+1] : p[0];

				sum += ( (b.x-a.x) * (b.y+a.y) );
			}

			return sum == 0f ? WindingOrder.Unknown : (sum >= 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
		}
	}
}