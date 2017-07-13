using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	A mesh, material and optional transform matrix combination.
	 */
	public class pb_Model
	{
		// The name of this model.
		public string name;
		// Vertex positions.
		public Vector3[] positions;
		// Vertex normals. May be null, or if not must match positions length.
		public Vector3[] normals;
		// Vertex textures (UV0). May be null, or if not must match positions length.
		public Vector2[] textures;
		// Vertex colors. May be null, or if not must match positions length.
		public Color[] colors;
		// Triangle data (submesh[face[indices[]]]).
		public int[][][] indices;
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
			this.positions = mesh.vertices;
			this.normals = mesh.normals;
			this.textures = mesh.uv;
			this.colors = mesh.colors;
			this.materials = materials;
			this.indices = new int[materials.Length][][];
			this.matrix = matrix;

			for(int subMeshIndex = 0; subMeshIndex < materials.Length; subMeshIndex++)
			{
				this.indices[subMeshIndex] = new int[1][];
				this.indices[subMeshIndex][0] = mesh.GetTriangles(subMeshIndex);
			}
		}

		public pb_Model(string name, pb_Object mesh)
		{
			mesh.ToMesh();			
			mesh.Refresh();			

			MeshRenderer mr = mesh.gameObject.GetComponent<MeshRenderer>();
			Material[] sharedMaterials = mr != null ? mr.sharedMaterials : new Material[0] {};
			int subMeshCount = sharedMaterials.Length;

			this.name = name;
			this.positions = mesh.vertices.Select(x => x).ToArray();
			this.normals = mesh.msh.normals;
			this.textures = mesh.uv == null ? null : mesh.uv.Select(x => x).ToArray();
			this.colors = mesh.colors == null ? null : mesh.colors.Select(x => x).ToArray();
			this.matrix = mesh.transform.localToWorldMatrix;	
			this.materials = sharedMaterials;
			this.indices = new int[subMeshCount][][];

			for(int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
			{
				pb_Face[] faces = mesh.faces.Where(x => x.material == sharedMaterials[subMeshIndex]).ToArray();

				int[][] indices = new int[faces.Length][];
				for(int ff = 0; ff < faces.Length; ff++)
					faces[ff].ToQuadOrTriangles(out indices[ff]);

				this.indices[subMeshIndex] = indices;
			}

			// catch null-material faces
			pb_Face[] facesWithNoMaterial = mesh.faces.Where(x => x.material == null).ToArray();

			if(facesWithNoMaterial != null && facesWithNoMaterial.Length > 0)
			{
				pbUtil.Add<Material>(this.materials, pb_Constant.DefaultMaterial);

				int[][] indices = new int[facesWithNoMaterial.Length][];

				for(int ff = 0; ff < facesWithNoMaterial.Length; ff++)
					facesWithNoMaterial[ff].ToQuadOrTriangles(out indices[ff]);

				pbUtil.Add<int[][]>(this.indices, indices);
			}

			mesh.Optimize();
		}

		/**
		 *	Vertex count for the mesh (corresponds to positions length).
		 */
		public int vertexCount
		{
			get
			{
				return positions == null ? 0 : positions.Length;
			}
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
				Material[] materials = model.materials;
				int subMeshCount = materials == null ? 1 : materials.Length;
				Matrix4x4 matrix = options.applyTransforms ? model.matrix : Matrix4x4.identity;

				int vertexCount = model.vertexCount;

				Vector3[] positions = model.positions;
				Vector3[] normals = model.normals;
				Vector2[] textures0 = model.textures;
				Color[] colors = options.vertexColors ? model.colors : null;

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
				for(int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
				{
					if(subMeshCount > 1)
						sb.AppendLine(string.Format("g {0}_{1}", model.name, subMeshIndex));
					else
						sb.AppendLine(string.Format("g {0}", model.name));

					if(subMeshIndex < materialCount)
					{
						if(materials[subMeshIndex] != null)
							sb.AppendLine(string.Format("usemtl {0}", materialMap[materials[subMeshIndex]]));
					}

					int[][] faces = model.indices[subMeshIndex];

					for(int ff = 0; ff < faces.Length; ff++)
					{
						int[] triangles = faces[ff];

						if(triangles.Length == 4)
						{
							if(reverseWinding)
							{
								sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2} {3}/{3}/{3}",
									triangles[3] + triangleOffset,
									triangles[2] + triangleOffset,
									triangles[1] + triangleOffset,
									triangles[0] + triangleOffset));
							}
							else
							{
								sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2} {3}/{3}/{3}",
									triangles[0] + triangleOffset,
									triangles[1] + triangleOffset,
									triangles[2] + triangleOffset,
									triangles[3] + triangleOffset));
							}
						}
						else
						{
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
#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_3)
						if( ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv ||
							ShaderUtil.GetTexDim(mat.shader, i) != UnityEngine.Rendering.TextureDimension.Tex2D )
#else
						if( ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv )
#endif
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
