using UnityEngine;
using System.Collections;
using UnityEditor;
using ProBuilder2.Common;
using Newtonsoft.Json;

namespace ProBuilder2.UpgradeKit
{
	public class debug_SerializationTest : Editor {

		[MenuItem("Tools/SERIALIZE")]
		static void tdsotidsj()
		{
			foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
			{
				pb_SerializableObject ser = new pb_SerializableObject(pb);
					
				string json = JsonConvert.SerializeObject(ser, Formatting.Indented);
				
				Debug.Log(json);
				
				GameObject go = new GameObject();
				go.transform.position = pb.transform.position + Vector3.right * 2;

				pb_Object pb2 = go.AddComponent<pb_Object>();
				InitObjectWithSerializedObject(pb2, ser);
			}
		}


		static void InitObjectWithSerializedObject(pb_Object pb, pb_SerializableObject serialized)
		{
			pb.SetVertices( serialized.GetVertices() );

			pb.msh = new Mesh();	// on older probuilder versions, SetUV also applied to mesh - this initializes the mesh so that 
									// SetUv() doesn't get a null ref when setting.
			pb.msh.vertices = pb.vertices;

			pb.SetUV( serialized.GetUVs() );

			pb.SetColors( serialized.GetColors() );

			pb.SetSharedIndices( serialized.GetSharedIndices().ToPbIntArray() );
			pb.SetSharedIndicesUV( serialized.GetSharedIndicesUV().ToPbIntArray() );

			pb.SetFaces( serialized.GetFaces() );

			pb.ToMesh();
			pb.Refresh();
			pb.GenerateUV2(true);

			pb.GetComponent<pb_Entity>().SetEntity(EntityType.Detail);
		}
	}
}