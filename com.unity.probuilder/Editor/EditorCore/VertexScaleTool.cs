using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class VertexScaleTool : VertexTool
	{
		Vector3 m_Scale;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			base.DoTool(handlePosition, handleRotation);

			if (!isEditing)
				m_Scale = Vector3.one;

			EditorGUI.BeginChangeCheck();

			m_Scale = Handles.ScaleHandle(m_Scale, handlePosition, handleRotation, HandleUtility.GetHandleSize(handlePosition));

			if (EditorGUI.EndChangeCheck())
			{
				if (!isEditing)
					BeginEdit("Scale Selection");

				Apply(Matrix4x4.Scale(m_Scale));
			}
		}
	}

}
