using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Experimental.CSG
{
	/// <summary>
	/// Represents a polygon face with an arbitrary number of vertexes.
	/// </summary>
	sealed class CSG_Polygon
	{
		public List<CSG_Vertex> vertexes;
		public CSG_Plane plane;

		public CSG_Polygon(List<CSG_Vertex> list)
		{
			this.vertexes = list;
			this.plane = new CSG_Plane(list[0].position, list[1].position, list[2].position);
		}

		public void Flip()
		{
			this.vertexes.Reverse();

			for(int i = 0; i < vertexes.Count; i++)
				vertexes[i].Flip();

			plane.Flip();
		}

		public override string ToString()
		{
			// return System.String.Format("V: {0}, {1}, {2}\nN: ({3}, {4}, {5})",
			// 	new object[] {
			// 		vertexes[0].position.ToString(),
			// 		vertexes[1].position.ToString(),
			// 		vertexes[2].position.ToString(),
			// 		plane.normal.x,
			// 		plane.normal.y,
			// 		plane.normal.z
			// 	});
			return "N: " + plane.normal;
		}
	}
}