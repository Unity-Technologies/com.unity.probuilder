using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class TextureMoveTool : TextureTool
	{
		Vector3 m_Position = Vector3.zero;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!isEditing)
				m_Position = Vector3.zero;

			EditorHandleUtility.PushMatrix();

			Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);

			EditorGUI.BeginChangeCheck();

			m_Position = Handles.Slider(m_Position, Vector3.up);

			if (EditorGUI.EndChangeCheck())
			{
			}

			EditorHandleUtility.PopMatrix();
		}
	}
}

//			UVEditor uvEditor = UVEditor.instance;
//			if (!uvEditor) return;
//
//			Vector3 cached = m_TextureHandlePosition;
//
//			m_TextureHandlePosition = Handles.PositionHandle(m_TextureHandlePosition, m_HandleRotation);
//
//			if (m_CurrentEvent.alt) return;
//
//			if (m_TextureHandlePosition != cached)
//			{
//				cached = Quaternion.Inverse(m_HandleRotation) * m_TextureHandlePosition;
//				cached.y = -cached.y;
//
//				Vector3 lossyScale = selection[0].transform.lossyScale;
//				Vector3 pos = cached.DivideBy(lossyScale);
//
//				if (!m_IsMovingTextures)
//				{
//					m_TextureHandlePositionPrevious = pos;
//					m_IsMovingTextures = true;
//				}
//
//				uvEditor.SceneMoveTool(pos - m_TextureHandlePositionPrevious);
//				m_TextureHandlePositionPrevious = pos;
//				uvEditor.Repaint();
//			}
