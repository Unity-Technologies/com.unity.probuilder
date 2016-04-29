using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class pb_MenuItems : EditorWindow
{
	const string DOCUMENTATION_URL = "http://www.protoolsforunity3d.com/docs/probuilder/";

	static pb_Editor editor { get { return pb_Editor.instance; } }

#region WINDOW

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/About", false, pb_Constant.MENU_ABOUT)]
	public static void MenuInitAbout()
	{
		pb_AboutWindow.Init("Assets/ProCore/ProBuilder/About/pc_AboutEntry_ProBuilder.txt", true);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Documentation", false, pb_Constant.MENU_ABOUT)]
	public static void MenuInitDocumentation()
	{
		Application.OpenURL(DOCUMENTATION_URL);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/" + pb_Constant.PRODUCT_NAME + " Window", false, pb_Constant.MENU_EDITOR)]
	public static void OpenEditorWindow()
	{
		pb_Editor.MenuOpenWindow();
	}
#endregion

#region ProBuilder/Edit

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Debug/Lightmap Settings Tool", false, pb_Constant.MENU_REPAIR)]
	public static void LightmapWindowInit()
	{
		pb_Lightmap_Editor.Init(editor);
	}
#endregion

#region VERTEX COLORS

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", true, pb_Constant.MENU_VERTEX_COLORS)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", true, pb_Constant.MENU_VERTEX_COLORS)]
	public static bool VerifyApplyVertexColor()
	{
		return pb_Editor.instance != null && pb_Editor.instance.selectedVertexCount > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset1() {
		pb_Vertex_Color_Toolbar.SetFaceColors(1);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset2() {
		pb_Vertex_Color_Toolbar.SetFaceColors(2);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset3() {
		pb_Vertex_Color_Toolbar.SetFaceColors(3);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset4() {
		pb_Vertex_Color_Toolbar.SetFaceColors(4);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset5() {
		pb_Vertex_Color_Toolbar.SetFaceColors(5);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset6() {
		pb_Vertex_Color_Toolbar.SetFaceColors(6);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset7() {
		pb_Vertex_Color_Toolbar.SetFaceColors(7);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset8() {
		pb_Vertex_Color_Toolbar.SetFaceColors(8);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset9() {
		pb_Vertex_Color_Toolbar.SetFaceColors(9);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset0() {
		pb_Vertex_Color_Toolbar.SetFaceColors(0);
	}
#endregion
}
