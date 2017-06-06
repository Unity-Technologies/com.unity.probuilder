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
		// Coordinate system to use when exporting. Unity is left handed where most other
		// applications are right handed.
		public enum Handedness
		{
			Left,
			Right
		};

		// Convert from left to right handed coordinates on export?
		public Handedness handedness = Handedness.Right;

		// If absoluteTexturePath is false then the material textures will be copied to the export path.
		// If true the material library will point to the existing texture path in the Unity project.
		public bool absoluteTexturePath = true;
	}

	/**
	 *	Utilities for writing mesh data to the Wavefront OBJ format.
	 *
	 */
	public static class pb_Obj
	{
		/**
		 * Write the contents of a single obj & mtl from a set of models.
		 */
		public static bool Export(string name, IEnumerable<pb_Model> models, out string objContents, out string mtlContents, pb_ObjOptions options = null)
		{
			Dictionary<Material, string> materialMap = null;

			if(options == null)
				options = new pb_ObjOptions();

			mtlContents = WriteMtlContents(models, options, out materialMap);
			objContents = WriteObjContents(name, models, materialMap, options);

			return true;
		}

		private static string WriteObjContents(string name, IEnumerable<pb_Model> models, Dictionary<Material, string> materialMap, pb_ObjOptions options)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("# Exported from ProBuilder");
			sb.AppendLine(string.Format("mtllib ./{0}.mtl", name));
			sb.AppendLine(string.Format("o {0}", name));
			sb.AppendLine();

			int triangleOffset = 1;

			bool reverseWinding = options.handedness == pb_ObjOptions.Handedness.Right;
			float handedness = options.handedness == pb_ObjOptions.Handedness.Left ? 1f : -1f;

			foreach(pb_Model model in models)
			{
				Mesh mesh = model.mesh;
				Material[] materials = model.materials;
				Matrix4x4 matrix = model.matrix;

				int vertexCount = mesh.vertexCount;

				Vector3[] positions = mesh.vertices;
				Vector3[] normals = mesh.normals;
				Vector2[] textures0 = mesh.uv;

				for(int i = 0; i < vertexCount; i++)
				{
					if(positions != null)
					{
						positions[i] = matrix.MultiplyPoint3x4(positions[i]);
						positions[i].x *= handedness;
					}

					if(normals != null)
					{
						normals[i] = matrix.MultiplyVector(normals[i]);
						normals[i].x *= handedness;
					}
				}

				int materialCount = materials != null ? materials.Length : 0;

				sb.AppendLine(string.Format("g {0}", model.name));

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
					sb.AppendLine(string.Format("g {0}_{1}", mesh.name, submeshIndex));

					if(submeshIndex < materialCount)
						sb.AppendLine(string.Format("usemtl {0}", materialMap[materials[submeshIndex]]));

					int[] triangles = mesh.GetTriangles(submeshIndex);

					for(int i = 0; i < triangles.Length; i += 3)
					{
						if(reverseWinding)
						{
							sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
								triangles[i+2] + triangleOffset,
								triangles[i+1] + triangleOffset,
								triangles[i+0] + triangleOffset));
						}
						else
						{
							sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
								triangles[i+0] + triangleOffset,
								triangles[i+1] + triangleOffset,
								triangles[i+2] + triangleOffset));
						}
					}

					sb.AppendLine();
				}

				triangleOffset += vertexCount;
			}

			return sb.ToString();
		}

		/**
		 * Write the material file for an OBJ. This function handles making the list of Materials unique & ensuring
		 * unique names for each group. Material to named mtl group are stored in materialMap.
		 */
		private static string WriteMtlContents(IEnumerable<pb_Model> models, pb_ObjOptions options, out Dictionary<Material, string> materialMap)
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
							name = string.Format("{0}_{1}", material.name, nameIncrement++);

						materialMap.Add(material, name);
					}
				}
			}

			StringBuilder sb = new StringBuilder();

			foreach(KeyValuePair<Material, string> group in materialMap)
			{
				string path = AssetDatabase.GetAssetPath(group.Key.mainTexture);
				// remove "Assets/" from start of path
				path = path.Substring(7, path.Length - 7);
				string textureName = options.absoluteTexturePath ? string.Format("{0}/{1}", Application.dataPath, path) : Path.GetFileName(path);

				sb.AppendLine(string.Format("newmtl {0}", group.Value));
				sb.AppendLine(string.Format("map_Kd {0}", textureName));
				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
