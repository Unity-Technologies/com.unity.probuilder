using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for working with triangle data.
	/// </summary>
	public static class TriangleWinding
	{
		/// <summary>
		/// Reverse the winding order for each passed pb_Face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		public static void ReverseWindingOrder(this ProBuilderMesh pb, Face[] faces)
		{
			for(int i = 0; i < faces.Length; i++)
				faces[i].ReverseIndices();
		}

		/// <summary>
		/// Attempt to figure out the winding order the passed face.
		/// </summary>
		/// <remarks>May return WindingOrder.Unknown.</remarks>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static WindingOrder GetWindingOrder(this ProBuilderMesh pb, Face face)
		{
			Vector2[] p = Projection.PlanarProject(pb, face);
			return GetWindingOrder(p);
		}

		static WindingOrder GetWindingOrder(IList<Vertex> vertices, IList<int> indices)
		{
			Vector2[] p = Projection.PlanarProject(vertices, indices);
			return GetWindingOrder(p);
		}

		/// <summary>
		/// Return the winding order of a set of ordered points.
		/// </summary>
		/// <remarks>http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order</remarks>
		/// <param name="points">A set of unordered indices.</param>
		/// <returns>The winding order if found, WindingOrder.Unknown if not.</returns>
		public static WindingOrder GetWindingOrder(IList<Vector2> points)
		{
			float sum = 0f;

			int len = points.Count;

			// http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
			for(int i = 0; i < len; i++)
			{
				Vector2 a = points[i];
				Vector2 b = i < len - 1 ? points[i+1] : points[0];

				sum += ( (b.x - a.x) * (b.y+a.y) );
			}

			return sum == 0f ? WindingOrder.Unknown : (sum > 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
		}

		/// <summary>
		/// Reverses the orientation of the middle edge in a quad.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static bool FlipEdge(this ProBuilderMesh pb, Face face)
		{
			int[] indices = face.indices;

			if(indices.Length != 6)
				return false;

			int[] mode = InternalUtility.FilledArray<int>(1, indices.Length);

			for(int x = 0; x < indices.Length - 1; x++)
			{
				for(int y = x+1; y < indices.Length; y++)
				{
					if(indices[x] == indices[y])
					{
						mode[x]++;
						mode[y]++;
					}
				}
			}

			if(	mode[0] + mode[1] + mode[2] != 5 ||
				mode[3] + mode[4] + mode[5] != 5 )
				return false;

			int i0 = indices[ mode[0] == 1 ? 0 : mode[1] == 1 ? 1 : 2 ];
			int i1 = indices[ mode[3] == 1 ? 3 : mode[4] == 1 ? 4 : 5 ];

			int used = -1;

			if(mode[0] == 2)
			{
				used = indices[0];
				indices[0] =  i1;
			}
			else if(mode[1] == 2)
			{
				used = indices[1];
				indices[1] = i1;
			}
			else if(mode[2] == 2)
			{
				used = indices[2];
				indices[2] = i1;
			}

			if(mode[3] == 2 && indices[3] != used)
				indices[3] = i0;
			else if(mode[4] == 2 && indices[4] != used)
				indices[4] = i0;
			else if(mode[5] == 2 && indices[5] != used)
				indices[5] = i0;

			return true;
		}

		/// <summary>
		/// Merge all faces into a single face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		[System.Obsolete("Please use pb_MergeFaces.Merge(pb_Object target, IEnumerable<pb_Face> faces)")]
		public static Face MergeFaces(this ProBuilderMesh pb, Face[] faces)
		{
			List<int> collectedIndices = new List<int>(faces[0].indices);

			for(int i = 1; i < faces.Length; i++)
			{
				collectedIndices.AddRange(faces[i].indices);
			}

			Face mergedFace = new Face(collectedIndices.ToArray(),
			                                 faces[0].material,
			                                 faces[0].uv,
			                                 faces[0].smoothingGroup,
			                                 faces[0].textureGroup,
			                                 faces[0].elementGroup,
			                                 faces[0].manualUV);

			Face[] rebuiltFaces = new Face[pb.faces.Length - faces.Length + 1];

			int n = 0;
			foreach(Face f in pb.faces)
			{
				if(System.Array.IndexOf(faces, f) < 0)
				{
					rebuiltFaces[n++] = f;
				}
			}

			rebuiltFaces[n] = mergedFace;

			pb.SetFaces(rebuiltFaces);

			// merge vertices that are on top of one another now that they share a face
			Dictionary<int, int> shared = new Dictionary<int, int>();

			for(int i = 0; i < mergedFace.indices.Length; i++)
			{
				int sharedIndex = pb.sharedIndices.IndexOf(mergedFace.indices[i]);

				if(shared.ContainsKey(sharedIndex))
				{
					mergedFace.indices[i] = shared[sharedIndex];
				}
				else
				{
					shared.Add(sharedIndex, mergedFace.indices[i]);
				}
			}

			pb.RemoveUnusedVertices();

			return mergedFace;
		}
	}
}
