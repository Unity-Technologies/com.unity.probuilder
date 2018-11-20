using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class TextureRotateTool : TextureTool
	{
		float m_Rotation;
		Vector3 m_Euler;
		Quaternion m_Quaternion;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!isEditing)
				m_Rotation = 0f;

			EditorGUI.BeginChangeCheck();

			var size = HandleUtility.GetHandleSize(handlePosition);
			
			EditorHandleUtility.PushMatrix();

			Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);

			Handles.color = Color.blue;
			m_Euler.z = m_Rotation;
			m_Quaternion = Quaternion.Euler(m_Euler);
			m_Quaternion = Handles.Disc(m_Quaternion, Vector3.zero, Vector3.forward, size, relativeSnapEnabled, relativeSnapRotation);
			m_Euler = m_Quaternion.eulerAngles;
			m_Rotation = m_Euler.z;

			EditorHandleUtility.PopMatrix();

			if (EditorGUI.EndChangeCheck())
			{
				if(!isEditing)
					BeginEdit("Rotate Textures");

				if (relativeSnapEnabled)
					m_Rotation = Snapping.SnapValue(m_Rotation, relativeSnapX);
				else if (progridsSnapEnabled)
					m_Rotation = Snapping.SnapValue(m_Rotation, progridsSnapValue);

				foreach (var mesh in meshAndElementGroupPairs)
				{
					if (!(mesh is MeshAndTextures))
						continue;

					var origins = ((MeshAndTextures)mesh).origins;
					var positions = ((MeshAndTextures)mesh).textures;

					foreach (var group in mesh.elementGroups)
					{
						foreach(var index in group.indices)
							positions[index] = group.postApplyMatrix.MultiplyPoint(
								Math.RotateAroundPoint(
									group.preApplyMatrix.MultiplyPoint3x4(origins[index]), Vector2.zero, -m_Rotation));
					}

					mesh.mesh.mesh.SetUVs(k_TextureChannel, positions);
				}
			}
		}
	}
}
