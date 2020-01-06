using System;
using System.Linq;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
#if !UNITY_2019_3_OR_NEWER
using System.Reflection;
#endif

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Shape creation panel implementation.
    /// </summary>
    sealed class ShapeEditor : ConfigurableWindow
    {
        [Serializable]
        abstract class ShapeBuilder
        {
            public virtual string name
            {
                get { return ObjectNames.NicifyVariableName(GetType().Name); }
            }

            public abstract void OnGUI();

            public abstract ProBuilderMesh Build(bool isPreview = false);
        }

        public static void MenuOpenShapeCreator()
        {
            GetWindow<ShapeEditor>("Shape Tool");
        }

        Vector2 m_Scroll = Vector2.zero;
        static Pref<int> s_CurrentIndex = new Pref<int>("ShapeEditor.s_CurrentIndex", 0, SettingsScope.User);
        [UserSetting("Toolbar", "Close Shape Window after Build", "When true the shape window will close after hitting the build button.")]
        static Pref<bool> s_CloseWindowAfterCreateShape = new Pref<bool>("editor.closeWindowAfterShapeCreation", false);
        [SerializeField]
        GameObject m_PreviewObject;

        [SerializeField]
        ShapeBuilder[] m_ShapeBuilders = new ShapeBuilder[]
        {
            new Cube(),
            new Sprite(),
            new Prism(),
            new Stair(),
            new Cylinder(),
            new Door(),
            new Plane(),
            new Pipe(),
            new Cone(),
            new Arch(),
            new Sphere(),
            new Torus(),
            new Custom()
        };

        string[] m_ShapeTypes;

        void OnEnable()
        {
            PrefabStage.prefabStageOpened += PrefabStageOpened;
            PrefabStage.prefabStageClosing += PrefabStageClosing;
            m_ShapeTypes = m_ShapeBuilders.Select(x => x.name).ToArray();
            // Delaying the call til end of frame fixes an issue where entering play mode would cause the preview object
            // to not appear in the Hierarchy until the Shape Editor is interacted with.
            EditorApplication.delayCall += () => SetPreviewMesh(m_ShapeBuilders[s_CurrentIndex].Build());
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            PrefabStage.prefabStageOpened -= PrefabStageOpened;
            PrefabStage.prefabStageClosing -= PrefabStageClosing;
            DestroyPreviewObject(false);
        }

        void PlayModeStateChanged(PlayModeStateChange state)
        {
            if(state == PlayModeStateChange.EnteredEditMode)
                SetPreviewMesh(m_ShapeBuilders[s_CurrentIndex].Build());
        }

        void PrefabStageOpened(PrefabStage stage)
        {
            if(m_PreviewObject != null)
                EditorUtility.MoveToActiveScene(m_PreviewObject);
        }

        void PrefabStageClosing(PrefabStage stage)
        {
            // Closing is called while the PrefabStage is still open, so we can't use EditorUtility.MoveToActiveScene
            if (m_PreviewObject != null)
            {
                m_PreviewObject.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(m_PreviewObject, SceneManager.GetActiveScene());
            }
        }

        [MenuItem("GameObject/3D Object/" + PreferenceKeys.pluginTitle + " Cube _%k")]
        public static void MenuCreateCube()
        {
            ProBuilderMesh mesh = ShapeGenerator.GenerateCube(EditorUtility.newShapePivotLocation, Vector3.one);
            UndoUtility.RegisterCreatedObjectUndo(mesh.gameObject, "Create Shape");
            EditorUtility.InitObject(mesh);
        }

        void OnGUI()
        {
            DoContextMenu();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                CreateSelectedShape(true);

            GUILayout.Label("Shape Selector", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            s_CurrentIndex.value = EditorGUILayout.Popup(s_CurrentIndex, m_ShapeTypes);

            GUILayout.Label("Shape Settings", EditorStyles.boldLabel);

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            var shape = m_ShapeBuilders[s_CurrentIndex];

            shape.OnGUI();

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                ProBuilderSettings.Save();
                SetPreviewMesh(shape.Build());
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Build"))
                CreateSelectedShape();
        }

        void CreateSelectedShape(bool forceCloseWindow = false)
        {
            var res = m_ShapeBuilders[s_CurrentIndex].Build();
            EditorUtility.InitObject(res);
            ApplyPreviewTransform(res);
            DestroyPreviewObject(true);

            Undo.RegisterCreatedObjectUndo(res.gameObject, "Create Shape");

            if (forceCloseWindow || s_CloseWindowAfterCreateShape)
                Close();
        }

        void DestroyPreviewObject(bool immediate)
        {
            if (immediate)
                DestroyPreviewObjectInternal();
            else
                EditorApplication.delayCall += DestroyPreviewObjectInternal;
        }

        void DestroyPreviewObjectInternal()
        {
            if (m_PreviewObject != null)
            {
                if (Selection.Contains(m_PreviewObject.gameObject))
                    MeshSelection.RemoveFromSelection(m_PreviewObject.gameObject);

                if (m_PreviewObject.GetComponent<MeshFilter>().sharedMesh != null)
                    DestroyImmediate(m_PreviewObject.GetComponent<MeshFilter>().sharedMesh);

                DestroyImmediate(m_PreviewObject);

                // When entering play mode the editor tracker isn't rebuilt before the Inspector redraws, meaning the
                // preview object is still assumed to be in the selection. Flush the selection changes by rebuilding
                // active editor tracker fixes this.
#if UNITY_2019_3_OR_NEWER
                ActiveEditorTracker.RebuildAllIfNecessary();
#else
                var rebuildAllTrackers = typeof(ActiveEditorTracker).GetMethod("Internal_RebuildAllIfNecessary", BindingFlags.Static | BindingFlags.NonPublic);
                if(rebuildAllTrackers != null)
                    rebuildAllTrackers.Invoke(null, null);
#endif
            }
        }

        void SetPreviewMesh(ProBuilderMesh mesh)
        {
            ApplyPreviewTransform(mesh);
            Mesh umesh = mesh.mesh;
            if(umesh != null)
                umesh.hideFlags = HideFlags.DontSave;

            if (m_PreviewObject)
            {
                var mf = m_PreviewObject.GetComponent<MeshFilter>();
                if (mf.sharedMesh != null)
                    DestroyImmediate(mf.sharedMesh);
                m_PreviewObject.GetComponent<MeshFilter>().sharedMesh = umesh;
                mesh.preserveMeshAssetOnDestroy = true;
                m_PreviewObject.name = mesh.gameObject.name;
                DestroyImmediate(mesh.gameObject);
            }
            else
            {
                m_PreviewObject = mesh.gameObject;
                mesh.preserveMeshAssetOnDestroy = true;
                DestroyImmediate(mesh);
                Selection.activeTransform = m_PreviewObject.transform;
            }

            m_PreviewObject.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.ShapePreviewMaterial;
            EditorUtility.MoveToActiveScene(m_PreviewObject.gameObject);
        }

        void ApplyPreviewTransform(ProBuilderMesh mesh)
        {
            var position = Vector3.zero;
            var scale = Vector3.one;
            var rotation = Quaternion.identity;
            var previous = m_PreviewObject != null;

            if (previous)
            {
                position = m_PreviewObject.transform.position;
                rotation = m_PreviewObject.transform.localRotation;
                scale = m_PreviewObject.transform.localScale;

                mesh.transform.position = position;
                mesh.transform.localRotation = rotation;
                mesh.transform.localScale = scale;
            }
            else
            {
                EditorUtility.ScreenCenter(mesh.gameObject);
                EditorUtility.SetPivotLocationAndSnap(mesh);
            }
        }

        [Serializable]
        class Cube : ShapeBuilder
        {
            static Pref<Vector3> s_CubeSize = new Pref<Vector3>("ShapeBuilder.Cube.s_CubeSize", Vector3.one, SettingsScope.User);

            public override void OnGUI()
            {
                s_CubeSize.value = EditorGUILayout.Vector3Field("Size", s_CubeSize);
                s_CubeSize.value = Vector3.Max(s_CubeSize.value, new Vector3(.001f, .001f, .001f));
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateCube(EditorUtility.newShapePivotLocation, s_CubeSize);
            }
        }

        [Serializable]
        class Sprite : ShapeBuilder
        {
            static Pref<Axis> s_Axis = new Pref<Axis>("ShapeBuilder.Sprite.s_Axis", Axis.Forward, SettingsScope.User);

            public override void OnGUI()
            {
                s_Axis.value = (Axis)EditorGUILayout.EnumPopup("Axis", s_Axis);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GeneratePlane(EditorUtility.newShapePivotLocation, 1, 1, 0, 0, s_Axis);
            }
        }

        [Serializable]
        class Prism : ShapeBuilder
        {
            static Pref<Vector3> s_PrismSize = new Pref<Vector3>("ShapeBuilder.Prism.s_PrismSize", Vector3.one, SettingsScope.User);

            public override void OnGUI()
            {
                s_PrismSize.value = EditorGUILayout.Vector3Field("Size", s_PrismSize);
                s_PrismSize.value = Vector3.Max(s_PrismSize.value, new Vector3(.001f, .001f, .001f));
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GeneratePrism(EditorUtility.newShapePivotLocation, s_PrismSize);
            }
        }

        [Serializable]
        class Stair : ShapeBuilder
        {
            static Pref<int> s_Steps = new Pref<int>("ShapeBuilder.Stair.s_Steps", 6, SettingsScope.User);
            static Pref<Vector3> s_Size = new Pref<Vector3>("ShapeBuilder.Stair.s_Size", new Vector3(2f, 2.5f, 4f), SettingsScope.User);
            static Pref<float> s_Circumference = new Pref<float>("ShapeBuilder.Stair.s_Circumference", 0f, SettingsScope.User);
            static Pref<bool> s_Sides = new Pref<bool>("ShapeBuilder.Stair.s_Sides", true, SettingsScope.User);
            static Pref<bool> s_Mirror = new Pref<bool>("ShapeBuilder.Stair.s_Mirror", false, SettingsScope.User);

            public override void OnGUI()
            {
                s_Steps.value = (int)Mathf.Max(UI.EditorGUIUtility.FreeSlider("Steps", s_Steps, 2, 64), 2);
                s_Sides.value = EditorGUILayout.Toggle("Build Sides", s_Sides);
                s_Circumference.value = EditorGUILayout.Slider("Curvature", s_Circumference, 0f, 360f);

                Vector3 size = s_Size.value;

                if (s_Circumference > 0f)
                {
                    s_Mirror.value = EditorGUILayout.Toggle("Mirror", s_Mirror);

                    size.x =
                        Mathf.Max(
                            UI.EditorGUIUtility.FreeSlider(
                                new GUIContent("Stair Width", "The width of an individual stair step."), size.x,
                                .01f, 10f), .01f);
                    size.y =
                        Mathf.Max(
                            UI.EditorGUIUtility.FreeSlider(
                                new GUIContent("Stair Height",
                                    "The total height of this staircase.  You may enter any value in the float field."),
                                size.y, .01f, 10f), .01f);
                    size.z =
                        Mathf.Max(
                            UI.EditorGUIUtility.FreeSlider(
                                new GUIContent("Inner Radius", "The distance from the center that stairs begin."),
                                size.z, 0f, 10f), 0f);

                    s_Size.value = size;
                }
                else
                {
                    size.x = UI.EditorGUIUtility.FreeSlider("Width", size.x, 0.01f, 10f);
                    size.y = UI.EditorGUIUtility.FreeSlider("Height", size.y, 0.01f, 10f);
                    size.z = UI.EditorGUIUtility.FreeSlider("Depth", size.z, 0.01f, 10f);
                }

                s_Size.value = size;
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                if (s_Circumference > 0f)
                {
                    return ShapeGenerator.GenerateCurvedStair(
                        EditorUtility.newShapePivotLocation,
                        s_Size.value.x,
                        s_Size.value.y,
                        s_Size.value.z,
                        s_Mirror ? -s_Circumference : s_Circumference,
                        s_Steps,
                        s_Sides);
                }

                return ShapeGenerator.GenerateStair(
                    EditorUtility.newShapePivotLocation,
                    s_Size,
                    s_Steps,
                    s_Sides);
            }
        }

        [Serializable]
        class Cylinder : ShapeBuilder
        {
            static Pref<int> s_AxisSegments = new Pref<int>("ShapeBuilder.Cylinder.s_AxisSegments", 8, SettingsScope.User);
            static Pref<float> s_Radius = new Pref<float>("ShapeBuilder.Cylinder.s_Radius", .5f, SettingsScope.User);
            static Pref<float> s_Height = new Pref<float>("ShapeBuilder.Cylinder.s_Height", 1f, SettingsScope.User);
            static Pref<int> s_HeightSegments = new Pref<int>("ShapeBuilder.Cylinder.s_HeightSegments", 0, SettingsScope.User);
            static Pref<bool> s_Smooth = new Pref<bool>("ShapeBuilder.Cylinder.s_Smooth", true, SettingsScope.User);

            public override void OnGUI()
            {
                s_Radius.value = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Radius.value = Mathf.Clamp(s_Radius, .01f, Mathf.Infinity);

                s_AxisSegments.value = EditorGUILayout.IntField("Number of Sides", s_AxisSegments);
                s_AxisSegments.value = UnityEngine.ProBuilder.Math.Clamp(s_AxisSegments, 4, 48);

                s_Height.value = EditorGUILayout.FloatField("Height", s_Height);

                s_HeightSegments.value = EditorGUILayout.IntField("Height Segments", s_HeightSegments);
                s_HeightSegments.value = UnityEngine.ProBuilder.Math.Clamp(s_HeightSegments, 0, 48);

                s_Smooth.value = EditorGUILayout.Toggle("Smooth", s_Smooth);

                if (s_AxisSegments % 2 != 0)
                    s_AxisSegments.value++;

                if (s_HeightSegments < 0)
                    s_HeightSegments.value = 0;
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateCylinder(
                    EditorUtility.newShapePivotLocation,
                    s_AxisSegments,
                    s_Radius,
                    s_Height,
                    s_HeightSegments,
                    s_Smooth ? 1 : -1);
            }
        }

        [Serializable]
        class Door : ShapeBuilder
        {
            static Pref<float> s_Width = new Pref<float>("ShapeBuilder.Door.s_Width", 4.0f, SettingsScope.User);
            static Pref<float> s_Height = new Pref<float>("ShapeBuilder.Door.s_Height", 4.0f, SettingsScope.User);
            static Pref<float> s_LedgeHeight = new Pref<float>("ShapeBuilder.Door.s_LedgeHeight", 1.0f, SettingsScope.User);
            static Pref<float> s_LegWidth = new Pref<float>("ShapeBuilder.Door.s_LegWidth", 1.0f, SettingsScope.User);
            static Pref<float> s_Depth = new Pref<float>("ShapeBuilder.Door.s_Depth", 0.5f, SettingsScope.User);

            public override void OnGUI()
            {
                s_Width.value = EditorGUILayout.FloatField("Total Width", s_Width);
                s_Width.value = Mathf.Clamp(s_Width, 1.0f, 500.0f);

                s_Height.value = EditorGUILayout.FloatField("Total Height", s_Height);
                s_Height.value = Mathf.Clamp(s_Height, 1.0f, 500.0f);

                s_Depth.value = EditorGUILayout.FloatField("Total Depth", s_Depth);
                s_Depth.value = Mathf.Clamp(s_Depth, 0.01f, 500.0f);

                s_LedgeHeight.value = EditorGUILayout.FloatField("Door Height", s_LedgeHeight);
                s_LedgeHeight.value = Mathf.Clamp(s_LedgeHeight, 0.01f, 500.0f);

                s_LegWidth.value = EditorGUILayout.FloatField("Leg Width", s_LegWidth);
                s_LegWidth.value = Mathf.Clamp(s_LegWidth, 0.01f, 2.0f);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateDoor(
                    EditorUtility.newShapePivotLocation,
                    s_Width,
                    s_Height,
                    s_LedgeHeight,
                    s_LegWidth,
                    s_Depth);
            }
        }

        [Serializable]
        class Plane : ShapeBuilder
        {
            static Pref<float> s_Height = new Pref<float>("ShapeBuilder.Plane.s_Height", 10, SettingsScope.User);
            static Pref<float> s_Width = new Pref<float>("ShapeBuilder.Plane.s_Width", 10, SettingsScope.User);
            static Pref<int> s_HeightSegments = new Pref<int>("ShapeBuilder.Plane.s_HeightSegments", 3, SettingsScope.User);
            static Pref<int> s_WidthSegments = new Pref<int>("ShapeBuilder.Plane.s_WidthSegments", 3, SettingsScope.User);
            static Pref<Axis> s_Axis = new Pref<Axis>("ShapeBuilder.Plane.s_Axis", Axis.Up, SettingsScope.User);

            public override void OnGUI()
            {
                s_Axis.value = (Axis)EditorGUILayout.EnumPopup("Axis", s_Axis);

                s_Width.value = EditorGUILayout.FloatField("Width", s_Width);
                s_Height.value = EditorGUILayout.FloatField("Length", s_Height);

                if (s_Height < 1f)
                    s_Height.value = 1f;

                if (s_Width < 1f)
                    s_Width.value = 1f;

                s_WidthSegments.value = EditorGUILayout.IntField("Width Segments", s_WidthSegments);
                s_HeightSegments.value = EditorGUILayout.IntField("Length Segments", s_HeightSegments);

                if (s_WidthSegments < 0)
                    s_WidthSegments.value = 0;

                if (s_HeightSegments < 0)
                    s_HeightSegments.value = 0;
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GeneratePlane(
                    EditorUtility.newShapePivotLocation,
                    s_Height,
                    s_Width,
                    s_HeightSegments,
                    s_WidthSegments,
                    s_Axis);
            }
        }

        [Serializable]
        class Pipe : ShapeBuilder
        {
            static Pref<float> s_Radius = new Pref<float>("ShapeBuilder.Pipe.s_Radius", 1f, SettingsScope.User);
            static Pref<float> s_Height = new Pref<float>("ShapeBuilder.Pipe.s_Height", 2f, SettingsScope.User);
            static Pref<float> s_Thickness = new Pref<float>("ShapeBuilder.Pipe.s_Thickness", .2f, SettingsScope.User);
            static Pref<int> s_AxisSegments = new Pref<int>("ShapeBuilder.Pipe.s_AxisSegments", 6, SettingsScope.User);
            static Pref<int> s_HeightSegments = new Pref<int>("ShapeBuilder.Pipe.s_HeightSegments", 1, SettingsScope.User);

            public override void OnGUI()
            {
                s_Radius.value = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Height.value = EditorGUILayout.FloatField("Height", s_Height);
                s_Thickness.value = EditorGUILayout.FloatField("Thickness", s_Thickness);
                s_AxisSegments.value = EditorGUILayout.IntField("Number of Sides", s_AxisSegments);
                s_HeightSegments.value = EditorGUILayout.IntField("Height Segments", s_HeightSegments);

                if (s_Radius < .1f)
                    s_Radius.value = .1f;

                if (s_Height < .1f)
                    s_Height.value = .1f;

                s_HeightSegments.value = (int)Mathf.Clamp(s_HeightSegments, 0f, 32f);
                s_Thickness.value = Mathf.Clamp(s_Thickness, .01f, s_Radius - .01f);
                s_AxisSegments.value = (int)Mathf.Clamp(s_AxisSegments, 3f, 32f);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GeneratePipe(
                    EditorUtility.newShapePivotLocation,
                    s_Radius,
                    s_Height,
                    s_Thickness,
                    s_AxisSegments,
                    s_HeightSegments
                    );
            }
        }

        [Serializable]
        class Cone : ShapeBuilder
        {
            static Pref<float> s_Radius = new Pref<float>("ShapeBuilder.Cone.s_Radius", 1f, SettingsScope.User);
            static Pref<float> s_Height = new Pref<float>("ShapeBuilder.Cone.s_Height", 2f, SettingsScope.User);
            static Pref<int> s_AxisSegments = new Pref<int>("ShapeBuilder.Cone.s_AxisSegments", 6, SettingsScope.User);

            public override void OnGUI()
            {
                s_Radius.value = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Height.value = EditorGUILayout.FloatField("Height", s_Height);
                s_AxisSegments.value = EditorGUILayout.IntField("Number of Sides", s_AxisSegments);

                if (s_Radius < .1f)
                    s_Radius.value = .1f;

                if (s_Height < .1f)
                    s_Height.value = .1f;

                s_AxisSegments.value = (int)Mathf.Clamp(s_AxisSegments, 3f, 32f);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateCone(
                    EditorUtility.newShapePivotLocation,
                    s_Radius,
                    s_Height,
                    s_AxisSegments
                    );
            }
        }

        [Serializable]
        class Arch : ShapeBuilder
        {
            static Pref<float> s_Angle = new Pref<float>("ShapeBuilder.Arch.s_Angle", 180.0f, SettingsScope.User);
            static Pref<float> s_Radius = new Pref<float>("ShapeBuilder.Arch.s_Radius", 3.0f, SettingsScope.User);
            static Pref<float> s_Width = new Pref<float>("ShapeBuilder.Arch.s_Width", 0.50f, SettingsScope.User);
            static Pref<float> s_Depth = new Pref<float>("ShapeBuilder.Arch.s_Depth", 1f, SettingsScope.User);
            static Pref<int> s_RadiusSegments = new Pref<int>("ShapeBuilder.Arch.s_RadiusSegments", 6, SettingsScope.User);
            static Pref<bool> s_EndCaps = new Pref<bool>("ShapeBuilder.Arch.s_EndCaps", true, SettingsScope.User);

            const bool k_InsideFaces = true;
            const bool k_OutsideFaces = true;
            const bool k_FrontFaces = true;
            const bool k_BackFaces = true;

            public override void OnGUI()
            {
                s_Radius.value = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Radius.value = s_Radius <= 0f ? .01f : s_Radius;

                s_Width.value = EditorGUILayout.FloatField("Thickness", s_Width);
                s_Width.value = Mathf.Clamp(s_Width, 0.01f, 100f);

                s_Depth.value = EditorGUILayout.FloatField("Depth", s_Depth);
                s_Depth.value = Mathf.Clamp(s_Depth, 0.1f, 500.0f);

                s_RadiusSegments.value = EditorGUILayout.IntField("Number of Sides", s_RadiusSegments);
                s_RadiusSegments.value = Mathf.Clamp(s_RadiusSegments, 2, 200);

                s_Angle.value = EditorGUILayout.FloatField("Arch Degrees", s_Angle);
                s_Angle.value = Mathf.Clamp(s_Angle, 0.0f, 360.0f);

                if (s_Angle < 360f)
                    s_EndCaps.value = EditorGUILayout.Toggle("End Caps", s_EndCaps);

                if (s_Angle > 180f)
                    s_RadiusSegments.value = System.Math.Max(3, s_RadiusSegments);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateArch(
                    EditorUtility.newShapePivotLocation,
                    s_Angle,
                    s_Radius,
                    Mathf.Clamp(s_Width, 0.01f, s_Radius),
                    s_Depth,
                    s_RadiusSegments + 1,
                    k_InsideFaces,
                    k_OutsideFaces,
                    k_FrontFaces,
                    k_BackFaces,
                    s_EndCaps);
            }
        }

        [Serializable]
        class Sphere : ShapeBuilder
        {
            static Pref<float> s_Radius = new Pref<float>("ShapeBuilder.Sphere.s_Radius", 1f, SettingsScope.User);
            static Pref<int> s_Subdivisions =new Pref<int>("ShapeBuilder.Sphere.s_Subdivisions",  1, SettingsScope.User);

            public override void OnGUI()
            {
                s_Radius.value = EditorGUILayout.Slider("Radius", s_Radius, 0.01f, 10f);
                s_Subdivisions.value = (int) EditorGUILayout.Slider("Subdivisions", s_Subdivisions, 0, 4);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                // To keep the preview snappy, shared indexes aren't built in IcosahadreonGenerator
                var mesh = ShapeGenerator.GenerateIcosahedron(
                        EditorUtility.newShapePivotLocation,
                        s_Radius,
                        s_Subdivisions,
                        !preview);

                if (!preview)
                    UVEditing.ProjectFacesBox(mesh, mesh.facesInternal);

                for (int i = 0; i < mesh.facesInternal.Length; i++)
                    mesh.facesInternal[i].manualUV = true;

                return mesh;
            }
        }

        [Serializable]
        class Torus : ShapeBuilder
        {
            static Pref<float> s_Radius = new Pref<float>("ShapeBuilder.Torus.s_Radius", 1f, SettingsScope.User);
            static Pref<float> s_TubeRadius = new Pref<float>("ShapeBuilder.Torus.s_TubeRadius", .3f, SettingsScope.User);
            static Pref<int> s_Rows = new Pref<int>("ShapeBuilder.Torus.s_Rows", 16, SettingsScope.User);
            static Pref<int> s_Columns = new Pref<int>("ShapeBuilder.Torus.s_Columns", 24, SettingsScope.User);
            static Pref<bool> s_Smooth = new Pref<bool>("ShapeBuilder.Torus.s_Smooth", true, SettingsScope.User);
            static Pref<float> s_HorizontalCircumference = new Pref<float>("ShapeBuilder.Torus.s_HorizontalCircumference", 360f, SettingsScope.User);
            static Pref<float> s_VerticalCircumference = new Pref<float>("ShapeBuilder.Torus.s_VerticalCircumference", 360f, SettingsScope.User);
            static Pref<Vector2> s_InnerOuter = new Pref<Vector2>("ShapeBuilder.Torus.s_InnerOuter", new Vector2(1f, .7f), SettingsScope.User);
            static Pref<bool> s_UseInnerOuterMethod = new Pref<bool>("shape.torusDefinesInnerOuter", false, SettingsScope.User);

            public override void OnGUI()
            {
                s_Rows.value = (int)EditorGUILayout.IntSlider(
                        new GUIContent("Rows", "How many rows the torus will have.  More equates to smoother geometry."),
                        s_Rows, 3, 32);
                s_Columns.value = (int)EditorGUILayout.IntSlider(
                        new GUIContent("Columns",
                            "How many columns the torus will have.  More equates to smoother geometry."), s_Columns, 3, 64);

                s_UseInnerOuterMethod.value = EditorGUILayout.Toggle("Define Inner / Out Radius", s_UseInnerOuterMethod);

                if (!s_UseInnerOuterMethod)
                {
                    s_Radius.value = EditorGUILayout.FloatField("Radius", s_Radius);

                    if (s_Radius < .001f)
                        s_Radius.value = .001f;

                    s_TubeRadius.value = UI.EditorGUIUtility.Slider(new GUIContent("Tube Radius", "How thick the donut will be."), s_TubeRadius, .01f, s_Radius);
                }
                else
                {
                    Vector2 innerOuter = s_InnerOuter;
                    innerOuter.x = s_Radius;
                    innerOuter.y = s_Radius - (s_TubeRadius * 2f);

                    innerOuter.x = EditorGUILayout.FloatField("Outer Radius", innerOuter.x);
                    innerOuter.y = UI.EditorGUIUtility.Slider(new GUIContent("Inner Radius", "Distance from center to inside of donut ring."), innerOuter.y, .001f, innerOuter.x);

                    s_Radius.value = innerOuter.x;
                    s_TubeRadius.value = (innerOuter.x - innerOuter.y) * .5f;
                    s_InnerOuter.value = innerOuter;
                }

                s_HorizontalCircumference.value = EditorGUILayout.Slider("Horizontal Circumference", s_HorizontalCircumference, .01f, 360f);
                s_VerticalCircumference.value = EditorGUILayout.Slider("Vertical Circumference", s_VerticalCircumference, .01f, 360f);

                s_Smooth.value = EditorGUILayout.Toggle("Smooth", s_Smooth);
            }

            public override ProBuilderMesh Build(bool isPreview = false)
            {
                var mesh = ShapeGenerator.GenerateTorus(
                        EditorUtility.newShapePivotLocation,
                        s_Rows,
                        s_Columns,
                        s_Radius,
                        s_TubeRadius,
                        s_Smooth,
                        s_HorizontalCircumference,
                        s_VerticalCircumference,
                        true);

                UVEditing.ProjectFacesBox(mesh, mesh.facesInternal);

                return mesh;
            }
        }

        [Serializable]
        class Custom : ShapeBuilder
        {
            static Pref<Vector2> scrollbar = new Pref<Vector2>("ShapeBuilder.Custom.scrollbar", new Vector2(0f, 0f), SettingsScope.User);
            static Pref<string> verts = new Pref<string>("ShapeBuilder.Custom.verts", "//Vertical Plane\n0, 0, 0\n1, 0, 0\n0, 1, 0\n1, 1, 0\n", SettingsScope.User);

            public override void OnGUI()
            {
                GUILayout.Label("Custom Geometry", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Vertices must be wound in faces, and counter-clockwise.\n(Think horizontally reversed Z)", MessageType.Info);

                scrollbar.value = GUILayout.BeginScrollView(scrollbar);
                verts.value = EditorGUILayout.TextArea(verts, GUILayout.MinHeight(160));
                GUILayout.EndScrollView();
            }

            public override ProBuilderMesh Build(bool isPreview = false)
            {
                var positions = InternalUtility.StringToVector3Array(verts);

                if (positions.Length % 4 == 0)
                    return ProBuilderMesh.CreateInstanceWithPoints(
                        InternalUtility.StringToVector3Array(verts)
                        );

                return ProBuilderMesh.Create();
            }
        }
    }
}
