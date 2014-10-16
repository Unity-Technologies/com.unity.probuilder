#if !PROTOTYPE
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Math;

public class AutoNodraw : EditorWindow
{

	const float COLLISION_DISTANCE = .02f;
	float userSetCollisionDistance = .02f;
	public static Material nodrawMat;

	static pb_Editor editor { get { return pb_Editor.instance; } }

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Auto Nodraw Tool", true, pb_Constant.MENU_TOOLS)]
	public static bool VerifyOpenAuotNdorwa()
	{
		return pb_Editor.instance != null;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Auto Nodraw Tool", false, pb_Constant.MENU_TOOLS)]
	public static void AutoNodrawWindow()
	{
		EditorWindow.GetWindow(typeof(AutoNodraw), true, "NoDraw Tool");
	}

	bool autoUpate = true;
	public void OnGUI()
	{
		userSetCollisionDistance = EditorGUILayout.Slider("Collsion Distance Check", userSetCollisionDistance, .001f, 1f);
		if(userSetCollisionDistance < .0001)
			userSetCollisionDistance = .0001f;

		autoUpate = EditorGUILayout.Toggle("Auto Update Selection", autoUpate);

		if(autoUpate && GUI.changed)
			SelectHiddenFaces(editor, userSetCollisionDistance);

		if(GUILayout.Button("Apply NoDraw"))
		{
			pb_Material_Editor.ApplyMaterial(editor.selection, pb_Constant.NoDrawMaterial);
			editor.ClearFaceSelection();
		}
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Select Hidden Faces")]
	public static void FindHiddenFaces()
	{
		SelectHiddenFaces( editor, COLLISION_DISTANCE);
	}

	public static void SelectHiddenFaces(pb_Editor editor, float collision_distance)
	{
		// Open the pb_Editor window if it isn't already open.
		// pb_Editor editor = pb_Editor.instance;

		// Clear out all selected
		editor.ClearSelection();

		// If we're in Mode based editing, make sure that we're also in geo mode. 
		editor.SetEditLevel(EditLevel.Geometry);

		// aaand also set to face seelction mode
		editor.SetSelectionMode(SelectMode.Face);

		// Find all ProBuilder objects in the scene.
		pb_Object[] pbs = (pb_Object[])Object.FindObjectsOfType(typeof(pb_Object));

		// Cycle through every quad
		foreach(pb_Object pb in pbs)
		{
			// Ignore if it isn't a detail or occluder
			if(pb.GetComponent<pb_Entity>().entityType != EntityType.Detail && pb.GetComponent<pb_Entity>().entityType != EntityType.Occluder)
				continue;	

			bool addToSelection = false;

			for(int i = 0; i < pb.faces.Length; i++)
			{
				pb_Face q = pb.faces[i];
				if(HiddenFace(pb, q, collision_distance))
				{
					// If a hidden face is found, set material to NoDraw
					// pb.SetQuadMaterial(q, nodrawMat);

					// Mark this object to be added to selection
					addToSelection = true;

					// Add hit face to SelectedFaces
					pb.AddToFaceSelection(i);
				}

			}

			if(addToSelection)
				editor.AddToSelection(pb.gameObject);
		}

		editor.UpdateSelection();
	}

	public static bool HiddenFace(pb_Object pb, pb_Face q, float dist)
	{
		// Grab the face normal
		Vector3 dir = pb_Math.Normal(pb.VerticesInWorldSpace(q.indices));

		// If casting from the center of the plane hits, chekc the rest of the points for collisions
		Vector3 orig = pb.transform.TransformPoint(pb_Math.Average(pb.GetVertices(q)));

		bool hidden = true;
		Transform hitObj = RaycastFaceCheck(orig, dir, dist, null);
		if(hitObj != null)
		{
			Vector3[] v = pb.VerticesInWorldSpace(q.indices);
			for(int i = 0; i < v.Length; i++)
			{
				if(null == RaycastFaceCheck(v[i], dir, dist, hitObj))
				{
					hidden = false;
					break;
				}
			}
		}
		else
			hidden = false;

		return hidden;
	}

	public static Transform RaycastFaceCheck(Vector3 origin, Vector3 dir, float dist, Transform targetTransform)
	{
		RaycastHit hit;
		if(Physics.Raycast(origin, dir, out hit, dist)) {
			// We've hit something.  Now check to see if it is a ProBuilder object,
			// and if so, make sure it's a visblocking brush.

			// if targetTransform isn't null, make sure that the hit object matches 
			if(targetTransform != null)
			{
				if(hit.transform != targetTransform)
					return null;
			}

			pb_Entity ent = hit.transform.GetComponent<pb_Entity>();
			if(ent != null)
			{
				if(ent.entityType == EntityType.Detail || ent.entityType == EntityType.Occluder)
					return hit.transform;		// it's a brush, blocks vision, return true
				else
					return null;		// not a vis blocking brush
			}
		}

		// It ain't a ProBuilder object of the entity type Brush or Occluder (world brush)
		return null;
	}
}
#endif