using UnityEngine;
using System.Linq;
using System.Collections.Generic;
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

		// Vertices
		public Vertex[] vertices;

		// Submeshes
		public Submesh[] submeshes;

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
		public int subMeshCount { get { return submeshes.Length; } }

		public Model()
		{}

		public Model(string name, Mesh mesh, Material material) : this(name, mesh, new Material[] { material }, Matrix4x4.identity)
		{}

		public Model(string name, Mesh mesh, Material[] materials, Matrix4x4 matrix)
		{
			this.name = name;
			this.vertices = Vertex.GetVertices(mesh);
			this.matrix = matrix;
			this.submeshes = new Submesh[mesh.subMeshCount];
			int matCount = materials != null ? materials.Length : 0;
			for(int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
				submeshes[subMeshIndex] = new Submesh(mesh, subMeshIndex, matCount > 0 ? materials[subMeshIndex % matCount] : BuiltinMaterials.DefaultMaterial);
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
			vertices = Vertex.GetVertices(mesh);
			submeshes = Face.GetSubmeshes(mesh.facesInternal, quads ? MeshTopology.Quads : MeshTopology.Triangles);
			matrix = mesh.transform.localToWorldMatrix;
			mesh.ToMesh(MeshTopology.Triangles);
			mesh.Refresh();
			mesh.Optimize();
		}
	}
}
