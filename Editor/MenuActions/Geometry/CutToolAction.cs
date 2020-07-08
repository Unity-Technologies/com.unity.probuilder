using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class CutToolAction : MenuAction
    {
        static Pref<bool> m_EdgeToEdge = new Pref<bool>("VertexInsertion.edgeToEdge", true);
        static Pref<bool> m_EndOnClicToStart = new Pref<bool>("VertexInsertion.endOnClicToStart", false);
        static Pref<bool> m_ConnectToStart = new Pref<bool>("VertexInsertion.connectToStart", true);

        public static bool EdgeToEdge
        {
            get { return m_EdgeToEdge; }
        }

        public static bool EndOnClicToStart
        {
            get { return m_EndOnClicToStart; }
        }

        public static bool ConnectToStart
        {
            get { return m_ConnectToStart; }
        }


        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Face_Subdivide", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Cut Tool",
            @"Inserts vertices in a face and subdivide it accordingly.",
            keyCommandAlt, keyCommandShift, 'V'
        );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        private PolygonalCut m_CutTarget;


        /// <summary>
        /// Called when the settings window is closed.
        /// </summary>
        protected override void OnSettingsDisable()
        {
            if (m_CutTarget != null)
                Undo.DestroyObjectImmediate(m_CutTarget);
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Point-to-point Cut - Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Add new vertices in the selected face. Press ESC to validate the shape. " +
                                    "\nUsing CTRL key allows to snap the selection to existing edges and vertices. " +
                                    "\nUsing SHIFT key allows to move points of the cut shape.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_ConnectToStart.value = EditorGUILayout.Toggle("Connect End to Start Point", m_ConnectToStart);

            m_EndOnClicToStart.value = EditorGUILayout.Toggle("Selecting Start Point is ending cut", m_EndOnClicToStart);

            m_EdgeToEdge.value = EditorGUILayout.Toggle("Cut From Edge To Edge", m_EdgeToEdge);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (m_CutTarget == null)
            {
                if (GUILayout.Button("Start Vertices Insertion"))
                    DoAction();
            }
            else
            {
                if (GUILayout.Button("Compute Cut"))
                    ComputeCut();
            }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            if (MeshSelection.selectedObjectCount > 1)
                return new ActionResult(ActionResult.Status.Failure, "Only one ProBuilder object must be selected");

            ProBuilderMesh firstObj = MeshSelection.activeMesh;

            m_CutTarget = Undo.AddComponent<PolygonalCut>(firstObj.gameObject);

            return new ActionResult(ActionResult.Status.Success,"Vertex On Face Insertion");
        }

        private void ComputeCut()
        {
            m_CutTarget.CutEnded = true;
        }
    }
}
