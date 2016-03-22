using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 * CustomEditor implementation for @pb_Entity.
	 */
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

			if(ent != null)
				pb = (pb_Object)ent.transform.GetComponent<pb_Object>();
			// if(ent.colliderType != pb_Entity.ColliderType.Upgraded) ent.GenerateCollisions();
		}

		public override void OnInspectorGUI()
		{
			if(pb == null) return;
			if(ent == null) return;
			
			EntityType et = ent.entityType;
			et = (EntityType)EditorGUILayout.EnumPopup("Entity Type", et);
			if(et != ent.entityType)
			{
				pbUndo.RecordObjects(new Object[] {ent, ent.gameObject.GetComponent<pb_Object>() }, "Set Entity Type");

				pb_Editor_Utility.SetEntityType(et, ent.gameObject);
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
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
			Collider[] colliders = serializedObject.targetObjects.Where(x => x is pb_Entity).SelectMany(x => ((pb_Entity)x).gameObject.GetComponents<Collider>()).ToArray();
			bool isTrigger = false;
			if( colliders != null )
				isTrigger = colliders.Any(x => x.isTrigger);

			RemoveColliders();
			
			foreach(pb_Entity obj in serializedObject.targetObjects)
			{
				GameObject go = obj.gameObject;

				switch(c)
				{
					case ColType.MeshCollider:
					{
						MeshCollider col = go.AddComponent<MeshCollider>();

						if(ent.entityType == EntityType.Trigger)
						{
							col.convex = true;
							col.isTrigger = true;
						}
						else if(ent.entityType == EntityType.Collider)
						{
							col.convex = true;
						}
						else if(isTrigger)
						{
							col.convex = true;
							col.isTrigger = true;
						}

						break;
					}

					case ColType.BoxCollider:	
					{
						BoxCollider col = go.AddComponent<BoxCollider>();

						if(ent.entityType == EntityType.Trigger || isTrigger)
							col.isTrigger = true;
						break;
					}

					case ColType.SphereCollider:	
					{
						SphereCollider col = go.AddComponent<SphereCollider>();
						if(ent.entityType == EntityType.Trigger || isTrigger)
							col.isTrigger = true;
						break;
					}

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
}