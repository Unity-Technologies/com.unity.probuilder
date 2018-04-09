using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// A mesh, material and optional transform matrix combination.
	/// </summary>
	class pb_Model
	{
		// The name of this model.
		public string name;

		// Vertices
		public pb_Vertex[] vertices;

		// Submeshes
		public pb_Submesh[] submeshes;

		// Optional transform matrix.
		public Matrix4x4 matrix;

		/**
		 *	Vertex count for the mesh (corresponds to vertices length).
		 */
		public int vertexCount
		{
			get
			{
				return vertices == null ? 0 : vertices.Length;
			}
		}

		/**
		 * Submesh count.
		 */
		public int subMeshCount { get { return submeshes.Length; } }

		public pb_Model()
		{}

		public pb_Model(string name, Mesh mesh, Material material) : this(name, mesh, new Material[] { material }, Matrix4x4.identity)
		{}

		public pb_Model(string name, Mesh mesh, Material[] materials, Matrix4x4 matrix)
		{
			this.name = name;
			this.vertices = pb_Vertex.GetVertices(mesh);
			this.matrix = matrix;
			this.submeshes = new pb_Submesh[mesh.subMeshCount];
			int matCount = materials != null ? materials.Length : 0;
			for(int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
				submeshes[subMeshIndex] = new pb_Submesh(mesh, subMeshIndex, matCount > 0 ? materials[subMeshIndex % matCount] : pb_Material.DefaultMaterial);
		}

		/**
		 * Create a pb_Model from a pb_Object, optionally converting to quad topology.
		 */
		public pb_Model(string name, pb_Object mesh, bool quads = true)
		{
			mesh.ToMesh(quads ? MeshTopology.Quads : MeshTopology.Triangles);
			mesh.Refresh(RefreshMask.UV | RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents);
			this.name = name;
			this.vertices = pb_Vertex.GetVertices(mesh);
			pb_Face.GetMeshIndices(mesh.faces, out this.submeshes, quads ? MeshTopology.Quads : MeshTopology.Triangles);
			this.matrix = mesh.transform.localToWorldMatrix;
			mesh.ToMesh(MeshTopology.Triangles);
			mesh.Refresh();
			mesh.Optimize();
		}
	}
}
