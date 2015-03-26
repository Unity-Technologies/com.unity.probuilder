using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProBuilder2.Common;
using Newtonsoft.Json;
using ProBuilder2.EditorCommon;
using tmp = ProBuilder2.SerializationTmp;

namespace ProBuilder2.Serialization
{
	static class BackwardsCompatibilityExtensions
	{
		public static void SetColors(this pb_Object pb, Color[] colors) { Debug.Log("FUCK");}
		public static Color[] GetColors(this pb_SerializableObject ser) { Debug.Log("DOUBLEFUCK"); return null; }
	}

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

			pb_Object[] objects = (pb_Object[])Resources.FindObjectsOfTypeAll(typeof(pb_Object));
			float len = objects.Length;
			int success = 0;

			if( len < 1 )
			{
				int c = ((pb_SerializedComponent[]) Resources.FindObjectsOfTypeAll(typeof(pb_SerializedComponent))).Length;

				if( c > 0 )
					EditorUtility.DisplayDialog("Prepare Scene", "There are " + c + " serialized components ready to be rebuilt into ProBuilder components in this scene.", "Okay");
				else
					EditorUtility.DisplayDialog("Prepare Scene", "No ProBuilder components found in scene.", "Okay");
			}
			else
			{
				if( !EditorUtility.DisplayDialog("Prepare Scene", "This will safely store all ProBuilder data in a new component, and remove ProBuilder components from all objects in the scene.\n\nThis must be run for each scene in your project.", "Okay", "Cancel") )
					return;

				foreach(pb_Object pb in objects)
				{
					EditorUtility.DisplayProgressBar("Serialize ProBuilder Data", "Object: " + pb.name, success / len);

					try
					{
						tmp.pb_SerializableObject serializedObject = new tmp.pb_SerializableObject(pb);
						pb_SerializableEntity serializedEntity = new pb_SerializableEntity(pb.GetComponent<pb_Entity>());

						string obj = JsonConvert.SerializeObject(serializedObject, Formatting.Indented);
						string entity = JsonConvert.SerializeObject(serializedEntity, Formatting.Indented);
						
						pb_SerializedComponent storage = pb.gameObject.GetComponent<pb_SerializedComponent>() ?? pb.gameObject.AddComponent<pb_SerializedComponent>();

						storage.SetObjectData(obj);
						storage.SetEntityData(entity);

						RemoveProBuilderScripts(pb);

						success++;
					}
					catch (System.Exception e)
					{
						Debug.LogError("Failed serializing: " + pb.name + "\nId: " + pb.gameObject.GetInstanceID() + "\nThis object will not be safely upgraded if you continue the process!\n" + e.ToString());
					}
				}

				EditorUtility.ClearProgressBar();

				EditorUtility.DisplayDialog("Prepare Scene", "Successfully serialized " + success + " / " + (int)len + " objects.", "Okay");
			}
		}

		[MenuItem("Tools/ProBuilder/Upgrade/Re-attach ProBuilder Scripts")]
		[MenuItem("Tools/DESERIALIZE")]
		static void MenuDeserialize()
		{
			pb_SerializedComponent[] serializedComponents = (pb_SerializedComponent[])Resources.FindObjectsOfTypeAll(typeof(pb_SerializedComponent));

			if( serializedComponents.Length < 1 )
			{
				EditorUtility.DisplayDialog("Deserialize ProBuilder Data", "No serialized ProBuilder components found in this scene.", "Okay");
			}
			else
			{
				int success = 0;
				float len = serializedComponents.Length;

				for(int i = 0; i < serializedComponents.Length; i++)
				{
					pb_SerializedComponent ser = serializedComponents[i];

					EditorUtility.DisplayProgressBar("Deserialize ProBuilder Data", "Object: " + ser.gameObject.name, success / len);

					try
					{
						pb_SerializableObject serializedObject = JsonConvert.DeserializeObject<pb_SerializableObject>(ser.GetObjectData());
						pb_SerializableEntity serializedEntity = JsonConvert.DeserializeObject<pb_SerializableEntity>(ser.GetEntityData());

						pb_Object pb = ser.gameObject.GetComponent<pb_Object>() ?? ser.gameObject.AddComponent<pb_Object>();
						InitObjectWithSerializedObject(pb, serializedObject);

						pb_Entity ent = ser.gameObject.GetComponent<pb_Entity>() ?? ser.gameObject.AddComponent<pb_Entity>();
						InitEntityWithSerializedObject(ent, serializedEntity);

						success++;
					}
					catch(System.Exception e)
					{
						Debug.LogError("Failed deserializing object: " + ser.gameObject.name + "\nObject ID: " + ser.gameObject.GetInstanceID() + "\n" + e.ToString());
						continue;
					}

					DestroyImmediate( ser, true );
				}

				EditorUtility.ClearProgressBar();

				EditorUtility.DisplayDialog("Deserialize ProBuilder Data", "Successfully deserialized " + success + " / " + (int)len + " objects.", "Okay");
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
			pb.SetColors( serialized.GetColors() );

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

			pb.Verify();

			if(pb.msh == null)
				return;

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