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
        /// Merge each pair of faces to a single face. Indexes are combined, but otherwise the properties of the first face in the pair take precedence. Returns a list of the new faces created.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pairs"></param>
        /// <param name="collapseCoincidentVertices"></param>
        /// <returns></returns>
        public static List<Face> MergePairs(ProBuilderMesh target, IEnumerable<SimpleTuple<Face, Face>> pairs, bool collapseCoincidentVertices = true)
        {
            HashSet<Face> remove = new HashSet<Face>();
            List<Face> add = new List<Face>();

            foreach (SimpleTuple<Face, Face> pair in pairs)
            {
                Face left = pair.item1;
                Face right = pair.item2;
                int leftLength = left.indexesInternal.Length;
                int rightLength = right.indexesInternal.Length;
                int[] indexes = new int[leftLength + rightLength];
                System.Array.Copy(left.indexesInternal, 0, indexes, 0, leftLength);
                System.Array.Copy(right.indexesInternal, 0, indexes, leftLength, rightLength);
                add.Add(new Face(indexes,
                        left.submeshIndex,
                        left.uv,
                        left.smoothingGroup,
                        left.textureGroup,
                        left.elementGroup,
                        left.manualUV));
                remove.Add(left);
                remove.Add(right);
            }

            List<Face> faces = target.facesInternal.Where(x => !remove.Contains(x)).ToList();
            faces.AddRange(add);
            target.faces = faces;

            if (collapseCoincidentVertices)
                CollapseCoincidentVertices(target, add);

            return add;
        }

        /// <summary>
        /// Merge a collection of faces to a single face. This function does not
        /// perform any sanity checks, it just merges faces. It's the caller's
        /// responsibility to make sure that the input is valid.
        /// In addition to merging faces this method also removes duplicate vertices
        /// created as a result of merging previously common vertices.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static Face Merge(ProBuilderMesh target, IEnumerable<Face> faces)
        {
            int mergedCount = faces != null ? faces.Count() : 0;

            if (mergedCount < 1)
                return null;

            Face first = faces.First();

            Face mergedFace = new Face(faces.SelectMany(x => x.indexesInternal).ToArray(),
                    first.submeshIndex,
                    first.uv,
                    first.smoothingGroup,
                    first.textureGroup,
                    first.elementGroup,
                    first.manualUV);

            Face[] rebuiltFaces = new Face[target.facesInternal.Length - mergedCount + 1];

            int n = 0;

            HashSet<Face> skip = new HashSet<Face>(faces);

            foreach (Face f in target.facesInternal)
            {
                if (!skip.Contains(f))
                    rebuiltFaces[n++] = f;
            }

            rebuiltFaces[n] = mergedFace;

            target.faces = rebuiltFaces;

            CollapseCoincidentVertices(target, new Face[] { mergedFace });

            return mergedFace;
        }

        /// <summary>
        /// Condense co-incident vertex positions per-face. vertices must already be marked as shared in the sharedIndexes
        /// array to be considered. This method is really only useful after merging faces.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="faces"></param>
        internal static void CollapseCoincidentVertices(ProBuilderMesh mesh, IEnumerable<Face> faces)
        {
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> matches = new Dictionary<int, int>();

            foreach (Face face in faces)
            {
                matches.Clear();

                for (int i = 0; i < face.indexesInternal.Length; i++)
                {
                    int common = lookup[face.indexesInternal[i]];

                    if (matches.ContainsKey(common))
                        face.indexesInternal[i] = matches[common];
                    else
                        matches.Add(common, face.indexesInternal[i]);
                }

                face.InvalidateCache();
            }

            MeshValidation.RemoveUnusedVertices(mesh);
        }
    }
}
