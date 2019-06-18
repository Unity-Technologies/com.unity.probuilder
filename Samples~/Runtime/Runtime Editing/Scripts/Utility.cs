using UnityEngine;
using UnityEngine.ProBuilder;

namespace ProBuilder.Examples
{
	struct MeshAndFace
	{
		public ProBuilderMesh mesh;
		public Face face;
	}

	static class Utility
	{
		internal static GameObject PickObject(Camera camera, Vector2 mousePosition)
		{
			var ray = camera.ScreenPointToRay(mousePosition);

			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
				return hit.collider.gameObject;

			return null;
		}

		internal static MeshAndFace PickFace(Camera camera, Vector3 mousePosition)
		{
			var res = new MeshAndFace();
			var go = PickObject(camera, mousePosition);

			if (go == null || !(res.mesh = go.GetComponent<ProBuilderMesh>()))
				return res;

			res.face = SelectionPicker.PickFace(camera, mousePosition, res.mesh);
			return res;
		}
	}
}
