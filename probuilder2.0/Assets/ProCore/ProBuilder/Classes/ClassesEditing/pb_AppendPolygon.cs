using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

namespace ProBuilder2.MeshOperations
{
	public static class pb_AppendPolygon
	{
		public static pb_ActionResult FillHole(this pb_Object pb, IList<int> indices)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);

			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));

			List<pb_Vertex> append_vertices = new List<pb_Vertex>();

			foreach(int i in common)
			{
				int index = sharedIndices[i][0];
				append_vertices.Add(new pb_Vertex(vertices[index]));
			}

			List<int> triangles;

			if(pb_Triangulation.TriangulateVertices(append_vertices, out triangles))
			{
				pb_FaceRebuildData data = new pb_FaceRebuildData();
				data.vertices = append_vertices;
				data.face = new pb_Face(triangles.ToArray());
				data.sharedIndices = common.ToList();
				List<pb_Face> faces = new List<pb_Face>(pb.faces);

				pb_FaceRebuildData.Apply(new pb_FaceRebuildData[] { data }, vertices, faces, lookup, null);

				pb.SetVertices(vertices);
				pb.SetFaces(faces.ToArray());
				pb.SetSharedIndices(lookup);

				pb.ToMesh();

				return new pb_ActionResult(Status.Success, "Fill Hole");
			}

			return new pb_ActionResult(Status.Failure, "Insufficient Points");
		}
	}
}
