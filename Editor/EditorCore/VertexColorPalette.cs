using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using ColorUtility = UnityEngine.ProBuilder.ColorUtility;

namespace UnityEditor.ProBuilder
{
	sealed class VertexColorPalette : EditorWindow
	{
		// Older versions of probuilder stored a fixed size array of colors in EditorPrefs.
		const int k_EditorPrefsColorPaletteCount = 10;

		static VertexColorPalette s_Instance = null;

		[SerializeField]
		ColorPalette m_ColorPalette = null;

		ColorPalette colorPalette
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
		static void CopyColorsFromEditorPrefs(ColorPalette target)
		{
			List<Color> colors = new List<Color>();

			for (int i = 0; i < k_EditorPrefsColorPaletteCount; i++)
			{
				Color color = Color.white;

				if (InternalUtility.TryParseColor(PreferencesInternal.GetString(PreferenceKeys.pbVertexColorPrefs + i), ref color))
					colors.Add(color);
			}

			if (colors.Count > 0)
			{
				target.SetColors(colors);
				UnityEditor.EditorUtility.SetDirty(target);
			}
		}

		/// <summary>
		/// Initialize this window.
		/// </summary>
		public static void MenuOpenWindow()
		{
			bool dockable = PreferencesInternal.GetBool(PreferenceKeys.pbVertexPaletteDockable);
			GetWindow<VertexColorPalette>(!dockable, "Vertex Colors", true);
		}

		static ColorPalette GetLastUsedColorPalette()
		{
			// serialized copy?
			ColorPalette palette = s_Instance != null ? s_Instance.m_ColorPalette : null;

			if (palette != null)
				return palette;

			// last set asset path?
			palette = AssetDatabase.LoadAssetAtPath<ColorPalette>(lastAssignedColorPalette);

			if (palette != null)
				return palette;

			// any existing palette in project?
			palette = FileUtility.FindAssetOfType<ColorPalette>();

			if(palette != null)
			{
				lastAssignedColorPalette = AssetDatabase.GetAssetPath(palette);
				return palette;
			}

			// create new default
			lastAssignedColorPalette = FileUtility.GetLocalDataDirectory() + "Default Color Palette.asset";
			palette = FileUtility.LoadRequired<ColorPalette>(lastAssignedColorPalette);
			CopyColorsFromEditorPrefs(palette);

			return palette;
		}

		void OnEnable()
		{
			s_Instance = this;
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
			PreferencesInternal.SetBool(PreferenceKeys.pbVertexPaletteDockable, isDockable);
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

			m_ColorPalette = (ColorPalette) EditorGUILayout.ObjectField(m_ColorPaletteGuiContent, m_ColorPalette, typeof(ColorPalette), false);

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
			ProBuilderMesh[] selection = InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms);

			UndoUtility.RecordSelection(selection, "Apply Vertex Colors");

			ProBuilderEditor editor = ProBuilderEditor.instance;

			if (editor && editor.editLevel == EditLevel.Geometry)
			{
				switch (editor.selectionMode)
				{
					case SelectMode.Face:
						foreach (ProBuilderMesh pb in selection)
						{
							Color[] colors = pb.colorsInternal;

							foreach (int i in pb.selectedIndicesInternal)
								colors[i] = col;

							pb.SetColors(colors);
						}
						break;
					case SelectMode.Edge:
					case SelectMode.Vertex:
						foreach (ProBuilderMesh pb in selection)
						{
							Color[] colors = pb.colorsInternal;

							foreach (int i in pb.sharedIndicesInternal.AllIndexesWithValues(pb.selectedIndicesInternal))
								colors[i] = col;

							pb.SetColors(colors);
						}
						break;
				}
			}
			else
			{
				foreach (ProBuilderMesh pb in selection)
				{
					foreach (Face face in pb.facesInternal)
						pb.SetFaceColor(face, col);
				}
			}

			foreach (ProBuilderMesh pb in selection)
			{
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			EditorUtility.ShowNotification("Set Vertex Colors\n" + ColorUtility.GetColorName(col));
		}
	}
}
