using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

public class pb_VertexPainter : EditorWindow
{

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Experimental/Vertex Painter Tool", false, pb_Constant.MENU_EXPERIMENTAL + 0)]
	public static void MenuOpenVertexPainterWindow()
	{
		pb_VertexColor_Editor.Init();
	}
}