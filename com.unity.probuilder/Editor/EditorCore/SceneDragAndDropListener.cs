using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UObject = UnityEngine.Object;
using System.Linq;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    static class SceneDragAndDropListener
    {
        static bool s_IsSceneViewDragAndDrop;
        static Mesh s_PreviewMesh;
        static Material s_PreviewMaterial;
        static ProBuilderMesh s_CurrentPreview;
        static bool s_IsFaceDragAndDropOverrideEnabled;
        static Matrix4x4 s_Matrix;

        static Func<Material> s_GetDefaultMaterialDelegate = null;

        static SceneDragAndDropListener()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            s_PreviewMesh = new Mesh()
            {
                name = "pb_DragAndDropListener::PreviewMesh",
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        static void OnBeforeAssemblyReload()
        {
            UObject.DestroyImmediate(s_PreviewMesh);
        }

        public static bool isDragging
        {
            get { return s_IsSceneViewDragAndDrop; }
        }

        static bool isFaceMode
        {
            get { return ProBuilderEditor.selectMode == SelectMode.Face; }
        }

        static Material GetDefaultMaterial()
        {
                if (s_GetDefaultMaterialDelegate == null)
                    s_GetDefaultMaterialDelegate = (Func<Material>)ReflectionUtility.GetOpenDelegate<Func<Material>>(typeof(Material), "GetDefaultMaterial");

                if (s_GetDefaultMaterialDelegate != null)
                    return s_GetDefaultMaterialDelegate();
                return null;
        }

        static Material GetMaterialFromDragReferences(UObject[] references, bool createMaterialForTexture)
        {
            Material mat = references.FirstOrDefault(x => x is Material) as Material;

            if (!createMaterialForTexture || mat != null)
                return mat;

            Texture2D tex = references.FirstOrDefault(x => x is Texture2D) as Texture2D;
            string texPath = tex != null ? AssetDatabase.GetAssetPath(tex) : null;

            if (!string.IsNullOrEmpty(texPath))
            {

                var defaultMaterial = GetDefaultMaterial();

                if (defaultMaterial == null)
                    mat = new Material(Shader.Find("Standard"));
                else
                    mat = new Material(defaultMaterial.shader);

                if (mat.shader == null)
                {
                    UObject.DestroyImmediate(mat);
                    return null;
                }

                mat.mainTexture = tex;

                int lastDot = texPath.LastIndexOf(".", StringComparison.InvariantCulture);
                texPath = texPath.Substring(0, texPath.Length - (texPath.Length - lastDot));
                texPath = AssetDatabase.GenerateUniqueAssetPath(texPath + ".mat");
                AssetDatabase.CreateAsset(mat, texPath);
                AssetDatabase.Refresh();

                return mat;
            }

            return null;
        }

        static void SetMeshPreview(ProBuilderMesh mesh)
        {
            if (s_CurrentPreview != mesh)
            {
                s_PreviewMesh.Clear();
                s_CurrentPreview = mesh;

                if (s_CurrentPreview != null)
                {
                    s_PreviewMaterial = GetMaterialFromDragReferences(DragAndDrop.objectReferences, false);
                    s_IsFaceDragAndDropOverrideEnabled = isFaceMode && s_PreviewMaterial != null && mesh.selectedFaceCount > 0;

                    if (s_IsFaceDragAndDropOverrideEnabled)
                    {
                        s_PreviewMesh.vertices = mesh.positionsInternal;

                        if (mesh.HasArrays(MeshArrays.Color))
                            s_PreviewMesh.colors = mesh.colorsInternal;
                        if (mesh.HasArrays(MeshArrays.Normal))
                            s_PreviewMesh.normals = mesh.normalsInternal;
                        if (mesh.HasArrays(MeshArrays.Texture0))
                            s_PreviewMesh.uv = mesh.texturesInternal;

                        s_Matrix = mesh.transform.localToWorldMatrix;
                        s_PreviewMesh.triangles = mesh.selectedFacesInternal.SelectMany(x => x.indexes).ToArray();
                    }
                }
                else
                {
                    s_IsFaceDragAndDropOverrideEnabled = false;
                }
            }
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            var evt = Event.current;

            if (evt.type == EventType.DragUpdated)
            {
                if (!s_IsSceneViewDragAndDrop)
                    s_IsSceneViewDragAndDrop = true;

                int submeshIndex;
                GameObject go = HandleUtility.PickGameObject(evt.mousePosition, out submeshIndex);

                SetMeshPreview(go != null ? go.GetComponent<ProBuilderMesh>() : null);

                if (s_IsFaceDragAndDropOverrideEnabled)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                }
            }
            else if (evt.type == EventType.DragExited)
            {
                if (s_IsFaceDragAndDropOverrideEnabled)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                    SetMeshPreview(null);
                }

                s_IsSceneViewDragAndDrop = false;
            }
            else if (evt.type == EventType.DragPerform)
            {
                s_IsSceneViewDragAndDrop = false;
                int submeshIndex;
                GameObject go = HandleUtility.PickGameObject(evt.mousePosition, out submeshIndex);
                SetMeshPreview(go != null ? go.GetComponent<ProBuilderMesh>() : null);

                if (s_CurrentPreview != null && s_IsFaceDragAndDropOverrideEnabled)
                {
                    UndoUtility.RecordObject(s_CurrentPreview, "Set Face Material");
                    UndoUtility.RecordObject(s_CurrentPreview.renderer, "Set Face Material");

                    s_CurrentPreview.SetMaterial(s_CurrentPreview.selectedFacesInternal, s_PreviewMaterial);

                    InternalMeshUtility.FilterUnusedSubmeshIndexes(s_CurrentPreview);

                    s_CurrentPreview.ToMesh();
                    s_CurrentPreview.Refresh();
                    s_CurrentPreview.Optimize();

                    evt.Use();
                }

                SetMeshPreview(null);
            }
            else if (evt.type == EventType.Repaint && s_IsFaceDragAndDropOverrideEnabled)
            {
                if (s_PreviewMaterial.SetPass(0))
                    Graphics.DrawMeshNow(s_PreviewMesh, s_Matrix, 0);
            }
        }
    }
}
