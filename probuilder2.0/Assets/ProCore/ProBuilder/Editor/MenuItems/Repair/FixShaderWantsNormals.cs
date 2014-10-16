using UnityEngine;
using UnityEditor;
using System.Collections;

public class FixShaderWantsNormals : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Fix \"Shader Wants Normals\" Warning", false, pb_Constant.MENU_REPAIR)]
	public static void MenuShaderWantsNormals()
	{
		pb_Object[] pbObjs = (pb_Object[])Resources.FindObjectsOfTypeAll(typeof(pb_Object));
		pb_Object[] prefabs = System.Array.FindAll(pbObjs, x => PrefabUtility.GetPrefabType(x.gameObject) == PrefabType.Prefab );
		
		foreach(pb_Object pb in prefabs)
		{
			pb.Refresh();
			EditorUtility.SetDirty(pb);
		}
	}
}
