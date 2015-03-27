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
			/**
			 * On older probuilder versions, SetUV also applied to mesh -
			 * this initializes the mesh so that SetUv() doesn't get a null
			 * ref when setting.
			 */

			pb.msh = new Mesh();

			if(!pb.gameObject.GetComponent<MeshRenderer>())
				pb.gameObject.AddComponent<MeshRenderer>();

			pb.SetVertices( serialized.GetVertices() );

			pb.msh.vertices = pb.vertices;

			if(!pb_UpgradeKitUtils.InvokeFunction(pb, "SetUV", new object[] { (object)serialized.GetUVs() } ))
				pb.msh.uv = serialized.GetUVs();

			if(!pb_UpgradeKitUtils.InvokeFunction(pb, "SetColors", new object[] { (object)serialized.GetColors() } ))
				pb.msh.colors = serialized.GetColors();

			pb_UpgradeKitUtils.InvokeFunction(pb, "SetSharedIndices", new object[] { (object)serialized.GetSharedIndices().ToPbIntArray() } );

			pb_UpgradeKitUtils.InvokeFunction(pb, "SetSharedIndicesUV", new object[] { (object)serialized.GetSharedIndicesUV().ToPbIntArray() } );

			pb.SetFaces( serialized.GetFaces() );

			pb_UpgradeKitUtils.RebuildMesh(pb);

			pb.GenerateUV2(true);

			pb_Entity entity = pb.GetComponent<pb_Entity>();
			if(entity == null) entity = pb.gameObject.AddComponent<pb_Entity>();
			entity.SetEntity( 0 );
		}
	}
}