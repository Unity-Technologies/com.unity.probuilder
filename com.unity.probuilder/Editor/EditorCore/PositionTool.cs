//#define DEBUG_HANDLES

using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    abstract class PositionTool : VertexManipulationTool
    {
        const bool k_CollectCoincidentVertices = true;

#if APPLY_POSITION_TO_SPACE_GIZMO
        Matrix4x4 m_CurrentDelta = Matrix4x4.identity;
#endif

        protected class MeshAndPositions : MeshAndElementGroupPair
        {
            Vector3[] m_Positions;

            public Vector3[] positions
            {
                get { return m_Positions; }
            }

            public MeshAndPositions(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation) : base(mesh, pivot, orientation, k_CollectCoincidentVertices)
            {
                m_Positions = mesh.positions.ToArray();

                var l2w = mesh.transform.localToWorldMatrix;

                for (int i = 0, c = m_Positions.Length; i < c; i++)
                    m_Positions[i] = l2w.MultiplyPoint3x4(m_Positions[i]);
            }
        }

        protected override MeshAndElementGroupPair GetMeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation)
        {
            return new MeshAndPositions(mesh, pivot, orientation);
        }

        protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            if (isEditing && currentEvent.type == EventType.Repaint)
            {
                foreach (var key in meshAndElementGroupPairs)
                {
                    foreach (var group in key.elementGroups)
                    {
#if DEBUG_HANDLES
                        var positions = ((MeshAndPositions)key).positions;

                        using (var faceDrawer = new EditorMeshHandles.TriangleDrawingScope(Color.cyan,
                                       UnityEngine.Rendering.CompareFunction.Always))
                        {
                            foreach (var face in key.mesh.GetSelectedFaces())
                            {
                                var indices = face.indexesInternal;

                                for (int i = 0, c = indices.Length; i < c; i += 3)
                                {
                                    faceDrawer.Draw(
                                        group.preApplyMatrix.MultiplyPoint3x4(positions[indices[i]]),
                                        group.preApplyMatrix.MultiplyPoint3x4(positions[indices[i + 1]]),
                                        group.preApplyMatrix.MultiplyPoint3x4(positions[indices[i + 2]])
                                        );
                                }
                            }
                        }
#endif
                    }
                }
            }
        }

        protected void Apply(Matrix4x4 delta)
        {
#if APPLY_POSITION_TO_SPACE_GIZMO
            m_CurrentDelta.SetColumn(3, delta.GetColumn(3));
#endif

            foreach (var key in meshAndElementGroupPairs)
            {
                if (!(key is MeshAndPositions))
                    continue;

                var kvp = (MeshAndPositions)key;
                var mesh = kvp.mesh;
                var worldToLocal = mesh.transform.worldToLocalMatrix;
                var origins = kvp.positions;
                var positions = mesh.positionsInternal;

                foreach (var group in kvp.elementGroups)
                {
                    foreach (var index in group.indices)
                    {
                        positions[index] = worldToLocal.MultiplyPoint3x4(
                                group.postApplyMatrix.MultiplyPoint3x4(
                                    delta.MultiplyPoint3x4(group.preApplyMatrix.MultiplyPoint3x4(origins[index]))));
                    }
                }

                mesh.mesh.vertices = positions;
                mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
                mesh.Refresh(RefreshMask.Normals);
            }

            ProBuilderEditor.UpdateMeshHandles(false);
        }
    }
}
