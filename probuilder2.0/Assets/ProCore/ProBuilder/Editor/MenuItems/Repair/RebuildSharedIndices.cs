using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

namespace ProBuilder2.Actions
{
	public class RebuildSharedIndices : Editor
	{
		// rebuilds the shared index cache for every pb_Object in scene.
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Shared Indices Cache", false, pb_Constant.MENU_REPAIR)]
		public static void Rebuild()
		{
			pb_Object[] pbObjs = (pb_Object[])GameObject.FindObjectsOfType(typeof(pb_Object));
			foreach(pb_Object pb in pbObjs)
			{
				pb_IntArray[] val = pb.sharedIndices;
					
				List<int> empty = new List<int>();

				for(int i = 0; i < val.Length; i++)
					if(val[i].IsEmpty())
						empty.Add(i);
				
				pb_IntArray[] trimmed = new pb_IntArray[val.Length-empty.Count];
				
				int n = 0;
				for(int i = 0; i < trimmed.Length; i++)
					if(!empty.Contains(i))
						trimmed[n++] = val[i];
							
				pb.SetSharedIndices( trimmed );
			}
		}
	}
}