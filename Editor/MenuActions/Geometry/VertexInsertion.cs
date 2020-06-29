using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class VertexInsertion : MenuAction
    {
        static Pref<bool> m_EndOnEdgeConnection = new Pref<bool>("VertexInsertion.endOnEdgeConnection", true);
        static Pref<bool> m_EndOnClicToStartPoint = new Pref<bool>("VertexInsertion.endOnClicToStartPoint", false);
        static Pref<bool> m_ConnectToStartPoint = new Pref<bool>("VertexInsertion.autoConnectToStartPoint", true);

        public static bool EndOnEdgeConnection
        {
            get { return m_EndOnEdgeConnection; }
        }

        public static bool EndOnClicToStartPoint
        {
            get { return m_EndOnClicToStartPoint; }
        }

        public static bool ConnectToStartPoint
        {
            get { return m_ConnectToStartPoint; }
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
            "Vertex Insertion",
            @"Inserts a vertex in a face at a desire position and creates new edges accordingly.",
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


        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Point-to-point Cut - Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("TODO.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_ConnectToStartPoint.value = EditorGUILayout.Toggle("Connect End to Start Point", m_ConnectToStartPoint);

            m_EndOnClicToStartPoint.value = EditorGUILayout.Toggle("Selecting Start Point is ending cut", m_EndOnClicToStartPoint);

            m_EndOnEdgeConnection.value = EditorGUILayout.Toggle("EdgeToEdgeCut", m_EndOnEdgeConnection);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Start Vertices Insertion"))
                DoAction();

            // if (GUILayout.Button("Do Point-to-point Cut"))
            //     DoAction();
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            if (MeshSelection.selectedObjectCount > 1)
                return new ActionResult(ActionResult.Status.Failure, "Only one ProBuilder object must be selected");

            ProBuilderMesh firstObj = MeshSelection.activeMesh;
            //UndoUtility.RegisterCreatedObjectUndo(firstObj.gameObject, "Create Polygonal Cut");
            PolygonalCut voFace = Undo.AddComponent<PolygonalCut>(firstObj.gameObject);
            voFace.polygonEditMode = PolygonalCut.PolygonEditMode.Add;

            return new ActionResult(ActionResult.Status.Success,"Vertex On Face Insertion");
        }
    }
}
