using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	A mesh, material and optional transform matrix combination.
	 */
	public class pb_Model
	{
		// The name of this model.
		public string name;
		// The geometry.
		public Mesh mesh;
		// Any materials referenced by the mesh. Should be equal in length to the submesh count.
		public Material[] materials;
		// Optional matrix to be applied to the mesh geometry before writing to Obj.
		public Matrix4x4 matrix;

		public pb_Model()
		{}

		public pb_Model(string name, Mesh mesh, Material material) : this(name, mesh, new Material[] { material }, Matrix4x4.identity)
		{}

		public pb_Model(string name, Mesh mesh, Material[] materials, Matrix4x4 matrix)
		{
			this.name = name;
			this.mesh = mesh;
			this.materials = materials;
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
			Dictionary<Material, string> materialMap = null;

			mtlContents = WriteMtlContents(models, out materialMap);
			objContents = WriteObjContents("test", models, options);

			return true;
		}

		public static string WriteObjContents(string name, IEnumerable<pb_Model> models, pb_ObjOptions options)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("# Exported from ProBuilder");

			sb.AppendLine();

			sb.AppendLine(string.Format("mtllib ./{0}.mtl", name));

			sb.AppendLine();

			foreach(pb_Model model in models)
			{
				Mesh mesh = model.mesh;
				Material[] materials = model.materials;
				Matrix4x4 matrix = model.matrix;

				int vertexCount = mesh.vertexCount;

				Vector3[] positions = mesh.vertices;
				Vector3[] normals = mesh.normals;
				Vector2[] textures0 = mesh.uv;
				int materialCount = materials != null ? materials.Length : 0;

				sb.AppendLine(string.Format("o {0}", model.name));

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
				for(int submeshIndex = 0; submeshIndex < mesh.subMeshCount; submeshIndex++)
				{
					sb.AppendLine(string.Format("g {0} {1}", mesh.name, submeshIndex));

					if(submeshIndex < materialCount)
						sb.AppendLine(string.Format("usemtl {0}", materials[submeshIndex].name));

					int[] triangles = mesh.GetTriangles(submeshIndex);

					for(int i = 0; i < triangles.Length; i += 3)
					{
						sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", triangles[i+0] + 1, triangles[i+1] + 1, triangles[i+2] + 1));
					}

					sb.AppendLine();
				}
			}

			return sb.ToString();
		}

		/**
		 * Write the material file for an OBJ. This function handles making the list of Materials unique & ensuring
		 * unique names for each group. Material to named mtl group are stored in materialMap.
		 */
		public static string WriteMtlContents(IEnumerable<pb_Model> models, out Dictionary<Material, string> materialMap)
		{
			materialMap = new Dictionary<Material, string>();

			foreach(pb_Model model in models)
			{
				foreach(Material material in model.materials)
				{
					if(!materialMap.ContainsKey(material))
					{
						string name = material.name;
						int nameIncrement = 1;

						while(materialMap.Any(x => x.Value.Equals(name)))
						{
							name = string.Format("{0} {1}", material.name, nameIncrement++);
						}

						materialMap.Add(material, name);
					}
				}
			}

			StringBuilder sb = new StringBuilder();

			foreach(KeyValuePair<Material, string> group in materialMap)
			{
				string path = AssetDatabase.GetAssetPath(group.Key.mainTexture);
				string textureName = Path.GetFileName(path);

				sb.AppendLine(string.Format("newmtl {0}", group.Value));
				sb.AppendLine(string.Format("map_Kd {0}", textureName));
				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
