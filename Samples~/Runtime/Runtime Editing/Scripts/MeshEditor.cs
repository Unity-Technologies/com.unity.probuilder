using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using PMath = UnityEngine.ProBuilder.Math;

namespace ProBuilder.Examples
{
    class MeshEditor : MonoBehaviour
    {
        Camera m_SceneCamera;
        CameraMotion m_CameraMotion;
        MeshAndFace m_Selection;

        class MeshState
        {
            public ProBuilderMesh mesh;
            public Vector3[] vertices;
            public Vector3[] origins;
            public List<int> indices;

            public MeshState(ProBuilderMesh mesh, IList<int> selectedIndices)
            {
                this.mesh = mesh;
                vertices = mesh.positions.ToArray();
                indices = mesh.GetCoincidentVertices(selectedIndices);
                origins = new Vector3[indices.Count];

                for (int i = 0, c = indices.Count; i < c; i++)
                    origins[i] = vertices[indices[i]];
            }
        }

        class DragState
        {
            public bool active;
            public Ray constraint;
            public float offset;
            public MeshState meshState;
        }

        DragState m_DragState = new DragState();

        void Awake()
        {
            m_SceneCamera = Camera.main;
            m_CameraMotion = m_SceneCamera.GetComponent<CameraMotion>();
            Camera.onPostRender += DrawSelection;
        }

        void Start()
        {
            m_CameraMotion.Focus(Vector3.zero, 10f);
        }

        void Update()
        {
            if(!m_DragState.active)
                m_Selection = Utility.PickFace(m_SceneCamera, Input.mousePosition);

            HandleInput();
        }

        void DrawSelection(Camera cam)
        {
            if (m_CameraMotion.active)
                return;

            Handles.Draw(m_Selection.mesh, m_Selection.face, Color.cyan);

            if (m_DragState.active)
            {
                var o = m_DragState.constraint.origin;
                var d = m_DragState.constraint.direction;
                Handles.DrawLine(o - d * 100f, o + d * 1000f, Color.green);
            }
        }

        void LateUpdate()
        {
            if (!m_DragState.active)
                m_CameraMotion.DoLateUpdate();
        }

        void HandleInput()
        {
            if (m_CameraMotion.active)
                return;

            if (Input.GetMouseButtonDown(0) && m_Selection.face != null)
            {
                BeginDrag();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
            else if (m_DragState.active && Input.GetMouseButton(0))
            {
                UpdateDrag();
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                if (m_Selection.mesh != null)
                    m_CameraMotion.Focus(m_Selection.mesh.gameObject);
                else
                    m_CameraMotion.Focus(Vector3.zero, 10f);
            }
        }

        void BeginDrag()
        {
            if (m_DragState.active || m_Selection.mesh == null || m_Selection.face == null)
                return;

            m_DragState.active = true;

            var trs = m_Selection.mesh.transform;

            // The constraint ray is stored in world space
            var origin = trs.TransformPoint(PMath.Average(m_Selection.mesh.positions, m_Selection.face.indexes));
            var direction = trs.TransformDirection(PMath.Normal(m_Selection.mesh, m_Selection.face));

            m_DragState.constraint = new Ray(origin, direction);
            m_DragState.meshState = new MeshState(m_Selection.mesh, m_Selection.face.distinctIndexes);
            m_DragState.offset = GetDragDistance();
        }

        void EndDrag()
        {
            m_DragState.active = false;
        }

        void UpdateDrag()
        {
            var distance = GetDragDistance() - m_DragState.offset;

            var mesh = m_Selection.mesh;
            var indices = m_DragState.meshState.indices;
            var vertices = m_DragState.meshState.vertices;
            var origins = m_DragState.meshState.origins;
            // Constraint is in world coordinates, but we need model space when applying changes to mesh values.
            var direction = mesh.transform.InverseTransformDirection(m_DragState.constraint.direction);

            for (int i = 0, c = indices.Count; i < c; i++)
                vertices[indices[i]] = origins[i] + direction * distance;

            mesh.positions = vertices;
            mesh.ToMesh();
            mesh.Refresh();
        }

        float GetDragDistance()
        {
            Ray constraint = m_DragState.constraint;
            Ray mouse = m_SceneCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 nearestPoint = PMath.GetNearestPointRayRay(constraint, mouse);
            float sign = System.Math.Sign(Vector3.Dot(nearestPoint - constraint.origin, constraint.direction));
            return Vector3.Distance(constraint.origin, nearestPoint) * sign;
        }
    }
}
