using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Merging faces together.
	/// </summary>
	static class MergeElements
	{
		/// <summary>
		/// Merge each pair of faces to a single face. Indices are combined, but otherwise the properties of the first face in the pair take precedence. Returns a list of the new faces created.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="pairs"></param>
		/// <param name="collapseCoincidentVertices"></param>
		/// <returns></returns>
		public static List<Face> MergePairs(ProBuilderMesh target, IEnumerable<SimpleTuple<Face, Face>> pairs, bool collapseCoincidentVertices = true)
		{
			HashSet<Face> remove = new HashSet<Face>();
			List<Face> add = new List<Face>();

			foreach(SimpleTuple<Face, Face> pair in pairs)
			{
				Face left = pair.item1;
				Face right = pair.item2;
				int leftLength = left.indices.Length;
				int rightLength = right.indices.Length;
				int[] indices = new int[leftLength + rightLength];
				System.Array.Copy(left.indices, 0, indices, 0, leftLength);
				System.Array.Copy(right.indices, 0, indices, leftLength, rightLength);
				add.Add(new Face(indices, left.material, left.uv, left.smoothingGroup, left.textureGroup, left.elementGroup, left.manualUV));
				remove.Add(left);
				remove.Add(right);
			}

			List<Face> faces = target.facesInternal.Where(x => !remove.Contains(x)).ToList();
			faces.AddRange(add);
			target.SetFaces(faces);

			if(collapseCoincidentVertices)
				CollapseCoincidentVertices(target, add);

			return add;
		}

		/// <summary>
		/// Merge a collection of faces to a single face. This function does not
		///	perform any sanity checks, it just merges faces. It's the caller's
		///	responsibility to make sure that the input is valid.
		///	In addition to merging faces this method also removes duplicate vertices
		///	created as a result of merging previously common vertices.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		public static Face Merge(ProBuilderMesh target, IEnumerable<Face> faces)
		{
			int mergedCount = faces != null ? faces.Count() : 0;

			if(mergedCount < 1)
				return null;

			Face first = faces.First();

			Face mergedFace = new Face(faces.SelectMany(x => x.indices).ToArray(),
				first.material,
				first.uv,
				first.smoothingGroup,
				first.textureGroup,
				first.elementGroup,
				first.manualUV);

			Face[] rebuiltFaces = new Face[target.facesInternal.Length - mergedCount + 1];

			int n = 0;

			HashSet<Face> skip = new HashSet<Face>(faces);

			foreach(Face f in target.facesInternal)
			{
				if(!skip.Contains(f))
					rebuiltFaces[n++] = f;
			}

			rebuiltFaces[n] = mergedFace;

			target.SetFaces(rebuiltFaces);

			CollapseCoincidentVertices(target, new Face[] { mergedFace });

			return mergedFace;
		}

		/// <summary>
		/// Condense co-incident vertex positions per-face. Vertices must already be marked as shared in the sharedIndices
		/// array to be considered. This method is really only useful after merging faces.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		internal static void CollapseCoincidentVertices(ProBuilderMesh pb, IEnumerable<Face> faces)
		{
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();
			Dictionary<int, int> matches = new Dictionary<int, int>();

			foreach(Face face in faces)
			{
				matches.Clear();

				for(int i = 0; i < face.indices.Length; i++)
				{
					int common = lookup[face.indices[i]];

					if(matches.ContainsKey(common))
						face.indices[i] = matches[common];
					else
						matches.Add(common, face.indices[i]);
				}

				face.InvalidateCache();
			}

			pb.RemoveUnusedVertices();
		}
	}
}
