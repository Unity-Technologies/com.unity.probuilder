using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder.Core;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder
{
	class VertexColorPalette : EditorWindow
	{
		// Older versions of probuilder stored a fixed size array of colors in EditorPrefs.
		const int k_EditorPrefsColorPaletteCount = 10;

		static VertexColorPalette m_Instance = null;

		[SerializeField]
		pb_ColorPalette m_ColorPalette = null;

		pb_ColorPalette colorPalette
		{
			get { return m_ColorPalette; }
		}

		static string lastAssignedColorPalette
		{
			get { return PreferencesInternal.GetString("pb_VertexColorPalette::lastAssignedColorPalette", ""); }
			set { PreferencesInternal.SetString("pb_VertexColorPalette::lastAssignedColorPalette", value); }
		}

		/// <summary>
		/// Older versions of probuilder stored a fixed size array of colors in EditorPrefs. Use this function to get a
		/// pb_ColorPalette from the older version.
		/// </summary>
		/// <returns></returns>
		static void CopyColorsFromEditorPrefs(pb_ColorPalette target)
		{
			List<Color> colors = new List<Color>();

			for (int i = 0; i < k_EditorPrefsColorPaletteCount; i++)
			{
				Color color = Color.white;

				if (pb_Util.TryParseColor(PreferencesInternal.GetString(pb_Constant.pbVertexColorPrefs + i), ref color))
					colors.Add(color);
			}

			if (colors.Count > 0)
			{
				target.colors = colors;
				UnityEditor.EditorUtility.SetDirty(target);
			}
		}

		/// <summary>
		/// Initialize this window.
		/// </summary>
		public static void MenuOpenWindow()
		{
			bool dockable = PreferencesInternal.GetBool(pb_Constant.pbVertexPaletteDockable);
			GetWindow<VertexColorPalette>(!dockable, "Vertex Colors", true);
		}

		static pb_ColorPalette GetLastUsedColorPalette()
		{
			// serialized copy?
			pb_ColorPalette palette = m_Instance != null ? m_Instance.m_ColorPalette : null;

			if (palette != null)
				return palette;

			// last set asset path?
			palette = AssetDatabase.LoadAssetAtPath<pb_ColorPalette>(lastAssignedColorPalette);

			if (palette != null)
				return palette;

			// any existing palette in project?
			palette = FileUtil.FindAssetOfType<pb_ColorPalette>();

			if(palette != null)
			{
				lastAssignedColorPalette = AssetDatabase.GetAssetPath(palette);
				return palette;
			}

			// create new default
			lastAssignedColorPalette = FileUtil.GetLocalDataDirectory() + "Default Color Palette.asset";
			palette = FileUtil.LoadRequired<pb_ColorPalette>(lastAssignedColorPalette);
			CopyColorsFromEditorPrefs(palette);

			return palette;
		}

		void OnEnable()
		{
			m_Instance = this;
			m_ColorPalette = GetLastUsedColorPalette();
		}

		void OpenContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Open As Floating Window", ""), false, () => { OpenWindowAsDockable(false); });
			menu.AddItem(new GUIContent("Open As Dockable Window", ""), false, () => { OpenWindowAsDockable(true); });
			menu.ShowAsContext();
		}

		void OpenWindowAsDockable(bool isDockable)
		{
			PreferencesInternal.SetBool(pb_Constant.pbVertexPaletteDockable, isDockable);
			GetWindow<VertexColorPalette>().Close();
			VertexColorPalette.MenuOpenWindow();
		}

		Vector2 m_Scroll = Vector2.zero;
		const int k_Padding = 4;
		const int k_ButtonWidth = 58;
		GUIContent m_ColorPaletteGuiContent = new GUIContent("Color Palette");

		void OnGUI()
		{
			var palette = GetLastUsedColorPalette();

			Event e = Event.current;

			switch (e.type)
			{
				case EventType.ContextClick:
					OpenContextMenu();
					break;
			}

			GUILayout.BeginHorizontal(EditorStyles.toolbar);

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
				ResetColors();

			GUILayout.EndHorizontal();

			m_ColorPalette = (pb_ColorPalette) EditorGUILayout.ObjectField(m_ColorPaletteGuiContent, m_ColorPalette, typeof(pb_ColorPalette), false);

			if (m_ColorPalette == null)
			{
				GUILayout.Label("Please Select a Color Palatte", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				return;
			}

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

			for (int i = 0; i < palette.Count; i++)
			{
				GUILayout.Space(4);

				GUILayout.BeginHorizontal();

				if(GUILayout.Button("Apply", GUILayout.ExpandWidth(false), GUILayout.MinWidth(60)))
					SetFaceColors(palette[i]);

				EditorGUI.BeginChangeCheck();
				palette[i] = EditorGUILayout.ColorField(palette[i]);
				if(EditorGUI.EndChangeCheck())
					UnityEditor.EditorUtility.SetDirty(palette);

				GUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
		}

		void ResetColors()
		{
			if (m_ColorPalette == null)
				m_ColorPalette = GetLastUsedColorPalette();

			m_ColorPalette.SetDefaultValues();
			UnityEditor.EditorUtility.SetDirty(m_ColorPalette);
		}

		public static void SetFaceColors(int index)
		{
			var palette = GetLastUsedColorPalette();
			SetFaceColors(palette[index]);
		}

		public static void SetFaceColors(Color col)
		{
			pb_Object[] selection = pb_Util.GetComponents<pb_Object>(Selection.transforms);

			UndoUtility.RecordSelection(selection, "Apply Vertex Colors");

			ProBuilderEditor editor = ProBuilderEditor.instance;

			if (editor && editor.editLevel == EditLevel.Geometry)
			{
				switch (editor.selectionMode)
				{
					case SelectMode.Face:
						foreach (pb_Object pb in selection)
						{
							Color[] colors = pb.colors;

							foreach (int i in pb.SelectedTriangles)
								colors[i] = col;

							pb.SetColors(colors);
						}
						break;
					case SelectMode.Edge:
					case SelectMode.Vertex:
						foreach (pb_Object pb in selection)
						{
							Color[] colors = pb.colors;

							foreach (int i in pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles))
								colors[i] = col;

							pb.SetColors(colors);
						}
						break;
				}
			}
			else
			{
				foreach (pb_Object pb in selection)
				{
					foreach (pb_Face face in pb.faces)
						pb.SetFaceColor(face, col);
				}
			}

			foreach (pb_Object pb in selection)
			{
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			EditorUtility.ShowNotification("Set Vertex Colors\n" + pb_ColorUtil.GetColorName(col));
		}
	}
}
