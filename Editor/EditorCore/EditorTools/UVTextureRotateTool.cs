using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    internal class UVTextureRotateTool : UVTextureTool
    {

        float m_Rotation;
        Vector3 m_Euler;
        Quaternion m_Quaternion;

        public override void OnToolGUI(EditorWindow window)
        {
            base.OnToolGUI(window);

            if (!isEditing)
                m_Rotation = 0f;

            EditorGUI.BeginChangeCheck();

            var size = HandleUtility.GetHandleSize(m_HandlePosition);

            EditorHandleUtility.PushMatrix();

            Handles.matrix = Matrix4x4.TRS(m_HandlePosition, m_HandleRotation, Vector3.one);

            Handles.color = Color.blue;
            m_Euler.z = m_Rotation;
            m_Quaternion = Quaternion.Euler(m_Euler);
            m_Quaternion = Handles.Disc(m_Quaternion, Vector3.zero, Vector3.forward, size, relativeSnapEnabled, ProBuilderSnapSettings.incrementalSnapRotateValue);
            m_Euler = m_Quaternion.eulerAngles;
            m_Rotation = m_Euler.z;

            EditorHandleUtility.PopMatrix();

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                    BeginEdit("Rotate Textures");

                if (relativeSnapEnabled)
                    m_Rotation = ProBuilderSnapping.SnapValue(m_Rotation, ProBuilderSnapSettings.incrementalSnapRotateValue);

                foreach (var mesh in elementSelection)
                {
                    if (!(mesh is MeshAndTextures))
                        continue;
                    var mat = (MeshAndTextures) mesh;

                    var origins = mat.origins;
                    var positions = mat.textures;

                    foreach (var group in mat.elementGroups)
                    {
                        foreach (var index in group.indices)
                            positions[index] = mat.postApplyMatrix.MultiplyPoint(
                                    Math.RotateAroundPoint(
                                        mat.preApplyMatrix.MultiplyPoint3x4(origins[index]), Vector2.zero, -m_Rotation));
                    }

                    mesh.mesh.mesh.SetUVs(k_TextureChannel, positions);
                }
            }
        }
    }
}
