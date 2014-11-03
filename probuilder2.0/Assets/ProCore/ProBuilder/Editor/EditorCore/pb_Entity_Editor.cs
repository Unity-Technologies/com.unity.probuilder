using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

[CustomEditor(typeof(pb_Entity))]
[CanEditMultipleObjects]
public class pb_Entity_Editor : Editor
{
	pb_Entity ent;
	pb_Object pb;

	public enum ColType
	{
		MeshCollider,
		BoxCollider,
		SphereCollider
	}

	public void OnEnable()
	{
		ent = (pb_Entity)target;
		pb = (pb_Object)ent.transform.GetComponent<pb_Object>();
		// if(ent.colliderType != pb_Entity.ColliderType.Upgraded) ent.GenerateCollisions();
	}

	public override void OnInspectorGUI()
	{
		EntityType et = ent.entityType;
		et = (EntityType)EditorGUILayout.EnumPopup("Entity Type", et);
		if(et != ent.entityType)
		{
			pbUndo.RecordObjects(new Object[] {ent, ent.gameObject.GetComponent<pb_Object>() }, "Set Entity Type");
			pb_Editor_Utility.SetEntityType(et, ent.gameObject);
		}


		GUILayout.Space(4);

		pb.userCollisions = EditorGUILayout.Toggle("Custom Collider", pb.userCollisions);

		// Convience
		if(pb.userCollisions)
			GUI.enabled = false;

		GUILayout.Label("Add Collider", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();

			if(GUILayout.Button("Mesh Collider", EditorStyles.miniButtonLeft))
				EditorApplication.delayCall += AddMeshCollider;

			if(GUILayout.Button("Box Collider", EditorStyles.miniButtonMid))
				EditorApplication.delayCall += AddBoxCollider;

			if(GUILayout.Button("Remove Collider", EditorStyles.miniButtonRight))
				EditorApplication.delayCall += RemoveColliders;


		GUILayout.EndHorizontal();

		GUI.enabled = true;

		// GUILayout.Space(4);

		// if(GUI.changed)
		// 	EditorUtility.SetDirty(ent);
	}

	void AddMeshCollider() {
		AddCollider(ColType.MeshCollider);
	}

	void AddBoxCollider() {
		AddCollider(ColType.BoxCollider);
	}

	private void AddCollider(ColType c)
	{
		RemoveColliders();
		
		foreach(pb_Entity obj in serializedObject.targetObjects)
		{
			GameObject go = obj.gameObject;

			switch(c)
			{
				case ColType.MeshCollider:
					go.AddComponent<MeshCollider>();
					break;

				case ColType.BoxCollider:	
					go.AddComponent<BoxCollider>();
					break;

				case ColType.SphereCollider:	
					go.AddComponent<SphereCollider>();
					break;

				default:
					break;
			}
		}

	}

	private void RemoveColliders()
	{
		foreach(pb_Entity obj in serializedObject.targetObjects)
		{
			foreach(Collider c in obj.gameObject.GetComponents<Collider>())
				DestroyImmediate(c);
		}
	}
}
