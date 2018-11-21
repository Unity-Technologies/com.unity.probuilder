using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// A set of options used when exporting OBJ models.
    /// </summary>
    sealed class ObjOptions
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

    /// <summary>
    /// Utilities for writing mesh data to the Wavefront OBJ format.
    /// </summary>
    static class ObjExporter
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
        static Dictionary<string, string> s_TextureMapKeys = new Dictionary<string, string>
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

        /// <summary>
        /// Write the contents of a single obj & mtl from a set of models.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="models"></param>
        /// <param name="objContents"></param>
        /// <param name="mtlContents"></param>
        /// <param name="textures"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Export(string name, IEnumerable<Model> models, out string objContents, out string mtlContents, out List<string> textures, ObjOptions options = null)
        {
            if (models == null || models.Count() < 1)
            {
                objContents = null;
                mtlContents = null;
                textures = null;
                return false;
            }

            Dictionary<Material, string> materialMap = null;

            if (options == null)
                options = new ObjOptions();

            mtlContents = WriteMtlContents(models, options, out materialMap, out textures);
            objContents = WriteObjContents(name, models, materialMap, options);

            return true;
        }

        static string WriteObjContents(string name, IEnumerable<Model> models, Dictionary<Material, string> materialMap, ObjOptions options)
        {
            // Empty names in OBJ groups can crash some 3d programs (meshlab)
            if (string.IsNullOrEmpty(name))
                name = "ProBuilderModel";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("# Exported from ProBuilder");
            sb.AppendLine("# http://www.procore3d.com/probuilder");
            sb.AppendLine(string.Format("# {0}", System.DateTime.Now));
            sb.AppendLine();
            sb.AppendLine(string.Format("mtllib ./{0}.mtl", name.Replace(" ", "_")));
            sb.AppendLine(string.Format("o {0}", name));
            sb.AppendLine();

            int triangleOffset = 1;

            bool reverseWinding = options.handedness == ObjOptions.Handedness.Right;
            float handedness = options.handedness == ObjOptions.Handedness.Left ? 1f : -1f;

            foreach (Model model in models)
            {
                int subMeshCount = model.submeshCount;
                Matrix4x4 matrix = options.applyTransforms ? model.matrix : Matrix4x4.identity;

                int vertexCount = model.vertexCount;

                Vector3[] positions;
                Color[] colors;
                Vector2[] textures0;
                Vector3[] normals;
                Vector4[] tangent;
                Vector2[] uv2;
                List<Vector4> uv3;
                List<Vector4> uv4;

                MeshArrays attribs = MeshArrays.Position | MeshArrays.Normal | MeshArrays.Texture0;
                if (options.vertexColors)
                    attribs = attribs | MeshArrays.Color;
                Vertex.GetArrays(model.vertices, out positions, out colors, out textures0, out normals, out tangent, out uv2, out uv3, out uv4, attribs);

                // Can skip this entirely if handedness matches Unity & not applying transforms.
                // matrix is set to identity if not applying transforms.
                if (options.handedness != ObjOptions.Handedness.Left || options.applyTransforms)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        if (positions != null)
                        {
                            positions[i] = matrix.MultiplyPoint3x4(positions[i]);
                            positions[i].x *= handedness;
                        }

                        if (normals != null)
                        {
                            normals[i] = matrix.MultiplyVector(normals[i]);
                            normals[i].x *= handedness;
                        }
                    }
                }

                sb.AppendLine(string.Format("g {0}", model.name));

                if (options.vertexColors && colors != null && colors.Length == vertexCount)
                {
                    for (int i = 0; i < vertexCount; i++)
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2} {3} {4} {5}",
                                positions[i].x, positions[i].y, positions[i].z,
                                colors[i].r, colors[i].g, colors[i].b));
                }
                else
                {
                    for (int i = 0; i < vertexCount; i++)
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}", positions[i].x, positions[i].y, positions[i].z));
                }

                sb.AppendLine();

                for (int i = 0; normals != null && i < vertexCount; i++)
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "vn {0} {1} {2}", normals[i].x, normals[i].y, normals[i].z));

                sb.AppendLine();

                for (int i = 0; textures0 != null && i < vertexCount; i++)
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "vt {0} {1}", textures0[i].x, textures0[i].y));

                sb.AppendLine();

                // Material assignment
                for (int submeshIndex = 0; submeshIndex < subMeshCount; submeshIndex++)
                {
                    Submesh submesh = model.submeshes[submeshIndex];

                    if (subMeshCount > 1)
                        sb.AppendLine(string.Format("g {0}_{1}", model.name, submeshIndex));
                    else
                        sb.AppendLine(string.Format("g {0}", model.name));

                    string materialName = "";

                    if (materialMap.TryGetValue(model.materials[submeshIndex], out materialName))
                        sb.AppendLine(string.Format("usemtl {0}", materialName));
                    else
                        sb.AppendLine(string.Format("usemtl {0}", "null"));

                    int[] indexes = submesh.m_Indexes;
                    int inc = submesh.m_Topology == MeshTopology.Quads ? 4 : 3;

                    for (int ff = 0; ff < indexes.Length; ff += inc)
                    {
                        if (inc == 4)
                        {
                            if (reverseWinding)
                            {
                                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2} {3}/{3}/{3}",
                                        indexes[ff + 3] + triangleOffset,
                                        indexes[ff + 2] + triangleOffset,
                                        indexes[ff + 1] + triangleOffset,
                                        indexes[ff + 0] + triangleOffset));
                            }
                            else
                            {
                                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2} {3}/{3}/{3}",
                                        indexes[ff + 0] + triangleOffset,
                                        indexes[ff + 1] + triangleOffset,
                                        indexes[ff + 2] + triangleOffset,
                                        indexes[ff + 3] + triangleOffset));
                            }
                        }
                        else
                        {
                            if (reverseWinding)
                            {
                                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                                        indexes[ff + 2] + triangleOffset,
                                        indexes[ff + 1] + triangleOffset,
                                        indexes[ff + 0] + triangleOffset));
                            }
                            else
                            {
                                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                                        indexes[ff + 0] + triangleOffset,
                                        indexes[ff + 1] + triangleOffset,
                                        indexes[ff + 2] + triangleOffset));
                            }
                        }
                    }

                    sb.AppendLine();
                }

                triangleOffset += vertexCount;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Write the material file for an OBJ. This function handles making the list of Materials unique & ensuring unique names for each group. Material to named mtl group are stored in materialMap.
        /// </summary>
        /// <param name="models"></param>
        /// <param name="options"></param>
        /// <param name="materialMap"></param>
        /// <param name="textures"></param>
        /// <returns></returns>
        static string WriteMtlContents(IEnumerable<Model> models, ObjOptions options, out Dictionary<Material, string> materialMap, out List<string> textures)
        {
            materialMap = new Dictionary<Material, string>();

            foreach (Model model in models)
            {
                for (int i = 0, c = model.submeshCount; i < c; i++)
                {
                    Material material = model.materials[i];

                    if (material == null)
                        continue;

                    if (!materialMap.ContainsKey(material))
                    {
                        string escapedName = material.name.Replace(" ", "_");
                        string name = escapedName;
                        int nameIncrement = 1;

                        while (materialMap.Any(x => x.Value.Equals(name)))
                            name = string.Format("{0}_{1}", escapedName, nameIncrement++);

                        materialMap.Add(material, name);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            textures = new List<string>();

            foreach (KeyValuePair<Material, string> group in materialMap)
            {
                Material mat = group.Key;

                sb.AppendLine(string.Format("newmtl {0}", group.Value));

                // Texture maps
                if (mat.shader != null)
                {
                    for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
                    {
                        if (ShaderUtil.GetPropertyType(mat.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                            continue;

                        string texPropertyName = ShaderUtil.GetPropertyName(mat.shader, i);

                        Texture texture = mat.GetTexture(texPropertyName);

                        string path = texture != null ? AssetDatabase.GetAssetPath(texture) : null;

                        if (!string.IsNullOrEmpty(path))
                        {
                            if (options.copyTextures)
                                textures.Add(path);

                            // remove "Assets/" from start of path
                            path = path.Substring(7, path.Length - 7);

                            string textureName = options.copyTextures ? Path.GetFileName(path) : string.Format("{0}/{1}", Application.dataPath, path);

                            string mtlKey = null;

                            if (s_TextureMapKeys.TryGetValue(texPropertyName, out mtlKey))
                            {
                                Vector2 offset = mat.GetTextureOffset(texPropertyName);
                                Vector2 scale  = mat.GetTextureScale(texPropertyName);

                                if (options.textureOffsetScale)
                                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} -o {1} {2} -s {3} {4} {5}", mtlKey, offset.x, offset.y, scale.x, scale.y, textureName));
                                else
                                    sb.AppendLine(string.Format("{0} {1}", mtlKey, textureName));
                            }
                        }
                    }
                }

                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;

                    // Diffuse
                    sb.AppendLine(string.Format("Kd {0}", string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", color.r, color.g, color.b)));
                    // Transparency
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "d {0}", color.a));
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
