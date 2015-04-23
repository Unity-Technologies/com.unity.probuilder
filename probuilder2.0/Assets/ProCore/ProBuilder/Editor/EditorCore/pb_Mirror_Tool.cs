using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Mirrors selected pb_Objects.
	 */
	public class pb_Mirror_Tool : EditorWindow 
	{
		#if !PROTOTYPE

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Mirror Tool")]
		public static void InitMirrorTool()
		{
			EditorWindow win = EditorWindow.GetWindow(typeof(pb_Mirror_Tool), true, "Mirror Tool", true);
			win.Show();
		}

		bool scaleX = false, scaleY = false, scaleZ = true;
		public void OnGUI()
		{
			GUILayout.Label("Mirror Axis", EditorStyles.boldLabel);
			scaleX = EditorGUILayout.Toggle("X", scaleX);
			scaleY = EditorGUILayout.Toggle("Y", scaleY);
			scaleZ = EditorGUILayout.Toggle("Z", scaleZ);

			if(GUILayout.Button("Mirror"))
			{
				List<GameObject> mirrors = new List<GameObject>();

				foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
				{
					pb_Object result = pb_Mirror_Tool.Mirror(pb, new Vector3(
						(scaleX) ? -1f : 1f,
						(scaleY) ? -1f : 1f,
						(scaleZ) ? -1f : 1f
						));

					mirrors.Add(result.gameObject);
				}

				if(pb_Editor.instance != null)
					pb_Editor.instance.SetSelection(mirrors.ToArray());
				else
					Selection.objects = mirrors.ToArray();
				
				SceneView.RepaintAll();
			}
		}

		/**
		 *	\brief Duplicates and mirrors the passed pb_Object.
		 *	@param pb The donor pb_Object.
		 *	@param axe The axis to mirror the object on.
		 *	\returns The newly duplicated pb_Object.
		 *	\sa ProBuilder.Axis
		 */
		public static pb_Object Mirror(pb_Object pb, Vector3 scale)
		{
			pb_Object p = pb_Object.InitWithObject(pb);
			p.MakeUnique();

			p.transform.parent = pb.transform.parent;

			p.transform.localRotation = pb.transform.localRotation;

			Vector3 lScale = p.gameObject.transform.localScale;

			p.transform.localScale = new Vector3(lScale.x * scale.x, lScale.y * scale.y, lScale.z * scale.z);

			// if flipping on an odd number of axes, flip winding order
			if( (scale.x * scale.y * scale.z) < 0)
				p.ReverseWindingOrder(p.faces);

			p.FreezeScaleTransform();

			p.transform.localScale = pb.transform.localScale;
			
			// Regenerate normals
			p.ToMesh();
			p.Refresh();
			p.Optimize();

			pb_Editor_Utility.InitObjectFlags(p, ColliderType.MeshCollider, pb.GetComponent<pb_Entity>().entityType);
			
			// InitObjectFlags runs ScreenCenter()
			p.transform.position = pb.transform.position;

			Undo.RegisterCreatedObjectUndo(p.gameObject, "Mirror Object");

			return p;
		}

		#endif
	}
}