using UnityEngine;
using UnityEditor;
using System.Linq;
using ProBuilder.Core;

namespace UnityEditor.ProBuilder
{
	static class MenuItems
	{
		const string DOCUMENTATION_URL = "http://procore3d.github.io/probuilder2/";

		private static ProBuilderEditor editor
		{
			get { return ProBuilderEditor.instance; }
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/About", false, pb_Constant.MENU_ABOUT)]
		public static void MenuInitAbout()
		{
			AboutWindow.Init();
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Documentation", false, pb_Constant.MENU_ABOUT)]
		public static void MenuInitDocumentation()
		{
			Application.OpenURL(DOCUMENTATION_URL);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/" + pb_Constant.PRODUCT_NAME + " Window", false,
			pb_Constant.MENU_EDITOR)]
		public static void OpenEditorWindow()
		{
			ProBuilderEditor.MenuOpenWindow();
		}

		static pb_Object[] selection
		{
			get { return Selection.transforms.GetComponents<pb_Object>(); }
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Extrude %e", true)]
		private static bool MenuVerifyExtrude()
		{
			ProBuilderEditor e = ProBuilderEditor.instance;

			return e != null &&
			       e.editLevel == EditLevel.Geometry &&
			       selection != null &&
			       selection.Length > 0 &&
			       (selection.Any(x => x.SelectedEdgeCount > 0) || selection.Any(x => x.SelectedFaces.Length > 0));
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Extrude %e", false, pb_Constant.MENU_GEOMETRY + 3)]
		private static void MenuDoExtrude()
		{
			MenuCommands.MenuExtrude(selection, false);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Loop &l", true, pb_Constant.MENU_SELECTION)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Ring &r", true, pb_Constant.MENU_SELECTION)]
		private static bool MenuVerifyRingLoop()
		{
			if (editor == null || editor.editLevel != EditLevel.Geometry)
				return false;

			if (editor.selectionMode == SelectMode.Edge)
				return MeshSelection.Top().Any(x => x.SelectedEdgeCount > 0);
			else if (editor.selectionMode == SelectMode.Face)
				return MeshSelection.Top().Any(x => x.SelectedFaceCount > 0);
			return false;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Loop &l", false, pb_Constant.MENU_SELECTION)]
		private static void MenuSelectLoop()
		{
			switch (editor.selectionMode)
			{
				case SelectMode.Edge:
					MenuCommands.MenuLoopSelection(selection);
					break;

				case SelectMode.Face:
					MenuCommands.MenuLoopFaces(selection);
					break;
			}
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Select Ring &r", false, pb_Constant.MENU_SELECTION)]
		private static void MenuSelectRing()
		{
			switch (editor.selectionMode)
			{
				case SelectMode.Edge:
					MenuCommands.MenuRingSelection(selection);
					break;

				case SelectMode.Face:
					MenuCommands.MenuRingFaces(selection);
					break;
			}
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", true,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static bool VerifyApplyVertexColor()
		{
			return ProBuilderEditor.instance != null && ProBuilderEditor.instance.selectedVertexCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset1()
		{
			VertexColorPalette.SetFaceColors(1);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset2()
		{
			VertexColorPalette.SetFaceColors(2);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset3()
		{
			VertexColorPalette.SetFaceColors(3);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset4()
		{
			VertexColorPalette.SetFaceColors(4);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset5()
		{
			VertexColorPalette.SetFaceColors(5);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset6()
		{
			VertexColorPalette.SetFaceColors(6);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset7()
		{
			VertexColorPalette.SetFaceColors(7);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset8()
		{
			VertexColorPalette.SetFaceColors(8);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset9()
		{
			VertexColorPalette.SetFaceColors(9);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", false,
			pb_Constant.MENU_VERTEX_COLORS)]
		public static void MenuSetVertexColorPreset0()
		{
			VertexColorPalette.SetFaceColors(0);
		}
	}
}
