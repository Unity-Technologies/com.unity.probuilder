using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class TextureScaleTool : TextureTool
    {
        Vector2 m_Scale;
        float m_UniformScale;

        protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            if (!isEditing)
            {
                m_Scale.x = 1f;
                m_Scale.y = 1f;
                m_UniformScale = 1f;
            }

            EditorGUI.BeginChangeCheck();

            var size = HandleUtility.GetHandleSize(handlePosition);

            EditorHandleUtility.PushMatrix();

            Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);

            var snap = relativeSnapEnabled
                ? relativeSnapScale
                : progridsSnapEnabled
                ? progridsSnapValue
                : 0f;

            Handles.color = Color.red;
            m_Scale.x = Handles.ScaleSlider(m_Scale.x, Vector3.zero, Vector3.right, Quaternion.identity, size, snap);

            Handles.color = Color.green;
            m_Scale.y = Handles.ScaleSlider(m_Scale.y, Vector3.zero, Vector3.up, Quaternion.identity, size, snap);

            Handles.color = Color.blue;
            m_UniformScale = Handles.ScaleValueHandle(m_UniformScale, Vector3.zero, Quaternion.identity, size, Handles.CubeHandleCap, snap);

            EditorHandleUtility.PopMatrix();

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                    BeginEdit("Scale Textures");

                var delta = m_Scale * m_UniformScale;

                delta.x = 1f / delta.x;
                delta.y = 1f / delta.y;

                foreach (var mesh in elementSelection)
                {
                    if (!(mesh is MeshAndTextures))
                        continue;

                    var mat = (MeshAndTextures) mesh;
                    var origins = mat.origins;
                    var positions = mat.textures;

                    foreach (var group in mesh.elementGroups)
                    {
                        foreach (var index in group.indices)
                            positions[index] = mat.postApplyMatrix.MultiplyPoint(
                                    Vector2.Scale(mat.preApplyMatrix.MultiplyPoint3x4(origins[index]), delta));
                    }

                    mesh.mesh.mesh.SetUVs(k_TextureChannel, positions);
                }
            }
        }
    }
}
