using UnityEngine;

namespace UnityEditor.ProBuilder
{
	class TextureScaleTool : TextureTool
	{
		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
//			UVEditor uvEditor = UVEditor.instance;
//			if (!uvEditor) return;
//
//			float size = HandleUtility.GetHandleSize(m_HandlePosition);
//
//			Matrix4x4 prev = Handles.matrix;
//			Handles.matrix = handleMatrix;
//
//			Vector3 cached = m_TextureScale;
//			m_TextureScale = Handles.ScaleHandle(m_TextureScale, Vector3.zero, Quaternion.identity, size);
//
//			if (m_CurrentEvent.alt) return;
//
//			if (cached != m_TextureScale)
//			{
//				if (!m_IsMovingTextures)
//					m_IsMovingTextures = true;
//
//				uvEditor.SceneScaleTool(m_TextureScale, cached);
//			}
//
//			Handles.matrix = prev;
		}
	}
}
