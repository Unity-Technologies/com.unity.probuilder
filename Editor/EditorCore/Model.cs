using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// A mesh, material and optional transform matrix combination.
    /// </summary>
    sealed class Model
    {
        // The name of this model.
        public string name;

        // vertices
        public Vertex[] vertices;

        // Submeshes
        public Submesh[] submeshes;

        public Material[] materials;

        // Optional transform matrix.
        public Matrix4x4 matrix;

        /// <summary>
        /// Vertex count for the mesh (corresponds to vertices length).
        /// </summary>
        public int vertexCount
        {
            get
            {
                return vertices == null ? 0 : vertices.Length;
            }
        }

        /// <summary>
        /// Submesh count.
        /// </summary>
        public int submeshCount { get { return submeshes.Length; } }

        public Model()
        {}

        public Model(string name, Mesh mesh, Material material) : this(name, mesh, new Material[] { material }, Matrix4x4.identity)
        {}

        public Model(string name, Mesh mesh, Material[] materials, Matrix4x4 matrix)
        {
            this.name = name;
            this.vertices = mesh.GetVertices();
            this.matrix = matrix;
            this.submeshes = new Submesh[mesh.subMeshCount];
            this.materials = new Material[mesh.subMeshCount];
            int matCount = materials != null ? materials.Length : 0;

            for (int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
            {
                submeshes[submeshIndex] = new Submesh(mesh, submeshIndex);

                if (matCount < 1)
                    materials[submeshIndex] = BuiltinMaterials.defaultMaterial;
                else
                    this.materials[submeshIndex] = materials[Math.Clamp(submeshIndex, 0, matCount - 1)];
            }
        }

        /// <summary>
        /// Create a pb_Model from a pb_Object, optionally converting to quad topology.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mesh"></param>
        /// <param name="quads"></param>
        public Model(string name, ProBuilderMesh mesh, bool quads = true)
        {
            mesh.ToMesh(quads ? MeshTopology.Quads : MeshTopology.Triangles);
            mesh.Refresh(RefreshMask.UV | RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents);
            this.name = name;
            vertices = mesh.GetVertices();
            submeshes = Submesh.GetSubmeshes(mesh.facesInternal, MaterialUtility.GetMaterialCount(mesh.renderer), quads ? MeshTopology.Quads : MeshTopology.Triangles);
            materials = new Material[submeshCount];

            for(int i = 0; i < submeshCount; ++i)
            {
                materials[i] = mesh.renderer.sharedMaterials[Math.Clamp(i, 0, materials.Length - 1)];
                if(materials[i] == null)
                    materials[i] = BuiltinMaterials.defaultMaterial;
            }


            matrix = mesh.transform.localToWorldMatrix;
            mesh.ToMesh(MeshTopology.Triangles);
            mesh.Refresh();
            mesh.Optimize();
        }
    }
}
