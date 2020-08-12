using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class CollapseVertices : MenuAction
    {
        Pref<bool> m_CollapseToFirst = new Pref<bool>("CollapseVertices.collapseToFirst", false);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Vert_Collapse", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Collapse Vertices",
                @"Merge all selected vertices into a single vertex, centered at the average of all selected points.",
                keyCommandAlt, 'C'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedSharedVertexCountObjectMax > 1; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Collapse Vertices Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Collapse To First setting decides where the collapsed vertex will be placed.\n\nIf True, the new vertex will be placed at the position of the first selected vertex.  If false, the new vertex is placed at the average position of all selected vertices.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_CollapseToFirst.value = EditorGUILayout.Toggle("Collapse To First", m_CollapseToFirst);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Collapse Vertices"))
                DoAction();
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            bool success = false;

            bool collapseToFirst = m_CollapseToFirst;

            UndoUtility.RecordSelection("Collapse Vertices");

            foreach (var mesh in MeshSelection.topInternal)
            {
                if (mesh.selectedIndexesInternal.Length > 1)
                {
                    int newIndex = mesh.MergeVertices(mesh.selectedIndexesInternal, collapseToFirst);

                    success = newIndex > -1;

                    if (success)
                        mesh.SetSelectedVertices(new int[] { newIndex });

                    mesh.ToMesh();
                    mesh.Refresh();
                    mesh.Optimize();
                }
            }

            ProBuilderEditor.Refresh();

            if (success)
                return new ActionResult(ActionResult.Status.Success, "Collapse Vertices");

            return new ActionResult(ActionResult.Status.Failure, "Collapse Vertices\nNo Vertices Selected");
        }
    }
}
