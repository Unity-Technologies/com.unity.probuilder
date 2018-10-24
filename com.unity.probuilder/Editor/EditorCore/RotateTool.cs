using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class RotateTool : VertexManipulationTool
	{
		Quaternion m_Rotation;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (Tools.pivotMode == PivotMode.Pivot)
			{
				EditorGUI.BeginChangeCheck();

				if (!m_IsEditing)
					m_Rotation = Quaternion.identity;

				var hm = Handles.matrix;
				Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);
				m_Rotation = Handles.RotationHandle(m_Rotation, Vector3.zero);
				Handles.matrix = hm;

				if (EditorGUI.EndChangeCheck())
				{
					if (!m_IsEditing)
						BeginEdit();

					Apply(Matrix4x4.Rotate(m_Rotation));
				}
			}
			else
			{
				EditorGUI.BeginChangeCheck();

				if (!m_IsEditing)
					m_Rotation = handleRotation;

				m_Rotation = Handles.RotationHandle(m_Rotation, handlePosition);

				if (EditorGUI.EndChangeCheck())
				{
					if (!m_IsEditing)
						BeginEdit();

					Apply(Matrix4x4.Rotate(m_Rotation * Quaternion.Inverse(handleRotationOrigin)));
				}
			}


			Handles.BeginGUI();
			GUILayout.Label("Rotation: " + m_Rotation.eulerAngles);
			Handles.EndGUI();
		}
	}
}