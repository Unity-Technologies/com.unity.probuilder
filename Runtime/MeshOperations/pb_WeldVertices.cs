using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for welding vertices.
	/// </summary>
	public static class pb_WeldVertices
	{
		/// <summary>
		/// Similar to Merge vertices, expect that this method only collapses vertices within a specified distance of
		/// one another (typically Mathf.Epsilon is used).
		/// </summary>
		/// <param name="pb">Target pb_Object.</param>
		/// <param name="indices">The vertex indices to be scanned for inclusion. To weld the entire object for example, pass pb.faces.SelectMany(x => x.indices).</param>
		/// <param name="neighborRadius">The minimum distance from another vertex to be considered within welding distance.</param>
		/// <param name="welds">The indices of any new vertices created by a weld.</param>
		/// <returns>An action result noting the status of the operation.</returns>
		public static pb_ActionResult WeldVertices(this pb_Object pb, int[] indices, float neighborRadius, out int[] welds)
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb);
			pb_IntArray[] sharedIndices = pb.sharedIndices;

			Dictionary<int, int> lookup = sharedIndices.ToDictionary();
			HashSet<int> common = pb_IntArrayUtility.GetCommonIndices(lookup, indices);
			int vertexCount = common.Count;

			// Make assumption that there will rarely be a time when a single weld encompasses more than 32 vertices.
			// If a radial search returns neighbors matching the max count, the search is re-done and maxNearestNeighbors
			// is set to the resulting length. This will be slow, but in most cases shouldn't happen ever, or if it does,
			// should only happen once or twice.
			int maxNearestNeighbors = System.Math.Min(32, common.Count());

			// 3 dimensions, duplicate entries allowed
			KdTree<float, int> tree = new KdTree<float, int>(3, new FloatMath(), AddDuplicateBehavior.Collect);

			foreach(int i in common)
			{
				Vector3 v = vertices[sharedIndices[i][0]].position;
				tree.Add( new float[] { v.x, v.y, v.z }, i );
			}

			float[] point = new float[3] { 0, 0, 0 };
			Dictionary<int, int> remapped = new Dictionary<int, int>();
			Dictionary<int, Vector3> averages = new Dictionary<int, Vector3>();
			int index = sharedIndices.Length;

			foreach(int commonIndex in common)
			{
				// already merged with another
				if(remapped.ContainsKey(commonIndex))
					continue;

				Vector3 v = vertices[sharedIndices[commonIndex][0]].position;

				point[0] = v.x;
				point[1] = v.y;
				point[2] = v.z;

				// Radial search at each point
				KdTreeNode<float, int>[] neighbors = tree.RadialSearch(point, neighborRadius, maxNearestNeighbors);

				// if first radial search filled the entire allotment reset the max neighbor count to 1.5x.
				// the result hopefully preventing double-searches in the next iterations.
				if(maxNearestNeighbors < vertexCount && neighbors.Length >= maxNearestNeighbors)
				{
					neighbors = tree.RadialSearch(point, neighborRadius, vertexCount);
					maxNearestNeighbors = System.Math.Min(vertexCount, neighbors.Length + neighbors.Length / 2);
				}

				Vector3 avg = Vector3.zero;
				float count = 0;

				for(int neighborIndex = 0; neighborIndex < neighbors.Length; neighborIndex++)
				{
					// common index of this neighbor
					int c = neighbors[neighborIndex].Value;

					// if it's already been added to another, skip it
					if(remapped.ContainsKey(c))
						continue;

					avg.x += neighbors[neighborIndex].Point[0];
					avg.y += neighbors[neighborIndex].Point[1];
					avg.z += neighbors[neighborIndex].Point[2];

					remapped.Add(c, index);

					count++;

					if(neighbors[neighborIndex].Duplicates != null)
					{
						for(int duplicateIndex = 0; duplicateIndex < neighbors[neighborIndex].Duplicates.Count; duplicateIndex++)
							remapped.Add(neighbors[neighborIndex].Duplicates[duplicateIndex], index);
					}
				}

				avg.x /= count;
				avg.y /= count;
				avg.z /= count;

				averages.Add(index, avg);

				index++;
			}

			welds = new int[remapped.Count];
			int n = 0;

			foreach(var kvp in remapped)
			{
				int[] tris = sharedIndices[kvp.Key];

				welds[n++] = tris[0];

				for(int i = 0; i < tris.Length; i++)
				{
					lookup[tris[i]] = kvp.Value;
					vertices[tris[i]].position = averages[kvp.Value];
				}
			}

			pb.SetSharedIndices(lookup);
			pb.SetVertices(vertices);
			pb.ToMesh();

			return new pb_ActionResult(Status.Success, "Weld Vertices");
		}

	}
}
