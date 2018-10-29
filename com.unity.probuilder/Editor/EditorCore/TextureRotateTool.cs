using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class TextureRotateTool : TextureTool
	{
		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
//			UVEditor uvEditor = UVEditor.instance;
//			if (!uvEditor) return;
//
//			float size = HandleUtility.GetHandleSize(m_HandlePosition);
//
//			if (m_CurrentEvent.alt) return;
//
//			Matrix4x4 prev = Handles.matrix;
//			Handles.matrix = handleMatrix;
//
//			Quaternion cached = m_TextureRotation;
//
//			m_TextureRotation = Handles.Disc(m_TextureRotation, Vector3.zero, Vector3.forward, size, false, 0f);
//
//			if (m_TextureRotation != cached)
//			{
//				if (!m_IsMovingTextures)
//					m_IsMovingTextures = true;
//
//				uvEditor.SceneRotateTool(-m_TextureRotation.eulerAngles.z);
//			}
//
//			Handles.matrix = prev;
		}
	}
}
