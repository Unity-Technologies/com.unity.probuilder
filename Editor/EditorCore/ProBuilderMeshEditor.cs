using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Custom editor for ProBuilderMesh type.
    /// </summary>
    [CustomEditor(typeof(ProBuilderMesh))]
    [CanEditMultipleObjects]
    sealed class ProBuilderMeshEditor : Editor
    {
        static class Styles
        {
            static bool s_Initialized;
            public static GUIStyle miniButton;
            public static GUIStyle helpBox;

            public static readonly GUIContent lightmapStatic = EditorGUIUtility.TrTextContent("Lightmap Static", "Controls whether the geometry will be marked as Static for lightmapping purposes. When enabled, this mesh will be present in lightmap calculations.");
            public static readonly GUIContent sharedMesh = EditorGUIUtility.TrTextContent("Mesh");

            const string k_IconPath = "EditableMesh/EditMeshContext";
            const string k_ComponentMessage = "Use the ProBuilder Edit Mode in the Scene Tools Overlay to edit this Mesh.";
            public static readonly GUIContent helpLabelContentIcon = new GUIContent(IconUtility.GetIcon(k_IconPath));
            public static readonly GUIContent helpLabelContent = EditorGUIUtility.TrTextContent(k_ComponentMessage);


            public static void Init()
            {
                if (s_Initialized)
                    return;
                s_Initialized = true;
                miniButton = new GUIStyle(GUI.skin.button);
                miniButton.stretchHeight = false;
                miniButton.stretchWidth = false;
                miniButton.padding = new RectOffset(6, 6, 3, 3);

                helpBox = new GUIStyle(EditorStyles.helpBox);
                helpBox.padding = new RectOffset(2, 2, 2, 2);
            }
        }

        internal static event System.Action onGetFrameBoundsEvent;
        ProBuilderMesh m_Mesh;

        SerializedObject m_GameObjectsSerializedObject;
        SerializedProperty m_UnwrapParameters;
        SerializedProperty m_StaticEditorFlags;
        bool m_AnyMissingLightmapUVs;
        bool m_ModifyingMesh;

        ProBuilderEditor editor
        {
            get { return ProBuilderEditor.instance; }
        }

        Renderer m_MeshRenderer = null;

        void OnEnable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if(target == null)
                return;

            m_Mesh = (ProBuilderMesh)target;

            if (!m_Mesh)
                return;

            m_GameObjectsSerializedObject = new SerializedObject(serializedObject.targetObjects.Select(t => ((Component)t).gameObject).ToArray());

            m_UnwrapParameters = serializedObject.FindProperty("m_UnwrapParameters");
            m_StaticEditorFlags = m_GameObjectsSerializedObject.FindProperty("m_StaticEditorFlags");
            m_MeshRenderer = m_Mesh.gameObject.GetComponent<Renderer>();

            VertexManipulationTool.beforeMeshModification += OnBeginMeshModification;
            VertexManipulationTool.afterMeshModification += OnFinishMeshModification;
        }

        void OnDisable()
        {
            VertexManipulationTool.beforeMeshModification -= OnBeginMeshModification;
            VertexManipulationTool.afterMeshModification -= OnFinishMeshModification;
        }

        void OnBeginMeshModification(IEnumerable<ProBuilderMesh> selection)
        {
            m_ModifyingMesh = true;
        }

        void OnFinishMeshModification(IEnumerable<ProBuilderMesh> selection)
        {
            m_ModifyingMesh = false;
        }

        public override void OnInspectorGUI()
        {
            if (m_UnwrapParameters ==  null || m_StaticEditorFlags == null)
                return;

            Styles.Init();

            // [SPLB-132] Reverting to custom helpbox as the default helpbox style as a trouble to handle custom icons
            // when using a screen with PixelPerPoints different than 1. This is done in trunk by setting the
            // Texture2d.pixelsPerPoints which is an internal property than cannot be access from here.
            EditorGUILayout.BeginHorizontal(Styles.helpBox);
            EditorGUIUtility.SetIconSize(new Vector2(32f, 32f));
            EditorGUILayout.LabelField(Styles.helpLabelContentIcon,
                GUILayout.Width(34), GUILayout.MinHeight(34), GUILayout.ExpandHeight(true));
            EditorGUIUtility.SetIconSize(Vector2.zero);
            EditorGUILayout.LabelField(Styles.helpLabelContent,
                new GUIStyle(EditorStyles.label){wordWrap = Styles.helpBox.wordWrap, fontSize = Styles.helpBox.fontSize, padding = new RectOffset(-2, 0, 0, 0)},
                GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();

            GUILayout.Box("Mesh property is driven by the ProBuilder component.", EditorStyles.helpBox);
            var guiEnabled = GUI.enabled;
            GUI.enabled = false;
            var guiStateMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = targets.Length > 1;
            EditorGUILayout.ObjectField(Styles.sharedMesh, m_Mesh.mesh, typeof(Mesh), false);
            EditorGUI.showMixedValue = guiStateMixed;
            GUI.enabled = guiEnabled;

            Vector3 bounds = m_MeshRenderer != null ? m_MeshRenderer.bounds.size : Vector3.zero;
            EditorGUILayout.Vector3Field("Object Size (read only)", bounds);

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            LightmapStaticSettings();
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

#if DEVELOPER_MODE
            GUILayout.Space(4);

            GUILayout.Label("Compiled Mesh Information", EditorStyles.boldLabel);

            MeshFilter filter;
            Mesh sharedMesh = null;

            EditorGUILayout.LabelField("Mesh Sync State", m_Mesh.meshSyncState.ToString());
            EditorGUILayout.LabelField("m_MeshVersionIndex", m_Mesh.versionIndex.ToString());
            EditorGUILayout.LabelField("m_NonSerializedMeshIndex", m_Mesh.nonSerializedVersionIndex.ToString());

            if (m_Mesh.TryGetComponent(out filter) && (sharedMesh = filter.sharedMesh) != null)
            {
                GUILayout.Label("Vertex Count: " + sharedMesh.vertexCount);
                GUILayout.Label("Submesh Count: " + sharedMesh.subMeshCount);
            }
            else
            {
                GUILayout.Label("No compiled mesh", EditorStyles.centeredGreyMiniLabel);
            }

            GUILayout.Space(4);

            GUILayout.Label("HideFlags & Driven Properties", EditorStyles.boldLabel);

            if(filter)
                GUILayout.Label($"MeshFilter {filter.hideFlags}");
            else
                GUILayout.Label($"MeshFilter (null)");

            MeshCollider collider;

            if (m_Mesh.TryGetComponent(out collider))
                GUILayout.Label($"MeshCollider.m_Mesh {DrivenPropertyManagerInternal.IsDriven(collider, "m_Mesh")}");

            GUILayout.Space(4);

            GUILayout.Label("Identifiers", EditorStyles.boldLabel);
            EditorGUI.showMixedValue = targets.Length > 1;
            EditorGUILayout.IntField("ProBuilderMesh", m_Mesh.GetInstanceID());
            EditorGUILayout.IntField("UnityEngine.Mesh", sharedMesh != null ? sharedMesh.GetInstanceID() : -1);
            EditorGUILayout.TextField("UnityEngine.Mesh.name", sharedMesh != null ? sharedMesh.name : "null");
            EditorGUI.showMixedValue = false;
#endif
        }

        void LightmapStaticSettings()
        {
            m_GameObjectsSerializedObject.Update();

            bool lightmapStatic = (m_StaticEditorFlags.intValue & (int)StaticEditorFlags.ContributeGI) != 0;

            EditorGUI.BeginChangeCheck();

            lightmapStatic = EditorGUILayout.Toggle(Styles.lightmapStatic, lightmapStatic);

            if (EditorGUI.EndChangeCheck())
            {
                SceneModeUtility.SetStaticFlags(m_GameObjectsSerializedObject.targetObjects, (int)StaticEditorFlags.ContributeGI, lightmapStatic);
            }

            if (lightmapStatic)
            {
                EditorGUILayout.PropertyField(m_UnwrapParameters, true);

                if (m_UnwrapParameters.isExpanded)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Reset", Styles.miniButton))
                        ResetUnwrapParams(m_UnwrapParameters);

                    if (GUILayout.Button("Apply", Styles.miniButton))
                        RebuildLightmapUVs();

                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }

                if (!m_ModifyingMesh)
                {
                    m_AnyMissingLightmapUVs = targets.Any(x =>
                    {
                        if (x is ProBuilderMesh)
                            return !((ProBuilderMesh)x).HasArrays(MeshArrays.Texture1);

                        return false;
                    });
                }

                if (m_AnyMissingLightmapUVs)
                {
                    EditorGUILayout.HelpBox("Lightmap UVs are missing, please generate Lightmap UVs.", MessageType.Warning);

                    if (GUILayout.Button("Generate Lightmap UVs"))
                        RebuildLightmapUVs();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("To enable generation of lightmap UVs for this Mesh, please enable the 'Lightmap Static' property.", MessageType.Info);
            }

        }

        void RebuildLightmapUVs(bool forceRebuildAll = true)
        {
            foreach (var obj in targets)
            {
                if (obj is ProBuilderMesh)
                {
                    var mesh = (ProBuilderMesh)obj;

                    if (!mesh.gameObject.HasStaticFlag(StaticEditorFlags.ContributeGI))
                        continue;

                    if (forceRebuildAll || !mesh.HasArrays(MeshArrays.Texture1))
                        mesh.Optimize(true);
                }
            }
        }

        void ResetUnwrapParams(SerializedProperty prop)
        {
            var hardAngle = prop.FindPropertyRelative("m_HardAngle");
            var packMargin = prop.FindPropertyRelative("m_PackMargin");
            var angleError = prop.FindPropertyRelative("m_AngleError");
            var areaError = prop.FindPropertyRelative("m_AreaError");

            hardAngle.floatValue = UnwrapParameters.k_HardAngle;
            packMargin.floatValue = UnwrapParameters.k_PackMargin;
            angleError.floatValue = UnwrapParameters.k_AngleError;
            areaError.floatValue = UnwrapParameters.k_AreaError;

            RebuildLightmapUVs();
        }

        bool HasFrameBounds()
        {
            if (m_Mesh == null)
                m_Mesh = (ProBuilderMesh)target;

            return ProBuilderEditor.instance != null &&
                InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Sum(x => x.selectedIndexesInternal.Length) > 0;
        }

        Bounds OnGetFrameBounds()
        {
            if (!ProBuilderEditor.selectMode.IsMeshElementMode())
                return m_MeshRenderer != null ? m_MeshRenderer.bounds : default(Bounds);

            if (onGetFrameBoundsEvent != null)
                onGetFrameBoundsEvent();

            Vector3 min = Vector3.zero, max = Vector3.zero;
            bool init = false;

            foreach (ProBuilderMesh mesh in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
            {
                int[] tris = mesh.selectedIndexesInternal;

                if (tris == null || tris.Length < 1)
                    continue;

                Vector3[] verts = mesh.positionsInternal;
                var trs = mesh.transform;

                if (!init)
                {
                    init = true;
                    min = trs.TransformPoint(verts[tris[0]]);
                    max = trs.TransformPoint(verts[tris[0]]);
                }

                for (int i = 0, c = tris.Length; i < c; i++)
                {
                    Vector3 p = trs.TransformPoint(verts[tris[i]]);

                    min.x = Mathf.Min(p.x, min.x);
                    max.x = Mathf.Max(p.x, max.x);

                    min.y = Mathf.Min(p.y, min.y);
                    max.y = Mathf.Max(p.y, max.y);

                    min.z = Mathf.Min(p.z, min.z);
                    max.z = Mathf.Max(p.z, max.z);
                }
            }

            return new Bounds((min + max) / 2f, max != min ? max - min : Vector3.one * .1f);
        }
    }
}
