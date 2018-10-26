using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class MoveTool : VertexManipulationTool
	{
		const float k_CardinalAxisError = .001f;
		Vector3 m_HandlePosition;
		Matrix4x4 m_Translation = Matrix4x4.identity;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			if (!m_IsEditing)
				m_HandlePosition = handlePosition;

			EditorGUI.BeginChangeCheck();

			m_HandlePosition = Handles.PositionHandle(m_HandlePosition, handleRotation);

			if (EditorGUI.EndChangeCheck())
			{
				if (!m_IsEditing)
					BeginEdit("Translate Selection");

				var delta = m_HandlePosition - handlePositionOrigin;

				if (vertexDragging)
				{
					Vector3 nearest;

					if (FindNearestVertex(currentEvent.mousePosition, out nearest))
					{
						var unrotated = handleRotationOriginInverse * delta;
						var dir = Math.ToMask(unrotated, k_CardinalAxisError);

						if (dir.IntSum() == 1)
						{
							var rot_dir = handleRotationOrigin * dir * 10000f;

							m_HandlePosition = HandleUtility.ProjectPointLine(nearest,
								handlePositionOrigin + rot_dir,
								handlePositionOrigin - rot_dir);

							delta = m_HandlePosition - handlePositionOrigin;
						}
					}
				}
				else if (snapEnabled)
				{
					var travel = delta.magnitude;
					delta = delta.normalized * Snapping.SnapValue(travel, snapValue);
					m_HandlePosition = handlePosition + delta;
				}

				switch (pivotPoint)
				{
					case PivotPoint.WorldBoundingBoxCenter:
						break;

					case PivotPoint.ModelBoundingBoxCenter:
						delta = handleRotationOriginInverse * delta;
						break;

					case PivotPoint.IndividualOrigins:
						delta = handleRotationOriginInverse * delta;
						break;
				}

				m_Translation.SetTRS(delta, Quaternion.identity, Vector3.one);

				Apply(m_Translation);
			}
		}
	}
}