using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class VertexRotateTool : VertexTool
	{
		Quaternion m_Rotation;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			base.DoTool(handlePosition, handleRotation);

			if (Tools.pivotMode == PivotMode.Pivot)
			{
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

					Apply(Matrix4x4.Rotate(m_Rotation));
				}
			}
			else
			{
				EditorGUI.BeginChangeCheck();

				if (!isEditing)
					m_Rotation = handleRotation;

				m_Rotation = Handles.RotationHandle(m_Rotation, handlePosition);

				if (EditorGUI.EndChangeCheck())
				{
					if (!isEditing)
						BeginEdit("Rotate Selection");

					Apply(Matrix4x4.Rotate(m_Rotation * Quaternion.Inverse(handleRotationOrigin)));
				}
			}
		}
	}
}