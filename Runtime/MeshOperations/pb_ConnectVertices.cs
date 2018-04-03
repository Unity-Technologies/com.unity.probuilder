using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Utility class for connecting vertices.
	/// </summary>
	public static class pb_ConnectVertices
	{
		/// <summary>
		/// Connect vertices inserts an edge between a list of indices.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="indices">A list of indices (corresponding to the pb_Object.vertices array) to connect with new edges.</param>
		/// <param name="newVertices">A list of newly created vertex indices.</param>
		/// <returns>An action result indicating the status of the operation.</returns>
		public static pb_ActionResult Connect(this pb_Object pb, IList<int> indices, out int[] newVertices)
		{
			int sharedIndexOffset = pb.sharedIndices.Length;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			HashSet<int> distinct = new HashSet<int>(indices.Select(x=>lookup[x]));
			HashSet<int> affected = new HashSet<int>();

			foreach(int i in distinct)
				affected.UnionWith(pb.sharedIndices[i].array);

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
			HashSet<int> usedTextureGroups = new HashSet<int>(pb.faces.Select(x => x.textureGroup));
			int newTextureGroupIndex = 1;

			foreach(KeyValuePair<pb_Face, List<int>> split in splits)
			{
				pb_Face face = split.Key;

				List<ConnectFaceRebuildData> res = split.Value.Count == 2 ?
					ConnectIndicesInFace(face, split.Value[0], split.Value[1], vertices, lookup) :
					ConnectIndicesInFace(face, split.Value, vertices, lookup, sharedIndexOffset++);

				if(res == null)
					continue;

				if(face.textureGroup < 0)
				{
					while(usedTextureGroups.Contains(newTextureGroupIndex))
						newTextureGroupIndex++;

					usedTextureGroups.Add(newTextureGroupIndex);
				}

				foreach(ConnectFaceRebuildData c in res)
				{
					c.faceRebuildData.face.textureGroup 	= face.textureGroup < 0 ? newTextureGroupIndex : face.textureGroup;
					c.faceRebuildData.face.uv 				= new pb_UV(face.uv);
					c.faceRebuildData.face.smoothingGroup 	= face.smoothingGroup;
					c.faceRebuildData.face.manualUV 		= face.manualUV;
					c.faceRebuildData.face.material 		= face.material;
				}

				successfulSplits.Add(face);
				appendFaces.AddRange(res);
			}

			pb_FaceRebuildData.Apply( appendFaces.Select(x => x.faceRebuildData), pb, vertices, null, lookup, null );
			pb.SetSharedIndices(lookup);
			pb.SetSharedIndicesUV(new pb_IntArray[0]);
			int removedVertexCount = pb.DeleteFaces(successfulSplits).Length;

			lookup = pb.sharedIndices.ToDictionary();

			HashSet<int> newVertexIndices = new HashSet<int>();

			for(int i = 0; i < appendFaces.Count; i++)
				for(int n = 0; n < appendFaces[i].newVertexIndices.Count; n++)
					newVertexIndices.Add( lookup[appendFaces[i].newVertexIndices[n] + (appendFaces[i].faceRebuildData.Offset() - removedVertexCount)] );

			newVertices = newVertexIndices.Select(x => pb.sharedIndices[x][0]).ToArray();

			pb.ToMesh();

			return new pb_ActionResult(Status.Success, string.Format("Connected {0} Vertices", distinct.Count));
		}

		static List<ConnectFaceRebuildData> ConnectIndicesInFace(
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
			Vector3 nrm = pb_Math.Normal(vertices, face.indices);

			for(int i = 0; i < n_vertices.Length; i++)
			{
				pb_FaceRebuildData f = pb_AppendPolygon.FaceWithVertices(n_vertices[i], false);
				f.sharedIndices = n_sharedIndices[i];

				Vector3 fn = pb_Math.Normal(n_vertices[i], f.face.indices);

				if(Vector3.Dot(nrm, fn) < 0)
					f.face.ReverseIndices();

				faces.Add(new ConnectFaceRebuildData(f, n_indices[i]));
			}

			return faces;
		}

		static List<ConnectFaceRebuildData> ConnectIndicesInFace(
			pb_Face face,
			List<int> indices,
			List<pb_Vertex> vertices,
			Dictionary<int, int> lookup,
			int sharedIndexOffset)
		{
			if(indices.Count < 3)
				return null;

			List<pb_Edge> perimeter = pb_WingedEdge.SortEdgesByAdjacency(face);

			int splitCount = indices.Count;

			List<List<pb_Vertex>> n_vertices = pb_Util.Fill<List<pb_Vertex>>(x => { return new List<pb_Vertex>(); }, splitCount);
			List<List<int>> n_sharedIndices = pb_Util.Fill<List<int>>(x => { return new List<int>(); }, splitCount);
			List<List<int>> n_indices = pb_Util.Fill<List<int>>(x => { return new List<int>(); }, splitCount);

			pb_Vertex center = pb_Vertex.Average(vertices, indices);
			Vector3 nrm = pb_Math.Normal(vertices, face.indices);

			int index = 0;

			for(int i = 0; i < perimeter.Count; i++)
			{
				int cur = perimeter[i].x;

				n_vertices[index].Add(vertices[cur]);
				n_sharedIndices[index].Add(lookup[cur]);

				if( indices.Contains(cur) )
				{
					n_indices[index].Add(n_vertices[index].Count);
					n_vertices[index].Add(center);
					n_sharedIndices[index].Add(sharedIndexOffset);

					index = (index + 1) % splitCount;

					n_indices[index].Add(n_vertices[index].Count);
					n_vertices[index].Add(vertices[cur]);
					n_sharedIndices[index].Add(lookup[cur]);
				}
			}

			List<ConnectFaceRebuildData> faces = new List<ConnectFaceRebuildData>();

			for(int i = 0; i < n_vertices.Count; i++)
			{
				if(n_vertices[i].Count < 3)
					continue;

				pb_FaceRebuildData f = pb_AppendPolygon.FaceWithVertices(n_vertices[i], false);
				f.sharedIndices = n_sharedIndices[i];

				Vector3 fn = pb_Math.Normal(n_vertices[i], f.face.indices);

				if(Vector3.Dot(nrm, fn) < 0)
					f.face.ReverseIndices();

				faces.Add(new ConnectFaceRebuildData(f, n_indices[i]));
			}

			return faces;
		}
	}
}
