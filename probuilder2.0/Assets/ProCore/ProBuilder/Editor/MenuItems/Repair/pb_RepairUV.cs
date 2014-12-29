using UnityEditor;
using UnityEngine;

public class pb_RepairUV : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Invert UV Scale (Scene)", false, pb_Constant.MENU_REPAIR + 30)]
	public static void MenuInvertSceneFaceUVScale()
	{
		pb_Upgrade.InvertUVScale_Scene();
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Invert UV Scale (Selected Objects)", false, pb_Constant.MENU_REPAIR + 30)]
	public static void MenuInvertSelectedObjectsUVScale()
	{
		pb_Upgrade.InvertUVScale_Selection();
	}
	
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Invert UV Scale (Selected Faces)", false, pb_Constant.MENU_REPAIR + 30)]
	public static void MenuInvertSelectedFacesUVScale()
	{
		pb_Upgrade.InvertUVScale_SelectedFaces();
	}
}