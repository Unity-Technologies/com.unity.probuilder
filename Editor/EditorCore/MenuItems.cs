using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	static class MenuItems
	{
		const string DOCUMENTATION_URL = "http://procore3d.github.io/probuilder2/";

		static ProBuilderEditor editor
		{
			get { return ProBuilderEditor.instance; }
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/About", false, PreferenceKeys.menuAbout)]
		public static void MenuInitAbout()
		{
			AboutWindow.Init();
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Documentation", false, PreferenceKeys.menuAbout)]
		public static void MenuInitDocumentation()
		{
			Application.OpenURL(DOCUMENTATION_URL);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/" + PreferenceKeys.pluginTitle + " Window", false,
			PreferenceKeys.menuEditor)]
		public static void OpenEditorWindow()
		{
			ProBuilderEditor.MenuOpenWindow();
		}

		static ProBuilderMesh[] selection
		{
			get { return Selection.transforms.GetComponents<ProBuilderMesh>(); }
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Geometry/Extrude %e", true)]
		static bool MenuVerifyExtrude()
		{
			ProBuilderEditor e = ProBuilderEditor.instance;

			return e != null &&
			       e.editLevel == EditLevel.Geometry &&
			       selection != null &&
			       selection.Length > 0 &&
			       (selection.Any(x => x.selectedEdgeCount > 0) || selection.Any(x => x.selectedFacesInternal.Length > 0));
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Geometry/Extrude %e", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuDoExtrude()
		{
			MenuCommands.MenuExtrude(selection, false);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Selection/Select Loop &l", true, PreferenceKeys.menuSelection)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Selection/Select Ring &r", true, PreferenceKeys.menuSelection)]
		private static bool MenuVerifyRingLoop()
		{
			if (editor == null || editor.editLevel != EditLevel.Geometry)
				return false;

			if (editor.selectionMode == SelectMode.Edge)
				return MeshSelection.Top().Any(x => x.selectedEdgeCount > 0);
			else if (editor.selectionMode == SelectMode.Face)
				return MeshSelection.Top().Any(x => x.selectedFaceCount > 0);
			return false;
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Selection/Select Loop &l", false, PreferenceKeys.menuSelection)]
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

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Selection/Select Ring &r", false, PreferenceKeys.menuSelection)]
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

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", true,
			PreferenceKeys.menuVertexColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", true,
			PreferenceKeys.menuVertexColors)]
		public static bool VerifyApplyVertexColor()
		{
			return ProBuilderEditor.instance != null && ProBuilderEditor.instance.selectedVertexCount > 0;
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 1 &#1", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset1()
		{
			VertexColorPalette.SetFaceColors(1);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 2 &#2", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset2()
		{
			VertexColorPalette.SetFaceColors(2);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 3 &#3", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset3()
		{
			VertexColorPalette.SetFaceColors(3);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 4 &#4", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset4()
		{
			VertexColorPalette.SetFaceColors(4);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 5 &#5", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset5()
		{
			VertexColorPalette.SetFaceColors(5);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 6 &#6", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset6()
		{
			VertexColorPalette.SetFaceColors(6);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 7 &#7", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset7()
		{
			VertexColorPalette.SetFaceColors(7);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 8 &#8", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset8()
		{
			VertexColorPalette.SetFaceColors(8);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 9 &#9", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset9()
		{
			VertexColorPalette.SetFaceColors(9);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Vertex Colors/Set Selected Faces to Preset 0 &#0", false,
			PreferenceKeys.menuVertexColors)]
		public static void MenuSetVertexColorPreset0()
		{
			VertexColorPalette.SetFaceColors(0);
		}
	}
}
