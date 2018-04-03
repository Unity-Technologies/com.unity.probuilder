using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Merging faces together.
	/// </summary>
	static class pb_MergeFaces
	{
		/// <summary>
		/// Merge each pair of faces to a single face. Indices are combined, but otherwise the properties of the first face in the pair take precedence. Returns a list of the new faces created.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="pairs"></param>
		/// <param name="collapseCoincidentVertices"></param>
		/// <returns></returns>
		public static List<pb_Face> MergePairs(pb_Object target, IEnumerable<pb_Tuple<pb_Face, pb_Face>> pairs, bool collapseCoincidentVertices = true)
		{
			HashSet<pb_Face> remove = new HashSet<pb_Face>();
			List<pb_Face> add = new List<pb_Face>();

			foreach(pb_Tuple<pb_Face, pb_Face> pair in pairs)
			{
				pb_Face left = pair.Item1;
				pb_Face right = pair.Item2;
				int leftLength = left.indices.Length;
				int rightLength = right.indices.Length;
				int[] indices = new int[leftLength + rightLength];
				System.Array.Copy(left.indices, 0, indices, 0, leftLength);
				System.Array.Copy(right.indices, 0, indices, leftLength, rightLength);
				add.Add(new pb_Face(indices, left.material, left.uv, left.smoothingGroup, left.textureGroup, left.elementGroup, left.manualUV));
				remove.Add(left);
				remove.Add(right);
			}

			List<pb_Face> faces = target.faces.Where(x => !remove.Contains(x)).ToList();
			faces.AddRange(add);
			target.SetFaces(faces.ToArray());

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
		public static pb_Face Merge(pb_Object target, IEnumerable<pb_Face> faces)
		{
			int mergedCount = faces != null ? faces.Count() : 0;

			if(mergedCount < 1)
				return null;

			pb_Face first = faces.First();

			pb_Face mergedFace = new pb_Face(faces.SelectMany(x => x.indices).ToArray(),
				first.material,
				first.uv,
				first.smoothingGroup,
				first.textureGroup,
				first.elementGroup,
				first.manualUV);

			pb_Face[] rebuiltFaces = new pb_Face[target.faces.Length - mergedCount + 1];

			int n = 0;

			HashSet<pb_Face> skip = new HashSet<pb_Face>(faces);

			foreach(pb_Face f in target.faces)
			{
				if(!skip.Contains(f))
					rebuiltFaces[n++] = f;
			}

			rebuiltFaces[n] = mergedFace;

			target.SetFaces(rebuiltFaces);

			CollapseCoincidentVertices(target, new pb_Face[] { mergedFace });

			return mergedFace;
		}

		/// <summary>
		/// Condense co-incident vertex positions per-face. Vertices must already be marked as shared in the sharedIndices
		/// array to be considered. This method is really only useful after merging faces.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		internal static void CollapseCoincidentVertices(pb_Object pb, IEnumerable<pb_Face> faces)
		{
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> matches = new Dictionary<int, int>();

			foreach(pb_Face face in faces)
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
			}

			pb.RemoveUnusedVertices();
		}
	}
}
