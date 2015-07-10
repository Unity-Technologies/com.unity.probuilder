#if !PROTOTYPE
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{

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
		
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Color Painter", false, pb_Constant.MENU_WINDOW + 4)]
		public static void InitVertexColorPainter()
		{
			pb_VertexColor_Editor.MenuOpenWindow();
		}

	#endif
	}
}
#endif