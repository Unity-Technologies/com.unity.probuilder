using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Shape creation panel implementation.
    /// </summary>
    sealed class ShapeEditor : ConfigurableWindow
    {
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
        static int s_CurrentIndex = 0;
        GameObject m_PreviewObject;

        [UserSetting("Toolbar", "Close Shape Window after Build", "When true the shape window will close after hitting the build button.")]
        static Pref<bool> s_CloseWindowAfterCreateShape = new Pref<bool>("editor.closeWindowAfterShapeCreation", false);

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
            m_ShapeTypes = m_ShapeBuilders.Select(x => x.name).ToArray();

            SetPreviewMesh(m_ShapeBuilders[s_CurrentIndex].Build());
        }

        void OnDestroy()
        {
            DestroyPreviewObject();
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

            s_CurrentIndex = EditorGUILayout.Popup(s_CurrentIndex, m_ShapeTypes);

            GUILayout.Label("Shape Settings", EditorStyles.boldLabel);

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            var shape = m_ShapeBuilders[s_CurrentIndex];

            shape.OnGUI();

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
                SetPreviewMesh(shape.Build());

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Build"))
                CreateSelectedShape();
        }

        void CreateSelectedShape(bool forceCloseWindow = false)
        {
            var res = m_ShapeBuilders[s_CurrentIndex].Build();
            EditorUtility.InitObject(res);
            ApplyPreviewTransform(res);
            DestroyPreviewObject();

            Undo.RegisterCreatedObjectUndo(res.gameObject, "Create Shape");

            if (forceCloseWindow || s_CloseWindowAfterCreateShape)
                Close();
        }

        void DestroyPreviewObject()
        {
            if (m_PreviewObject != null)
            {
                if (m_PreviewObject.GetComponent<MeshFilter>().sharedMesh != null)
                    DestroyImmediate(m_PreviewObject.GetComponent<MeshFilter>().sharedMesh);

                DestroyImmediate(m_PreviewObject);
            }
        }

        void SetPreviewMesh(ProBuilderMesh mesh)
        {
            ApplyPreviewTransform(mesh);

            DestroyPreviewObject();

            mesh.selectable = false;
            m_PreviewObject = mesh.gameObject;

            mesh.preserveMeshAssetOnDestroy = true;
            var umesh = mesh.mesh;
            DestroyImmediate(mesh);

            umesh.hideFlags = HideFlags.DontSave;
            m_PreviewObject.hideFlags = HideFlags.DontSave;
            m_PreviewObject.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.ShapePreviewMaterial;

            Selection.activeTransform = m_PreviewObject.transform;
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
            }

            if (previous)
            {
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

        class Cube : ShapeBuilder
        {
            static Vector3 s_CubeSize = Vector3.one;

            public override void OnGUI()
            {
                s_CubeSize = EditorGUILayout.Vector3Field("Size", s_CubeSize);

                if (s_CubeSize.x <= 0)
                    s_CubeSize.x = .01f;
                if (s_CubeSize.y <= 0)
                    s_CubeSize.y = .01f;
                if (s_CubeSize.z <= 0)
                    s_CubeSize.z = .01f;
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateCube(EditorUtility.newShapePivotLocation, s_CubeSize);
            }
        }

        class Sprite : ShapeBuilder
        {
            static Axis s_Axis = Axis.Forward;

            public override void OnGUI()
            {
                s_Axis = (Axis)EditorGUILayout.EnumPopup("Axis", s_Axis);
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GeneratePlane(EditorUtility.newShapePivotLocation, 1, 1, 0, 0, s_Axis);
            }
        }

        class Prism : ShapeBuilder
        {
            static Vector3 s_PrismSize = Vector3.one;

            public override void OnGUI()
            {
                s_PrismSize = EditorGUILayout.Vector3Field("Size", s_PrismSize);

                if (s_PrismSize.x < 0) s_PrismSize.x = 0.01f;
                if (s_PrismSize.y < 0) s_PrismSize.y = 0.01f;
                if (s_PrismSize.z < 0) s_PrismSize.z = 0.01f;
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GeneratePrism(EditorUtility.newShapePivotLocation, s_PrismSize);
            }
        }

        class Stair : ShapeBuilder
        {
            static int s_Steps = 6;
            static Vector3 s_Size = new Vector3(2f, 2.5f, 4f);
            static float s_Circumference = 0f;
            static bool s_Sides = true;
            static bool s_Mirror = false;

            public override void OnGUI()
            {
                s_Steps = (int)Mathf.Max(UI.EditorGUIUtility.FreeSlider("Steps", s_Steps, 2, 64), 2);

                s_Sides = EditorGUILayout.Toggle("Build Sides", s_Sides);

                s_Circumference = EditorGUILayout.Slider("Curvature", s_Circumference, 0f, 360f);

                if (s_Circumference > 0f)
                {
                    s_Mirror = EditorGUILayout.Toggle("Mirror", s_Mirror);

                    s_Size.x =
                        Mathf.Max(
                            UI.EditorGUIUtility.FreeSlider(
                                new GUIContent("Stair Width", "The width of an individual stair step."), s_Size.x,
                                .01f, 10f), .01f);
                    s_Size.y =
                        Mathf.Max(
                            UI.EditorGUIUtility.FreeSlider(
                                new GUIContent("Stair Height",
                                    "The total height of this staircase.  You may enter any value in the float field."),
                                s_Size.y, .01f, 10f), .01f);
                    s_Size.z =
                        Mathf.Max(
                            UI.EditorGUIUtility.FreeSlider(
                                new GUIContent("Inner Radius", "The distance from the center that stairs begin."),
                                s_Size.z, 0f, 10f), 0f);
                }
                else
                {
                    s_Size.x = UI.EditorGUIUtility.FreeSlider("Width", s_Size.x, 0.01f, 10f);
                    s_Size.y = UI.EditorGUIUtility.FreeSlider("Height", s_Size.y, 0.01f, 10f);
                    s_Size.z = UI.EditorGUIUtility.FreeSlider("Depth", s_Size.z, 0.01f, 10f);
                }
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                if (s_Circumference > 0f)
                {
                    return ShapeGenerator.GenerateCurvedStair(
                        EditorUtility.newShapePivotLocation,
                        s_Size.x,
                        s_Size.y,
                        s_Size.z,
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

        class Cylinder : ShapeBuilder
        {
            static int s_AxisSegments = 8;
            static float s_Radius = .5f;
            static float s_Height = 1f;
            static int s_HeighSegments = 0;
            static bool s_Smooth = true;

            public override void OnGUI()
            {
                s_Radius = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Radius = Mathf.Clamp(s_Radius, .01f, Mathf.Infinity);

                s_AxisSegments = EditorGUILayout.IntField("Number of Sides", s_AxisSegments);
                s_AxisSegments = UnityEngine.ProBuilder.Math.Clamp(s_AxisSegments, 4, 48);

                s_Height = EditorGUILayout.FloatField("Height", s_Height);

                s_HeighSegments = EditorGUILayout.IntField("Height Segments", s_HeighSegments);
                s_HeighSegments = UnityEngine.ProBuilder.Math.Clamp(s_HeighSegments, 0, 48);

                s_Smooth = EditorGUILayout.Toggle("Smooth", s_Smooth);

                if (s_AxisSegments % 2 != 0)
                    s_AxisSegments++;

                if (s_HeighSegments < 0)
                    s_HeighSegments = 0;
            }

            public override ProBuilderMesh Build(bool preview = false)
            {
                return ShapeGenerator.GenerateCylinder(
                    EditorUtility.newShapePivotLocation,
                    s_AxisSegments,
                    s_Radius,
                    s_Height,
                    s_HeighSegments,
                    s_Smooth ? 1 : -1);
            }
        }

        class Door : ShapeBuilder
        {
            static float s_Width = 4.0f;
            static float s_Height = 4.0f;
            static float s_LedgeHeight = 1.0f;
            static float s_LegWidth = 1.0f;
            static float s_Depth = 0.5f;

            public override void OnGUI()
            {
                s_Width = EditorGUILayout.FloatField("Total Width", s_Width);
                s_Width = Mathf.Clamp(s_Width, 1.0f, 500.0f);

                s_Height = EditorGUILayout.FloatField("Total Height", s_Height);
                s_Height = Mathf.Clamp(s_Height, 1.0f, 500.0f);

                s_Depth = EditorGUILayout.FloatField("Total Depth", s_Depth);
                s_Depth = Mathf.Clamp(s_Depth, 0.01f, 500.0f);

                s_LedgeHeight = EditorGUILayout.FloatField("Door Height", s_LedgeHeight);
                s_LedgeHeight = Mathf.Clamp(s_LedgeHeight, 0.01f, 500.0f);

                s_LegWidth = EditorGUILayout.FloatField("Leg Width", s_LegWidth);
                s_LegWidth = Mathf.Clamp(s_LegWidth, 0.01f, 2.0f);
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

        class Plane : ShapeBuilder
        {
            static float s_Height = 10;
            static float s_Width = 10;
            static int s_HeightSegments = 3;
            static int s_WidthSegments = 3;
            static Axis s_Axis = Axis.Up;

            public override void OnGUI()
            {
                s_Axis = (Axis)EditorGUILayout.EnumPopup("Axis", s_Axis);

                s_Width = EditorGUILayout.FloatField("Width", s_Width);
                s_Height = EditorGUILayout.FloatField("Length", s_Height);

                if (s_Height < 1f)
                    s_Height = 1f;

                if (s_Width < 1f)
                    s_Width = 1f;

                s_WidthSegments = EditorGUILayout.IntField("Width Segments", s_WidthSegments);
                s_HeightSegments = EditorGUILayout.IntField("Length Segments", s_HeightSegments);

                if (s_WidthSegments < 0)
                    s_WidthSegments = 0;

                if (s_HeightSegments < 0)
                    s_HeightSegments = 0;
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

        class Pipe : ShapeBuilder
        {
            static float s_Radius = 1f;
            static float s_Height = 2f;
            static float s_Thickness = .2f;
            static int s_AxisSegments = 6;
            static int s_HeightSegments = 1;

            public override void OnGUI()
            {
                s_Radius = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Height = EditorGUILayout.FloatField("Height", s_Height);
                s_Thickness = EditorGUILayout.FloatField("Thickness", s_Thickness);
                s_AxisSegments = EditorGUILayout.IntField("Number of Sides", s_AxisSegments);
                s_HeightSegments = EditorGUILayout.IntField("Height Segments", s_HeightSegments);

                if (s_Radius < .1f)
                    s_Radius = .1f;

                if (s_Height < .1f)
                    s_Height = .1f;

                s_HeightSegments = (int)Mathf.Clamp(s_HeightSegments, 0f, 32f);
                s_Thickness = Mathf.Clamp(s_Thickness, .01f, s_Radius - .01f);
                s_AxisSegments = (int)Mathf.Clamp(s_AxisSegments, 3f, 32f);
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

        class Cone : ShapeBuilder
        {
            static float s_Radius = 1f;
            static float s_Height = 2f;
            static int s_AxisSegments = 6;

            public override void OnGUI()
            {
                s_Radius = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Height = EditorGUILayout.FloatField("Height", s_Height);
                s_AxisSegments = EditorGUILayout.IntField("Number of Sides", s_AxisSegments);

                if (s_Radius < .1f)
                    s_Radius = .1f;

                if (s_Height < .1f)
                    s_Height = .1f;

                s_AxisSegments = (int)Mathf.Clamp(s_AxisSegments, 3f, 32f);
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

        class Arch : ShapeBuilder
        {
            static float s_Angle = 180.0f;
            static float s_Radius = 3.0f;
            static float s_Width = 0.50f;
            static float s_Depth = 1f;
            static int s_RadiusSegments = 6;
            static bool s_EndCaps = true;
            const bool k_InsideFaces = true;
            const bool k_OutsideFaces = true;
            const bool k_FrontFaces = true;
            const bool k_BackFaces = true;

            public override void OnGUI()
            {
                s_Radius = EditorGUILayout.FloatField("Radius", s_Radius);
                s_Radius = s_Radius <= 0f ? .01f : s_Radius;

                s_Width = EditorGUILayout.FloatField("Thickness", s_Width);
                s_Width = Mathf.Clamp(s_Width, 0.01f, 100f);

                s_Depth = EditorGUILayout.FloatField("Depth", s_Depth);
                s_Depth = Mathf.Clamp(s_Depth, 0.1f, 500.0f);

                s_RadiusSegments = EditorGUILayout.IntField("Number of Sides", s_RadiusSegments);
                s_RadiusSegments = Mathf.Clamp(s_RadiusSegments, 2, 200);

                s_Angle = EditorGUILayout.FloatField("Arch Degrees", s_Angle);
                s_Angle = Mathf.Clamp(s_Angle, 0.0f, 360.0f);

                if (s_Angle < 360f)
                    s_EndCaps = EditorGUILayout.Toggle("End Caps", s_EndCaps);

                if (s_Angle > 180f)
                    s_RadiusSegments = System.Math.Max(3, s_RadiusSegments);
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

        class Sphere : ShapeBuilder
        {
            static float s_Radius = 1f;
            static int s_Subdivisions = 1;

            public override void OnGUI()
            {
                s_Radius = EditorGUILayout.Slider("Radius", s_Radius, 0.01f, 10f);
                s_Subdivisions = (int)EditorGUILayout.Slider("Subdivisions", s_Subdivisions, 0, 4);
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

        class Torus : ShapeBuilder
        {
            static float s_Radius = 1f;
            static float s_TubeRadius = .3f;
            static int s_Rows = 16;
            static int s_Columns = 24;
            static bool s_Smooth = true;
            static float s_HorizontalCirumference = 360f;
            static float s_VerticalCircumference = 360f;
            static Vector2 s_InnerOuter = new Vector2(1f, .7f);

            static Pref<bool> s_UseInnerOuterMethod =
                new Pref<bool>("shape.torusDefinesInnerOuter", false, SettingsScope.User);

            public override void OnGUI()
            {
                s_Rows = (int)EditorGUILayout.IntSlider(
                        new GUIContent("Rows", "How many rows the torus will have.  More equates to smoother geometry."),
                        s_Rows, 3, 32);
                s_Columns = (int)EditorGUILayout.IntSlider(
                        new GUIContent("Columns",
                            "How many columns the torus will have.  More equates to smoother geometry."), s_Columns, 3, 64);

                s_UseInnerOuterMethod.value =
                    EditorGUILayout.Toggle("Define Inner / Out Radius", s_UseInnerOuterMethod);

                if (!s_UseInnerOuterMethod)
                {
                    s_Radius = EditorGUILayout.FloatField("Radius", s_Radius);

                    if (s_Radius < .001f)
                        s_Radius = .001f;

                    s_TubeRadius = UI.EditorGUIUtility.Slider(
                            new GUIContent("Tube Radius", "How thick the donut will be."), s_TubeRadius, .01f, s_Radius);
                }
                else
                {
                    s_InnerOuter.x = s_Radius;
                    s_InnerOuter.y = s_Radius - (s_TubeRadius * 2f);

                    s_InnerOuter.x = EditorGUILayout.FloatField("Outer Radius", s_InnerOuter.x);
                    s_InnerOuter.y = UI.EditorGUIUtility.Slider(
                            new GUIContent("Inner Radius", "Distance from center to inside of donut ring."), s_InnerOuter.y,
                            .001f, s_InnerOuter.x);

                    s_Radius = s_InnerOuter.x;
                    s_TubeRadius = (s_InnerOuter.x - s_InnerOuter.y) * .5f;
                }

                s_HorizontalCirumference =
                    EditorGUILayout.Slider("Horizontal Circumference", s_HorizontalCirumference, .01f, 360f);
                s_VerticalCircumference =
                    EditorGUILayout.Slider("Vertical Circumference", s_VerticalCircumference, .01f, 360f);

                s_Smooth = EditorGUILayout.Toggle("Smooth", s_Smooth);
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
                        s_HorizontalCirumference,
                        s_VerticalCircumference,
                        true);

                UVEditing.ProjectFacesBox(mesh, mesh.facesInternal);

                return mesh;
            }
        }

        class Custom : ShapeBuilder
        {
            static Vector2 scrollbar = new Vector2(0f, 0f);
            static string verts = "//Vertical Plane\n0, 0, 0\n1, 0, 0\n0, 1, 0\n1, 1, 0\n";

            public override void OnGUI()
            {
                GUILayout.Label("Custom Geometry", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Vertices must be wound in faces, and counter-clockwise.\n(Think horizontally reversed Z)", MessageType.Info);

                scrollbar = GUILayout.BeginScrollView(scrollbar);
                verts = EditorGUILayout.TextArea(verts, GUILayout.MinHeight(160));
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
