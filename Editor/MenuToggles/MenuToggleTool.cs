using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine;
using UObject = UnityEngine.Object;


namespace UnityEditor.ProBuilder
{
    public class MenuToggleTool : MenuToggle
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
            get { return IconUtility.GetIcon("Toolbar/ExtrudeFace_Individual", IconSkin.Pro); }
        }

        protected override Texture2D disabledIcon
        {
            get { return IconUtility.GetIcon(string.Format("{0}_disabled", "Toolbar/ExtrudeFace_Individual"), IconSkin.Pro); }
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
            "Menu Toggle",
            "This is an example of menu toggle in opposition to menu action.",
            keyCommandSuper, 'M'
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
