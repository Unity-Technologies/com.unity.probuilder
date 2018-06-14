#pragma warning disable 0618

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Assign materials to faces and objects.
	/// </summary>
	sealed class MaterialEditor : EditorWindow
	{
		// Reference to pb_Editor instance.
		static ProBuilderEditor editor { get { return ProBuilderEditor.instance; } }

		// Reference to the currently open pb_Material_Editor
		public static MaterialEditor instance { get; private set; }

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 1 &1", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 2 &2", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 3 &3", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 4 &4", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 5 &5", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 6 &6", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 7 &7", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 8 &8", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 9 &9", true, PreferenceKeys.menuMaterialColors)]
		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 10 &0", true, PreferenceKeys.menuMaterialColors)]
		public static bool VerifyMaterialAction()
		{
			return ProBuilderEditor.instance != null && ProBuilderEditor.instance.selection.Length > 0;
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 1 &1", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial0() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[0]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 2 &2", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial1() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[1]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 3 &3", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial2() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[2]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 4 &4", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial3() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[3]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 5 &5", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial4() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[4]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 6 &6", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial5() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[5]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 7 &7", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial6() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[6]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 8 &8", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial7() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[7]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 9 &9", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial8() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[8]);
		}

		[MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 10 &0", false, PreferenceKeys.menuMaterialColors)]
		public static void ApplyMaterial9() {
			ApplyMaterial(MeshSelection.TopInternal(), CurrentPalette[9]);
		}

		// Path to the required default material palette. If not valid material palettes are
		// found a new one will be created with this path (relative to ProBuilder folder).
		static string s_DefaultMaterialPalettePath;

		static string DefaultMaterialPalettePath
		{
			get
			{
				if(string.IsNullOrEmpty(s_DefaultMaterialPalettePath))
					s_DefaultMaterialPalettePath = FileUtility.GetLocalDataDirectory() + "/Default Material Palette.asset";
				return s_DefaultMaterialPalettePath;
			}
		}

		// The currently loaded material palette asset.
		static MaterialPalette s_CurrentPalette = null;
		// The user set "quick material"
		static Material s_QueuedMaterial;
		// Custom style for material row background
		GUIStyle m_RowBackgroundStyle;
		// The view scroll position.
		Vector2 m_ViewScroll = Vector2.zero;
		// All available material palettes
		MaterialPalette[] m_AvailablePalettes = null;
		// List of string names for all available palettes (plus one entry for 'Add New')
		string[] m_AvailablePalettes_Str = null;
		// The index of the currently loaded material palette in m_AvailablePalettes
		int m_CurrentPaletteIndex = 0;

		/// <summary>
		/// The currently loaded material palette, or a default.
		/// </summary>
		public static MaterialPalette CurrentPalette
		{
			get
			{
				if (s_CurrentPalette == null)
				{
					// Attempt to load the last user-set material palette
					s_CurrentPalette =
						AssetDatabase.LoadAssetAtPath<MaterialPalette>(
							PreferencesInternal.GetString(PreferenceKeys.pbCurrentMaterialPalette));

					// If not set (or deleted), fall back on default
					if (s_CurrentPalette != null)
						return s_CurrentPalette;

					// No dice - iterate any other pb_MaterialPalette objects in the project (favoring first found)
					s_CurrentPalette = FileUtility.FindAssetOfType<MaterialPalette>();

					if (s_CurrentPalette != null)
						return s_CurrentPalette;

					// If no existing pb_MaterialPalette objects in project:
					// - create a new one
					// - check for the older pb_ObjectArray and copy data to new default
					s_CurrentPalette = FileUtility.LoadRequired<MaterialPalette>(DefaultMaterialPalettePath);

					string[] m_LegacyMaterialArrays = AssetDatabase.FindAssets("t:pb_ObjectArray");

					for (int i = 0; m_LegacyMaterialArrays != null && i < m_LegacyMaterialArrays.Length; i++)
					{
						pb_ObjectArray poa = AssetDatabase.LoadAssetAtPath<pb_ObjectArray>(AssetDatabase.GUIDToAssetPath(m_LegacyMaterialArrays[i]));

						// Make sure there's actually something worth copying
						if (poa != null && poa.array != null && poa.array.Any(x => x != null && x is Material))
						{
							s_CurrentPalette.array = poa.GetArray<Material>();
							break;
						}
					}
				}
				return s_CurrentPalette;
			}
		}

		public static void MenuOpenMaterialEditor()
		{
			EditorWindow.GetWindow<MaterialEditor>(
				PreferencesInternal.GetBool(PreferenceKeys.pbMaterialEditorFloating),
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
			s_CurrentPalette = null;
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
			PreferencesInternal.SetBool(PreferenceKeys.pbMaterialEditorFloating, floating);
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

					s_QueuedMaterial = (Material)EditorGUILayout.ObjectField(s_QueuedMaterial, typeof(Material), true);

					GUILayout.Space(2);

					if(GUILayout.Button("Apply (Ctrl+Shift+Click)"))
						ApplyMaterial(MeshSelection.TopInternal(), s_QueuedMaterial);

					GUI.enabled = editor != null && editor.selectedFaceCount > 0;
					if(GUILayout.Button("Match Selection"))
					{
						ProBuilderMesh tp;
						Face tf;
						if( editor.GetFirstSelectedFace(out tp, out tf) )
							s_QueuedMaterial = tf.material;
					}
					GUI.enabled = true;

				GUILayout.EndVertical();

				GUI.Box( new Rect(left, r.y + r.height + 2, 64, 64), "" );
				if(s_QueuedMaterial != null && s_QueuedMaterial.mainTexture != null)
					EditorGUI.DrawPreviewTexture( new Rect(left+2, r.y + r.height + 4, 60, 60), s_QueuedMaterial.mainTexture, s_QueuedMaterial, ScaleMode.StretchToFill, 0);
				else
				{
					GUI.Box( new Rect(left+2, r.y + r.height + 4, 60, 60), "" );
					GUI.Label( new Rect(left +2, r.height + 28, 120, 32), "None\n(Texture)");
				}

			GUILayout.EndHorizontal();

			GUILayout.Space(4);

			GUI.backgroundColor = PreferenceKeys.proBuilderDarkGray;
			UI.EditorGUIUtility.DrawSeparator(2);
			GUI.backgroundColor = Color.white;

			GUILayout.Label("Material Palette", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			m_CurrentPaletteIndex = EditorGUILayout.Popup("", m_CurrentPaletteIndex, m_AvailablePalettes_Str);

			if(EditorGUI.EndChangeCheck())
			{
				MaterialPalette newPalette = null;

				// Add a new material palette
				if(m_CurrentPaletteIndex >= m_AvailablePalettes.Length)
				{
					string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Material Palette.asset");
					newPalette = FileUtility.LoadRequired<MaterialPalette>(path);
					EditorGUIUtility.PingObject(newPalette);
				}
				else
				{
					newPalette = m_AvailablePalettes[m_CurrentPaletteIndex];
				}

				SetMaterialPalette(newPalette);
			}

			EditorGUI.BeginChangeCheck();
			s_CurrentPalette = (MaterialPalette) EditorGUILayout.ObjectField(s_CurrentPalette, typeof(MaterialPalette), false);
			if(EditorGUI.EndChangeCheck())
				SetMaterialPalette(s_CurrentPalette);

			GUILayout.Space(4);

			Material[] materials = CurrentPalette;

			m_ViewScroll = GUILayout.BeginScrollView(m_ViewScroll);

			for(int i = 0; i < materials.Length; i++)
			{
				if(i == 10)
				{
					GUILayout.Space(2);
					GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
					UI.EditorGUIUtility.DrawSeparator(1);
					GUI.backgroundColor = Color.white;
					GUILayout.Space(2);
				}

				GUILayout.BeginHorizontal();
					if(i < 10)
					{
						if(GUILayout.Button("Alt + " + (i == 9 ? 0 : (i+1)).ToString(), EditorStyles.miniButton, GUILayout.MaxWidth(58)))
							ApplyMaterial(MeshSelection.TopInternal(), materials[i]);
					}
					else
					{
						if(GUILayout.Button("Apply", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(44)))
							ApplyMaterial(MeshSelection.TopInternal(), materials[i]);

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
		public bool ClickShortcutCheck(EventModifiers em, ProBuilderMesh pb, Face quad)
		{
			if(UVEditor.instance == null)
			{
				if(em == (EventModifiers.Control | EventModifiers.Shift))
				{
					UndoUtility.RecordObject(pb, "Quick Apply");
					quad.material = s_QueuedMaterial;
					OnFaceChanged(pb);
					EditorUtility.ShowNotification("Quick Apply Material");
					return true;
				}
			}

			return false;
		}

		public static void ApplyMaterial(IEnumerable<ProBuilderMesh> selection, Material mat)
		{
			if(mat == null) return;

			UndoUtility.RecordSelection(selection.ToArray(), "Set Face Materials");

			foreach(ProBuilderMesh pb in selection)
			{
				Face[] faces = pb.selectedFaceCount > 0 ? pb.GetSelectedFaces() : pb.facesInternal;
				foreach (var face in faces)
					face.material = mat;
				OnFaceChanged(pb);
			}

			if(ProBuilderEditor.instance != null && ProBuilderEditor.instance.selectedFaceCount > 0)
				EditorUtility.ShowNotification("Set Material\n" + mat.name);
		}

		private static void SaveUserMaterials(Material[] materials)
		{
			s_CurrentPalette.array = materials;
			UnityEditor.EditorUtility.SetDirty(s_CurrentPalette);
			AssetDatabase.SaveAssets();
		}

		private static void OnFaceChanged( ProBuilderMesh pb )
		{
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		private void SetMaterialPalette(MaterialPalette palette)
		{
			s_CurrentPalette = palette;
			RefreshAvailablePalettes();
		}

		private void RefreshAvailablePalettes()
		{
			MaterialPalette cur = CurrentPalette;
			m_AvailablePalettes = FileUtility.FindAndLoadAssets<MaterialPalette>();
			m_AvailablePalettes_Str = m_AvailablePalettes.Select(x => x.name).ToArray();
			ArrayUtility.Add<string>(ref m_AvailablePalettes_Str, string.Empty);
			ArrayUtility.Add<string>(ref m_AvailablePalettes_Str, "New Material Palette...");
			m_CurrentPaletteIndex = System.Array.IndexOf(m_AvailablePalettes, cur);
			PreferencesInternal.SetString(PreferenceKeys.pbCurrentMaterialPalette, AssetDatabase.GetAssetPath(cur));
		}
	}
}
