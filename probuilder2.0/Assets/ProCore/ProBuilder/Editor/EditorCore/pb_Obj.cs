using UnityEngine;
using UnityEditor;
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

		// If copyTextures is true then the material textures will be copied to the export path.
		// If false the material library will point to the existing texture path in the Unity project.
		public bool copyTextures = true;

		// Should meshes be exported in local (false) or world (true) space.
		public bool applyTransforms = true;

		// Some modeling programs support reading vertex colors as an additional {r, g, b} following 
		// the vertex position {x, y, z}
		public bool vertexColors = false;

		// Write the texture map offset and scale. Not supported by all importers.
		public bool textureOffsetScale = false;
	}

	/**
	 *	Utilities for writing mesh data to the Wavefront OBJ format.
	 *
	 */
	public static class pb_Obj
	{
		/**
	     * Standard shader defines:
		 * Albedo       | map_Kd           | _MainTex
		 * Metallic     | Pm/map_Pm*       | _MetallicGlossMap
		 * Normal       | map_bump / bump  | _BumpMap
		 * Height       | disp             | _ParallaxMap
		 * Occlusion    |                  | _OcclusionMap
		 * Emission     | Ke/map_Ke*       | _EmissionMap
		 * DetailMask   | map_d            | _DetailMask
		 * DetailAlbedo |                  | _DetailAlbedoMap
		 * DetailNormal |                  | _DetailNormalMap
		 *
		 * *http://exocortex.com/blog/extending_wavefront_mtl_to_support_pbr
		 */
		static Dictionary<string, string> m_TextureMapKeys = new Dictionary<string, string>
		{
			{ "_MainTex", "map_Kd" },
			{ "_MetallicGlossMap", "map_Pm" },
			{ "_BumpMap", "bump" },
			{ "_ParallaxMap", "disp" },
			{ "_EmissionMap", "map_Ke" },
			{ "_DetailMask", "map_d" },
			// Alternative naming conventions - possibly useful if someone 
			// runs into an issue with another 3d modeling app.
			// { "_MetallicGlossMap", "Pm" },
			// { "_BumpMap", "map_bump" },
			// { "_BumpMap", "norm" },
			// { "_EmissionMap", "Ke" },
		};

		/**
		 * Write the contents of a single obj & mtl from a set of models.
		 */
		public static bool Export(string name, IEnumerable<pb_Model> models, out string objContents, out string mtlContents, out List<string> textures, pb_ObjOptions options = null)
		{
			if(models == null || models.Count() < 1)
			{
				objContents = null;
				mtlContents = null;
				textures = null;
				return false;
			}

			Dictionary<Material, string> materialMap = null;

			if(options == null)
				options = new pb_ObjOptions();

			mtlContents = WriteMtlContents(models, options, out materialMap, out textures);
			objContents = WriteObjContents(name, models, materialMap, options);

			return true;
		}

		private static string WriteObjContents(string name, IEnumerable<pb_Model> models, Dictionary<Material, string> materialMap, pb_ObjOptions options)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("# Exported from ProBuilder");
			sb.AppendLine("# http://www.procore3d.com/probuilder");
			sb.AppendLine(string.Format("# {0}", System.DateTime.Now));
			sb.AppendLine();
			sb.AppendLine(string.Format("mtllib ./{0}.mtl", name.Replace(" ", "_")));
			sb.AppendLine(string.Format("o {0}", name));
			sb.AppendLine();

			int triangleOffset = 1;

			bool reverseWinding = options.handedness == pb_ObjOptions.Handedness.Right;
			float handedness = options.handedness == pb_ObjOptions.Handedness.Left ? 1f : -1f;

			foreach(pb_Model model in models)
			{
				Mesh mesh = model.mesh;
				Material[] materials = model.materials;
				Matrix4x4 matrix = options.applyTransforms ? model.matrix : Matrix4x4.identity;

				int vertexCount = mesh.vertexCount;

				Vector3[] positions = mesh.vertices;
				Vector3[] normals = mesh.normals;
				Vector2[] textures0 = mesh.uv;
				Color[] colors = options.vertexColors ? mesh.colors : null;

				// Can skip this entirely if handedness matches Unity & not applying transforms.
				if(options.handedness != pb_ObjOptions.Handedness.Left || options.applyTransforms)
				{
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
				}

				int materialCount = materials != null ? materials.Length : 0;

				sb.AppendLine(string.Format("g {0}", model.name));

				if(options.vertexColors && colors != null && colors.Length == vertexCount)
				{
					for(int i = 0; i < vertexCount; i++)
						sb.AppendLine(string.Format("v {0} {1} {2} {3} {4} {5}",
							positions[i].x, positions[i].y, positions[i].z,
							colors[i].r, colors[i].g, colors[i].b ));
				}
				else
				{		
					for(int i = 0; i < vertexCount; i++)
						sb.AppendLine(string.Format("v {0} {1} {2}", positions[i].x, positions[i].y, positions[i].z));
				}

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
					if(mesh.subMeshCount > 1)
						sb.AppendLine(string.Format("g {0}_{1}", mesh.name, submeshIndex));
					else
						sb.AppendLine(string.Format("g {0}", mesh.name));

					if(submeshIndex < materialCount)
					{
						if(materials[submeshIndex] != null)
							sb.AppendLine(string.Format("usemtl {0}", materialMap[materials[submeshIndex]]));
					}

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
		private static string WriteMtlContents(IEnumerable<pb_Model> models, pb_ObjOptions options, out Dictionary<Material, string> materialMap, out List<string> textures)
		{
			materialMap = new Dictionary<Material, string>();

			foreach(pb_Model model in models)
			{
				foreach(Material material in model.materials)
				{
					if(material == null)
						continue;

					if(!materialMap.ContainsKey(material))
					{
						string escapedName = material.name.Replace(" ", "_");
						string name = escapedName;
						int nameIncrement = 1;

						while(materialMap.Any(x => x.Value.Equals(name)))
							name = string.Format("{0}_{1}", escapedName, nameIncrement++);

						materialMap.Add(material, name);
					}
				}
			}

			StringBuilder sb = new StringBuilder();
			textures = new List<string>();

			foreach(KeyValuePair<Material, string> group in materialMap)
			{
				Material mat = group.Key;

				sb.AppendLine(string.Format("newmtl {0}", group.Value));

				// Texture maps
				if(mat.shader != null)
				{
					for(int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
					{
						if( ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv ||
							ShaderUtil.GetTexDim(mat.shader, i) != UnityEngine.Rendering.TextureDimension.Tex2D )
							continue;

						string texPropertyName = ShaderUtil.GetPropertyName(mat.shader, i);

						Texture texture = mat.GetTexture(texPropertyName);

						string path = texture != null ? AssetDatabase.GetAssetPath(texture) : null;

						if(!string.IsNullOrEmpty(path))
						{
							if(options.copyTextures)
								textures.Add(path);

							// remove "Assets/" from start of path
							path = path.Substring(7, path.Length - 7);
						
							string textureName = options.copyTextures ? Path.GetFileName(path) : string.Format("{0}/{1}", Application.dataPath, path);

							string mtlKey = null;

							if(m_TextureMapKeys.TryGetValue(texPropertyName, out mtlKey))
							{
								Vector2 offset = mat.GetTextureOffset(texPropertyName);
								Vector2 scale  = mat.GetTextureScale(texPropertyName);

								if(options.textureOffsetScale)
									sb.AppendLine(string.Format("{0} -o {1} {2} -s {3} {4} {5}", mtlKey, offset.x, offset.y, scale.x, scale.y, textureName));
								else
									sb.AppendLine(string.Format("{0} {1}", mtlKey, textureName));
							}
						}
					}
				}

				if(mat.HasProperty("_Color"))
				{
					Color color = mat.color;

					// Diffuse
					sb.AppendLine(string.Format("Kd {0}", string.Format("{0} {1} {2}", color.r, color.g, color.b)));
					// Transparency
					sb.AppendLine(string.Format("d {0}", color.a));
				}
				else
				{
					sb.AppendLine("Kd 1.0 1.0 1.0");
					sb.AppendLine("d 1.0");
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
