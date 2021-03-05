using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ExtrudeFaces : MenuAction
    {
        Pref<float> m_ExtrudeDistance = new Pref<float>("ExtrudeFaces.distance", .5f);

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
                "Extrude Faces",
                "Extrude selected faces, either as a group or individually.\n\nAlt + Click this button to show additional Extrude options.",
                keyCommandSuper, 'E'
            );

        public ExtrudeFaces()
        {
            m_Icons = new Texture2D[3];
            m_Icons[(int)ExtrudeMethod.IndividualFaces] = IconUtility.GetIcon("Toolbar/ExtrudeFace_Individual", IconSkin.Pro);
            m_Icons[(int)ExtrudeMethod.VertexNormal] = IconUtility.GetIcon("Toolbar/ExtrudeFace_VertexNormals", IconSkin.Pro);
            m_Icons[(int)ExtrudeMethod.FaceNormal] = IconUtility.GetIcon("Toolbar/ExtrudeFace_FaceNormals", IconSkin.Pro);
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Extrude Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Extrude Amount determines how far a face will be moved along it's normal when extruding.  This value can be negative.\n\nYou may also choose to Extrude by Face Normal, Vertex Normal, or as Individual Faces.", MessageType.Info);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(m_Icons[(int)extrudeMethod]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            extrudeMethod = (ExtrudeMethod)EditorGUILayout.EnumPopup("Extrude By", extrudeMethod);
            m_ExtrudeDistance.value = EditorGUILayout.FloatField("Distance", m_ExtrudeDistance);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Extrude Faces"))
                PerformAction();
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Extrude");

            int extrudedFaceCount = 0;

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                mesh.ToMesh();
                mesh.Refresh(RefreshMask.Normals);

                if (mesh.selectedFaceCount < 1)
                    continue;

                extrudedFaceCount += mesh.selectedFaceCount;
                var selectedFaces = mesh.GetSelectedFaces();

                mesh.Extrude(selectedFaces,
                    VertexManipulationTool.s_ExtrudeMethod,
                    m_ExtrudeDistance);

                mesh.SetSelectedFaces(selectedFaces);

                mesh.Rebuild();
                mesh.Optimize();
            }

            ProBuilderEditor.Refresh();

            if (extrudedFaceCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Extrude");

            return new ActionResult(ActionResult.Status.Canceled, "Extrude\nEmpty Selection");
        }
    }
}
