using UnityEngine;
using ProBuilder2.Common;
using System.Collections.Generic;
using System.Text;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	A mesh, material and optional transform matrix combination.
	 */
	public class pb_Model
	{
		public Mesh mesh;
		public Material material;
		public Matrix4x4 matrix;

		public pb_Model()
		{}

		public pb_Model(Mesh mesh, Material material) : this(mesh, material, Matrix4x4.identity)
		{}

		public pb_Model(Mesh mesh, Material material, Matrix4x4 matrix)
		{
			this.mesh = mesh;
			this.material = material;
			this.matrix = matrix;
		}
	}

	/**
	 *	A set of options used when exporting OBJ models.
	 */
	public class pb_ObjOptions
	{

	}

	/**
	 *	Utilities for writing mesh data to the Wavefront OBJ format.
	 *
	 */
	public static class pb_Obj
	{
		/**
		 * Write the Obj and Mtl file contents to string.
		 */
		public static bool Export(IEnumerable<pb_Model> models, out string objContents, out string mtlContents, pb_ObjOptions options = null)
		{
			objContents = WriteObjContents(models, options);
			mtlContents = null;

			return true;
		}

		public static string WriteObjContents(IEnumerable<pb_Model> models, pb_ObjOptions options)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("# Exported from ProBuilder");

			sb.AppendLine();

			foreach(pb_Model model in models)
			{
				Mesh mesh = model.mesh;
				Material material = model.material;
				Matrix4x4 matrix = model.matrix;

				int vertexCount = mesh.vertexCount;

				Vector3[] positions = mesh.vertices;
				Vector3[] normals = mesh.normals;
				Vector2[] textures0 = mesh.uv;
				int[] triangles = mesh.triangles;

				sb.AppendLine(string.Format("g {0}", mesh.name));

				for(int i = 0; i < vertexCount; i++)
					sb.AppendLine(string.Format("v {0} {1} {2}", positions[i].x, positions[i].y, positions[i].z));

				sb.AppendLine();

				for(int i = 0; normals != null && i < vertexCount; i++)
					sb.AppendLine(string.Format("vn {0} {1} {2}", normals[i].x, normals[i].y, normals[i].z));

				sb.AppendLine();

				for(int i = 0; textures0 != null && i < vertexCount; i++)
					sb.AppendLine(string.Format("vt {0} {1}", textures0[i].x, textures0[i].y));

				sb.AppendLine();

				// Material assignment
				sb.AppendLine(string.Format("g {0}", mesh.name));
				sb.AppendLine(string.Format("usemtl {0}", material.name));

				for(int i = 0; i < triangles.Length; i += 3)
				{
					sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", triangles[i+0] + 1, triangles[i+1] + 1, triangles[i+2] + 1));
				}
			}

			return sb.ToString();
		}
	}
}
