using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using ProBuilder2.Interface;
using ProBuilder2.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{

#if !PROTOTYPE

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

		private static pb_Editor editor { get { return pb_Editor.instance; } }
		public static pb_Material_Editor instance { get; private set; }

		const string USER_MATERIALS_PATH = "Assets/ProCore/" + pb_Constant.PRODUCT_NAME + "/Data/UserMaterials.asset";

		public static void MenuOpenMaterialEditor()
		{
			EditorWindow.GetWindow<pb_Material_Editor>(true, "Material Editor", true).Show();
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 1 &1", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial0() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[0]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 2 &2", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial1() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[1]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 3 &3", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial2() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[2]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 4 &4", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial3() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[3]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 5 &5", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial4() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[4]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 6 &6", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial5() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[5]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 7 &7", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial6() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[6]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 8 &8", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial7() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[7]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 9 &9", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial8() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[8]);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Materials/Apply Material Preset 10 &0", false, pb_Constant.MENU_MATERIAL_COLORS)]
		public static void ApplyMaterial9() {
			Material[] mats;
			if( LoadMaterialPalette(out mats) ) ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), mats[9]);
		}

		void OnEnable()
		{
			instance = this;
			this.autoRepaintOnSceneChange = true;

			this.minSize = new Vector2(236, 250);
			this.maxSize = new Vector2(236, 10000);

			rowBackgroundStyle = new GUIStyle();
			rowBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;

			LoadMaterialPalette(out materials);
		}

		void OnDisable()
		{
			instance = null;
		}

		static bool LoadMaterialPalette(out Material[] materials)
		{
			pb_ObjectArray poa = (pb_ObjectArray)AssetDatabase.LoadAssetAtPath(USER_MATERIALS_PATH, typeof(pb_ObjectArray));

			if(poa != null)
			{
				materials = poa.GetArray<Material>();
				return true;
			}
			else
			{
				materials = new Material[10] 
				{
					pb_Constant.DefaultMaterial,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				return false;
			}
		}

		void SaveUserMaterials()
		{
			pb_ObjectArray poa = (pb_ObjectArray)ScriptableObject.CreateInstance(typeof(pb_ObjectArray));
			poa.array = materials;

			if(!System.IO.Directory.Exists("Assets/ProCore"))
				AssetDatabase.CreateFolder("Assets/", "ProCore");
			
			if(!System.IO.Directory.Exists("Assets/ProCore/ProBuilder"))
				AssetDatabase.CreateFolder("Assets/ProCore/", "ProBuilder");
			
			if(!System.IO.Directory.Exists("Assets/ProCore/ProBuilder/Data"))
				AssetDatabase.CreateFolder("Assets/ProCore/ProBuilder", "Data");

			AssetDatabase.CreateAsset(poa, USER_MATERIALS_PATH);
			AssetDatabase.SaveAssets();
		}

		// Functional vars
		Material queuedMaterial;
		Material[] materials;

		// GUI vars
		GUIStyle rowBackgroundStyle;
		Vector2 scroll = Vector2.zero;

		void OnGUI()
		{
			GUILayout.Label("Quick Material", EditorStyles.boldLabel);
			Rect r = GUILayoutUtility.GetLastRect();
			int left = Screen.width - 68;

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width-74));
				GUILayout.BeginVertical();

					queuedMaterial = (Material)EditorGUILayout.ObjectField(queuedMaterial, typeof(Material), false);

					GUILayout.Space(2);

					if(GUILayout.Button("Apply (Ctrl+Shift+Click)"))	
						ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), queuedMaterial);

					GUI.enabled = editor != null && editor.selectedFaceCount > 0;
					if(GUILayout.Button("Match Selection"))
					{
						pb_Object tp;
						pb_Face tf;
						if( editor.GetFirstSelectedFace(out tp, out tf) )
							queuedMaterial = tf.material;
					}
					GUI.enabled = true;

				GUILayout.EndVertical();

				GUI.Box( new Rect(left, r.y + r.height + 2, 64, 64), "" );
				if(queuedMaterial != null && queuedMaterial.mainTexture != null)	
					EditorGUI.DrawPreviewTexture( new Rect(left+2, r.y + r.height + 4, 60, 60), queuedMaterial.mainTexture, queuedMaterial, ScaleMode.StretchToFill, 0);
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

			scroll = GUILayout.BeginScrollView(scroll);
				
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
								ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), materials[i]);
						}
						else
						{
							if(GUILayout.Button("Apply", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(44)))
								ApplyMaterial(pbUtil.GetComponents<pb_Object>(Selection.transforms), materials[i]);
							
							GUI.backgroundColor = Color.red;
							if(GUILayout.Button("", EditorStyles.miniButtonRight, GUILayout.MaxWidth(14)))
							{
								Material[] temp = new Material[materials.Length-1];
								System.Array.Copy(materials, 0, temp, 0, materials.Length-1);
								materials = temp;
								SaveUserMaterials();
								return;
							}
							GUI.backgroundColor = Color.white;
						}

						EditorGUI.BeginChangeCheck();
							materials[i] = (Material)EditorGUILayout.ObjectField(materials[i], typeof(Material), false);
						if( EditorGUI.EndChangeCheck() )
							SaveUserMaterials();

					GUILayout.EndHorizontal();
				}


				if(GUILayout.Button("Add"))	
				{
					Material[] temp = new Material[materials.Length+1];
					System.Array.Copy(materials, 0, temp, 0, materials.Length);
					materials = temp;
					SaveUserMaterials();
				}
			GUILayout.EndScrollView();
		}

		/**
		 * Applies the currently queued material to the selected face and eats the event.
		 */
		public bool ClickShortcutCheck(EventModifiers em, pb_Object pb, pb_Face quad)
		{
			if(em == (EventModifiers.Control | EventModifiers.Shift))
			{
				pbUndo.RecordObject(pb, "Quick Apply NoDraw");
				pb.SetFaceMaterial(quad, queuedMaterial);
				OnFaceChanged(pb);
				pb_Editor_Utility.ShowNotification("Quick Apply Material");
				return true;
			}

			return false;
		}

		public static void ApplyMaterial(pb_Object[] selection, Material mat)
		{
			if(mat == null) return;

			pbUndo.RecordObjects(selection, "Set Face Materials");

			foreach(pb_Object pb in selection)
			{
				pb_Face[] faces = pb.SelectedFaces;
				pb.SetFaceMaterial(faces == null || faces.Length < 1 ? pb.faces : faces, mat);

				OnFaceChanged(pb);
			}

			if(pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0)
				pb_Editor_Utility.ShowNotification("Set Material\n" + mat.name);
		}

		private static void OnFaceChanged( pb_Object pb )
		{
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
			
			// StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags( pb.gameObject );
			
			// // if nodraw not found, and entity type should be batching static
			// if(pb.GetComponent<pb_Entity>().entityType != EntityType.Mover)
			// {
			// 	flags = flags | StaticEditorFlags.BatchingStatic;
			// 	GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);
			// }
		}
	}
	#endif
}