using UnityEngine;
using UnityEditor;
using System.Collections;

/**
 * Due to the Prototype build process, windows that are exclusive to ProBuilder cannot 
 * simply be #if def'ed out .
 */
public class pb_ProBuilderMenuItems : Editor 
{
#if !PROTOTYPE
	[MenuItem("Tools/ProBuilder/UV Editor Window", false, pb_Constant.MENU_WINDOW + 2)]
	public static void OpenUVWindow()
	{
		pb_UV_Editor.MenuOpenUVEditor();
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Material Palette", false, pb_Constant.MENU_WINDOW + 3)]
	public static void InitMaterialEditor()
	{
		pb_Material_Editor.MenuOpenMaterialEditor();
	}
#endif
}
