using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 * Assign materials to faces and objects.
	 */
	public class pb_Material_Editor : EditorWindow
	{
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

		// Reference to pb_Editor instance.
		private static pb_Editor editor { get { return pb_Editor.instance; } }
		// Reference to the currently open pb_Material_Editor
		public static pb_Material_Editor instance { get; private set; }
		// Path to the required default material palette. If not valid material palettes are 
		// found a new one will be created with this path (relative to ProBuilder folder).
		private const string m_DefaultMaterialPalettePath = "Data/UserMaterials.asset";
		// The currently loaded material palette asset.
		private static pb_ObjectArray m_CurrentPalette = null;
		// The currently loaded material palette material array.
		private static Material[] m_CurrentMaterials = null;
		// The user set "quick material"
		private static Material m_QueuedMaterial;
		// Custom style for material row background
		private GUIStyle m_RowBackgroundStyle;
		// The view scroll position.
		private Vector2 m_ViewScroll = Vector2.zero;

		/**
		 *	Array of the currently loaded materials.
		 */
		public static Material[] CurrentPalette
		{
			get
			{
				if(m_CurrentMaterials != null)
					return m_CurrentMaterials;
				
				if(m_CurrentPalette == null)
					m_CurrentPalette = LoadMaterialPalette();

				m_CurrentMaterials = m_CurrentPalette.GetArray<Material>();

				return m_CurrentMaterials;
			}
		}

#if PROTOTYPE
		public static void MenuOpenMaterialEditor()
		{
			Debug.LogWarning("Material Editor is ProBuilder Advanced feature.");
		}
#else

		public static void MenuOpenMaterialEditor()
		{
			EditorWindow.GetWindow<pb_Material_Editor>(
				pb_Preferences_Internal.GetBool(pb_Constant.pbMaterialEditorFloating),
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
		}

		private void OnDisable()
		{
			instance = null;
		}

		private static pb_ObjectArray LoadMaterialPalette()
		{
			string m_LastMaterialPalette = pb_Preferences_Internal.GetString(pb_Constant.pbCurrentMaterialPalette, pb_FileUtil.PathFromRelative(m_DefaultMaterialPalettePath));
			
			pb_ObjectArray poa = pb_FileUtil.Load<pb_ObjectArray>(m_LastMaterialPalette);

			if(poa == null)
			{
				poa = pb_FileUtil.LoadRequiredRelative<pb_ObjectArray>(m_DefaultMaterialPalettePath);
				poa.array = new Material[1] { pb_Constant.DefaultMaterial };
			}

			return poa;
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
			pb_Preferences_Internal.SetBool(pb_Constant.pbMaterialEditorFloating, floating);
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

					m_QueuedMaterial = (Material)EditorGUILayout.ObjectField(m_QueuedMaterial, typeof(Material), false);

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
			pb_GUI_Utility.DrawSeparator(2);
			GUI.backgroundColor = Color.white;

			GUILayout.Label("Material Palette", EditorStyles.boldLabel);

			Material[] materials = CurrentPalette;

			m_ViewScroll = GUILayout.BeginScrollView(m_ViewScroll);

			for(int i = 0; i < materials.Length; i++)
			{
				if(i == 10)
				{
					GUILayout.Space(2);
					GUI.backgroundColor = pb_Constant.ProBuilderLightGray;
					pb_GUI_Utility.DrawSeparator(1);
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
			if(pb_UV_Editor.instance == null)
			{
				if(em == (EventModifiers.Control | EventModifiers.Shift))
				{
					pbUndo.RecordObject(pb, "Quick Apply");
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

			pbUndo.RecordSelection(selection.ToArray(), "Set Face Materials");

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
			AssetDatabase.SaveAssets();
		}

		private static void OnFaceChanged( pb_Object pb )
		{
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}
#endif
	}
}
