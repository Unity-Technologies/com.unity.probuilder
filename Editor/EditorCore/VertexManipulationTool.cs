using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor.SettingsManagement;

#if !UNITY_2018_3_OR_NEWER
using UnityEditor.SettingsManagement;
#endif

#if DEBUG_HANDLES
using UnityEngine.Rendering;
#endif

namespace UnityEditor.ProBuilder
{
    abstract class VertexManipulationTool
    {
        static Pref<HandleOrientation> s_HandleOrientation = new Pref<HandleOrientation>("editor.handleOrientation", HandleOrientation.World, SettingsScope.User);
        static Pref<PivotPoint> s_PivotPoint = new Pref<PivotPoint>("editor.pivotPoint", PivotPoint.Center, SettingsScope.User);

        [UserSetting(UserSettingsProvider.developerModeCategory, "PivotMode.Pivot", "Set the behavior of the \"Pivot\" handle mode when editing mesh elements.")]
        static Pref<PivotPoint> s_PivotModePivotEquivalent = new Pref<PivotPoint>("editor.pivotModePivotEquivalent", PivotPoint.ActiveElement, SettingsScope.User);

        [UserSetting(UserSettingsProvider.developerModeCategory, "Show Internal Pivot and Orientation")]
        static Pref<bool> s_ShowHandleSettingsInScene = new Pref<bool>("developer.showHandleSettingsInScene", false, SettingsScope.User);

        internal static PivotPoint pivotModePivotEquivalent
        {
            get { return s_PivotModePivotEquivalent; }
            set { s_PivotModePivotEquivalent.SetValue(value); }
        }

        internal static bool showHandleSettingsInScene
        {
            get { return s_ShowHandleSettingsInScene; }
        }

        // Store PivotRotation so that we can detect changes and update our handles appropriately
        static PivotRotation s_PivotRotation;

        /// <value>
        /// Where the handle is positioned relative to the current selection.
        /// </value>
        /// <remarks>
        /// Relates to the UnityEditor.PivotMode enum, with additional options.
        /// </remarks>
        public static PivotPoint pivotPoint
        {
            get
            {
                SyncPivotPoint();

                return Tools.pivotMode == PivotMode.Pivot
                    ? pivotModePivotEquivalent
                    : PivotPoint.Center;
            }
        }

        static void SyncPivotPoint()
        {
            var unity = s_PivotPoint.value == PivotPoint.Center ? PivotMode.Center : PivotMode.Pivot;

            if (Tools.pivotMode != unity)
            {
                s_PivotPoint.SetValue(Tools.pivotMode == PivotMode.Center ? PivotPoint.Center : s_PivotModePivotEquivalent.value, true);
                MeshSelection.InvalidateElementSelection();
            }
        }

