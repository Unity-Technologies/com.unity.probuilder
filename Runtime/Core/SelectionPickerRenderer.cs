//#define PB_RENDER_PICKER_TEXTURE

using System.Collections.Generic;
using System.Linq;
using UObject = UnityEngine.Object;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A collection of settings that define how mesh element picking behaves.
    /// </summary>
    public struct PickerOptions
    {
        /// <summary>
        /// Gets or sets whether to perform depth testing when testing elements with raycasting.
        /// </summary>
        /// <returns>
        /// True to select only visible elements; false to select all elements regardless of visibility.
        /// </returns>
        public bool depthTest { get; set; }

        /// <summary>
        /// Gets or sets whether to require elements to be completely encompassed by the `rect` selection
        /// (RectSelectMode.Complete) or to allow elements only partially inside the `rect` selection (RectSelectMode.Partial).
        /// </summary>
        /// <remarks>
        /// Does not apply to vertex picking.
        /// </remarks>
        public RectSelectMode rectSelectMode { get; set; }

        static readonly PickerOptions k_Default = new PickerOptions()
        {
            depthTest = true,
            rectSelectMode = RectSelectMode.Partial,
        };

        /// <summary>
        /// Represents a set of picking options with default values. By default, the <see cref="depthTest"/> property
        /// is set to true and the <see cref="rectSelectMode" /> property is set to <see cref="RectSelectMode.Partial"/>.
        /// </summary>
        public static PickerOptions Default
        {
            get { return k_Default; }
        }

        /// <summary>
        /// Evaluates whether the specified object is equivalent to this one.
        /// </summary>
        /// <param name="obj">The object to compare to this object.</param>
        /// <returns>True if both objects are PickerOptions and their property values match; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is PickerOptions))
                return false;

            return Equals((PickerOptions)obj);
        }

        /// <summary>
        /// Evaluates whether the specified PickerOptions object contains the same settings as this one has.
        /// </summary>
        /// <param name="other">The PickerOptions object to compare to this object.</param>
        /// <returns>True if the <see cref="depthTest"/> and <see cref="rectSelectMode" /> property values match; false otherwise.</returns>
        public bool Equals(PickerOptions other)
        {
            return depthTest == other.depthTest && rectSelectMode == other.rectSelectMode;
        }

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (depthTest.GetHashCode() * 397) ^ (int)rectSelectMode;
            }
        }

        /// <summary>
        /// Returns true if the two PickerOptions objects have matching values.
        /// </summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        /// <returns>True if the objects have matching values; false otherwise.</returns>
        public static bool operator==(PickerOptions a, PickerOptions b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns true if the values for the two PickerOptions objects don't match.
        /// </summary>
        /// <param name="a">The first object to compare.</param>
        /// <param name="b">The second object to compare.</param>
        /// <returns>True if the values for the two PickerOptions objects don't match; false otherwise.</returns>
        public static bool operator!=(PickerOptions a, PickerOptions b)
        {
            return !a.Equals(b);
        }
    }

    /// <summary>
    /// Provides functions for picking elements in a view by rendering a picker texture and testing pixels.
    /// </summary>
    static partial class SelectionPickerRenderer
    {
        internal interface ISelectionPickerRenderer
        {
            Texture2D RenderLookupTexture(Camera camera, Shader shader, string tag, int width, int height);
        }

        const string k_FacePickerOcclusionTintUniform = "_Tint";
        static readonly Color k_Blackf = new Color(0f, 0f, 0f, 1f);
        static readonly Color k_Whitef = new Color(1f, 1f, 1f, 1f);
        const uint k_PickerHashNone = 0x00;
        const uint k_PickerHashMin = 0x1;
        const uint k_PickerHashMax = 0x00FFFFFF;
        const uint k_MinEdgePixelsForValidSelection = 1;

        static bool s_Initialized = false;
        static ISelectionPickerRenderer s_PickerRenderer = null;

        static RenderTextureFormat renderTextureFormat
        {
            get
            {
                if (s_Initialized)
                    return s_RenderTextureFormat;

                s_Initialized = true;

                for (int i = 0; i < s_PreferredFormats.Length; i++)
                {
                    if (SystemInfo.SupportsRenderTextureFormat(s_PreferredFormats[i]))
                    {
                        s_RenderTextureFormat = s_PreferredFormats[i];
                        break;
                    }
                }

                return s_RenderTextureFormat;
            }
        }

        static TextureFormat textureFormat { get { return TextureFormat.ARGB32; } }

        static RenderTextureFormat s_RenderTextureFormat = RenderTextureFormat.Default;

        static RenderTextureFormat[] s_PreferredFormats = new RenderTextureFormat[]
        {
            RenderTextureFormat.ARGB32,
            RenderTextureFormat.ARGBFloat,
        };

        /// <summary>
        /// Returns an appropriate implementation based on the graphic pipeline
        /// to generate the lookup texture.
        /// URP and Standard pipeline share the same picker implementation for now.
        /// </summary>
        static ISelectionPickerRenderer pickerRenderer
        {
            get
            {
                if (s_PickerRenderer == null)
                    s_PickerRenderer =
                        ShouldUseHDRP()?
                        (ISelectionPickerRenderer)new SelectionPickerRendererHDRP()
                        : new SelectionPickerRendererStandard();
                return s_PickerRenderer;
            }
        }

        /// <summary>
        /// Given a camera and selection rect (in screen space) return a Dictionary containing the number of faces touched by the rect.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="pickerRect"></param>
        /// <param name="selection"></param>
        /// <param name="renderTextureWidth"></param>
        /// <param name="renderTextureHeight"></param>
        /// <returns></returns>
        public static Dictionary<ProBuilderMesh, HashSet<Face>> PickFacesInRect(
            Camera camera,
            Rect pickerRect,
            IList<ProBuilderMesh> selection,
            int renderTextureWidth = -1,
            int renderTextureHeight = -1)
        {
            Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map;
            Texture2D tex = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);

            Color32[] pix = tex.GetPixels32();

            int ox = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
            int oy = System.Math.Max(0, Mathf.FloorToInt((tex.height - pickerRect.y) - pickerRect.height));
            int imageWidth = tex.width;
            int imageHeight = tex.height;
            int width = Mathf.FloorToInt(pickerRect.width);
            int height = Mathf.FloorToInt(pickerRect.height);
            UObject.DestroyImmediate(tex);

            Dictionary<ProBuilderMesh, HashSet<Face>> selected = new Dictionary<ProBuilderMesh, HashSet<Face>>();
            SimpleTuple<ProBuilderMesh, Face> hit;
            HashSet<Face> faces = null;
            HashSet<uint> used = new HashSet<uint>();

#if PB_RENDER_PICKER_TEXTURE
            List<Color> rectImg = new List<Color>();
#endif

            for (int y = oy; y < System.Math.Min(oy + height, imageHeight); y++)
            {
                for (int x = ox; x < System.Math.Min(ox + width, imageWidth); x++)
                {
#if PB_RENDER_PICKER_TEXTURE
                    rectImg.Add(pix[y * imageWidth + x]);
#endif

                    uint v = SelectionPickerRenderer.DecodeRGBA(pix[y * imageWidth + x]);

                    if (used.Add(v) && map.TryGetValue(v, out hit))
                    {
                        if (selected.TryGetValue(hit.item1, out faces))
                            faces.Add(hit.item2);
                        else
                            selected.Add(hit.item1, new HashSet<Face>() { hit.item2 });
                    }
                }
            }

#if PB_RENDER_PICKER_TEXTURE
            if (width > 0 && height > 0)
            {
//              Debug.Log("used: \n" + used.Select(x => string.Format("{0} ({1})", x, EncodeRGBA(x))).ToString("\n"));
                Texture2D img = new Texture2D(width, height);
                img.SetPixels(rectImg.ToArray());
                img.Apply();
                byte[] bytes = img.EncodeToPNG();
                System.IO.File.WriteAllBytes("Assets/rect.png", bytes);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                UObject.DestroyImmediate(img);
            }
#endif

            return selected;
        }

        /// <summary>
        /// Selects the vertex indexes contained within a rect (rectangular selection).
        /// </summary>
        /// <param name="camera">Use this camera to evaluate whether any vertices are inside the `rect`.</param>
        /// <param name="pickerRect">Rect is in GUI space, where 0,0 is the top left of the screen, and `width = cam.pixelWidth / pointsPerPixel`.</param>
        /// <param name="selection">The collection of objects to check for selectability. ProBuilder verifies whether these objects fall inside the `rect`.</param>
        /// <param name="doDepthTest">True to include mesh elements that are hidden; false otherwise</param>
        /// <param name="renderTextureWidth">Width of the texture. If not specified, ProBuilder uses auto sizing (matches the camera's texture width).</param>
        /// <param name="renderTextureHeight">Height of the texture. If not specified, ProBuilder uses auto sizing (matches the camera's texture height).</param>
        /// <returns>A dictionary of ProBuilderMesh objects and vertex indices that are in the selection `rect`. </returns>
        public static Dictionary<ProBuilderMesh, HashSet<int>> PickVerticesInRect(
            Camera camera,
            Rect pickerRect,
            IList<ProBuilderMesh> selection,
            bool doDepthTest,
            int renderTextureWidth = -1,
            int renderTextureHeight = -1)
        {
            Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map;
            Dictionary<ProBuilderMesh, HashSet<int>> selected = new Dictionary<ProBuilderMesh, HashSet<int>>();

#if PB_RENDER_PICKER_TEXTURE
            List<Color> rectImg = new List<Color>();
#endif

            Texture2D tex = RenderSelectionPickerTexture(camera, selection, doDepthTest, out map, renderTextureWidth, renderTextureHeight);
            Color32[] pix = tex.GetPixels32();

            int ox = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
            int oy = System.Math.Max(0, Mathf.FloorToInt((tex.height - pickerRect.y) - pickerRect.height));
            int imageWidth = tex.width;
            int imageHeight = tex.height;
            int width = Mathf.FloorToInt(pickerRect.width);
            int height = Mathf.FloorToInt(pickerRect.height);

            UObject.DestroyImmediate(tex);

            SimpleTuple<ProBuilderMesh, int> hit;
            HashSet<int> indexes = null;
            HashSet<uint> used = new HashSet<uint>();

            for (int y = oy; y < System.Math.Min(oy + height, imageHeight); y++)
            {
                for (int x = ox; x < System.Math.Min(ox + width, imageWidth); x++)
                {
                    uint v = DecodeRGBA(pix[y * imageWidth + x]);

#if PB_RENDER_PICKER_TEXTURE
                    rectImg.Add(pix[y * imageWidth + x]);
#endif

                    if (used.Add(v) && map.TryGetValue(v, out hit))
                    {
                        if (selected.TryGetValue(hit.item1, out indexes))
                            indexes.Add(hit.item2);
                        else
                            selected.Add(hit.item1, new HashSet<int>() { hit.item2 });
                    }
                }
            }

            var coincidentSelection = new Dictionary<ProBuilderMesh, HashSet<int>>();

            // workaround for picking vertices that share a position but are not shared
            foreach (var meshSelection in selected)
            {
                var positions = meshSelection.Key.positionsInternal;
                var sharedVertices = meshSelection.Key.sharedVerticesInternal;
                var positionHash = new HashSet<int>(meshSelection.Value.Select(x => VectorHash.GetHashCode(positions[sharedVertices[x][0]])));
                var collected = new HashSet<int>();

                for (int i = 0, c = sharedVertices.Length; i < c; i++)
                {
                    var hash = VectorHash.GetHashCode(positions[sharedVertices[i][0]]);
                    if (positionHash.Contains(hash))
                        collected.Add(i);
                }

                coincidentSelection.Add(meshSelection.Key, collected);
            }
            selected = coincidentSelection;

#if PB_RENDER_PICKER_TEXTURE
            if (width > 0 && height > 0)
            {
                Texture2D img = new Texture2D(width, height);
                img.SetPixels(rectImg.ToArray());
                img.Apply();
                System.IO.File.WriteAllBytes("Assets/rect_" + s_RenderTextureFormat.ToString() + ".png", img.EncodeToPNG());
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                UObject.DestroyImmediate(img);
            }
#endif

            return selected;
        }

        /// <summary>
        /// Selects the <see cref="Edge" /> objects contained within a rect (rectangular selection).
        /// </summary>
        /// <param name="camera">Use this camera to evaluate whether any edge object(s) are inside the `rect`.</param>
        /// <param name="pickerRect">Rect is in GUI space, where 0,0 is the top left of the screen, and `width = cam.pixelWidth / pointsPerPixel`.</param>
        /// <param name="selection">The collection of objects to check for selectability. ProBuilder verifies whether these objects fall inside the `rect`.</param>
        /// <param name="doDepthTest">True to include mesh elements that are hidden; false otherwise</param>
        /// <param name="renderTextureWidth">Width of the texture. If not specified, ProBuilder uses auto sizing (matches the camera's texture width).</param>
        /// <param name="renderTextureHeight">Height of the texture. If not specified, ProBuilder uses auto sizing (matches the camera's texture height).</param>
        /// <returns>A dictionary of ProBuilderMesh and Edge objects that are in the selection `rect`. </returns>
        public static Dictionary<ProBuilderMesh, HashSet<Edge>> PickEdgesInRect(
            Camera camera,
            Rect pickerRect,
            IList<ProBuilderMesh> selection,
            bool doDepthTest,
            int renderTextureWidth = -1,
            int renderTextureHeight = -1)
        {
            var selected = new Dictionary<ProBuilderMesh, HashSet<Edge>>();

#if PB_RENDER_PICKER_TEXTURE
            List<Color> rectImg = new List<Color>();
#endif

            Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map;
            Texture2D tex = RenderSelectionPickerTexture(camera, selection, doDepthTest, out map, renderTextureWidth, renderTextureHeight);
            Color32[] pix = tex.GetPixels32();

#if PB_RENDER_PICKER_TEXTURE
            System.IO.File.WriteAllBytes("Assets/edge_scene.png", tex.EncodeToPNG());
#endif

            int ox = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
            int oy = System.Math.Max(0, Mathf.FloorToInt((tex.height - pickerRect.y) - pickerRect.height));
            int imageWidth = tex.width;
            int imageHeight = tex.height;
            int width = Mathf.FloorToInt(pickerRect.width);
            int height = Mathf.FloorToInt(pickerRect.height);
            UObject.DestroyImmediate(tex);

            var pixelCount = new Dictionary<uint, uint>();

            for (int y = oy; y < System.Math.Min(oy + height, imageHeight); y++)
            {
                for (int x = ox; x < System.Math.Min(ox + width, imageWidth); x++)
                {
#if PB_RENDER_PICKER_TEXTURE
                    rectImg.Add(pix[y * imageWidth + x]);
#endif
                    uint v = DecodeRGBA(pix[y * imageWidth + x]);

                    if (v == k_PickerHashNone || v == k_PickerHashMax)
                        continue;

                    if (!pixelCount.ContainsKey(v))
                        pixelCount.Add(v, 1);
                    else
                        pixelCount[v] = pixelCount[v] + 1;
                }
            }

            foreach (var kvp in pixelCount)
            {
                SimpleTuple<ProBuilderMesh, Edge> hit;

                if (kvp.Value > k_MinEdgePixelsForValidSelection && map.TryGetValue(kvp.Key, out hit))
                {
                    HashSet<Edge> edges = null;

                    if (selected.TryGetValue(hit.item1, out edges))
                        edges.Add(hit.item2);
                    else
                        selected.Add(hit.item1, new HashSet<Edge>() {hit.item2});
                }
            }

#if PB_RENDER_PICKER_TEXTURE
            if (width > 0 && height > 0)
            {
                Texture2D img = new Texture2D(width, height);
                img.SetPixels(rectImg.ToArray());
                img.Apply();
                System.IO.File.WriteAllBytes("Assets/edge_rect_" + s_RenderTextureFormat.ToString() + ".png", img.EncodeToPNG());
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                UObject.DestroyImmediate(img);
            }
#endif

            return selected;
        }

        /// <summary>
        /// Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, face} dictionary.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="selection"></param>
        /// <param name="map"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        internal static Texture2D RenderSelectionPickerTexture(
            Camera camera,
            IList<ProBuilderMesh> selection,
            out Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map,
            int width = -1,
            int height = -1)
        {
            var pickerObjects = GenerateFacePickingObjects(selection, out map);

            BuiltinMaterials.facePickerMaterial.SetColor(k_FacePickerOcclusionTintUniform, k_Whitef);

            Texture2D tex = pickerRenderer.RenderLookupTexture(camera, BuiltinMaterials.selectionPickerShader, "ProBuilderPicker", width, height);

            foreach (GameObject go in pickerObjects)
            {
                UObject.DestroyImmediate(go.GetComponent<MeshFilter>().sharedMesh);
                UObject.DestroyImmediate(go);
            }

            return tex;
        }

        /// <summary>
        /// Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, sharedIndex} dictionary.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="selection"></param>
        /// <param name="doDepthTest"></param>
        /// <param name="map"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        internal static Texture2D RenderSelectionPickerTexture(
            Camera camera,
            IList<ProBuilderMesh> selection,
            bool doDepthTest,
            out Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map,
            int width = -1,
            int height = -1)
        {
            GameObject[] depthObjects, pickerObjects;

            GenerateVertexPickingObjects(selection, doDepthTest, out map, out depthObjects, out pickerObjects);

            BuiltinMaterials.facePickerMaterial.SetColor(k_FacePickerOcclusionTintUniform, k_Blackf);

            Texture2D tex = pickerRenderer.RenderLookupTexture(camera, BuiltinMaterials.selectionPickerShader, "ProBuilderPicker", width, height);

            for (int i = 0, c = pickerObjects.Length; i < c; i++)
            {
                UObject.DestroyImmediate(pickerObjects[i].GetComponent<MeshFilter>().sharedMesh);
                UObject.DestroyImmediate(pickerObjects[i]);
            }

            if (doDepthTest)
            {
                for (int i = 0, c = depthObjects.Length; i < c; i++)
                {
                    UObject.DestroyImmediate(depthObjects[i]);
                }
            }

            return tex;
        }

        /// <summary>
        /// Render the pb_Object selection with the special selection picker shader and return a texture and color -> {object, edge} dictionary.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="selection"></param>
        /// <param name="doDepthTest"></param>
        /// <param name="map"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        internal static Texture2D RenderSelectionPickerTexture(
            Camera camera,
            IList<ProBuilderMesh> selection,
            bool doDepthTest,
            out Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map,
            int width = -1,
            int height = -1)
        {
            GameObject[] depthObjects, pickerObjects;
            GenerateEdgePickingObjects(selection, doDepthTest, out map, out depthObjects, out pickerObjects);

            BuiltinMaterials.facePickerMaterial.SetColor(k_FacePickerOcclusionTintUniform, k_Blackf);

            Texture2D tex = pickerRenderer.RenderLookupTexture(camera, BuiltinMaterials.selectionPickerShader, "ProBuilderPicker", width, height);

            for (int i = 0, c = pickerObjects.Length; i < c; i++)
            {
                UObject.DestroyImmediate(pickerObjects[i].GetComponent<MeshFilter>().sharedMesh);
                UObject.DestroyImmediate(pickerObjects[i]);
            }

            if (doDepthTest)
            {
                for (int i = 0, c = depthObjects.Length; i < c; i++)
                {
                    UObject.DestroyImmediate(depthObjects[i]);
                }
            }
            return tex;
        }

        static GameObject[] GenerateFacePickingObjects(
            IList<ProBuilderMesh> selection,
            out Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map)
        {
            int selectionCount = selection.Count;
            GameObject[] pickerObjects = new GameObject[selectionCount];
            map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>>();

            uint index = 0;

            for (int i = 0; i < selectionCount; i++)
            {
                var pb = selection[i];

                Mesh m = new Mesh();
                m.vertices = pb.positionsInternal;
                m.triangles = pb.facesInternal.SelectMany(x => x.indexesInternal).ToArray();
                Color32[] colors = new Color32[m.vertexCount];

                foreach (Face f in pb.facesInternal)
                {
                    Color32 color = EncodeRGBA(index++);
                    map.Add(DecodeRGBA(color), new SimpleTuple<ProBuilderMesh, Face>(pb, f));

                    for (int n = 0; n < f.distinctIndexesInternal.Length; n++)
                        colors[f.distinctIndexesInternal[n]] = color;
                }

                m.colors32 = colors;

                GameObject go = InternalUtility.MeshGameObjectWithTransform(pb.name + " (Face Depth Test)", pb.transform, m,
                    BuiltinMaterials.facePickerMaterial, true);

                pickerObjects[i] = go;
            }

            return pickerObjects;
        }

        static void GenerateVertexPickingObjects(
            IList<ProBuilderMesh> selection,
            bool doDepthTest,
            out Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map,
            out GameObject[] depthObjects,
            out GameObject[] pickerObjects)
        {
            map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, int>>();

            // don't start at 0 because that means one vertex would be black, matching
            // the color used to cull hidden vertices.
            uint index = 0x02;
            int selectionCount = selection.Count;
            pickerObjects = new GameObject[selectionCount];

            for (int i = 0; i < selectionCount; i++)
            {
                // build vertex billboards
                var pb = selection[i];

                var mesh = BuildVertexMesh(pb, map, ref index);
                GameObject go = InternalUtility.MeshGameObjectWithTransform(pb.name + " (Vertex Billboards)", pb.transform, mesh,
                    BuiltinMaterials.vertexPickerMaterial, true);

                pickerObjects[i] = go;
            }

            if (doDepthTest)
            {
                depthObjects = new GameObject[selectionCount];

                // copy the select gameobject just for z-write
                for (int i = 0; i < selectionCount; i++)
                {
                    var pb = selection[i];

                    GameObject go = InternalUtility.MeshGameObjectWithTransform(pb.name + " (Depth Mask)", pb.transform, pb.mesh,
                        BuiltinMaterials.facePickerMaterial, true);

                    depthObjects[i] = go;
                }
            }
            else
            {
                depthObjects = null;
            }
        }

        static void GenerateEdgePickingObjects(
            IList<ProBuilderMesh> selection,
            bool doDepthTest,
            out Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map,
            out GameObject[] depthObjects,
            out GameObject[] pickerObjects)
        {
            map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>>();

            uint index = 0x2;
            int selectionCount = selection.Count;
            pickerObjects = new GameObject[selectionCount];

            for (int i = 0; i < selectionCount; i++)
            {
                // build edge billboards
                var pb = selection[i];

                var mesh = BuildEdgeMesh(pb, map, ref index);
                GameObject go = InternalUtility.MeshGameObjectWithTransform(pb.name + " (Edge Billboards)", pb.transform, mesh,
                    BuiltinMaterials.edgePickerMaterial, true);

                pickerObjects[i] = go;
            }

            if (doDepthTest)
            {
                depthObjects = new GameObject[selectionCount];

                for (int i = 0; i < selectionCount; i++)
                {
                    var pb = selection[i];

                    // copy the select gameobject just for z-write
                    GameObject go = InternalUtility.MeshGameObjectWithTransform(pb.name + " (Depth Mask)", pb.transform, pb.mesh,
                        BuiltinMaterials.facePickerMaterial, true);

                    depthObjects[i] = go;
                }
            }
            else
            {
                depthObjects = null;
            }
        }

        static Mesh BuildVertexMesh(ProBuilderMesh pb, Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map, ref uint index)
        {
            int length = System.Math.Min(pb.sharedVerticesInternal.Length, ushort.MaxValue / 4 - 1);

            Vector3[]   t_billboards        = new Vector3[length * 4];
            Vector2[]   t_uvs               = new Vector2[length * 4];
            Vector2[]   t_uv2               = new Vector2[length * 4];
            Color[]     t_col               = new Color[length * 4];
            int[]       t_tris              = new int[length * 6];

            int n = 0;
            int t = 0;

            Vector3 up = Vector3.up;
            Vector3 right = Vector3.right;

            for (int i = 0; i < length; i++)
            {
                Vector3 v = pb.positionsInternal[pb.sharedVerticesInternal[i][0]];

                t_billboards[t + 0] = v;
                t_billboards[t + 1] = v;
                t_billboards[t + 2] = v;
                t_billboards[t + 3] = v;

                t_uvs[t + 0] = Vector3.zero;
                t_uvs[t + 1] = Vector3.right;
                t_uvs[t + 2] = Vector3.up;
                t_uvs[t + 3] = Vector3.one;

                t_uv2[t + 0] = -up - right;
                t_uv2[t + 1] = -up + right;
                t_uv2[t + 2] =  up - right;
                t_uv2[t + 3] =  up + right;

                t_tris[n + 0] = t + 0;
                t_tris[n + 1] = t + 1;
                t_tris[n + 2] = t + 2;
                t_tris[n + 3] = t + 1;
                t_tris[n + 4] = t + 3;
                t_tris[n + 5] = t + 2;

                Color32 color = EncodeRGBA(index);
                map.Add(index++, new SimpleTuple<ProBuilderMesh, int>(pb, i));

                t_col[t + 0] = color;
                t_col[t + 1] = color;
                t_col[t + 2] = color;
                t_col[t + 3] = color;

                t += 4;
                n += 6;
            }

            Mesh mesh = new Mesh();
            mesh.name = "Vertex Billboard";
            mesh.vertices = t_billboards;
            mesh.uv = t_uvs;
            mesh.uv2 = t_uv2;
            mesh.colors = t_col;
            mesh.triangles = t_tris;

            return mesh;
        }

        static Mesh BuildEdgeMesh(ProBuilderMesh pb, Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map, ref uint index)
        {
            int edgeCount = 0;
            int faceCount = pb.faceCount;

            for (int i = 0; i < faceCount; i++)
                edgeCount += pb.facesInternal[i].edgesInternal.Length;

            int elementCount = System.Math.Min(edgeCount, ushort.MaxValue / 2 - 1);

            Vector3[] positions = new Vector3[elementCount * 2];
            Color32[] color = new Color32[elementCount * 2];
            int[] tris = new int[elementCount * 2];

            int edgeIndex = 0;

            for (int i = 0; i < faceCount && edgeIndex < elementCount; i++)
            {
                for (int n = 0; n < pb.facesInternal[i].edgesInternal.Length && edgeIndex < elementCount; n++)
                {
                    var edge = pb.facesInternal[i].edgesInternal[n];

                    Vector3 a = pb.positionsInternal[edge.a];
                    Vector3 b = pb.positionsInternal[edge.b];
                    int positionIndex = edgeIndex * 2;

                    positions[positionIndex + 0] = a;
                    positions[positionIndex + 1] = b;

                    Color32 c = EncodeRGBA(index);

                    map.Add(index++, new SimpleTuple<ProBuilderMesh, Edge>(pb, edge));

                    color[positionIndex + 0] = c;
                    color[positionIndex + 1] = c;

                    tris[positionIndex + 0] = positionIndex + 0;
                    tris[positionIndex + 1] = positionIndex + 1;

                    edgeIndex++;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Edge Billboard";
            mesh.vertices = positions;
            mesh.colors32 = color;
            mesh.subMeshCount = 1;
            mesh.SetIndices(tris, MeshTopology.Lines, 0);

            return mesh;
        }

        /// <summary>
        /// Decodes Color32.RGB values to a 32-bit unsigned int, using the RGB as the little bytes. Discards the hi byte (alpha).
        /// </summary>
        /// <param name="color">The color to decode.</param>
        /// <returns>32-bit unsigned int containing the decoded RGB values.</returns>
        public static uint DecodeRGBA(Color32 color)
        {
            uint r = (uint)color.r;
            uint g = (uint)color.g;
            uint b = (uint)color.b;

            if (System.BitConverter.IsLittleEndian)
                return r << 16 | g << 8 | b;
            else
                return r << 24 | g << 16 | b << 8;
        }

        /// <summary>
        /// Encodes the low 24 bits of a 32-bit unsigned int to Color32.RGB values, using 255 for the alpha.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns>Color32 containing the encoded values.</returns>
        public static Color32 EncodeRGBA(uint hash)
        {
            // skip using BitConverter.GetBytes since this is super simple
            // bit math, and allocating arrays for each conversion is expensive
            if (System.BitConverter.IsLittleEndian)
                return new Color32(
                    (byte)(hash >> 16 & 0xFF),
                    (byte)(hash >>  8 & 0xFF),
                    (byte)(hash       & 0xFF),
                    (byte)(255));
            else
                return new Color32(
                    (byte)(hash >> 24 & 0xFF),
                    (byte)(hash >> 16 & 0xFF),
                    (byte)(hash >>  8 & 0xFF),
                    (byte)(255));
        }

        static bool ShouldUseHDRP()
        {
#if HDRP_7_1_0_OR_NEWER
            return true;
#else
            return false;
#endif
        }
    }
}
