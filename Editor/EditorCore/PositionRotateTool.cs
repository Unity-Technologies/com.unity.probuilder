using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class PositionRotateTool : PositionTool
    {
        Quaternion m_Rotation;

        protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            base.DoTool(handlePosition, handleRotation);

            if (showHandleInfo && isEditing)
            {
                var euler = m_Rotation.eulerAngles;

                DrawDeltaInfo(string.Format("Rotate: {0,5:##0.00}, {1,5:##0.00}, {2,5:##0.00}",
                    euler.x,
                    euler.y,
                    euler.z));
            }

            EditorGUI.BeginChangeCheck();

            if (!isEditing)
                m_Rotation = Quaternion.identity;

            var hm = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);
            m_Rotation = Handles.RotationHandle(m_Rotation, Vector3.zero);
            Handles.matrix = hm;

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                    BeginEdit("Rotate Selection");

                ApplyRotation(m_Rotation);
            }
        }

        void ApplyRotation(Quaternion rotation)
        {
            foreach (var key in elementSelection)
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
                    var postApplyMatrix = GetPostApplyMatrix(group);
                    var preApplyMatrix = postApplyMatrix.inverse;

                    foreach (var index in group.indices)
                    {
                        positions[index] = worldToLocal.MultiplyPoint3x4(
                                postApplyMatrix.MultiplyPoint3x4(
                                    rotation * preApplyMatrix.MultiplyPoint3x4(origins[index])));
                    }
                }

                mesh.mesh.vertices = positions;
                mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
                mesh.Refresh(RefreshMask.Normals);
            }

            ProBuilderEditor.Refresh(false);
        }
    }
}
