using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProBuilder2.Common;
using Newtonsoft.Json;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Serialization
{
	/**
 	 * Methods for storing data about ProBuilder objects that may be translated back into PB post-upgrade.
	 */
	public class pb_UpgradeBridgeEditor : Editor
	{
		[MenuItem("Tools/ProBuilder/Upgrade/Prepare Scene for Upgrade")]
		[MenuItem("Tools/SERIALIZE")]
		static void MenuSerialize()
		{
			if(pb_Editor.instance != null)
				pb_Editor.instance.Close();

			foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())// Resources.FindObjectsOfTypeAll(typeof(pb_Object)))
			{
				pb_SerializableObject serializedObject = new pb_SerializableObject(pb);
				pb_SerializableEntity serializedEntity = new pb_SerializableEntity(pb.GetComponent<pb_Entity>());

				string obj = JsonConvert.SerializeObject(serializedObject, Formatting.Indented);
				string entity = JsonConvert.SerializeObject(serializedEntity, Formatting.Indented);
				
				pb_SerializedComponent storage = pb.gameObject.GetComponent<pb_SerializedComponent>() ?? pb.gameObject.AddComponent<pb_SerializedComponent>();

				storage.SetObjectData(obj);
				storage.SetEntityData(entity);

				RemoveProBuilderScripts(pb);
			}
		}

		[MenuItem("Tools/ProBuilder/Upgrade/Re-attach ProBuilder Scripts")]
		[MenuItem("Tools/RESERIALIZE")]
		static void MenuDeserialize()
		{
			pb_SerializedComponent[] serializedComponents = Selection.transforms.GetComponents<pb_SerializedComponent>();

			for(int i = 0; i < serializedComponents.Length; i++)
			{
				pb_SerializedComponent ser = serializedComponents[i];

				try
				{
					pb_SerializableObject serializedObject = JsonConvert.DeserializeObject<pb_SerializableObject>(ser.GetObjectData());
					pb_SerializableEntity serializedEntity = JsonConvert.DeserializeObject<pb_SerializableEntity>(ser.GetEntityData());

					pb_Object pb = ser.gameObject.GetComponent<pb_Object>() ?? ser.gameObject.AddComponent<pb_Object>();
					InitObjectWithSerializedObject(pb, serializedObject);

					pb_Entity ent = ser.gameObject.GetComponent<pb_Entity>() ?? ser.gameObject.AddComponent<pb_Entity>();
					InitEntityWithSerializedObject(ent, serializedEntity);
				}
				catch(System.Exception e)
				{
					Debug.LogError("Failed deserializing object: " + ser.gameObject.name + "\nObject ID: " + ser.gameObject.GetInstanceID() + "\n" + e.ToString());
					continue;
				}

				DestroyImmediate( ser );
			}
		}

		/**
		 * Initialize a pb_Object component with data from a pb_SerializableObject.
		 * If you are initializing a completely new pb_Object, use pb_Object.InitWithSerializableObject() instead.
		 */
		static void InitObjectWithSerializedObject(pb_Object pb, pb_SerializableObject serialized)
		{
			pb.SetVertices( serialized.vertices );
			pb.SetUV( serialized.uv );
			pb.SetColors( serialized.color );

			pb.SetSharedIndices( serialized.sharedIndices.ToPbIntArray() );
			pb.SetSharedIndicesUV( serialized.sharedIndicesUV.ToPbIntArray() );

			pb.SetFaces( serialized.faces );

			pb.ToMesh();
			pb.Refresh();

			pb.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);
		}

		static void InitEntityWithSerializedObject(pb_Entity entity, pb_SerializableEntity serialized)
		{
			// SetEntityType is an extension method (editor-only) that also sets the static flags to 
			// match the entity use.
			entity.SetEntityType(serialized.entityType);
		}

		static void RemoveProBuilderScripts(pb_Object pb)
		{
			GameObject go = pb.gameObject;

			// Copy the mesh (since destroying pb_Object will take the mesh reference with it)
			Mesh m = pbUtil.DeepCopyMesh(pb.msh);

			// Destroy pb_Object first, then entity.  Order is important.
			DestroyImmediate(pb);
			
			if(go.GetComponent<pb_Entity>())
				DestroyImmediate(go.GetComponent<pb_Entity>());

			// Set the mesh back.
			go.GetComponent<MeshFilter>().sharedMesh = m;

			if(go.GetComponent<MeshCollider>())
				go.GetComponent<MeshCollider>().sharedMesh = m;
		}
	}
}