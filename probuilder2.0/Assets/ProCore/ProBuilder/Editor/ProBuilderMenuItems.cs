using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class ProBuilderMenuItems : EditorWindow
{
	const string DOCUMENTATION_URL = "http://www.protoolsforunity3d.com/docs/probuilder/";

	static pb_Editor editor { get { return pb_Editor.instance; } }

#region WINDOW

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/About", false, 0)]
	public static void MenuInitAbout()
	{
		pc_AboutWindow.Init("Assets/ProCore/" + pb_Constant.PRODUCT_NAME + "/About/pc_AboutEntry_ProBuilder.txt", true);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Documentation", false, 0)]
	public static void MenuInitDocumentation()
	{
		Application.OpenURL(DOCUMENTATION_URL);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/" + pb_Constant.PRODUCT_NAME + " Window", false, pb_Constant.MENU_WINDOW + 0)]
	public static void OpenEditorWindow()
	{
		pb_Editor.MenuOpenWindow();
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Shape Window %#k", false, pb_Constant.MENU_WINDOW + 2)]
	public static void ShapeMenu()
	{
		EditorWindow.GetWindow(typeof(pb_Geometry_Interface), true, "Shape Menu", true);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Color Palette", false, pb_Constant.MENU_WINDOW + 3)]
	public static void InitVertexColorEditor()
	{
		bool openInDockableWindow = !pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
		EditorWindow.GetWindow<pb_VertexColorInterface>(openInDockableWindow, "Vertex Colors", true);
	}

	/**
	 * Finds a pb_Editor window (or makes one), then closes it.
	 */
	public static void ForceCloseEditor()
	{
		EditorWindow.GetWindow<pb_Editor>().Close();
	}
#endregion

#region ProBuilder/Edit

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Edit Level", false, pb_Constant.MENU_EDITOR + 0)]
	public static void ToggleEditLevel()
	{
		editor.ToggleEditLevel();
		switch(editor.editLevel)
		{
			case EditLevel.Top:
				pb_Editor_Utility.ShowNotification("Top Level Editing");
				break;

			case EditLevel.Geometry:
				pb_Editor_Utility.ShowNotification("Geometry Editing");
				break;
		}
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Selection Mode", false, pb_Constant.MENU_EDITOR + 1)]
	public static void ToggleSelectMode()
	{
		editor.ToggleSelectionMode();
		switch(editor.selectionMode)
		{
			case SelectMode.Face:
				pb_Editor_Utility.ShowNotification("Editing Faces");
				break;

			case SelectMode.Vertex:
				pb_Editor_Utility.ShowNotification("Editing Vertices");
				break;

			case SelectMode.Edge:
				pb_Editor_Utility.ShowNotification("Editing Edges\n(Beta!)");
				break;
		}
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Toggle Handle Coordinates", false, pb_Constant.MENU_EDITOR + 2)]
	public static void ToggleHandleAlignment()
	{
		editor.ToggleHandleAlignment();		
		pb_Editor_Utility.ShowNotification("Handle Alignment: " + ((HandleAlignment)editor.handleAlignment).ToString());
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Editor/Lightmap Settings Window", false, pb_Constant.MENU_EDITOR + 3)]
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
		return pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset1() {
		pb_VertexColorInterface.SetFaceColors(1);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset2() {
		pb_VertexColorInterface.SetFaceColors(2);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset3() {
		pb_VertexColorInterface.SetFaceColors(3);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset4() {
		pb_VertexColorInterface.SetFaceColors(4);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset5() {
		pb_VertexColorInterface.SetFaceColors(5);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset6() {
		pb_VertexColorInterface.SetFaceColors(6);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset7() {
		pb_VertexColorInterface.SetFaceColors(7);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset8() {
		pb_VertexColorInterface.SetFaceColors(8);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset9() {
		pb_VertexColorInterface.SetFaceColors(9);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", false, pb_Constant.MENU_VERTEX_COLORS)]
	public static void MenuSetVertexColorPreset0() {
		pb_VertexColorInterface.SetFaceColors(0);
	}
#endregion
}