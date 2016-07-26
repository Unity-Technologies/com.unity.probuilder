using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.MeshOperations
{
	/**
	 *	Utility class for connecting vertices.
	 */
	public static class pb_ConnectVertices
	{
		public static pb_ActionResult Connect(this pb_Object pb, IList<int> indices, out pb_Edge[] connectingEdges)
		{
			connectingEdges = null;

			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			HashSet<int> affected = new HashSet<int>(pb_IntArrayUtility.AllIndicesWithValues(pb.sharedIndices, lookup, indices));
			Dictionary<pb_Face, List<int>> splits = new Dictionary<pb_Face, List<int>>();
			List<pb_Vertex> vertices = new List<pb_Vertex>(pb_Vertex.GetVertices(pb));

			foreach(pb_Face face in pb.faces)
			{
				int[] faceIndices = face.distinctIndices;

				for(int i = 0; i < faceIndices.Length; i++)
				{
					if( affected.Contains(faceIndices[i]) )
						splits.AddOrAppend(face, faceIndices[i]);
				}
			}

			List<ConnectFaceRebuildData> appendFaces = new List<ConnectFaceRebuildData>();
			List<pb_Face> successfulSplits = new List<pb_Face>();

			foreach(KeyValuePair<pb_Face, List<int>> split in splits)
			{
				List<ConnectFaceRebuildData> res = split.Value.Count == 2 ? 
					ConnectIndicesInFace(split.Key, split.Value[0], split.Value[1], vertices, lookup) :
					null;

				if(res == null)
					continue;

				successfulSplits.Add(split.Key);
				appendFaces.AddRange( res );
			}

			// foreach(var kvp in splits)
			// 	Debug.Log(kvp.Key + "\n" + kvp.Value.ToString(","));

			pb_FaceRebuildData.Apply( appendFaces.Select(x => x.faceRebuildData), pb, vertices, null, lookup, null );
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			int removedVertexCount = pb.DeleteFaces(successfulSplits).Length;

			pb.ToMesh();

			return pb_ActionResult.NoSelection;
		}

		private static List<ConnectFaceRebuildData> ConnectIndicesInFace(
			pb_Face face,
			int a,
			int b,
			List<pb_Vertex> vertices,
			Dictionary<int, int> lookup)
		{
			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

			List<pb_Vertex>[] n_vertices = new List<pb_Vertex>[] {
				new List<pb_Vertex>(),
				new List<pb_Vertex>()
			};

			List<int>[] n_sharedIndices = new List<int>[] {
				new List<int>(),
				new List<int>()
			};

			List<int>[] n_indices = new List<int>[] {
				new List<int>(),
				new List<int>()
			};

			int index = 0;

			Debug.Log(perimeter.ToString(",") + "\n" + a + ", " + b);

			for(int i = 0; i < perimeter.Count; i++)
			{
				// trying to connect two vertices that are already connected
				if(perimeter[i].Contains(a) && perimeter[i].Contains(b))
					return null;

				int cur = perimeter[i].x;

				n_vertices[index].Add(vertices[cur]);
				n_sharedIndices[index].Add(lookup[cur]);

				if(cur == a || cur == b)
				{
					index = (index + 1) % 2;

					n_indices[index].Add(n_vertices[index].Count);
					n_vertices[index].Add(vertices[cur]);
					n_sharedIndices[index].Add(lookup[cur]);
				}
			}

			List<ConnectFaceRebuildData> faces = new List<ConnectFaceRebuildData>();

			for(int i = 0; i < n_vertices.Length; i++)
			{
				Debug.Log(n_vertices[i].ToString("\n"));
				pb_FaceRebuildData f = pb_AppendPolygon.FaceWithVertices(n_vertices[i], false);
				f.sharedIndices = n_sharedIndices[i];
				faces.Add(new ConnectFaceRebuildData(f, n_indices[i]));
			}

			return faces;
		}
	}
}
