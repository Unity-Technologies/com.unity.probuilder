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

            SemVer version;

            if(Version.TryGetPackageVersion(out version))
                sb.AppendLine("# ProBuilder " + version.MajorMinorPatch);
            else
                sb.AppendLine("# ProBuilder");
            sb.AppendLine("# https://unity3d.com/unity/features/worldbuilding/probuilder");
            sb.AppendLine(string.Format("# {0}", System.DateTime.Now));
            sb.AppendLine();
            sb.AppendLine(string.Format("mtllib ./{0}.mtl", name.Replace(" ", "_")));
            sb.AppendLine(string.Format("o {0}", name));
            sb.AppendLine();

            // obj orders indices 1 indexed
            int positionOffset = 1;
            int normalOffset = 1;
            int textureOffset = 1;

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

                Dictionary<int, int> positionIndexMap;
                var positionCount = AppendPositions(sb, positions, colors, true, options.vertexColors, out positionIndexMap);

                sb.AppendLine();

                Dictionary<int, int> textureIndexMap;
                var textureCount = AppendArrayVec2(sb, textures0, "vt", true, out textureIndexMap);

                sb.AppendLine();

                Dictionary<int, int> normalIndexMap;
                var normalCount = AppendArrayVec3(sb, normals, "vn", true, out normalIndexMap);

                sb.AppendLine();

                // Material assignment
                for (int submeshIndex = 0; submeshIndex < subMeshCount; submeshIndex++)
                {
                    Submesh submesh = model.submeshes[submeshIndex];

                    string materialName = "";

                    if (materialMap.TryGetValue(model.materials[submeshIndex], out materialName))
                        sb.AppendLine(string.Format("usemtl {0}", materialName));
                    else
                        sb.AppendLine(string.Format("usemtl {0}", "null"));

                    int[] indexes = submesh.m_Indexes;
                    int inc = submesh.m_Topology == MeshTopology.Quads ? 4 : 3;
                    int inc1 = inc - 1;

                    int o0 = reverseWinding ? inc1 : 0;
                    int o1 = reverseWinding ? inc1 - 1 : 1;
                    int o2 = reverseWinding ? inc1 - 2 : 2;
                    int o3 = reverseWinding ? inc1 - 3 : 3;

                    for (int ff = 0; ff < indexes.Length; ff += inc)
                    {
                        int p0 = positionIndexMap[indexes[ff + o0]] + positionOffset;
                        int p1 = positionIndexMap[indexes[ff + o1]] + positionOffset;
                        int p2 = positionIndexMap[indexes[ff + o2]] + positionOffset;

                        int t0 = textureIndexMap[indexes[ff + o0]] + textureOffset;
                        int t1 = textureIndexMap[indexes[ff + o1]] + textureOffset;
                        int t2 = textureIndexMap[indexes[ff + o2]] + textureOffset;

                        int n0 = normalIndexMap[indexes[ff + o0]] + normalOffset;
                        int n1 = normalIndexMap[indexes[ff + o1]] + normalOffset;
                        int n2 = normalIndexMap[indexes[ff + o2]] + normalOffset;

                        if (inc == 4)
                        {
                            int p3 = positionIndexMap[indexes[ff + o3]] + positionOffset;
                            int n3 = normalIndexMap[indexes[ff + o3]] + normalOffset;
                            int t3 = textureIndexMap[indexes[ff + o3]] + textureOffset;

                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                "f {0}/{4}/{8} {1}/{5}/{9} {2}/{6}/{10} {3}/{7}/{11}",
                                    p0, p1, p2, p3,
                                    t0, t1, t2, t3,
                                    n0, n1, n2, n3
                                    ));
                        }
                        else
                        {
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                "f {0}/{3}/{6} {1}/{4}/{7} {2}/{5}/{8}",
                                p0, p1, p2,
                                t0, t1, t2,
                                n0, n1, n2
                                ));

                        }
                    }

                    sb.AppendLine();
                }

                positionOffset += positionCount;
                normalOffset += normalCount;
                textureOffset += textureCount;
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

        struct PositionColorKey : System.IEquatable<PositionColorKey>
        {
            public IntVec3 position;
            public IntVec4 color;

            public PositionColorKey(Vector3 p, Color c)
            {
                position = new IntVec3(p);
                color = new IntVec4(c);
            }

            public bool Equals(PositionColorKey other)
            {
                return position.Equals(other.position) && color.Equals(other.color);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                return obj is PositionColorKey && Equals(obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (position.GetHashCode() * 397) ^ color.GetHashCode();
                }
            }
        }

        // AppendPositions separately from AppendArrayVec3 to support the non-spec color extension that some DCCs can read
        static int AppendPositions(StringBuilder sb, Vector3[] positions, Color[] colors, bool mergeCoincident, bool includeColors, out Dictionary<int, int> coincidentIndexMap)
        {
            var writeColors = includeColors && colors != null && colors.Length == positions.Length;

            Dictionary<PositionColorKey, int> common = new Dictionary<PositionColorKey, int>();
            coincidentIndexMap = new Dictionary<int, int>();

            int index = 0;

            for (int i = 0, c = positions.Length; i < c; i++)
            {
                var position = positions[i];
                var color = writeColors ? colors[i] : Color.white;

                var key = new PositionColorKey(position, color);
                int vertexIndex;

                if (mergeCoincident)
                {
                    if (!common.TryGetValue(key, out vertexIndex))
                    {
                        vertexIndex = index++;
                        common.Add(key, vertexIndex);
                    }
                    else
                    {
                        coincidentIndexMap.Add(i, vertexIndex);
                        continue;
                    }
                }
                else
                {
                    vertexIndex = index++;
                }

                coincidentIndexMap.Add(i, vertexIndex);

                if (writeColors)
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2} {3} {4} {5}",
                        position.x,
                        position.y,
                        position.z,
                        color.r,
                        color.g,
                        color.b));
                }
                else
                {
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}",
                        position.x,
                        position.y,
                        position.z));
                }
            }

            return index;
        }

        static int AppendArrayVec2(StringBuilder sb, Vector2[] array, string prefix, bool mergeCoincident, out Dictionary<int, int> coincidentIndexMap)
        {
            coincidentIndexMap = new Dictionary<int, int>();

            if (array == null)
                return 0;

            Dictionary<IntVec2, int> common = new Dictionary<IntVec2, int>();
            int index = 0;

            for (int i = 0, c = array.Length; i < c; i++)
            {
                var texture = array[i];
                var key = new IntVec2(texture);
                int vertexIndex;

                if (mergeCoincident)
                {
                    if (!common.TryGetValue(key, out vertexIndex))
                    {
                        vertexIndex = index++;
                        common.Add(key, vertexIndex);
                    }
                    else
                    {
                        coincidentIndexMap.Add(i, vertexIndex);
                        continue;
                    }
                }
                else
                {
                    vertexIndex = index++;
                }

                coincidentIndexMap.Add(i, vertexIndex);

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}",
                    prefix,
                    texture.x,
                    texture.y));
            }

            return index;
        }

        static int AppendArrayVec3(StringBuilder sb, Vector3[] array, string prefix, bool mergeCoincident, out Dictionary<int, int> coincidentIndexMap)
        {
            coincidentIndexMap = new Dictionary<int, int>();

            if (array == null)
                return 0;

            Dictionary<IntVec3, int> common = new Dictionary<IntVec3, int>();
            int index = 0;

            for (int i = 0, c = array.Length; i < c; i++)
            {
                var vec = array[i];
                var key = new IntVec3(vec);
                int vertexIndex;

                if (mergeCoincident)
                {
                    if (!common.TryGetValue(key, out vertexIndex))
                    {
                        vertexIndex = index++;
                        common.Add(key, vertexIndex);
                    }
                    else
                    {
                        coincidentIndexMap.Add(i, vertexIndex);
                        continue;
                    }
                }
                else
                {
                    vertexIndex = index++;
                }

                coincidentIndexMap.Add(i, vertexIndex);

                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3}",
                    prefix,
                    vec.x,
                    vec.y,
                    vec.z));
            }

            return index;
        }

    }
}
