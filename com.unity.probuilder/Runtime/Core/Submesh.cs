using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A set of indexes and material.
    /// </summary>
    [Serializable]
    public sealed class Submesh
    {
        [SerializeField]
        internal int[] m_Indexes;

        [SerializeField]
        internal MeshTopology m_Topology;

        [SerializeField]
        internal int m_SubmeshIndex;

        /// <value>
        /// Indexes making up this submesh. Can be triangles or quads, check with topology.
        /// </value>
        public IEnumerable<int> indexes
        {
            get { return new ReadOnlyCollection<int>(m_Indexes); }
            set { m_Indexes = value.ToArray(); }
        }

        /// <value>
        /// What is the topology (triangles, quads) of this submesh?
        /// </value>
        public MeshTopology topology
        {
            get { return m_Topology; }
            set { m_Topology = value; }
        }

        /// <value>
        /// The index in the sharedMaterials array that this submesh aligns with.
        /// </value>
        public int submeshIndex
        {
            get { return m_SubmeshIndex; }
            set { m_SubmeshIndex = value; }
        }

        /// <summary>
        /// Create new Submesh.
        /// </summary>
        /// <param name="submeshIndex">The index of this submesh corresponding to the MeshRenderer.sharedMaterials property.</param>
        /// <param name="topology">What topology is this submesh. ProBuilder only recognizes Triangles and Quads.</param>
        /// <param name="indexes">The triangles or quads.</param>
        public Submesh(int submeshIndex, MeshTopology topology, IEnumerable<int> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes");

            m_Indexes = indexes.ToArray();
            m_Topology = topology;
            m_SubmeshIndex = submeshIndex;
        }

        /// <summary>
        /// Create new Submesh from a mesh, submesh index, and material.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="subMeshIndex">Which submesh to read from.</param>
        public Submesh(Mesh mesh, int subMeshIndex)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            m_Indexes = mesh.GetIndices(subMeshIndex);
            m_Topology = mesh.GetTopology(subMeshIndex);
            m_SubmeshIndex = subMeshIndex;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", m_SubmeshIndex, m_Topology.ToString(), m_Indexes != null ? m_Indexes.Length.ToString() : "0");
        }

        internal static int GetSubmeshCount(ProBuilderMesh mesh)
        {
            int count = 0;
            foreach (var face in mesh.facesInternal)
                count = Math.Max(count, face.submeshIndex);
            return count + 1;
        }

        /// <summary>
        /// Create submeshes from a set of faces. Currently only Quads and Triangles are supported.
        /// </summary>
        /// <param name="faces">The faces to be included in the resulting submeshes. This method handles groups submeshes by comparing the material property of each face.</param>
        /// <param name="submeshCount">How many submeshes to create. Usually you will just want to pass the length of the MeshRenderer.sharedMaterials array.</param>
        /// <param name="preferredTopology">Should the resulting submeshes be in quads or triangles. Note that quads are not guaranteed; ie, some faces may not be able to be represented in quad format and will fall back on triangles.</param>
        /// <returns>An array of Submeshes.</returns>
        /// <exception cref="NotImplementedException">Thrown in the event that a MeshTopology other than Quads or Triangles is passed.</exception>
        public static Submesh[] GetSubmeshes(IEnumerable<Face> faces, int submeshCount, MeshTopology preferredTopology = MeshTopology.Triangles)
        {
            if (preferredTopology != MeshTopology.Triangles && preferredTopology != MeshTopology.Quads)
                throw new System.NotImplementedException("Currently only Quads and Triangles are supported.");

            if (faces == null)
                throw new ArgumentNullException("faces");

            bool wantsQuads = preferredTopology == MeshTopology.Quads;

            List<int>[] quads = wantsQuads ? new List<int>[submeshCount] : null;
            List<int>[] tris = new List<int>[submeshCount];
            int maxSubmeshIndex = submeshCount - 1;

            for (int i = 0; i < submeshCount; i++)
            {
                if (wantsQuads)
                    quads[i] = new List<int>();

                tris[i] = new List<int>();
            }

            foreach (var face in faces)
            {
                if (face.indexesInternal == null || face.indexesInternal.Length < 1)
                    continue;

                int submeshIndex = Math.Clamp(face.submeshIndex, 0, maxSubmeshIndex);

                if (wantsQuads && face.IsQuad())
                    quads[submeshIndex].AddRange(face.ToQuad());
                else
                    tris[submeshIndex].AddRange(face.indexesInternal);
            }

            var submeshes = new Submesh[submeshCount];

            switch (preferredTopology)
            {
                case MeshTopology.Triangles:
                {
                    for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                        submeshes[submeshIndex] = new Submesh(submeshIndex, MeshTopology.Triangles, tris[submeshIndex]);
                    break;
                }

                case MeshTopology.Quads:
                {
                    for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                    {
                        // If a submesh is a mix of triangles and quads, fall back to triangles.
                        if (tris[submeshIndex].Count > 0)
                        {
                            var tri = tris[submeshIndex];
                            var quad = quads[submeshIndex];

                            int triCount = tri.Count;
                            int quadCount = quad.Count;

                            int[] triangles = new int[triCount + ((quadCount / 4) * 6)];

                            for (int i = 0; i < triCount; i++)
                                triangles[i] = tri[i];

                            for (int i = 0, n = triCount; i < quadCount; i += 4, n += 6)
                            {
                                triangles[n + 0] = quad[i + 0];
                                triangles[n + 1] = quad[i + 1];
                                triangles[n + 2] = quad[i + 2];

                                triangles[n + 3] = quad[i + 2];
                                triangles[n + 4] = quad[i + 3];
                                triangles[n + 5] = quad[i + 0];
                            }

                            submeshes[submeshIndex] = new Submesh(submeshIndex, MeshTopology.Triangles, triangles);
                        }
                        else
                        {
                            submeshes[submeshIndex] = new Submesh(submeshIndex, MeshTopology.Quads, quads[submeshIndex]);
                        }
                    }
                    break;
                }
            }

            return submeshes;
        }

        internal static void MapFaceMaterialsToSubmeshIndex(ProBuilderMesh mesh)
        {
            var materials = mesh.renderer.sharedMaterials;
            var submeshCount = materials.Length;

            foreach (var face in mesh.facesInternal)
            {
#pragma warning disable 618
                if (face.material == null)
                    continue;
                var index = Array.IndexOf(materials, face.material);
                face.submeshIndex = Math.Clamp(index, 0, submeshCount - 1);
                face.material = null;
#pragma warning restore 618
            }
        }
    }
}
