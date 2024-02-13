using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class CollapseVertices : MenuAction
    {
        Pref<bool> m_CollapseToFirst = new Pref<bool>("CollapseVertices.collapseToFirst", false);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Vert_Collapse";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Collapse Vertices",
                @"Merge all selected vertices into a single vertex, centered at the first vertex or average position of all selected points."
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

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();
            root.style.minWidth = 150;

            var toggle = new Toggle("Collapse to First");
            toggle.tooltip = "Collapse To First setting decides where the collapsed vertex will be placed. If True, " +
                "the new vertex will be placed at the position of the first selected vertex. If false, the new vertex " +
                "is placed at the average position of all selected vertices.";
            toggle.SetValueWithoutNotify(m_CollapseToFirst);
            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                m_CollapseToFirst.SetValue(evt.newValue);

                PreviewActionManager.UpdatePreview();
            });
            root.Add(toggle);

            return root;
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
                PerformAction();
        }

        protected override ActionResult PerformActionImplementation()
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
