using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

#if DEBUG_HANDLES
using UnityEngine.Rendering;
#endif

namespace UnityEditor.ProBuilder
{
    abstract class VertexManipulationTool
    {
        static Pref<HandleOrientation> s_HandleOrientation = new Pref<HandleOrientation>("editor.handleOrientation", HandleOrientation.World, SettingsScope.User);
#if PROBUILDER_ENABLE_HANDLE_OVERRIDE
        static Pref<PivotPoint> s_PivotPoint = new Pref<PivotPoint>("editor.pivotPoint", PivotPoint.Center, SettingsScope.User);
#endif

        // Enable this define to access PivotPoint.ActiveSelection. This also has the effect of ignoring Tools.pivotMode and Tools.pivotRotation settings.
#if !PROBUILDER_ENABLE_HANDLE_OVERRIDE
        static PivotRotation s_PivotRotation;
#endif

        public static PivotPoint pivotPoint
        {
#if PROBUILDER_ENABLE_HANDLE_OVERRIDE
            get { return s_PivotPoint; }
            set { s_PivotPoint.SetValue(value, true); }
#else
            get
            {
                return Tools.pivotMode == PivotMode.Pivot
                    ? Experimental.pivotModePivotEquivalent
                    : PivotPoint.Center;
            }
#endif
        }

        public static HandleOrientation handleOrientation
        {
            get
            {
#if !PROBUILDER_ENABLE_HANDLE_OVERRIDE
                SyncPivotRotation();
#endif
                return s_HandleOrientation;
            }
            set
            {
                s_HandleOrientation.SetValue(value, true);

#if !PROBUILDER_ENABLE_HANDLE_OVERRIDE
                if (value != HandleOrientation.ActiveElement)
                    Tools.pivotRotation = value == HandleOrientation.ActiveObject
                        ? PivotRotation.Local
                        : PivotRotation.Global;

                var toolbar = typeof(EditorWindow).Assembly.GetType("UnityEditor.Toolbar");
                var repaint = toolbar.GetMethod("RepaintToolbar", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                repaint.Invoke(null, null);
#endif
            }
        }

#if !PROBUILDER_ENABLE_HANDLE_OVERRIDE
        // Sync ProBuilder HandleOrientation to the current Tools.PivotRotation
        static void SyncPivotRotation()
        {
            if (s_PivotRotation != Tools.pivotRotation)
            {
                s_HandleOrientation.SetValue(Tools.pivotRotation == PivotRotation.Global
                    ? HandleOrientation.World
                    : HandleOrientation.ActiveObject);
                s_PivotRotation = Tools.pivotRotation;
                return;
            }

            var value = s_HandleOrientation.value;
            var unity = value == HandleOrientation.ActiveObject ? PivotRotation.Local : PivotRotation.Global;

            if (value != HandleOrientation.ActiveElement)
            {
                if (unity != Tools.pivotRotation)
                    s_HandleOrientation.SetValue(Tools.pivotRotation == PivotRotation.Global
                        ? HandleOrientation.World
                        : HandleOrientation.ActiveObject,
                        true);
            }
        }

#endif

        /// <value>
        /// Called when vertex modifications are complete.
        /// </value>
        public static event Action<ProBuilderMesh[]> afterMeshModification;

        /// <value>
        /// Called immediately prior to beginning vertex modifications. The ProBuilderMesh will be in un-altered state at this point (meaning ProBuilderMesh.ToMesh and ProBuilderMesh.Refresh have been called, but not Optimize).
        /// </value>
        public static event Action<ProBuilderMesh[]> beforeMeshModification;

        internal static Pref<bool> s_ExtrudeEdgesAsGroup = new Pref<bool>("editor.extrudeEdgesAsGroup", true);
        internal static Pref<ExtrudeMethod> s_ExtrudeMethod = new Pref<ExtrudeMethod>("editor.extrudeMethod", ExtrudeMethod.FaceNormal);

        Vector3 m_HandlePosition;
        Quaternion m_HandleRotation;
        Vector3 m_HandlePositionOrigin;
        Quaternion m_HandleRotationOrigin;
        List<MeshAndElementGroupPair> m_MeshAndElementGroupPairs = new List<MeshAndElementGroupPair>();
        bool m_IsEditing;

        float m_ProgridsSnapValue = .25f;
        bool m_SnapAxisConstraint = true;
        bool m_ProgridsSnapEnabled;
        static bool s_Initialized;
        static FieldInfo s_VertexDragging;
        static MethodInfo s_FindNearestVertex;
        static object[] s_FindNearestVertexArguments = new object[] { null, null, null };

        protected IEnumerable<MeshAndElementGroupPair> meshAndElementGroupPairs
        {
            get { return m_MeshAndElementGroupPairs; }
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

        protected abstract MeshAndElementGroupPair GetMeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation);

        public void OnSceneGUI(Event evt)
        {
            currentEvent = evt;

            if (evt.type == EventType.MouseUp || evt.type == EventType.Ignore)
                FinishEdit();

            if (!m_IsEditing)
            {
                m_HandlePosition = MeshSelection.GetHandlePosition();
                m_HandleRotation = MeshSelection.GetHandleRotation();
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

            m_HandlePositionOrigin = m_HandlePosition;
            m_HandleRotationOrigin = m_HandleRotation;
            handleRotationOriginInverse = Quaternion.Inverse(m_HandleRotation);

            m_ProgridsSnapEnabled = ProGridsInterface.SnapEnabled();
            m_ProgridsSnapValue = ProGridsInterface.SnapValue();
            m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

            foreach (var mesh in selection)
            {
                mesh.ToMesh();
                mesh.Refresh();
            }

            m_MeshAndElementGroupPairs.Clear();

            foreach (var mesh in MeshSelection.topInternal)
                m_MeshAndElementGroupPairs.Add(GetMeshAndElementGroupPair(mesh, pivotPoint, handleOrientation));

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
