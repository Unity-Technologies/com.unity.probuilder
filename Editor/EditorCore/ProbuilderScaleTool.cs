using UnityEngine;

namespace UnityEditor.ProBuilder
{
    class ProbuilderScaleTool : PositionTool
    {
        Vector3 m_Scale;

        protected override void DoToolGUI()
        {
            if (showHandleInfo && isEditing)
                DrawDeltaInfo("Scale: " + m_Scale.ToString("0.00"));

            if (!isEditing)
                m_Scale = Vector3.one;

            EditorGUI.BeginChangeCheck();

            var size = HandleUtility.GetHandleSize(m_HandlePosition);

            EditorHandleUtility.PushMatrix();
            Handles.matrix = Matrix4x4.TRS(m_HandlePosition, m_HandleRotation, Vector3.one);
            m_Scale = Handles.ScaleHandle(m_Scale, Vector3.zero, Quaternion.identity, size);
            EditorHandleUtility.PopMatrix();

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                    BeginEdit("Scale Selection");

                Apply(Matrix4x4.Scale(m_Scale));
            }
        }
    }
}
