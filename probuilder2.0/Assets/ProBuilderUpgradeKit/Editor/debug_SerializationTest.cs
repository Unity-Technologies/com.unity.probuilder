using UnityEngine;
using System.Collections;
using UnityEditor;
using ProBuilder2.Common;
using Newtonsoft.Json;

namespace ProBuilder2.UpgradeKit
{
	public class debug_SerializationTest : Editor {

		// [MenuItem("Tools/SERIALIZE TEST")]
		static void tdsotidsj()
		{
			foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
			{
				pb_SerializableObject pbobj = new pb_SerializableObject(pb);
				pb_SerializableEntity entity = new pb_SerializableEntity(pb.GetComponent<pb_Entity>());
					
				string obj = JsonConvert.SerializeObject(pbobj, Formatting.Indented);
				string ent = JsonConvert.SerializeObject(entity, Formatting.Indented);
				
				GameObject go = new GameObject();

				go.transform.position = pb.transform.position + Vector3.right * 2;

				pb_Object pb2 = go.AddComponent<pb_Object>();
				pb_Entity et2 = go.GetComponent<pb_Entity>();	///< pb_Object [requires] component pb_Entity.

				pb_SerializableObject deserialized_object = (pb_SerializableObject) JsonConvert.DeserializeObject<pb_SerializableObject>(obj);
				pb_SerializableEntity deserialized_entity = (pb_SerializableEntity) JsonConvert.DeserializeObject<pb_SerializableEntity>(ent);

				pb_EditorUpgradeKitUtils.InitObjectWithSerializedObject(pb2, deserialized_object);
				pb_EditorUpgradeKitUtils.InitEntityWithSerializedObject(et2, deserialized_entity);

				pb2.GenerateUV2(true);
			}
		}
	}
}