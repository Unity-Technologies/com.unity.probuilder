using UnityEngine;
using UnityEditor;

public class pb_VertexPainter : EditorWindow
{

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Vertex Painter Tool")]
	public static void MenuOpenVertexPainterWindow()
	{
		pb_VertexColor_Editor.Init();
	}
}