using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations
{
    /// <summary>
    /// Methods for merging multiple <see cref="ProBuilderMesh"/> objects to a single mesh.
    /// </summary>
	public static class CombineMeshes
	{
        /// <summary>
        /// Merge a collection of <see cref="ProBuilderMesh"/> objects to as few meshes as possible. This may result in
        /// more than one mesh due to a max vertex count limit of 65535.
        /// </summary>
        /// <param name="meshes">A collection of meshes to be merged.</param>
        /// <returns>
        /// A list of merged meshes. In most cases this will be a single mesh. However it can be multiple in cases
        /// where the resulting vertex count exceeds the maximum allowable value.
        /// </returns>
        public static List<ProBuilderMesh> Combine(IEnumerable<ProBuilderMesh> meshes)
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
                    vertices.Add(transform.TransformVertex(meshVertices[i]));

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
                    var material = materials[Math.Clamp(face.submeshIndex, 0, materialCount - 1)];
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

            var res = SplitByMaxVertexCount(vertices, faces, sharedVertices, sharedTextures);
            var pivot = meshes.LastOrDefault().transform.position;

            foreach (var m in res)
            {
                m.renderer.sharedMaterials = materialMap.ToArray();
                InternalMeshUtility.FilterUnusedSubmeshIndexes(m);
                m.SetPivot(pivot);
                UVEditing.SetAutoAndAlignUnwrapParamsToUVs(m, autoUvFaces);

            }

            return res;
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
                st.Any() ? SharedVertex.ToSharedVertices(st) : null,
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

            if (mv.Any())
                meshes.Add(CreateMeshFromSplit(mv, mf, sharedVertexLookup, sharedTextureLookup, remap, new Material[submeshCount]));

            return meshes;
        }
	}
}
