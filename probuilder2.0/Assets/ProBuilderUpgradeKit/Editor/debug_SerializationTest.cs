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
			}
		}
	}
}