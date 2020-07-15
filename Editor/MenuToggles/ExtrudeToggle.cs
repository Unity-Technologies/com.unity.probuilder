using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace UnityEditor.ProBuilder
{
    public class ExtrudeToggle : MenuToggle
    {
        ExtrudeMethod extrudeMethod
        {
            get { return VertexManipulationTool.s_ExtrudeMethod; }
            set { VertexManipulationTool.s_ExtrudeMethod.value = value; }
        }

        static string GetExtrudeIconString(ExtrudeMethod m)
        {
            return m == ExtrudeMethod.VertexNormal ? "Toolbar/ExtrudeFace_VertexNormals"
                : m == ExtrudeMethod.FaceNormal ? "Toolbar/ExtrudeFace_FaceNormals"
                : "Toolbar/ExtrudeFace_Individual";
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon(GetExtrudeIconString(extrudeMethod), IconSkin.Pro); }
        }

        protected override Texture2D disabledIcon
        {
            get { return IconUtility.GetIcon(string.Format("{0}_disabled", GetExtrudeIconString(extrudeMethod)), IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        Texture2D[] m_Icons = null;

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Extrude Toggle",
            "Extrude selected faces, either as a group or individually.\n\nAlt + Click this button to show additional Extrude options.",
            keyCommandSuper, 'E'
        );

        GUIContent m_ShapeTitle;


        public override ActionResult StartActivation()
        {
            Debug.Log( "Menu Tool Activation - Start" );
            GUIContent m_ShapeTitle = new GUIContent("Toggle Tool");
            return ActionResult.Success;
        }

        public override void UpdateAction()
        {
             Debug.Log( "Menu Tool Activation - Update" );
             //SceneViewOverlay.Window(m_ShapeTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);
        }

        public override ActionResult EndActivation()
        {
            Debug.Log( "Menu Tool Activation - End" );
            return ActionResult.Success;
        }

        void OnOverlayGUI(UObject target, SceneView view)
        {

        }
    }
}
