using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor.ProBuilder.UI;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewPolyShape : MenuAction
    {
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

        bool CanCreateNewPolyShape()
        {
            //If inspector is locked we cannot create new PolyShape.
            //First created inspector seems to hold a specific semantic where
            //if not unlocked no matter how many inspectors are present they will
            //not allow the creation of new PolyShape.
#if UNITY_2019_1_OR_NEWER           
            var inspWindows = InspectorWindow.GetInspectors();
            bool someInspectorLocked = false;
            foreach (var insp in inspWindows)
            {
                if (insp.isLocked)
                {
                    someInspectorLocked = true;
                    break;
                }
            }
            if (someInspectorLocked == true)
            {
                if (UnityEditor.EditorUtility.DisplayDialog(                                   
                                    L10n.Tr("Inspector Locked"),
                                    L10n.Tr("To create new Poly Shape you need access to all Inspectors, which are currently locked. Do you wish to unlock all Inpsectors?"),
                                    L10n.Tr("Unlock"),
                                    L10n.Tr("Cancel")))
                {
                    foreach (var insp in inspWindows)
                    {
                        insp.isLocked = false;
                    }
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

        public override ActionResult DoAction()
        {
            if (!CanCreateNewPolyShape())
                return new ActionResult(ActionResult.Status.Canceled, "Canceled Create Poly Shape");

            GameObject go = new GameObject();
            PolyShape poly = go.AddComponent<PolyShape>();
            ProBuilderMesh pb = poly.gameObject.AddComponent<ProBuilderMesh>();
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
            UndoUtility.RegisterCreatedObjectUndo(go, "Create Poly Shape");
            poly.polyEditMode = PolyShape.PolyEditMode.Path;

            return new ActionResult(ActionResult.Status.Success, "Create Poly Shape");
        }
    }
}
