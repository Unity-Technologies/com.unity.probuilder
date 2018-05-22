using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <inheritdoc />
	/// <summary>
	/// Custom editor for pb_Object type.
	/// </summary>
	[CustomEditor(typeof(ProBuilderMesh))]
	[CanEditMultipleObjects]
	sealed class ProBuilderMeshEditor : Editor
	{
		public delegate void OnGetFrameBoundsDelegate();

		public static event OnGetFrameBoundsDelegate OnGetFrameBoundsEvent;

		ProBuilderMesh pb;

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

			if (target is ProBuilderMesh)
				pb = (ProBuilderMesh) target;
			else
				return;

			ren = pb.gameObject.GetComponent<Renderer>();
			SelectionRenderState s = EditorUtility.GetSelectionRenderState();
			EditorUtility.SetSelectionRenderState(ren, editor != null ? s & SelectionRenderState.Outline : s);

			// If Verify returns false, that means the mesh was rebuilt - so generate UV2 again

			foreach (ProBuilderMesh selpb in Selection.transforms.GetComponents<ProBuilderMesh>())
				EditorUtility.EnsureMeshSyncState(selpb);
		}

		public override void OnInspectorGUI()
		{
			GUI.backgroundColor = Color.green;

			if (GUILayout.Button("Open " + PreferenceKeys.pluginTitle))
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

			if (pb.selectedIndicesInternal.Length > 0)
			{
				GUILayout.Space(5);

				offset = EditorGUILayout.Vector3Field("Quick Offset", offset);

				if (GUILayout.Button("Apply Offset"))
				{
					foreach (ProBuilderMesh ipb in Selection.transforms.GetComponents<ProBuilderMesh>())
					{
						UndoUtility.RecordObject(ipb, "Offset Vertices");

						ipb.ToMesh();

						ipb.TranslateVerticesInWorldSpace(ipb.selectedIndicesInternal, offset);

						ipb.Refresh();
						ipb.Optimize();
					}

					ProBuilderEditor.Refresh();
				}
			}
		}

		bool HasFrameBounds()
		{
			if (pb == null)
				pb = (ProBuilderMesh) target;

			return ProBuilderEditor.instance != null &&
			       InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms).Sum(x => x.selectedIndicesInternal.Length) > 0;
		}

		Bounds OnGetFrameBounds()
		{
			if (OnGetFrameBoundsEvent != null) OnGetFrameBoundsEvent();

			Vector3 min = Vector3.zero, max = Vector3.zero;
			bool init = false;

			foreach (ProBuilderMesh mesh in InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms))
			{
				int[] tris = mesh.selectedIndicesInternal;

				if (tris == null || tris.Length < 1)
					continue;

				Vector3[] verts = mesh.positionsInternal;
				var trs = mesh.transform;

				if (!init)
				{
					init = true;
					min = trs.TransformPoint(verts[tris[0]]);
					max = trs.TransformPoint(verts[tris[0]]);
				}

				for (int i = 0, c = tris.Length; i < c; i++)
				{
					Vector3 p = trs.TransformPoint(verts[tris[i]]);

					min.x = Mathf.Min(p.x, min.x);
					max.x = Mathf.Max(p.x, max.x);

					min.y = Mathf.Min(p.y, min.y);
					max.y = Mathf.Max(p.y, max.y);

					min.z = Mathf.Min(p.z, min.z);
					max.z = Mathf.Max(p.z, max.z);
				}
			}

			return new Bounds((min + max) / 2f, max != min ? max - min : Vector3.one * .1f);
		}
	}
}
