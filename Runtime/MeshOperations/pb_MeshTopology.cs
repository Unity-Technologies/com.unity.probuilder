using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Utilities for converting between triangle and quad topologies.
	/// </summary>
	static class pb_MeshTopology
	{
		/// <summary>
		/// Convert as many faces as possible to quads.
		/// </summary>
		/// <remarks>Not implemented.</remarks>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool ToQuads(pb_Object target)
		{
			return false;
		}

		public static ActionResult ToTriangles(this pb_Object pb, IList<Face> faces, out Face[] newFaces)
		{
			List<pb_Vertex> vertices = new List<pb_Vertex>( pb_Vertex.GetVertices(pb) );
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			List<FaceRebuildData> rebuild = new List<FaceRebuildData>();

			foreach(Face face in faces)
			{
				List<FaceRebuildData> res = BreakFaceIntoTris(face, vertices, lookup);
				rebuild.AddRange(res);
			}

			FaceRebuildData.Apply(rebuild, pb, vertices, null, lookup, null);
			pb.DeleteFaces(faces);
			pb.ToMesh();

			newFaces = rebuild.Select(x => x.face).ToArray();

			return new ActionResult(Status.Success, string.Format("Triangulated {0} {1}", faces.Count, faces.Count < 2 ? "Face" : "Faces"));
		}

		private static List<FaceRebuildData> BreakFaceIntoTris(Face face, List<pb_Vertex> vertices, Dictionary<int, int> lookup)
		{
			int[] tris = face.indices;
			int triCount = tris.Length;
			List<FaceRebuildData> rebuild = new List<FaceRebuildData>(triCount / 3);

			for(int i = 0; i < triCount; i += 3)
			{
				FaceRebuildData r = new FaceRebuildData();

				r.face = new Face(face);
				r.face.SetIndices( new int[] { 0, 1, 2} );

				r.vertices = new List<pb_Vertex>() {
					vertices[tris[i  ]],
					vertices[tris[i+1]],
					vertices[tris[i+2]]
				};

				r.sharedIndices = new List<int>() {
					lookup[tris[i  ]],
					lookup[tris[i+1]],
					lookup[tris[i+2]]
				};

				rebuild.Add(r);
			}

			return rebuild;
		}
	}
}

