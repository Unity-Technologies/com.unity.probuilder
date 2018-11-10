using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class PositionScaleTool : PositionTool
	{
		Vector3 m_Scale;

		protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
		{
			base.DoTool(handlePosition, handleRotation);

			if (!isEditing)
				m_Scale = Vector3.one;

			EditorGUI.BeginChangeCheck();

			var size = HandleUtility.GetHandleSize(handlePosition);
			EditorHandleUtility.PushMatrix();
			Handles.matrix = Matrix4x4.TRS(handlePosition, handleRotation, Vector3.one);
			m_Scale = Handles.ScaleHandle(m_Scale, Vector3.zero, Quaternion.identity, size);
			EditorHandleUtility.PopMatrix();

			if (EditorGUI.EndChangeCheck())
			{
				if (!isEditing)
					BeginEdit("Scale Selection");

				Apply(Matrix4x4.Scale(m_Scale));
			}
		}

		void ApplyScale(Vector3 scale)
		{
			foreach (var key in meshAndElementGroupPairs)
			{
				if (!(key is MeshAndPositions))
					continue;

				var kvp = (MeshAndPositions)key;
				var mesh = kvp.mesh;
				var worldToLocal = mesh.transform.worldToLocalMatrix;
				var origins = kvp.positions;
				var positions = mesh.positionsInternal;

				foreach (var group in kvp.elementGroups)
				{
					foreach (var index in group.indices)
					{
						// res = Group pre-apply matrix * world vertex position
						// res += translation
						// res = Group post-apply matrix * res
						// positions[i] = mesh.worldToLocal * res
						positions[index] = worldToLocal.MultiplyPoint3x4(
							group.postApplyMatrix.MultiplyPoint3x4(
								Vector3.Scale(group.preApplyMatrix.MultiplyPoint3x4(origins[index]), scale)));
					}
				}

				mesh.mesh.vertices = positions;
				mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
				mesh.Refresh(RefreshMask.Normals);
			}

			ProBuilderEditor.UpdateMeshHandles(false);
		}
	}

}
