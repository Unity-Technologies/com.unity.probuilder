#if UNITY_4_5 || UNITY_4_5_0 || UNITY_4_5_1 || UNITY_4_5_2 || UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_5_6 || UNITY_4_5_7 || UNITY_4_5_8 || UNITY_4_5_9 || UNITY_4_6 || UNITY_4_6_0 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9 || UNITY_4_7 || UNITY_4_7_0 || UNITY_4_7_1 || UNITY_4_7_2 || UNITY_4_7_3 || UNITY_4_7_4 || UNITY_4_7_5 || UNITY_4_7_6 || UNITY_4_7_7 || UNITY_4_7_8 || UNITY_4_7_9 || UNITY_4_8 || UNITY_4_8_0 || UNITY_4_8_1 || UNITY_4_8_2 || UNITY_4_8_3 || UNITY_4_8_4 || UNITY_4_8_5 || UNITY_4_8_6 || UNITY_4_8_7 || UNITY_4_8_8 || UNITY_4_8_9 || UNITY_5
#define UNITY_4_5
#define UNITY_4_3
#define UNITY_4
#endif
#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5 || UNITY_4_3_6 || UNITY_4_3_7 || UNITY_4_3_8 || UNITY_4_3_9 || UNITY_4_4 || UNITY_4_4_0 || UNITY_4_4_1 || UNITY_4_4_2 || UNITY_4_4_3 || UNITY_4_4_4 || UNITY_4_4_5 || UNITY_4_4_6 || UNITY_4_4_7 || UNITY_4_4_8 || UNITY_4_4_9 || UNITY_4_5 || UNITY_4_5_0 || UNITY_4_5_1 || UNITY_4_5_2 || UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_5_6 || UNITY_4_5_7 || UNITY_4_5_8 || UNITY_4_5_9 || UNITY_4_6 || UNITY_4_6_0 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9 || UNITY_4_7 || UNITY_4_7_0 || UNITY_4_7_1 || UNITY_4_7_2 || UNITY_4_7_3 || UNITY_4_7_4 || UNITY_4_7_5 || UNITY_4_7_6 || UNITY_4_7_7 || UNITY_4_7_8 || UNITY_4_7_9 || UNITY_4_8 || UNITY_4_8_0 || UNITY_4_8_1 || UNITY_4_8_2 || UNITY_4_8_3 || UNITY_4_8_4 || UNITY_4_8_5 || UNITY_4_8_6 || UNITY_4_8_7 || UNITY_4_8_8 || UNITY_4_8_9
#define UNITY_4_3
#define UNITY_4
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

public class pb_MirrorTool : EditorWindow 
{
	#if !PROTOTYPE

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Mirror Tool")]
	public static void InitMirrorTool()
	{
		EditorWindow win = EditorWindow.GetWindow(typeof(pb_MirrorTool), true, "Mirror Tool", true);
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
			foreach(pb_Object pb in pbUtil.GetComponents<pb_Object>(Selection.transforms))
			{
				pb_MirrorTool.Mirror(pb, new Vector3(
					(scaleX) ? -1f : 1f,
					(scaleY) ? -1f : 1f,
					(scaleZ) ? -1f : 1f
					));
			}
			
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
		pb_Object p = ProBuilder.CreateObjectWithObject(pb);
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
		
		p.Refresh();
		p.GenerateUV2(true);

		pb_Editor_Utility.InitObjectFlags(p, ColliderType.MeshCollider, pb.GetComponent<pb_Entity>().entityType);
		
		// InitObjectFlags runs ScreenCenter()
		p.transform.position = pb.transform.position;

		Undo.RegisterCreatedObjectUndo(p.gameObject, "Mirror Object");

		return p;
	}

	#endif
}
