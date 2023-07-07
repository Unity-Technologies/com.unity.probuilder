using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Provides methods for merging multiple <see cref="ProBuilderMesh"/> objects into a single mesh.
    /// </summary>
	public static class CombineMeshes
    {
        /// <summary>
        /// Merges a collection of <see cref="ProBuilderMesh"/> objects to create as few meshes as possible. This may result in
        /// more than one mesh due to a max vertex count limit of 65535.
        /// </summary>
        /// <param name="meshes">The collection of meshes to merge.</param>
        /// <returns>
        /// A list of merged meshes. In most cases this will be a single mesh. However it can be multiple in cases
        /// where the resulting vertex count exceeds the maximum allowable value.
        /// </returns>
        [Obsolete("Combine(IEnumerable<ProBuilderMesh> meshes) is deprecated. Plase use Combine(IEnumerable<ProBuilderMesh> meshes, ProBuilderMesh meshTarget).")]
        public static List<ProBuilderMesh> Combine(IEnumerable<ProBuilderMesh> meshes)
        {
            return CombineToNewMeshes(meshes);
        }

        /// <summary>
        /// Merges a collection of <see cref="ProBuilderMesh"/> objects into as few meshes as possible. It re-uses the `meshTarget` object as the first
        /// destination for the first <see cref="ProBuilderMesh.maxVertexCount"/> -1 vertices. If the sum of vertices is above <see cref="ProBuilderMesh.maxVertexCount"/> - 1,
        /// it generates new meshes unless there is a single mesh left. In that case it appends it to the return list.
        /// </summary>
        /// <param name="meshes">A collection of meshes to merge. This collection should include the `meshTarget` object.</param>
        /// <param name="meshTarget">A mesh to use as the starting point for merging and which will be kept as a reference (target).</param>
        /// <returns>
        /// A list of merged meshes. In most cases this is a single mesh corresponding to `meshTarget`. However it can be multiple in cases
        /// where the resulting vertex count exceeds the maximum allowable value.
        /// </returns>
        public static List<ProBuilderMesh> Combine(IEnumerable<ProBuilderMesh> meshes, ProBuilderMesh meshTarget)
        {
            if (meshes == null)
                throw new ArgumentNullException("meshes");

            if (meshTarget == null)
                throw new ArgumentNullException("meshTarget");

            if (!meshes.Any() || meshes.Count() < 2 )
                return null;

            if (!meshes.Contains(meshTarget))
                return null;

            var vertices = new List<Vertex>(meshTarget.GetVertices());
            var faces = new List<Face>(meshTarget.facesInternal);
            var sharedVertices = new List<SharedVertex>(meshTarget.sharedVertices);
            var sharedTextures = new List<SharedVertex>(meshTarget.sharedTextures);
            int offset = meshTarget.vertexCount;
            var materialMap = new List<Material>(meshTarget.renderer.sharedMaterials);
            var targetTransform = meshTarget.transform;

            var firstMeshContributors = new List<ProBuilderMesh>();
            var remainderMeshContributors = new List<ProBuilderMesh>();

            var currentMeshVertexCount = offset;
            foreach (var mesh in meshes)
            {
                if (mesh != meshTarget)
                {
                    if (currentMeshVertexCount + mesh.vertexCount < ProBuilderMesh.maxVertexCount)
                    {
                        currentMeshVertexCount += mesh.vertexCount;
                        firstMeshContributors.Add(mesh);
                    }
                    else
                    {
                        remainderMeshContributors.Add(mesh);
                    }
                }
            }

            var autoUvFaces = new List<Face>();
            AccumulateMeshesInfo(
                firstMeshContributors,
                offset,
                ref vertices,
                ref faces,
                ref autoUvFaces,
                ref sharedVertices,
                ref sharedTextures,
                ref materialMap,
                targetTransform
            );

            meshTarget.SetVertices(vertices);
            meshTarget.faces = faces;
            meshTarget.sharedVertices = sharedVertices;
            meshTarget.sharedTextures = sharedTextures != null ? sharedTextures.ToArray() : null;
            meshTarget.renderer.sharedMaterials = materialMap.ToArray();
            meshTarget.ToMesh();
            meshTarget.Refresh();
            UvUnwrapping.SetAutoAndAlignUnwrapParamsToUVs(meshTarget, autoUvFaces);

            MeshValidation.EnsureMeshIsValid(meshTarget, out int removedVertices);

            var returnedMesh = new List<ProBuilderMesh>() { meshTarget };
            if (remainderMeshContributors.Count > 1)
            {
                var newMeshes = CombineToNewMeshes(remainderMeshContributors);
                foreach (var mesh in newMeshes)
                {
                    MeshValidation.EnsureMeshIsValid(mesh, out removedVertices);
                    returnedMesh.Add(mesh);
                }
            }
            else if (remainderMeshContributors.Count == 1)
            {
                returnedMesh.Add(remainderMeshContributors[0]);
            }

            return returnedMesh;
        }

        static List<ProBuilderMesh> CombineToNewMeshes(IEnumerable<ProBuilderMesh> meshes)
        {
            if (meshes == null)
                throw new ArgumentNullException("meshes");

            if (!meshes.Any() || meshes.Count() < 2)
                return null;

            var vertices = new List<Vertex>();
            var faces = new List<Face>();
            var autoUvFaces = new List<Face>();
            var sharedVertices = new List<SharedVertex>();
            var sharedTextures = new List<SharedVertex>();
            int offset = 0;
            var materialMap = new List<Material>();

            AccumulateMeshesInfo(
               meshes,
               offset,
               ref vertices,
               ref faces,
               ref autoUvFaces,
               ref sharedVertices,
               ref sharedTextures,
               ref materialMap
           );

            var res = SplitByMaxVertexCount(vertices, faces, sharedVertices, sharedTextures);
            var pivot = meshes.LastOrDefault().transform.position;

            foreach (var m in res)
            {
                m.renderer.sharedMaterials = materialMap.ToArray();
                InternalMeshUtility.FilterUnusedSubmeshIndexes(m);
                m.SetPivot(pivot);
                UvUnwrapping.SetAutoAndAlignUnwrapParamsToUVs(m, autoUvFaces);
            }

            return res;
        }

        static void AccumulateMeshesInfo(
                IEnumerable<ProBuilderMesh> meshes,
                int offset,
                ref List<Vertex> vertices,
                ref List<Face> faces,
                ref List<Face> autoUvFaces,
                ref List<SharedVertex> sharedVertices,
                ref List<SharedVertex> sharedTextures,
                ref List<Material> materialMap,
                Transform targetTransform = null
            )
        {
            foreach (var mesh in meshes)
            {
                var meshVertexCount = mesh.vertexCount;
                var transform = mesh.transform;
                var meshVertices = mesh.GetVertices();
                var meshFaces = mesh.facesInternal;
                var meshSharedVertices = mesh.sharedVertices;
                var meshSharedTextures = mesh.sharedTextures;
                var materials = mesh.renderer.sharedMaterials;
                var materialCount = materials.Length;

                for (int i = 0; i < meshVertexCount; i++)
                {
                    var worldVertex = transform.TransformVertex(meshVertices[i]);
                    if (targetTransform != null)
                        vertices.Add(targetTransform.InverseTransformVertex(worldVertex));
                    else
                        vertices.Add(worldVertex);
                }

                foreach (var face in meshFaces)
                {
                    var newFace = new Face(face);
                    newFace.ShiftIndexes(offset);

                    // prevents uvs from shifting when being converted from local coords to world space
                    if (!newFace.manualUV && !newFace.uv.useWorldSpace)
                    {
                        newFace.manualUV = true;
                        autoUvFaces.Add(newFace);
                    }
                    var material = materialCount > 0 ? materials[Math.Clamp(face.submeshIndex, 0, materialCount - 1)] : null;
                    var submeshIndex = materialMap.IndexOf(material);

                    if (submeshIndex > -1)
                    {
                        newFace.submeshIndex = submeshIndex;
                    }
                    else
                    {
                        if (material == null)
                        {
                            newFace.submeshIndex = 0;
                        }
                        else
                        {
                            newFace.submeshIndex = materialMap.Count;
                            materialMap.Add(material);
                        }
                    }

                    faces.Add(newFace);
                }

                foreach (var sv in meshSharedVertices)
                {
                    var nsv = new SharedVertex(sv);
                    nsv.ShiftIndexes(offset);
                    sharedVertices.Add(nsv);
                }

                foreach (var st in meshSharedTextures)
                {
                    var nst = new SharedVertex(st);
                    nst.ShiftIndexes(offset);
                    sharedTextures.Add(nst);
                }

                offset += meshVertexCount;
            }
        }

        static ProBuilderMesh CreateMeshFromSplit(List<Vertex> vertices,
            List<Face> faces,
            Dictionary<int, int> sharedVertexLookup,
            Dictionary<int, int> sharedTextureLookup,
            Dictionary<int, int> remap,
            Material[] materials)
        {
            // finalize mesh
            var sv = new Dictionary<int, int>();
            var st = new Dictionary<int, int>();

            foreach (var f in faces)
            {
                for (int i = 0, c = f.indexesInternal.Length; i < c; i++)
                    f.indexesInternal[i] = remap[f.indexesInternal[i]];

                f.InvalidateCache();
            }

            foreach (var kvp in remap)
            {
                int v;

                if (sharedVertexLookup.TryGetValue(kvp.Key, out v))
                    sv.Add(kvp.Value, v);

                if (sharedTextureLookup.TryGetValue(kvp.Key, out v))
                    st.Add(kvp.Value, v);
            }

            return ProBuilderMesh.Create(
                vertices,
                faces,
                SharedVertex.ToSharedVertices(sv),
                st.Count > 0 ? SharedVertex.ToSharedVertices(st) : null,
                materials);
        }

        /// <summary>
        /// Break a ProBuilder mesh into multiple meshes if it's vertex count is greater than maxVertexCount.
        /// </summary>
        /// <returns></returns>
        internal static List<ProBuilderMesh> SplitByMaxVertexCount(IList<Vertex> vertices, IList<Face> faces, IList<SharedVertex> sharedVertices, IList<SharedVertex> sharedTextures, uint maxVertexCount = ProBuilderMesh.maxVertexCount)
        {
            uint vertexCount = (uint)vertices.Count;
            uint meshCount = System.Math.Max(1u, vertexCount / maxVertexCount);
            var submeshCount = faces.Max(x => x.submeshIndex) + 1;

            if (meshCount < 2)
                return new List<ProBuilderMesh>() { ProBuilderMesh.Create(vertices, faces, sharedVertices, sharedTextures, new Material[submeshCount]) };

            var sharedVertexLookup = new Dictionary<int, int>();
            SharedVertex.GetSharedVertexLookup(sharedVertices, sharedVertexLookup);

            var sharedTextureLookup = new Dictionary<int, int>();
            SharedVertex.GetSharedVertexLookup(sharedTextures, sharedTextureLookup);

            var meshes = new List<ProBuilderMesh>();
            var mv = new List<Vertex>();
            var mf = new List<Face>();
            var remap = new Dictionary<int, int>();

            foreach (var face in faces)
            {
                if (mv.Count + face.distinctIndexes.Count > maxVertexCount)
                {
                    // finalize mesh
                    meshes.Add(CreateMeshFromSplit(mv, mf, sharedVertexLookup, sharedTextureLookup, remap, new Material[submeshCount]));
                    mv.Clear();
                    mf.Clear();
                    remap.Clear();
                }

                foreach (int i in face.distinctIndexes)
                {
                    mv.Add(vertices[i]);
                    remap.Add(i, mv.Count - 1);
                }

                mf.Add(face);
            }

            if (mv.Count > 0)
                meshes.Add(CreateMeshFromSplit(mv, mf, sharedVertexLookup, sharedTextureLookup, remap, new Material[submeshCount]));

            return meshes;
        }
	}
}
