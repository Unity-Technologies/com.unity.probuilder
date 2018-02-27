#pragma warning disable 0618

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.Interface;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Assign materials to faces and objects.
	/// </summary>
	class pb_MaterialEditor : EditorWindow
	{
		// Reference to pb_Editor instance.
		private static pb_Editor editor { get { return pb_Editor.instance; } }
		// Reference to the currently open pb_Material_Editor
		public static pb_MaterialEditor instance { get; private set; }

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 1 &1", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 2 &2", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 3 &3", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 4 &4", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 5 &5", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 6 &6", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 7 &7", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 8 &8", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 9 &9", true, pb_Constant.MENU_MATERIAL_COLORS)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 10 &0", true, pb_Constant.MENU_MATERIAL_COLORS)]
		public static bool VerifyMaterialAction()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selection.Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 1 &1", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial0() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[0]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 2 &2", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial1() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[1]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 3 &3", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial2() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[2]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 4 &4", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial3() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[3]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 5 &5", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial4() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[4]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 6 &6", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial5() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[5]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 7 &7", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial6() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[6]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 8 &8", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial7() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[7]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 9 &9", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial8() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[8]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 10 &0", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial9() {
			ApplyMaterial(pb_Selection.Top(), CurrentPalette[9]);
		}

		// Path to the required default material palette. If not valid material palettes are
		// found a new one will be created with this path (relative to ProBuilder folder).
		static string m_DefaultMaterialPalettePath;

		static string DefaultMaterialPalettePath
		{
			get
			{
				if(string.IsNullOrEmpty(m_DefaultMaterialPalettePath))
					m_DefaultMaterialPalettePath = pb_FileUtil.GetLocalDataDirectory() + "/Default Material Palette.asset";
				return m_DefaultMaterialPalettePath;
			}
		}

		// The currently loaded material palette asset.
		static pb_MaterialPalette m_CurrentPalette = null;
		// The user set "quick material"
		static Material m_QueuedMaterial;
		// Custom style for material row background
		GUIStyle m_RowBackgroundStyle;
		// The view scroll position.
		Vector2 m_ViewScroll = Vector2.zero;
		// All available material palettes
		pb_MaterialPalette[] m_AvailablePalettes = null;
		// List of string names for all available palettes (plus one entry for 'Add New')
		string[] m_AvailablePalettes_Str = null;
		// The index of the currently loaded material palette in m_AvailablePalettes
		int m_CurrentPaletteIndex = 0;

		/// <summary>
		/// The currently loaded material palette, or a default.
		/// </summary>
		public static pb_MaterialPalette CurrentPalette
		{
			get
			{
				if (m_CurrentPalette == null)
				{
					// Attempt to load the last user-set material palette
					m_CurrentPalette =
						AssetDatabase.LoadAssetAtPath<pb_MaterialPalette>(
							pb_PreferencesInternal.GetString(pb_Constant.pbCurrentMaterialPalette));

					// If not set (or deleted), fall back on default
					if (m_CurrentPalette != null)
						return m_CurrentPalette;

					// No dice - iterate any other pb_MaterialPalette objects in the project (favoring first found)
					m_CurrentPalette = pb_FileUtil.FindAssetOfType<pb_MaterialPalette>();

					if (m_CurrentPalette != null)
						return m_CurrentPalette;

					// If no existing pb_MaterialPalette objects in project:
					// - create a new one
					// - check for the older pb_ObjectArray and copy data to new default
					m_CurrentPalette = pb_FileUtil.LoadRequired<pb_MaterialPalette>(DefaultMaterialPalettePath);

					string[] m_LegacyMaterialArrays = AssetDatabase.FindAssets("t:pb_ObjectArray");

					for (int i = 0; m_LegacyMaterialArrays != null && i < m_LegacyMaterialArrays.Length; i++)
					{
						pb_ObjectArray poa = AssetDatabase.LoadAssetAtPath<pb_ObjectArray>(AssetDatabase.GUIDToAssetPath(m_LegacyMaterialArrays[i]));

						// Make sure there's actually something worth copying
						if (poa != null && poa.array != null && poa.array.Any(x => x != null && x is Material))
						{
							m_CurrentPalette.array = poa.GetArray<Material>();
							break;
						}
					}
				}
				return m_CurrentPalette;
			}
		}

		public static void MenuOpenMaterialEditor()
		{
			EditorWindow.GetWindow<pb_MaterialEditor>(
				pb_PreferencesInternal.GetBool(pb_Constant.pbMaterialEditorFloating),
				"Material Editor",
				true).Show();
		}

		private void OnEnable()
		{
			instance = this;
			this.autoRepaintOnSceneChange = true;
			this.minSize = new Vector2(236, 250);
			m_RowBackgroundStyle = new GUIStyle();
			m_RowBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;
			m_CurrentPalette = null;
			RefreshAvailablePalettes();
		}

		private void OnDisable()
		{
			instance = null;
		}

		private void OpenContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem (new GUIContent("Window/Open as Floating Window", ""), false, () => { SetFloating(true); } );
			menu.AddItem (new GUIContent("Window/Open as Dockable Window", ""), false, () => { SetFloating(false); } );

			menu.ShowAsContext ();
		}

		private void SetFloating(bool floating)
		{
			pb_PreferencesInternal.SetBool(pb_Constant.pbMaterialEditorFloating, floating);
			this.Close();
			MenuOpenMaterialEditor();
		}

		private void OnGUI()
		{
			if(Event.current.type == EventType.ContextClick)
				OpenContextMenu();

			GUILayout.Label("Quick Material", EditorStyles.boldLabel);
			Rect r = GUILayoutUtility.GetLastRect();
			int left = Screen.width - 68;

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width-74));
				GUILayout.BeginVertical();

					m_QueuedMaterial = (Material)EditorGUILayout.ObjectField(m_QueuedMaterial, typeof(Material), true);

					GUILayout.Space(2);

					if(GUILayout.Button("Apply (Ctrl+Shift+Click)"))
						ApplyMaterial(pb_Selection.Top(), m_QueuedMaterial);

					GUI.enabled = editor != null && editor.selectedFaceCount > 0;
					if(GUILayout.Button("Match Selection"))
					{
						pb_Object tp;
						pb_Face tf;
						if( editor.GetFirstSelectedFace(out tp, out tf) )
							m_QueuedMaterial = tf.material;
					}
					GUI.enabled = true;

				GUILayout.EndVertical();

				GUI.Box( new Rect(left, r.y + r.height + 2, 64, 64), "" );
				if(m_QueuedMaterial != null && m_QueuedMaterial.mainTexture != null)
					EditorGUI.DrawPreviewTexture( new Rect(left+2, r.y + r.height + 4, 60, 60), m_QueuedMaterial.mainTexture, m_QueuedMaterial, ScaleMode.StretchToFill, 0);
				else
				{
					GUI.Box( new Rect(left+2, r.y + r.height + 4, 60, 60), "" );
					GUI.Label( new Rect(left +2, r.height + 28, 120, 32), "None\n(Texture)");
				}

			GUILayout.EndHorizontal();

			GUILayout.Space(4);

			GUI.backgroundColor = pb_Constant.ProBuilderDarkGray;
			pb_EditorGUIUtility.DrawSeparator(2);
			GUI.backgroundColor = Color.white;

			GUILayout.Label("Material Palette", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			m_CurrentPaletteIndex = EditorGUILayout.Popup("", m_CurrentPaletteIndex, m_AvailablePalettes_Str);

			if(EditorGUI.EndChangeCheck())
			{
				pb_MaterialPalette newPalette = null;

				// Add a new material palette
				if(m_CurrentPaletteIndex >= m_AvailablePalettes.Length)
				{
					string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Material Palette.asset");
					newPalette = pb_FileUtil.LoadRequired<pb_MaterialPalette>(path);
					EditorGUIUtility.PingObject(newPalette);
				}
				else
				{
					newPalette = m_AvailablePalettes[m_CurrentPaletteIndex];
				}

				SetMaterialPalette(newPalette);
			}

			EditorGUI.BeginChangeCheck();
			m_CurrentPalette = (pb_MaterialPalette) EditorGUILayout.ObjectField(m_CurrentPalette, typeof(pb_MaterialPalette), false);
			if(EditorGUI.EndChangeCheck())
				SetMaterialPalette(m_CurrentPalette);

			GUILayout.Space(4);

			Material[] materials = CurrentPalette;

			m_ViewScroll = GUILayout.BeginScrollView(m_ViewScroll);

			for(int i = 0; i < materials.Length; i++)
			{
				if(i == 10)
				{
					GUILayout.Space(2);
					GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
					pb_EditorGUIUtility.DrawSeparator(1);
					GUI.backgroundColor = Color.white;
					GUILayout.Space(2);
				}

				GUILayout.BeginHorizontal();
					if(i < 10)
					{
						if(GUILayout.Button("Alt + " + (i == 9 ? 0 : (i+1)).ToString(), EditorStyles.miniButton, GUILayout.MaxWidth(58)))
							ApplyMaterial(pb_Selection.Top(), materials[i]);
					}
					else
					{
						if(GUILayout.Button("Apply", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(44)))
							ApplyMaterial(pb_Selection.Top(), materials[i]);

						GUI.backgroundColor = Color.red;
						if(GUILayout.Button("", EditorStyles.miniButtonRight, GUILayout.MaxWidth(14)))
						{
							Material[] temp = new Material[materials.Length-1];
							System.Array.Copy(materials, 0, temp, 0, materials.Length-1);
							materials = temp;
							SaveUserMaterials(materials);
							return;
						}
						GUI.backgroundColor = Color.white;
					}

					EditorGUI.BeginChangeCheck();
						materials[i] = (Material)EditorGUILayout.ObjectField(materials[i], typeof(Material), false);
					if( EditorGUI.EndChangeCheck() )
						SaveUserMaterials(materials);

				GUILayout.EndHorizontal();
			}


			if(GUILayout.Button("Add"))
			{
				Material[] temp = new Material[materials.Length+1];
				System.Array.Copy(materials, 0, temp, 0, materials.Length);
				materials = temp;
				SaveUserMaterials(materials);
			}

			GUILayout.EndScrollView();
		}

		/**
		 * Applies the currently queued material to the selected face and eats the event.
		 */
		public bool ClickShortcutCheck(EventModifiers em, pb_Object pb, pb_Face quad)
		{
			if(pb_UVEditor.instance == null)
			{
				if(em == (EventModifiers.Control | EventModifiers.Shift))
				{
					pb_Undo.RecordObject(pb, "Quick Apply");
					pb.SetFaceMaterial( new pb_Face[1] { quad }, m_QueuedMaterial);
					OnFaceChanged(pb);
					pb_EditorUtility.ShowNotification("Quick Apply Material");
					return true;
				}
			}

			return false;
		}

		public static void ApplyMaterial(IEnumerable<pb_Object> selection, Material mat)
		{
			if(mat == null) return;

			pb_Undo.RecordSelection(selection.ToArray(), "Set Face Materials");

			foreach(pb_Object pb in selection)
			{
				pb_Face[] faces = pb.SelectedFaces;
				pb.SetFaceMaterial(faces == null || faces.Length < 1 ? pb.faces : faces, mat);

				OnFaceChanged(pb);
			}

			if(pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0)
				pb_EditorUtility.ShowNotification("Set Material\n" + mat.name);
		}

		private static void SaveUserMaterials(Material[] materials)
		{
			m_CurrentPalette.array = materials;
			EditorUtility.SetDirty(m_CurrentPalette);
			AssetDatabase.SaveAssets();
		}

		private static void OnFaceChanged( pb_Object pb )
		{
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		private void SetMaterialPalette(pb_MaterialPalette palette)
		{
			m_CurrentPalette = palette;
			RefreshAvailablePalettes();
		}

		private void RefreshAvailablePalettes()
		{
			pb_MaterialPalette cur = CurrentPalette;
			m_AvailablePalettes = pb_FileUtil.FindAndLoadAssets<pb_MaterialPalette>();
			m_AvailablePalettes_Str = m_AvailablePalettes.Select(x => x.name).ToArray();
			ArrayUtility.Add<string>(ref m_AvailablePalettes_Str, string.Empty);
			ArrayUtility.Add<string>(ref m_AvailablePalettes_Str, "New Material Palette...");
			m_CurrentPaletteIndex = System.Array.IndexOf(m_AvailablePalettes, cur);
			pb_PreferencesInternal.SetString(pb_Constant.pbCurrentMaterialPalette, AssetDatabase.GetAssetPath(cur));
		}
	}
}
