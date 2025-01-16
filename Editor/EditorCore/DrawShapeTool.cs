using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using UObject = UnityEngine.Object;
using Plane = UnityEngine.ProBuilder.Shapes.Plane;
using Sprite = UnityEngine.ProBuilder.Shapes.Sprite;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder
{

    [EditorTool("Create Cube", variantGroup = typeof(DrawShapeTool), variantPriority = 0)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Cube.png")]
    class CreateCubeTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Cube  %#K", false, PreferenceKeys.menuEditor + 1)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateCubeTool>();
            ProBuilderAnalytics.SendActionEvent("New Cube Shape", nameof(CreateCubeTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Cube", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Cube", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Cube", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Cube))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Cube));
            base.OnActivated();
        }
    }

    [EditorTool("Create Sphere",variantGroup = typeof(DrawShapeTool), variantPriority = 1)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Sphere.png")]
    class CreateSphereTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Sphere", false, PreferenceKeys.menuEditor + 2)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateSphereTool>();
            ProBuilderAnalytics.SendActionEvent("New Sphere Shape", nameof(CreateSphereTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Sphere", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Sphere", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Sphere", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Sphere))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Sphere));
            base.OnActivated();
        }
    }

    [EditorTool("Create Plane", variantGroup = typeof(DrawShapeTool), variantPriority = 2)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Plane.png")]
    class CreatePlaneTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Plane", false, PreferenceKeys.menuEditor + 3)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreatePlaneTool>();
            ProBuilderAnalytics.SendActionEvent("New Plane Shape", nameof(CreatePlaneTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Plane", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Plane", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Plane", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Plane))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Plane));
            base.OnActivated();
        }
    }

    [EditorTool("Create Cylinder",variantGroup = typeof(DrawShapeTool), variantPriority = 3)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Cylinder.png")]
    class CreateCylinderTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Cylinder", false, PreferenceKeys.menuEditor + 4)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateCylinderTool>();
            ProBuilderAnalytics.SendActionEvent("New Cylinder Shape", nameof(CreateCylinderTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Cylinder", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Cylinder", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Cylinder", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Cylinder))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Cylinder));
            base.OnActivated();
        }
    }

    [EditorTool("Create Cone", variantGroup = typeof(DrawShapeTool), variantPriority = 4)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Cone.png")]
    class CreateConeTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Cone", false, PreferenceKeys.menuEditor + 5)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateConeTool>();
            ProBuilderAnalytics.SendActionEvent("New Cone Shape", nameof(CreateConeTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Cone", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Cone", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Cone", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Cone))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Cone));
            base.OnActivated();
        }
    }

    [EditorTool("Create Prism",variantGroup = typeof(DrawShapeTool), variantPriority = 5)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Prism.png")]
    class CreatePrismTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Prism", false, PreferenceKeys.menuEditor + 6)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreatePrismTool>();
            ProBuilderAnalytics.SendActionEvent("New Prism Shape", nameof(CreatePrismTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Prism", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Prism", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Prism", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Prism))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Prism));
            base.OnActivated();
        }
    }

    [EditorTool("Create Stairs", variantGroup = typeof(DrawShapeTool), variantPriority = 6)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Stairs.png")]
    class CreateStairsTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Stairs", false, PreferenceKeys.menuEditor + 7)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateStairsTool>();
            ProBuilderAnalytics.SendActionEvent("New Stairs Shape", nameof(CreateStairsTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Stairs", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Stairs", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Stairs", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Stairs))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Stairs));
            base.OnActivated();
        }
    }

    [EditorTool("Create Torus",variantGroup = typeof(DrawShapeTool), variantPriority = 7)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Torus.png")]
    class CreateTorusTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Torus", false, PreferenceKeys.menuEditor + 8)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateTorusTool>();
            ProBuilderAnalytics.SendActionEvent("New Torus Shape", nameof(CreateTorusTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Torus", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Torus", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Torus", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Torus))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Torus));
            base.OnActivated();
        }
    }

    [EditorTool("Create Pipe",variantGroup = typeof(DrawShapeTool), variantPriority = 8)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Pipe.png")]
    class CreatePipeTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Pipe", false, PreferenceKeys.menuEditor + 9)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreatePipeTool>();
            ProBuilderAnalytics.SendActionEvent("New Pipe Shape", nameof(CreatePipeTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Pipe", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Pipe", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Pipe", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Pipe))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Pipe));
            base.OnActivated();
        }
    }

    [EditorTool("Create Arch",variantGroup = typeof(DrawShapeTool), variantPriority = 9)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Arch.png")]
    class CreateArchTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Arch", false, PreferenceKeys.menuEditor + 10)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateArchTool>();
            ProBuilderAnalytics.SendActionEvent("New Arch Shape", nameof(CreateArchTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Arch", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Arch", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Arch", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Arch))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Arch));
            base.OnActivated();
        }
    }

    [EditorTool("Create Door",variantGroup = typeof(DrawShapeTool), variantPriority = 10)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Door.png")]
    class CreateDoorTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Door", false, PreferenceKeys.menuEditor + 11)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateDoorTool>();
            ProBuilderAnalytics.SendActionEvent("New Door Shape", nameof(CreateDoorTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Door", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Door", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Door", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Door))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Door));
            base.OnActivated();
        }
    }

    [EditorTool("Create Sprite",variantGroup = typeof(DrawShapeTool), variantPriority = 11)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Tools/ShapeTool/Sprite.png")]
    class CreateSpriteTool : DrawShapeTool
    {
        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create Shape/Sprite", false, PreferenceKeys.menuEditor + 12)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<CreateSpriteTool>();
            ProBuilderAnalytics.SendActionEvent("New Sprite Shape", nameof(CreateSpriteTool));
        }

        Pref<Vector3> m_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize.Sprite", Vector3.one);
        protected override Vector3 lastSize { get => m_LastSize.value; set => m_LastSize.SetValue(value); }

        Pref<Quaternion> m_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation.Sprite", Quaternion.identity);
        protected override Quaternion lastRotation { get => m_LastRotation.value; set => m_LastRotation.SetValue(value); }

        Pref<PivotLocation> m_PivotLocation = new Pref<PivotLocation>("ShapeBuilder.PivotLocation.Sprite", PivotLocation.Center);
        protected override PivotLocation shapePivotLocation { get => m_PivotLocation.value; set => m_PivotLocation.SetValue(value); }

        public override void OnActivated()
        {
            if (m_LastShapeCreated && !(m_LastShapeCreated.shape is Sprite))
                m_LastShapeCreated = null;
            s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, typeof(Sprite));
            base.OnActivated();
        }
    }

    abstract class DrawShapeTool : EditorTool
    {
        internal const int k_MinOverlayWidth = 300;

        ShapeState m_CurrentState;

        protected internal ProBuilderShape m_LastShapeCreated = null;

        internal ProBuilderShape m_ProBuilderShape;
        internal bool m_IsShapeInit;

        Editor m_ShapeEditor;

        bool m_HandleSelectionChanges = false;

        internal bool handleSelectionChange
        {
            set
            {
                if(m_HandleSelectionChanges == value)
                    return;

                m_HandleSelectionChanges = value;
                if(value)
                    MeshSelection.objectSelectionChanged += OnSelectionChanged;
                else
                    MeshSelection.objectSelectionChanged -= OnSelectionChanged;

            }
        }

        // Plane of interaction
        internal UnityEngine.Plane m_Plane;
        internal Vector3 m_PlaneForward, m_PlaneRight;
        internal Quaternion m_PlaneRotation;
        internal Vector3 m_BB_Origin, m_BB_OppositeCorner, m_BB_HeightCorner;

        // Shape's duplicate
        internal GameObject m_DuplicateGO = null;

        //Shape's properties
        internal bool m_IsOnGrid;

        internal Bounds m_Bounds;
        static readonly Color k_BoundsColor = new Color(.2f, .4f, .8f, 1f);

        static readonly GUIContent k_ShapeTitle = new GUIContent("Shape Settings");

        internal static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);
        public static Pref<bool> s_SettingsEnabled = new Pref<bool>("ShapeComponent.SettingsEnabled", false, SettingsScope.Project);


        protected abstract Vector3 lastSize { get; set; }
        protected abstract Quaternion lastRotation { get; set; }
        protected abstract PivotLocation shapePivotLocation { get; set; }

        public PivotLocation pivotLocation
        {
            get => shapePivotLocation;
            set
            {
                if (value != shapePivotLocation)
                {
                    if (instance != null && instance.m_LastShapeCreated != null && instance.currentShapeInOverlay == instance.m_LastShapeCreated)
                    {
                        var lastShape = instance.m_LastShapeCreated;
                        var lastShapeTrs = lastShape.transform;
                        var newPivotPosition = lastShape.shapeWorldCenter;
                        if (value != PivotLocation.Center)
                            newPivotPosition += instance.m_PlaneRotation * m_LastNonDuplicateCenterToOrigin;

                        instance.m_LastShapeCreated.Rebuild(newPivotPosition, lastShapeTrs.rotation, lastShape.shapeWorldBounds);
                    }
                    shapePivotLocation = value;
                }
            }
        }

        internal Vector3 m_LastNonDuplicateCenterToOrigin;
        PivotLocation m_LastPreviewPivotLocation;

        internal Vector3 previewPivotPosition
        {
            get
            {
                if (pivotLocation == PivotLocation.FirstVertex && instance != null)
                {
                    var lastCenterToOrigin = instance.m_LastNonDuplicateCenterToOrigin;
                    var lastCenterToOriginNorm = lastCenterToOrigin.normalized;

                    var deltaRot = instance.m_PlaneRotation;
                    lastCenterToOriginNorm = deltaRot * lastCenterToOriginNorm;

                    var pivotOffset = lastCenterToOriginNorm * lastCenterToOrigin.magnitude;
                    return pivotOffset + instance.m_Bounds.center;
                }

                return m_Bounds.center;
            }
        }

        int m_ControlID;

        internal float minSnapSize
        {
            get
            {
                if (m_IsOnGrid)
                {
                    return Mathf.Min(EditorSnapping.activeMoveSnapValue.x,
                        Mathf.Min(EditorSnapping.activeMoveSnapValue.y,
                            EditorSnapping.activeMoveSnapValue.z));
                }

                return Mathf.Min(EditorSnapping.incrementalSnapMoveValue.x,
                    Mathf.Min(EditorSnapping.incrementalSnapMoveValue.y,
                        EditorSnapping.incrementalSnapMoveValue.z));
            }
        }

        const float k_MinBoundLength = 0.001f; // 1mm

        // ideally this would be owned by the state machine
        public int controlID => m_ControlID;

        public static Type activeShapeType
        {
            get { return s_ActiveShapeIndex < 0 ? typeof(Cube) : EditorShapeUtility.availableShapeTypes[s_ActiveShapeIndex]; }
        }

        public override bool gridSnapEnabled => true;

        internal ProBuilderShape currentShapeInOverlay
        {
            get
            {
                if (m_CurrentState is ShapeState_InitShape  && m_LastShapeCreated != null)
                    return m_LastShapeCreated;

                if (m_CurrentState is ShapeState_DrawBaseShape &&
                    m_DuplicateGO != null && m_DuplicateGO.GetComponent<MeshRenderer>().enabled)
                    return m_DuplicateGO.GetComponent<ProBuilderShape>();

                return proBuilderShape;
            }
        }

        internal ProBuilderShape proBuilderShape
        {
            get
            {
                if (m_ProBuilderShape == null)
                {
                    m_ProBuilderShape = new GameObject("Shape", typeof(ProBuilderShape)).GetComponent<ProBuilderShape>();
                    m_ProBuilderShape.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    m_ProBuilderShape.hideFlags = HideFlags.None;
                    m_ProBuilderShape.SetShape(EditorShapeUtility.CreateShape(activeShapeType));
                    m_ProBuilderShape.size = lastSize;
                    m_ProBuilderShape.shapeRotation = lastRotation;
                }
                return m_ProBuilderShape;
            }
        }

        static DrawShapeTool s_Instance = null;
        internal static DrawShapeTool instance => s_Instance;

        void OnDisable()
        {
            if (m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
        }

        public override void OnActivated()
        {
            m_ProBuilderShape = null;

            MeshSelection.SetSelection((GameObject)null);
            handleSelectionChange = true;

            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
            ToolManager.activeToolChanged += OnActiveToolChanged;
            ToolManager.activeContextChanged += OnActiveContextChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            if (m_CurrentState == null)
                m_CurrentState = InitStateMachine();
            else
                m_CurrentState = ShapeState.ResetTool(this);

            s_Instance = this;
        }

        public override void OnWillBeDeactivated()
        {
            s_Instance = null;
            handleSelectionChange = false;
            m_LastShapeCreated = null;
            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
            ToolManager.activeToolChanged -= OnActiveToolChanged;
            ToolManager.activeContextChanged -= OnActiveContextChanged;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            if (m_ProBuilderShape != null && !( m_CurrentState is ShapeState_InitShape ))
                m_CurrentState = ShapeState.ResetState();

            if (m_DuplicateGO != null)
                DestroyImmediate(m_DuplicateGO);
        }

        void OnSelectModeChanged(SelectMode mode)
        {
            ToolManager.RestorePreviousPersistentTool();
        }

        void OnActiveToolChanged()
        {
            if(ToolManager.IsActiveTool(this))
                SetBounds(currentShapeInOverlay.size);
        }

        void OnActiveContextChanged()
        {
            if(ToolManager.activeContextType != typeof(GameObjectToolContext))
                ToolManager.RestorePreviousPersistentTool();
        }

        void HandleUndoRedoPerformed()
        {
            if(ToolManager.IsActiveTool(this))
                m_CurrentState = ShapeState.ResetState();
        }

        void OnSelectionChanged()
        {
            if(ToolManager.IsActiveTool(this))
            {
                if(Selection.activeGameObject != null
                   && (m_LastShapeCreated == null || Selection.activeGameObject != m_LastShapeCreated.gameObject))
                {
                    m_CurrentState = ShapeState.ResetState();
                    ToolManager.RestorePreviousPersistentTool();
                }
            }
        }

        /// <summary>
        /// Init the state machine associated to the tool.
        /// All states are linked together and initialized.
        /// </summary>
        /// <returns>
        /// Returns the current state of the StateMachine,
        /// this state machine will self-handle during its lifetime.
        /// </returns>
        ShapeState InitStateMachine()
        {
            ShapeState.tool = this;
            ShapeState initState = new ShapeState_InitShape();
            ShapeState drawBaseState = new ShapeState_DrawBaseShape();
            ShapeState drawHeightState = new ShapeState_DrawHeightShape();
            ShapeState.s_defaultState = initState;
            initState.m_nextState = drawBaseState;
            drawBaseState.m_nextState = drawHeightState;
            drawHeightState.m_nextState = initState;

            return ShapeState.StartStateMachine();
        }

        internal void SaveShapeParams(ProBuilderShape proBuilderShape)
        {
            lastSize = proBuilderShape.size;
            lastRotation = proBuilderShape.shapeRotation;

            EditorShapeUtility.SaveParams(proBuilderShape.shape);
        }

        internal void ApplyPrefsSettings(ProBuilderShape pBShape)
        {
            pBShape.size = lastSize;
            pBShape.shapeRotation = lastRotation;
        }

        // Transform the point according to the snapping settings
        public Vector3 GetPoint(Vector3 point, bool useIncrementSnap = false)
        {
            if(useIncrementSnap)
                return ProBuilderSnapping.Snap(point, EditorSnapping.incrementalSnapMoveValue);

            if (m_IsOnGrid && EditorSnapSettings.gridSnapActive)
                return ProBuilderSnapping.Snap(point, EditorSnapping.activeMoveSnapValue);

            return point;
        }

        internal void SetBounds(Vector3 size)
        {
            //Keep orientation created using mouse drag
            var dragDirection = m_BB_OppositeCorner - m_BB_Origin;
            float x = dragDirection.x < 0 ? -size.x : size.x;
            float z = dragDirection.z < 0 ? -size.z : size.z;

            m_BB_OppositeCorner = m_BB_Origin + new Vector3(x, 0, z);
            m_BB_HeightCorner = m_BB_Origin + size;
        }

        internal void DoDuplicateShapePreviewHandle(Vector3 position)
        {
            var evt = Event.current;

            if(evt.type != EventType.Repaint)
                return;

            bool previewShortcutActive = evt.shift && !(evt.control || evt.command);

            if (HandleUtility.nearestControl != m_ControlID || !previewShortcutActive)
            {
                if(m_DuplicateGO)
                    m_DuplicateGO.GetComponent<MeshRenderer>().enabled = false;
                return;
            }

            var size = currentShapeInOverlay.size;
            m_Bounds.size = size;
            position = GetPoint(position);
            var cornerPosition = position - size / 2f;
            cornerPosition.y = position.y;

            m_Bounds.center = cornerPosition + new Vector3(size.x / 2f, 0, size.z / 2f) + (size.y / 2f) * m_Plane.normal;
            var lastPreviewRotation = m_PlaneRotation;
            m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward, m_Plane.normal);
            var forceRebuildPreview = !m_PlaneRotation.Equals(lastPreviewRotation) ||
                                      m_LastPreviewPivotLocation != pivotLocation;
            m_LastPreviewPivotLocation = pivotLocation;

            var preview_BB_Origin = m_Bounds.center - m_PlaneRotation * (size / 2f);
            var preview_BB_HeightCorner = m_Bounds.center + m_PlaneRotation * (size / 2f);
            var preview_BB_OppositeCorner = preview_BB_HeightCorner - m_PlaneRotation * new Vector3(0, size.y, 0);


            ProBuilderShape shape;
            if (m_DuplicateGO == null)
            {
                var instantiated = ShapeFactory.Instantiate(activeShapeType);
                shape = instantiated.GetComponent<ProBuilderShape>();
                m_DuplicateGO = shape.gameObject;
                m_DuplicateGO.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                ApplyPrefsSettings(shape);
                shape.GetComponent<MeshRenderer>().sharedMaterial = BuiltinMaterials.ShapePreviewMaterial;

                EditorShapeUtility.CopyLastParams(shape.shape, shape.shape.GetType());
                shape.Rebuild(previewPivotPosition, m_PlaneRotation, m_Bounds);

                ProBuilderEditor.Refresh(false);
            }
            else
            {
                var rendererWasEnabled = true;
                if (m_DuplicateGO.TryGetComponent<MeshRenderer>(out var renderer) && !renderer.enabled)
                {
                    rendererWasEnabled = false;
                    renderer.enabled = true;
                }

                if (forceRebuildPreview || !rendererWasEnabled)
                {
                    shape = m_DuplicateGO.GetComponent<ProBuilderShape>();

                    ApplyPrefsSettings(shape);
                    EditorShapeUtility.CopyLastParams(shape.shape, shape.shape.GetType());
                    shape.Rebuild(previewPivotPosition, m_PlaneRotation, m_Bounds);

                    ProBuilderEditor.Refresh(false);
                }
            }

            var pivot = GetPoint(position);
            if (pivotLocation == PivotLocation.Center)
                pivot += .5f * size.y * m_Plane.normal;
            else
                pivot = previewPivotPosition;
            m_DuplicateGO.transform.SetPositionAndRotation(pivot, Quaternion.LookRotation(m_PlaneForward, m_Plane.normal));

            DrawBoundingBox(preview_BB_Origin, preview_BB_HeightCorner, preview_BB_OppositeCorner, false);
        }

        /// <summary>
        /// Recalculates the bounding box for this mesh's shape.
        /// </summary>
        void RecalculateBounds()
        {
            var diag = m_BB_OppositeCorner - m_BB_Origin;
            var forward = Vector3.Dot(diag, m_PlaneForward.normalized);
            var right = Vector3.Dot(diag, m_PlaneRight.normalized);

            var localHeight = Quaternion.Inverse(m_PlaneRotation) * (m_BB_HeightCorner - m_BB_OppositeCorner);
            var height = localHeight.y;

            m_Bounds.size = forward * Vector3.forward + right * Vector3.right + height * Vector3.up;
            m_Bounds.center = m_BB_Origin + 0.5f * ( m_BB_OppositeCorner - m_BB_Origin ) + 0.5f * (m_BB_HeightCorner - m_BB_OppositeCorner);

            //Prevent Z-fighting with the drawing surface
            if(Mathf.Abs(m_Bounds.center.y) < 0.0001f)
                m_Bounds.center = m_Bounds.center + 0.0001f * Vector3.up;

            m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward,m_Plane.normal);
        }

        internal void RebuildShape()
        {
            RecalculateBounds();

            if(m_Bounds.size.sqrMagnitude <= Mathf.Min(.01f , minSnapSize*minSnapSize)
               || Mathf.Abs(m_Bounds.extents.x) < k_MinBoundLength
               || Mathf.Abs(m_Bounds.extents.z) < k_MinBoundLength)
            {
                if(m_ProBuilderShape != null
                   && m_ProBuilderShape.mesh.vertexCount > 0)
                {
                    m_ProBuilderShape.size = Vector3.zero;
                    m_ProBuilderShape.mesh.Clear();
                    m_ProBuilderShape.mesh.Rebuild();
                    m_ProBuilderShape.Rebuild(new Bounds(m_BB_Origin, Vector3.zero), m_PlaneRotation);
                    ProBuilderEditor.Refresh(true);
                }
                return;
            }

            if (!m_IsShapeInit)
            {
                var shapeComponent = currentShapeInOverlay;
                EditorShapeUtility.CopyLastParams(shapeComponent.shape, shapeComponent.shape.GetType());
                shapeComponent.gameObject.hideFlags = HideFlags.HideInHierarchy;
                shapeComponent.mesh.renderer.sharedMaterial = EditorMaterialUtility.GetUserMaterial();
                shapeComponent.shapeRotation = Quaternion.identity;
                shapeComponent.gameObject.name = EditorShapeUtility.GetName(shapeComponent.shape);
                m_IsShapeInit = true;
            }

            proBuilderShape.Rebuild(pivotLocation == PivotLocation.Center ? m_Bounds.center : m_BB_Origin, m_PlaneRotation, m_Bounds);
            // PBLD-137 - continously refresh the material, otherwise if Resident Drawer is on, new faces are drawn without material applied until there's some other trigger:
            // renderer enable/disable, material re-apply, etc.
            m_ProBuilderShape.mesh.renderer.sharedMaterial = EditorMaterialUtility.GetUserMaterial();

            ProBuilderEditor.Refresh(false);

            SceneView.RepaintAll();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            //A current problem on EditorTools with MacOS seems to be calling OnToolGUI before OnActivated after a domain reload.
            if(s_Instance == null)
                return;

            // todo refactor overlays to use `Overlay` class
#pragma warning disable 618
            SceneViewOverlay.Window(k_ShapeTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);
#pragma warning restore 618

            var evt = Event.current;
            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            m_ControlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(m_ControlID);

            if(GUIUtility.hotControl == 0)
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);

            m_CurrentState = m_CurrentState.DoState(evt);
        }

        internal void DrawBoundingBox(Vector3 origin, Vector3 oppositeCorner, Vector3 heightCorner, bool drawCorners = true)
        {
            using (new Handles.DrawingScope(k_BoundsColor, Matrix4x4.TRS(m_Bounds.center, m_PlaneRotation.normalized, Vector3.one)))
            {
                Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            }

            if(!drawCorners)
                return;

            using (new Handles.DrawingScope(Color.white))
            {
                Handles.DotHandleCap(-1, origin, Quaternion.identity, HandleUtility.GetHandleSize(origin) * 0.05f, EventType.Repaint);
                Handles.DotHandleCap(-1, oppositeCorner, Quaternion.identity, HandleUtility.GetHandleSize(oppositeCorner) * 0.05f, EventType.Repaint);
            }
            using (new Handles.DrawingScope(EditorHandleDrawing.vertexSelectedColor))
            {
                Handles.DotHandleCap(-1, heightCorner, Quaternion.identity, HandleUtility.GetHandleSize(heightCorner) * 0.05f, EventType.Repaint);
            }
        }

        internal void DrawBoundingBox(bool drawCorners = true)
        {
            DrawBoundingBox(m_BB_Origin, m_BB_OppositeCorner, m_BB_HeightCorner, drawCorners);
        }

        void OnOverlayGUI(UObject overlayTarget, SceneView view)
        {
            string foldoutName = $"Shape Properties (New {currentShapeInOverlay.shape.GetType().Name})";
            if(currentShapeInOverlay == m_LastShapeCreated)
                foldoutName = "Shape Properties (" + m_LastShapeCreated.name + ")";

            Editor.CreateCachedEditor(currentShapeInOverlay, typeof(ProBuilderShapeEditor), ref m_ShapeEditor);

            GUILayout.BeginVertical(GUILayout.MinWidth(k_MinOverlayWidth));
            ((ProBuilderShapeEditor)m_ShapeEditor).m_ShapePropertyLabel.text = foldoutName;
            ((ProBuilderShapeEditor)m_ShapeEditor).DrawShapeParametersGUI(this);
            GUILayout.EndVertical();
        }
    }
}
