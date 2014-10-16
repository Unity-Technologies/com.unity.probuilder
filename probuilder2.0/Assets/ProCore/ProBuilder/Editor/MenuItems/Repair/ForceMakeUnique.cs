using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.Actions
{
	public class ForceMakeUnique : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Force 'Make Unique' All Scene Objects", false, pb_Constant.MENU_REPAIR)]
		public static void Inuit()
		{
			foreach(pb_Object pb in Resources.FindObjectsOfTypeAll(typeof(pb_Object)))	
				pb.MakeUnique();
		}
	}
}