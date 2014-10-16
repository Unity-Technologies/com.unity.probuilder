using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProBuilder2.Actions
{
	public class ComponentCheck : Editor
	{	
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Validate Components", false, pb_Constant.MENU_REPAIR)]
		public static void MenuProBuilderValidateComponents()
		{
			foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))	
			{
				if(!pb.gameObject.GetComponent<pb_Entity>())
					pb.gameObject.AddComponent<pb_Entity>();
			}

			foreach(pb_Entity ent in FindObjectsOfType(typeof(pb_Entity)))
			{
				if(!ent.gameObject.GetComponent<pb_Object>())
					DestroyImmediate(ent);
			}
		}
	}
}