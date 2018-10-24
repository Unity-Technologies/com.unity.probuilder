using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class ScaleTool : VertexManipulationTool
	{
		Vector3 m_Scale;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!m_IsEditing)
				m_Scale = Vector3.one;

			EditorGUI.BeginChangeCheck();

			m_Scale = Handles.ScaleHandle(m_Scale, handlePosition, handleRotation, HandleUtility.GetHandleSize(handlePosition));

			if (EditorGUI.EndChangeCheck())
			{
				if (!m_IsEditing)
					BeginEdit();

				Apply(Matrix4x4.Scale(m_Scale));
			}
		}
	}

}
