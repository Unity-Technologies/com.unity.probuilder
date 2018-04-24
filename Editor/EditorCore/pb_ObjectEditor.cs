using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.ProBuilder;
using ProBuilder.MeshOperations;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Custom editor for pb_Object type.
	/// </summary>
	[CustomEditor(typeof(pb_Object))]
	[CanEditMultipleObjects]
	class pb_ObjectEditor : Editor
	{
		public delegate void OnGetFrameBoundsDelegate();

		public static event OnGetFrameBoundsDelegate OnGetFrameBoundsEvent;

		pb_Object pb;

		ProBuilderEditor editor
		{
			get { return ProBuilderEditor.instance; }
		}

		Renderer ren = null;
		Vector3 offset = Vector3.zero;

		public void OnEnable()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			if (target is pb_Object)
				pb = (pb_Object) target;
			else
				return;

			ren = pb.gameObject.GetComponent<Renderer>();
			SelectionRenderState s = EditorUtility.GetSelectionRenderState();
			EditorUtility.SetSelectionRenderState(ren, editor != null ? s & SelectionRenderState.Outline : s);

			// If Verify returns false, that means the mesh was rebuilt - so generate UV2 again

			foreach (pb_Object selpb in Selection.transforms.GetComponents<pb_Object>())
				EditorUtility.VerifyMesh(selpb);
		}

		public override void OnInspectorGUI()
		{
			GUI.backgroundColor = Color.green;

			if (GUILayout.Button("Open " + pb_Constant.PRODUCT_NAME))
				ProBuilderEditor.MenuOpenWindow();

			GUI.backgroundColor = Color.white;

			if (!ren)
				return;

			Vector3 sz = ren.bounds.size;

			EditorGUILayout.Vector3Field("Object Size (read only)", sz);

#if PB_DEBUG
			GUILayout.TextField( string.IsNullOrEmpty(pb.asset_guid) ? "null" : pb.asset_guid );
#endif

			if (pb == null) return;

			if (pb.SelectedTriangles.Length > 0)
			{
				GUILayout.Space(5);

				offset = EditorGUILayout.Vector3Field("Quick Offset", offset);

				if (GUILayout.Button("Apply Offset"))
				{
					foreach (pb_Object ipb in Selection.transforms.GetComponents<pb_Object>())
					{
						UndoUtility.RecordObject(ipb, "Offset Vertices");

						ipb.ToMesh();

						ipb.TranslateVertices_World(ipb.SelectedTriangles, offset);

						ipb.Refresh();
						ipb.Optimize();
					}

					if (editor != null)
						editor.UpdateSelection();
				}
			}
		}

		bool HasFrameBounds()
		{
			if (pb == null)
				pb = (pb_Object) target;

			return ProBuilderEditor.instance != null &&
			       pb_Util.GetComponents<pb_Object>(Selection.transforms).Sum(x => x.SelectedTriangles.Length) > 0;
		}

		Bounds OnGetFrameBounds()
		{
			if (OnGetFrameBoundsEvent != null) OnGetFrameBoundsEvent();

			Vector3 min = Vector3.zero, max = Vector3.zero;
			bool init = false;

			foreach (pb_Object pbo in pb_Util.GetComponents<pb_Object>(Selection.transforms))
			{
				if (pbo.SelectedTriangles.Length < 1) continue;

				Vector3[] verts = pbo.VerticesInWorldSpace(pbo.SelectedTriangles);

				if (!init)
				{
					init = true;
					min = verts[0];
					max = verts[0];
				}

				for (int i = 0; i < verts.Length; i++)
				{
					min.x = Mathf.Min(verts[i].x, min.x);
					max.x = Mathf.Max(verts[i].x, max.x);

					min.y = Mathf.Min(verts[i].y, min.y);
					max.y = Mathf.Max(verts[i].y, max.y);

					min.z = Mathf.Min(verts[i].z, min.z);
					max.z = Mathf.Max(verts[i].z, max.z);
				}
			}

			return new Bounds((min + max) / 2f, max != min ? max - min : Vector3.one * .1f);
		}
	}
}