        /// <value>
        /// How the handle is rotated relative to the current selection.
        /// </value>
        public static HandleOrientation handleOrientation
        {
            get
            {
                SyncPivotRotation();
                return s_HandleOrientation;
            }
            set
            {
                s_HandleOrientation.SetValue(value, true);

                if (value != HandleOrientation.ActiveElement)
                    Tools.pivotRotation = value == HandleOrientation.ActiveObject
                        ? PivotRotation.Local
                        : PivotRotation.Global;

                MeshSelection.InvalidateElementSelection();

                var toolbar = typeof(EditorWindow).Assembly.GetType("UnityEditor.Toolbar");
                var repaint = toolbar.GetMethod("RepaintToolbar", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                repaint.Invoke(null, null);
            }
        }

        // Sync ProBuilder HandleOrientation to the current Tools.PivotRotation
        static void SyncPivotRotation()
        {
            if (s_PivotRotation != Tools.pivotRotation)
            {
                s_HandleOrientation.SetValue(Tools.pivotRotation == PivotRotation.Global
                    ? HandleOrientation.World
                    : HandleOrientation.ActiveObject);
                s_PivotRotation = Tools.pivotRotation;
                MeshSelection.InvalidateElementSelection();
                return;
            }

            var value = s_HandleOrientation.value;
            var unity = value == HandleOrientation.ActiveObject ? PivotRotation.Local : PivotRotation.Global;

            if (value != HandleOrientation.ActiveElement)
            {
                if (unity != Tools.pivotRotation)
                {
                    s_HandleOrientation.SetValue(Tools.pivotRotation == PivotRotation.Global
                            ? HandleOrientation.World
                            : HandleOrientation.ActiveObject,
                        true);
                    MeshSelection.InvalidateElementSelection();
                }
            }
        }

        /// <value>
        /// Called when vertex modifications are complete.
        /// </value>
        public static event Action<IEnumerable<ProBuilderMesh>> afterMeshModification;

        /// <value>
        /// Called immediately prior to beginning vertex modifications. The ProBuilderMesh will be in un-altered state at this point (meaning ProBuilderMesh.ToMesh and ProBuilderMesh.Refresh have been called, but not Optimize).
        /// </value>
        public static event Action<IEnumerable<ProBuilderMesh>> beforeMeshModification;

        internal static Pref<bool> s_ExtrudeEdgesAsGroup = new Pref<bool>("editor.extrudeEdgesAsGroup", true);
        internal static Pref<ExtrudeMethod> s_ExtrudeMethod = new Pref<ExtrudeMethod>("editor.extrudeMethod", ExtrudeMethod.FaceNormal);

        Vector3 m_HandlePosition;
        Quaternion m_HandleRotation;
        Vector3 m_HandlePositionOrigin;
        Quaternion m_HandleRotationOrigin;
        bool m_IsEditing;

        float m_ProgridsSnapValue = .25f;
        bool m_SnapAxisConstraint = true;
        bool m_ProgridsSnapEnabled;
        static bool s_Initialized;
        static FieldInfo s_VertexDragging;
        static MethodInfo s_FindNearestVertex;
        static object[] s_FindNearestVertexArguments = new object[] { null, null, null };

        internal IEnumerable<MeshAndElementSelection> elementSelection
        {
            get { return MeshSelection.elementSelection; }
        }

        protected static bool vertexDragging
        {
            get
            {
                Init();
                return s_VertexDragging != null && (bool)s_VertexDragging.GetValue(null);
            }
        }

        protected bool isEditing
        {
            get { return m_IsEditing; }
        }

        protected Event currentEvent { get; private set; }

        protected Vector3 handlePositionOrigin
        {
            get { return m_HandlePositionOrigin; }
        }

        protected Quaternion handleRotationOriginInverse { get; private set; }

        protected Quaternion handleRotationOrigin
        {
            get { return m_HandleRotationOrigin; }
        }

        protected float progridsSnapValue
        {
            get { return m_ProgridsSnapValue; }
        }

        protected bool snapAxisConstraint
        {
            get { return m_SnapAxisConstraint; }
        }

        protected bool progridsSnapEnabled
        {
            get { return m_ProgridsSnapEnabled; }
        }

        protected bool relativeSnapEnabled
        {
            get { return currentEvent.control; }
        }

        static void Init()
        {
            if (s_Initialized)
                return;
            s_Initialized = true;
            s_VertexDragging = typeof(Tools).GetField("vertexDragging", BindingFlags.NonPublic | BindingFlags.Static);
            s_FindNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal abstract MeshAndElementSelection GetElementSelection(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation);

        public void OnSceneGUI(Event evt)
        {
            // necessary because there is no callback on toolbar changes
            SyncPivotPoint();
            SyncPivotRotation();

            currentEvent = evt;

            if (evt.type == EventType.MouseUp || evt.type == EventType.Ignore)
                FinishEdit();

            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Face:
                case SelectMode.TextureFace:
                    if (MeshSelection.selectedFaceCount < 1)
                        return;
                    break;

                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                    if (MeshSelection.selectedEdgeCount < 1)
                        return;
                    break;

                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                    if (MeshSelection.selectedVertexCount < 1)
                        return;
                    break;
            }

            if (!m_IsEditing)
            {
                m_HandlePosition = MeshSelection.GetHandlePosition();
                m_HandleRotation = MeshSelection.GetHandleRotation();

                m_HandlePositionOrigin = m_HandlePosition;
                m_HandleRotationOrigin = m_HandleRotation;
                handleRotationOriginInverse = Quaternion.Inverse(m_HandleRotation);
            }

            DoTool(m_HandlePosition, m_HandleRotation);
        }

        protected abstract void DoTool(Vector3 handlePosition, Quaternion handleRotation);

        protected virtual void OnToolEngaged() {}

        protected virtual void OnToolDisengaged() {}

        protected void BeginEdit(string undoMessage)
        {
            if (m_IsEditing)
                return;

            // Disable iterative lightmapping
            Lightmapping.PushGIWorkflowMode();

            var selection = MeshSelection.topInternal.ToArray();

            UndoUtility.RegisterCompleteObjectUndo(selection, string.IsNullOrEmpty(undoMessage) ? "Modify Vertices" : undoMessage);

            if (beforeMeshModification != null)
                beforeMeshModification(selection);

            if (currentEvent.shift)
                Extrude();

            m_IsEditing = true;

            m_ProgridsSnapEnabled = ProGridsInterface.SnapEnabled();
            m_ProgridsSnapValue = ProGridsInterface.SnapValue();
            m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

            foreach (var mesh in selection)
            {
                mesh.ToMesh();
                mesh.Refresh();
            }

            OnToolEngaged();
        }

        protected void FinishEdit()
        {
            if (!m_IsEditing)
                return;

            Lightmapping.PopGIWorkflowMode();

            OnToolDisengaged();

            var selection = MeshSelection.topInternal.ToArray();

            foreach (var mesh in selection)
            {
                mesh.ToMesh();
                mesh.Refresh();
                mesh.Optimize();
            }

            ProBuilderEditor.Refresh();

            if (afterMeshModification != null)
                afterMeshModification(selection);

            m_IsEditing = false;
        }

        static void Extrude()
        {
            int ef = 0;

            var selection = MeshSelection.topInternal;
            var selectMode = ProBuilderEditor.selectMode;

            foreach (var mesh in selection)
            {
                switch (selectMode)
                {
                    case SelectMode.Edge:
                        if (mesh.selectedFaceCount > 0)
                            goto default;

                        Edge[] newEdges = mesh.Extrude(mesh.selectedEdges,
                                0.0001f,
                                s_ExtrudeEdgesAsGroup,
                                ProBuilderEditor.s_AllowNonManifoldActions);

                        if (newEdges != null)
                        {
                            ef += newEdges.Length;
                            mesh.SetSelectedEdges(newEdges);
                        }
                        break;

                    default:
                        int len = mesh.selectedFacesInternal.Length;

                        if (len > 0)
                        {
                            mesh.Extrude(mesh.selectedFacesInternal, s_ExtrudeMethod, 0.0001f);
                            mesh.SetSelectedFaces(mesh.selectedFacesInternal);
                            ef += len;
                        }

                        break;
                }

                mesh.ToMesh();
                mesh.Refresh();
            }

            if (ef > 0)
            {
                EditorUtility.ShowNotification("Extrude");
                ProBuilderEditor.Refresh();
            }
        }

        /// <summary>
        /// Find the nearest vertex among all visible objects.
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        protected static bool FindNearestVertex(Vector2 mousePosition, out Vector3 vertex)
        {
            s_FindNearestVertexArguments[0] = mousePosition;

            if (s_FindNearestVertex == null)
                s_FindNearestVertex = typeof(HandleUtility).GetMethod("findNearestVertex",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

            object result = s_FindNearestVertex.Invoke(null, s_FindNearestVertexArguments);
            vertex = (bool)result ? (Vector3)s_FindNearestVertexArguments[2] : Vector3.zero;
            return (bool)result;
        }
    }
}
