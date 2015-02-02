using UnityEngine;
using UnityEditor;

public class pb_VertexPainter : EditorWindow
{

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Experimental/Vertex Painter Tool", false, 0)]
	public static void MenuOpenVertexPainterWindow()
	{
		pb_VertexColor_Editor.Init();
	}
}