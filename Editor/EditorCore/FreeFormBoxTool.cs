using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Object = UnityEngine.Object;
using PBMeshUtility = UnityEngine.ProBuilder.MeshUtility;
using Math = UnityEngine.ProBuilder.Math;

#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif


namespace UnityEditor.ProBuilder
{
    [EditorTool("Free Form Box", typeof(ProBuilderMesh))]
    sealed class FreeFormBoxTool : BoxManipulationTool
    {
        class InternalModification
        {
            public Vector3[] vertices;
            public Quaternion rotation;

            public InternalModification(Vector3[] v)
            {
                vertices = v;
                rotation = Quaternion.identity;
            }
        }

        Dictionary<ProBuilderMesh, InternalModification> m_Modifications ;

        void OnEnable()
        {
            InitTool();
            m_BoundsHandleColor = Handles.s_SelectedColor;
            m_OverlayTitle = new GUIContent("Free Form Box Tool");
            m_Modifications = new Dictionary<ProBuilderMesh, InternalModification>();

            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;

#if !UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged += OnActiveToolChanged;
            ToolManager.activeToolChanging += OnActiveToolChanging;
#endif
        }

        void OnDisable()
        {
#if !UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanged -= OnActiveToolChanged;
            ToolManager.activeToolChanging -= OnActiveToolChanging;
#endif
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;
        }

#if UNITY_2020_2_OR_NEWER
        /// <summary>
        ///   <para>Invoked after this EditorTool becomes the active tool.</para>
        /// </summary>
        public override void OnActivated()
        {
#else
        public void OnActiveToolChanged()
        {
            if(!ToolManager.IsActiveTool(this))
                return;
#endif
            foreach(var obj in targets)
            {
                var pbmesh = obj as ProBuilderMesh;
                RegisterMesh(pbmesh);
            }
        }

#if UNITY_2020_2_OR_NEWER
        /// <summary>
        ///   <para>Invoked before this EditorTool stops being the active tool.</para>
        /// </summary>
        public override void OnWillBeDeactivated()
        {
#else
        public void OnActiveToolChanging()
        {
            if(!ToolManager.IsActiveTool(this))
                return;
#endif
            m_Modifications.Clear();
        }

        void OnObjectSelectionChanged()
        {
            if(!ToolManager.IsActiveTool(this))
                return;

            foreach(var mesh in m_Modifications.Keys)
            {
                if(!targets.Contains((Object) mesh))
                    EditorApplication.delayCall += () => m_Modifications.Remove(mesh);
            }

            foreach(var obj in targets)
            {
                var pbmesh = obj as ProBuilderMesh;
                if(!m_Modifications.ContainsKey(pbmesh))
                {
                    RegisterMesh(pbmesh);
                }
            }
        }

        /// <summary>
        ///   <para>Use this method to implement a custom editor tool.</para>
        /// </summary>
        /// <param name="window">The window that is displaying the custom editor tool.</param>
        public override void OnToolGUI(EditorWindow window)
        {
            base.OnToolGUI(window);

            foreach (var obj in targets)
            {
                var pbmesh = obj as ProBuilderMesh;

                if (pbmesh != null)
                {
                    if(m_BoundsHandleActive && GUIUtility.hotControl == k_HotControlNone)
                        EndBoundsEditing();

                    if(Mathf.Approximately(pbmesh.transform.lossyScale.sqrMagnitude, 0f))
                        return;

                    DoManipulationGUI(pbmesh);
                }
            }
        }

        protected override void DoManipulationGUI(Object toolTarget)
        {
            ProBuilderMesh mesh = toolTarget as ProBuilderMesh;
            if(mesh == null)
                return;

            var matrix = IsEditing
                ? m_ActiveBoundsState.positionAndRotationMatrix
                : Matrix4x4.TRS(mesh.transform.position, mesh.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.SetColor(m_BoundsHandleColor);

                EditorShapeUtility.CopyColliderPropertiesToHandle(
                    mesh.transform, mesh.mesh.bounds,
                    m_BoundsHandle, IsEditing, m_ActiveBoundsState);

                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(mesh);
                    UndoUtility.RegisterCompleteObjectUndo(mesh, "Scale Mesh Bounds "+mesh.name);
                    EditorShapeUtility.CopyHandlePropertiesToCollider(m_BoundsHandle, m_ActiveBoundsState);
                    ApplyProperties(mesh);
                }

                DoRotateHandlesGUI(toolTarget, mesh, mesh.mesh.bounds);
            }
        }

        protected override void UpdateTargetRotation(Object toolTarget, Quaternion rotation)
        {
            ProBuilderMesh mesh = toolTarget as ProBuilderMesh;
            if(mesh == null)
                return;

            if ( rotation.Equals(Quaternion.identity) )
                return;

            UndoUtility.RegisterCompleteObjectUndo(mesh, "Rotate mesh");

            InternalModification currentModification = m_Modifications[mesh];
            currentModification.rotation = rotation * currentModification.rotation;

            Bounds bounds = mesh.mesh.bounds;

            var origVerts = new Vector3[mesh.vertexCount] ;
            Array.Copy(currentModification.vertices, origVerts, mesh.vertexCount);

            for (int i = 0; i < origVerts.Length; ++i)
                origVerts[i] = currentModification.rotation * origVerts[i] + bounds.center;

            mesh.mesh.vertices = origVerts;
            mesh.ReplaceVertices(origVerts);
            PBMeshUtility.FitToSize(mesh, bounds.size);
        }

        protected override void OnOverlayGUI(Object target, SceneView view)
        {
            m_snapAngle = EditorGUILayout.IntSlider(m_SnapAngleContent, m_snapAngle, 1, 90);
        }

        void RegisterMesh(ProBuilderMesh pbmesh)
        {
            var boundsOffset = pbmesh.mesh.bounds.center;
            if(boundsOffset.sqrMagnitude > float.Epsilon)
            {
                Undo.RecordObject(pbmesh, "Modifying Mesh Pivot");
                pbmesh.SetPivot(pbmesh.transform.position
                                + pbmesh.transform.TransformDirection(boundsOffset));
            }

            m_Modifications.Add(pbmesh, new InternalModification(pbmesh.positionsInternal));
        }

        void ApplyProperties(ProBuilderMesh mesh)
        {
            var trs = mesh.transform;
            var meshCenter = Handles.matrix.MultiplyPoint3x4(m_ActiveBoundsState.boundsHandleValue.center);
            var size = Math.Abs(Vector3.Scale(m_ActiveBoundsState.boundsHandleValue.size, Math.InvertScaleVector(trs.lossyScale)));

            PBMeshUtility.FitToSize(mesh, size);
            mesh.transform.position = meshCenter;
            ProBuilderEditor.Refresh(false);
        }
    }
}
