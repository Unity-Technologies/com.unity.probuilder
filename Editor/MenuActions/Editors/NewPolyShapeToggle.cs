using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewPolyShapeToggle : MenuToggle
    {
        PolyShapeTool m_Tool;

        bool m_RestorePreviousMode;
        SelectMode m_PreviousMode;

        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/NewPolyShape", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return _tooltip; } }
        public override string menuTitle { get { return "New Poly Shape"; } }
        public override int toolbarPriority { get { return 1; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "New Polygon Shape",
                "Creates a new shape by clicking around a perimeter and extruding."
            );

        public override bool hidden
        {
            get { return false; }
        }

        public override bool enabled
        {
            get { return true; }
        }

        static bool CanCreateNewPolyShape()
        {
            //If inspector is locked we cannot create new PolyShape.
            //First created inspector seems to hold a specific semantic where
            //if not unlocked no matter how many inspectors are present they will
            //not allow the creation of new PolyShape.
#if UNITY_2019_1_OR_NEWER
            var inspWindows = InspectorWindow.GetInspectors();

            if (inspWindows.Any(x => x.isLocked))
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                                    L10n.Tr("Inspector Locked"),
                                    L10n.Tr("To create new Poly Shape you need access to all Inspectors, which are currently locked. Do you wish to unlock all Inpsectors?"),
                                    L10n.Tr("Unlock"),
                                    L10n.Tr("Cancel")))
                {
                    foreach (var insp in inspWindows)
                        insp.isLocked = false;
                }
                else
                {
                    return false;
                }
            }
#else
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var inspWindows = Resources.FindObjectsOfTypeAll(inspectorType);
            var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
            bool someInspectorLocked = false;
            foreach (var insp in inspWindows)
            {
                if ((bool)isLocked.GetGetMethod().Invoke(insp, null))
                {
                    someInspectorLocked = true;
                    break;
                }
            }
            if (someInspectorLocked == true)
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                                    "Inspector Locked",
                                    "To create new Poly Shape you need access to all Inspectors, which are currently locked. Do you wish to unlock all Inpsectors?",
                                    "Unlock",
                                    "Cancel"))
                {
                    foreach (var insp in inspWindows)
                    {
                        isLocked.GetSetMethod().Invoke(insp, new object[] { false });
                    }
                }
                else
                {
                    return false;
                }
            }
#endif
            return true;
        }

        protected override ActionResult StartActivation(StartEndCallBack onStart)
        {
            if (!CanCreateNewPolyShape())
                return new ActionResult(ActionResult.Status.Canceled, "Canceled Create Poly Shape");

            GameObject go = new GameObject("PolyShape");
            UndoUtility.RegisterCreatedObjectUndo(go, "Create Poly Shape");
            PolyShape poly = Undo.AddComponent<PolyShape>(go);
            ProBuilderMesh pb = Undo.AddComponent<ProBuilderMesh>(go);
            pb.CreateShapeFromPolygon(poly.m_Points, poly.extrude, poly.flipNormals);
            EditorUtility.InitObject(pb);

            // Special case - we don't want to reset the grid pivot because we rely on it to set the active plane for
            // interaction, regardless of whether snapping is enabled or not.
            if (ProGridsInterface.SnapEnabled() || ProGridsInterface.GridVisible())
            {
                Vector3 pivot;
                if (ProGridsInterface.GetPivot(out pivot))
                    go.transform.position = pivot;
            }
            MeshSelection.SetSelection(go);
            poly.polyEditMode = PolyShape.PolyEditMode.Path;

            m_RestorePreviousMode = true;
            m_PreviousMode = ProBuilderEditor.selectMode;
            ProBuilderEditor.selectMode = SelectMode.Object;

            m_Tool = ScriptableObject.CreateInstance<PolyShapeTool>();
            m_Tool.polygon = poly;
            ToolManager.SetActiveTool(m_Tool);

            Undo.RegisterCreatedObjectUndo(m_Tool, "Open Cut Tool");

            ToolManager.activeToolChanged += ActiveToolChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            onStart();
            return new ActionResult(ActionResult.Status.Success,"Create Poly Shape");
        }

        protected override ActionResult EndActivation(StartEndCallBack onEnd)
        {
            ToolManager.activeToolChanged -= ActiveToolChanged;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            if(m_Tool != null)
                m_Tool.End();

            if(m_RestorePreviousMode)
                ProBuilderEditor.selectMode = m_PreviousMode;

            onEnd();
            return new ActionResult(ActionResult.Status.Success,"End Poly Shape");
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            m_RestorePreviousMode = false;
            LeaveTool();
        }

        void ActiveToolChanged()
        {
            EditorApplication.delayCall += () => LeaveTool();
        }

        void LeaveTool()
        {
            ActionResult result = EndActivation(OnEnd);
            EditorUtility.ShowNotification(result.notification);
        }
    }
}
