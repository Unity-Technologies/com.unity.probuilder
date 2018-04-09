using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using ProBuilder.EditorCore;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	static class pb_Preferences
	{
		static int s_ShortcutIndex = 0;
		static Vector2 s_ShortcutScroll = Vector2.zero;
		const int k_ShortcutLineHeight = 20;

		static bool s_PrefsLoaded = false;

		static bool pbUseUnityColors;
		static float pbLineHandleSize;
		static float pbWireframeSize;
		static Color faceSelectedColor;
		static Color pbWireframeColor;
		static Color pbSelectedEdgeColor;
		static Color pbUnselectedEdgeColor;
		static Color vertexSelectedColor;
		static Color vertexUnselectedColor;
		static Color pbPreselectionColor;
		static bool pbSelectedFaceDither;

		static bool defaultOpenInDockableWindow;
		static Material pbDefaultMaterial;
		static Vector2 m_SettingsScroll = Vector2.zero;
		static bool pbShowEditorNotifications;
		static bool pbForceConvex = false;
		static bool pbForceVertexPivot = true;
		static bool pbForceGridPivot = true;
		static bool pbPerimeterEdgeBridgeOnly;
		static bool pbPBOSelectionOnly;
		static bool pbCloseShapeWindow = false;
		static bool pbUVEditorFloating = true;
		static bool pbStripProBuilderOnBuild = true;
		static bool pbDisableAutoUV2Generation = false;
		static bool pbShowSceneInfo = false;
		static bool pbUniqueModeShortcuts = false;
		static bool pbIconGUI = false;
		static bool pbShiftOnlyTooltips = false;
		static bool pbMeshesAreAssets = false;
		static bool pbElementSelectIsHamFisted = false;
		static bool pbEnableExperimental = false;

		static bool showMissingLightmapUvWarning = false;
		static bool pbManageLightmappingStaticFlag = false;
		static ShadowCastingMode pbShadowCastingMode = ShadowCastingMode.On;

		static StaticEditorFlags pbDefaultStaticFlags = (StaticEditorFlags) 0xFFFF;

		static ColliderType defaultColliderType = ColliderType.BoxCollider;
		static SceneToolbarLocation pbToolbarLocation = SceneToolbarLocation.UpperCenter;

		static float pbUVGridSnapValue;
		static float pbVertexHandleSize;

		static pb_Shortcut[] defaultShortcuts;

		[PreferenceItem(pb_Constant.PRODUCT_NAME)]
		static void PreferencesGUI()
		{
			LoadPrefs();

			m_SettingsScroll = EditorGUILayout.BeginScrollView(m_SettingsScroll);

			EditorGUI.BeginChangeCheck();

			if (GUILayout.Button("Reset All Preferences"))
				ResetToDefaults();

			/**
			 * GENERAL SETTINGS
			 */
			GUILayout.Label("General Settings", EditorStyles.boldLabel);

			pbStripProBuilderOnBuild =
				EditorGUILayout.Toggle(
					new GUIContent("Strip PB Scripts on Build",
						"If true, when building an executable all ProBuilder scripts will be stripped from your built product."),
					pbStripProBuilderOnBuild);
			pbDisableAutoUV2Generation = EditorGUILayout.Toggle(
				new GUIContent("Disable Auto UV2 Generation",
					"Disables automatic generation of UV2 channel.  If Unity is sluggish when working with large ProBuilder objects, disabling UV2 generation will improve performance.  Use `Actions/Generate UV2` or `Actions/Generate Scene UV2` to build lightmap UVs prior to baking."),
				pbDisableAutoUV2Generation);
			pbShowSceneInfo =
				EditorGUILayout.Toggle(
					new GUIContent("Show Scene Info", "Displays the selected object vertex and triangle counts in the scene view."),
					pbShowSceneInfo);
			pbShowEditorNotifications = EditorGUILayout.Toggle("Show Editor Notifications", pbShowEditorNotifications);

			/**
			 * TOOLBAR SETTINGS
			 */
			GUILayout.Label("Toolbar Settings", EditorStyles.boldLabel);

			pbIconGUI = EditorGUILayout.Toggle(
				new GUIContent("Use Icon GUI", "Toggles the ProBuilder window interface between text and icon versions."),
				pbIconGUI);
			pbShiftOnlyTooltips =
				EditorGUILayout.Toggle(new GUIContent("Shift Key Tooltips", "Tooltips will only show when the Shift key is held"),
					pbShiftOnlyTooltips);
			pbToolbarLocation = (SceneToolbarLocation) EditorGUILayout.EnumPopup("Toolbar Location", pbToolbarLocation);

			pbUniqueModeShortcuts = EditorGUILayout.Toggle(
				new GUIContent("Unique Mode Shortcuts",
					"When off, the G key toggles between Object and Element modes and H enumerates the element modes.  If on, G, H, J, and K are shortcuts to Object, Vertex, Edge, and Face modes respectively."),
				pbUniqueModeShortcuts);
			defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);

			/**
			 * DEFAULT SETTINGS
			 */
			GUILayout.Label("Defaults", EditorStyles.boldLabel);

			pbDefaultMaterial =
				(Material) EditorGUILayout.ObjectField("Default Material", pbDefaultMaterial, typeof(Material), false);

			pbDefaultStaticFlags = (StaticEditorFlags) EditorGUILayout.EnumFlagsField("Static Flags", pbDefaultStaticFlags);

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Default Collider");
			defaultColliderType = ((ColliderType) EditorGUILayout.EnumPopup((ColliderType) defaultColliderType));
			GUILayout.EndHorizontal();

			if ((ColliderType) defaultColliderType == ColliderType.MeshCollider)
				pbForceConvex = EditorGUILayout.Toggle("Force Convex Mesh Collider", pbForceConvex);

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Shadow Casting Mode");
			pbShadowCastingMode = (ShadowCastingMode) EditorGUILayout.EnumPopup(pbShadowCastingMode);
			GUILayout.EndHorizontal();

			/**
			 * HANDLE COLORS
			 */
			GUILayout.Label("Handles & Colors", EditorStyles.boldLabel);

			pbUseUnityColors = EditorGUILayout.Toggle("Use Unity Colors", pbUseUnityColors);

			if (!pbUseUnityColors)
			{
				pbWireframeColor = EditorGUILayout.ColorField("Wireframe", pbWireframeColor);
				pbPreselectionColor = EditorGUILayout.ColorField("Preselection", pbPreselectionColor);
				faceSelectedColor = EditorGUILayout.ColorField("Selected Face Color", faceSelectedColor);
				pbSelectedFaceDither = EditorGUILayout.Toggle("Dither Face Overlay", pbSelectedFaceDither);
				pbUnselectedEdgeColor = EditorGUILayout.ColorField("Unselected Edge Color", pbUnselectedEdgeColor);
				pbSelectedEdgeColor = EditorGUILayout.ColorField("Selected Edge Color", pbSelectedEdgeColor);
				vertexUnselectedColor = EditorGUILayout.ColorField("Unselected Vertex Color", vertexUnselectedColor);
				vertexSelectedColor = EditorGUILayout.ColorField("Selected Vertex Color", vertexSelectedColor);
			}

			pbVertexHandleSize = EditorGUILayout.Slider("Vertex Size", pbVertexHandleSize, 1f, 10f);

			bool geoLine = pb_MeshHandles.geometryShadersSupported;
			GUI.enabled = geoLine;
			pbLineHandleSize = EditorGUILayout.Slider("Line Size", geoLine ? pbLineHandleSize : 0f, 0f, 3f);
			pbWireframeSize = EditorGUILayout.Slider("Wireframe Size", geoLine ? pbWireframeSize : 0f, 0f, 3f);
			GUI.enabled = true;

			/**
			 * MISC. SETTINGS
			 */
			GUILayout.Label("Misc. Settings", EditorStyles.boldLabel);

			pbManageLightmappingStaticFlag = EditorGUILayout.Toggle(
				new GUIContent("Manage Lightmap Static Flag",
					"Allow ProBuilder to toggle off the Lightmap Static flag when no UV2 channel is present. This prevents lighting artifacts caused by a missing UV2 channel."),
				pbManageLightmappingStaticFlag);

			showMissingLightmapUvWarning = EditorGUILayout.Toggle("Show Missing Lightmap UVs Warning", showMissingLightmapUvWarning);

			pbPBOSelectionOnly =
				EditorGUILayout.Toggle(
					new GUIContent("Only PBO are Selectable",
						"If true, you will not be able to select non probuilder objects in Geometry and Texture mode"),
					pbPBOSelectionOnly);
			pbCloseShapeWindow =
				EditorGUILayout.Toggle(
					new GUIContent("Close shape window after building",
						"If true the shape window will close after hitting the build button"), pbCloseShapeWindow);

			GUILayout.Space(4);

			/**
			 * GEOMETRY EDITING SETTINGS
			 */
			GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);

			pbElementSelectIsHamFisted = !EditorGUILayout.Toggle(
				new GUIContent("Precise Element Selection",
					"When enabled you will be able to select object faces when in Vertex of Edge mode by clicking the center of a face.  When disabled, edge and vertex selection will always be restricted to the nearest element."),
				!pbElementSelectIsHamFisted);

			pbForceVertexPivot =
				EditorGUILayout.Toggle(
					new GUIContent("Force Pivot to Vertex Point",
						"If true, new objects will automatically have their pivot point set to a vertex instead of the center."),
					pbForceVertexPivot);
			pbForceGridPivot = EditorGUILayout.Toggle(
				new GUIContent("Force Pivot to Grid",
					"If true, newly instantiated pb_Objects will be snapped to the nearest point on grid.  If ProGrids is present, the snap value will be used, otherwise decimals are simply rounded to whole numbers."),
				pbForceGridPivot);
			pbPerimeterEdgeBridgeOnly = EditorGUILayout.Toggle(
				new GUIContent("Bridge Perimeter Edges Only",
					"If true, only edges on the perimeters of an object may be bridged.  If false, you may bridge any between any two edges you like."),
				pbPerimeterEdgeBridgeOnly);

			GUILayout.Space(4);

			GUILayout.Label("Experimental", EditorStyles.boldLabel);

			pbEnableExperimental = EditorGUILayout.Toggle(
				new GUIContent("Experimental Features",
					"Enables some experimental new features that we're trying out.  These may be incomplete or buggy, so please exercise caution when making use of this functionality!"),
				pbEnableExperimental);
			pbMeshesAreAssets =
				EditorGUILayout.Toggle(
					new GUIContent("Meshes Are Assets",
						"Experimental!  Instead of storing mesh data in the scene, this toggle creates a Mesh cache in the Project that ProBuilder will use."),
					pbMeshesAreAssets);

			GUILayout.Space(4);

			/**
			 * UV EDITOR SETTINGS
			 */
			GUILayout.Label("UV Editing Settings", EditorStyles.boldLabel);
			pbUVGridSnapValue = EditorGUILayout.FloatField("UV Snap Increment", pbUVGridSnapValue);
			pbUVGridSnapValue = Mathf.Clamp(pbUVGridSnapValue, .015625f, 2f);
			pbUVEditorFloating =
				EditorGUILayout.Toggle(
					new GUIContent("Editor window floating", "If true UV   Editor window will open as a floating window"),
					pbUVEditorFloating);

			GUILayout.Space(4);

			GUILayout.Label("Shortcut Settings", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(GUILayout.MinWidth(EditorGUIUtility.labelWidth), GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
			ShortcutSelectPanel();
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			ShortcutEditPanel();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();

			// Save the preferences
			if (EditorGUI.EndChangeCheck())
				SetPrefs();
		}

		public static void ResetToDefaults()
		{
			if (EditorUtility.DisplayDialog("Delete ProBuilder editor preferences?",
				"Are you sure you want to delete all existing ProBuilder preferences?\n\nThis action cannot be undone.", "Yes",
				"No"))
			{
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultEditLevel);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultSelectionMode);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbHandleAlignment);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbVertexColorTool);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbToolbarLocation);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultEntity);

				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUseUnityColors);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbLineHandleSize);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbWireframeSize);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedFaceColor);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbWireframeColor);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbPreselectionColor);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedFaceDither);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedVertexColor);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUnselectedVertexColor);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbSelectedEdgeColor);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUnselectedEdgeColor);

				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultOpenInDockableWindow);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbEditorPrefVersion);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbEditorShortcutsVersion);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultCollider);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbForceConvex);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbVertexColorPrefs);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowEditorNotifications);
#pragma warning disable 612
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDragCheckLimit);
#pragma warning restore 612
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbForceVertexPivot);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbForceGridPivot);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbManifoldEdgeExtrusion);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbPerimeterEdgeBridgeOnly);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbPBOSelectionOnly);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbCloseShapeWindow);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUVEditorFloating);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUVMaterialPreview);
#pragma warning disable 612
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowSceneToolbar);
#pragma warning restore 612
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbNormalizeUVsOnPlanarProjection);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbStripProBuilderOnBuild);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDisableAutoUV2Generation);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowSceneInfo);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbEnableBackfaceSelection);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbVertexPaletteDockable);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbExtrudeAsGroup);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUniqueModeShortcuts);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbMaterialEditorFloating);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShapeWindowFloating);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbIconGUI);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShiftOnlyTooltips);
#pragma warning disable 612
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDrawAxisLines);
#pragma warning restore 612
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbCollapseVertexToFirst);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbMeshesAreAssets);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbElementSelectIsHamFisted);
#pragma warning disable 618
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDragSelectWholeElement);
#pragma warning restore 618
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbEnableExperimental);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbFillHoleSelectsEntirePath);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDetachToNewObject);
#pragma warning disable 618
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbPreserveFaces);
#pragma warning restore 618
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbVertexHandleSize);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUVGridSnapValue);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbUVWeldDistance);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbWeldDistance);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbExtrudeDistance);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbBevelAmount);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbEdgeSubdivisions);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultShortcuts);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultMaterial);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbGrowSelectionUsingAngle);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbGrowSelectionAngle);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbGrowSelectionAngleIterative);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowDetail);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowOccluder);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowMover);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowCollider);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowTrigger);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowNoDraw);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShowMissingLightmapUvWarning);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbManageLightmappingStaticFlag);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbShadowCastingMode);
				pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultStaticFlags);
			}

			s_PrefsLoaded = false;
			LoadPrefs();
		}

		static void ShortcutSelectPanel()
		{
			GUILayout.Space(4);
			GUI.contentColor = Color.white;

			GUIStyle labelStyle = GUIStyle.none;

			if (EditorGUIUtility.isProSkin)
				labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

			labelStyle.alignment = TextAnchor.MiddleLeft;
			labelStyle.contentOffset = new Vector2(4f, 0f);

			s_ShortcutScroll =
				EditorGUILayout.BeginScrollView(s_ShortcutScroll, false, true, GUILayout.MinHeight(150));

			for (int n = 1; n < defaultShortcuts.Length; n++)
			{
				if (n == s_ShortcutIndex)
				{
					GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
					labelStyle.normal.background = EditorGUIUtility.whiteTexture;
					Color oc = labelStyle.normal.textColor;
					labelStyle.normal.textColor = Color.white;
					GUILayout.Box(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(k_ShortcutLineHeight),
						GUILayout.MaxHeight(k_ShortcutLineHeight));
					labelStyle.normal.background = null;
					labelStyle.normal.textColor = oc;
					GUI.backgroundColor = Color.white;
				}
				else
				{

					if (GUILayout.Button(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(k_ShortcutLineHeight),
						GUILayout.MaxHeight(k_ShortcutLineHeight)))
					{
						s_ShortcutIndex = n;
					}
				}
			}

			EditorGUILayout.EndScrollView();
		}

		static void ShortcutEditPanel()
		{
			GUILayout.Label("Key", EditorStyles.boldLabel);
			KeyCode key = defaultShortcuts[s_ShortcutIndex].key;
			key = (KeyCode) EditorGUILayout.EnumPopup(key);
			defaultShortcuts[s_ShortcutIndex].key = key;

			GUILayout.Label("Modifiers", EditorStyles.boldLabel);
			// EnumMaskField returns a bit-mask where the flags correspond to the indices of the enum, not the enum values,
			// so this isn't technically correct.
#if UNITY_2017_3_OR_NEWER
			EventModifiers em = (EventModifiers) defaultShortcuts[s_ShortcutIndex].eventModifiers;
			defaultShortcuts[s_ShortcutIndex].eventModifiers = (EventModifiers) EditorGUILayout.EnumFlagsField(em);
#else
			EventModifiers em = (EventModifiers) (((int)defaultShortcuts[shortcutIndex].eventModifiers) * 2);
			em = (EventModifiers)EditorGUILayout.EnumMaskField(em);
			defaultShortcuts[shortcutIndex].eventModifiers = (EventModifiers) (((int)em) / 2);
#endif
			GUILayout.Label("Description", EditorStyles.boldLabel);

			GUILayout.Label(defaultShortcuts[s_ShortcutIndex].description, EditorStyles.wordWrappedLabel);
		}

		static void LoadPrefs()
		{
			if (s_PrefsLoaded)
				return;
			s_PrefsLoaded = true;
			pbStripProBuilderOnBuild = pb_PreferencesInternal.GetBool(pb_Constant.pbStripProBuilderOnBuild);
			pbDisableAutoUV2Generation = pb_PreferencesInternal.GetBool(pb_Constant.pbDisableAutoUV2Generation);
			pbShowSceneInfo = pb_PreferencesInternal.GetBool(pb_Constant.pbShowSceneInfo);
			defaultOpenInDockableWindow = pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
			pbForceConvex = pb_PreferencesInternal.GetBool(pb_Constant.pbForceConvex);
			pbForceGridPivot = pb_PreferencesInternal.GetBool(pb_Constant.pbForceGridPivot);
			pbForceVertexPivot = pb_PreferencesInternal.GetBool(pb_Constant.pbForceVertexPivot);
			pbPerimeterEdgeBridgeOnly = pb_PreferencesInternal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);
			pbPBOSelectionOnly = pb_PreferencesInternal.GetBool(pb_Constant.pbPBOSelectionOnly);
			pbCloseShapeWindow = pb_PreferencesInternal.GetBool(pb_Constant.pbCloseShapeWindow);
			pbUVEditorFloating = pb_PreferencesInternal.GetBool(pb_Constant.pbUVEditorFloating);
			pbShowEditorNotifications = pb_PreferencesInternal.GetBool(pb_Constant.pbShowEditorNotifications);
			pbUniqueModeShortcuts = pb_PreferencesInternal.GetBool(pb_Constant.pbUniqueModeShortcuts);
			pbIconGUI = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI);
			pbShiftOnlyTooltips = pb_PreferencesInternal.GetBool(pb_Constant.pbShiftOnlyTooltips);
			pbMeshesAreAssets = pb_PreferencesInternal.GetBool(pb_Constant.pbMeshesAreAssets);
			pbElementSelectIsHamFisted = pb_PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted);
			pbEnableExperimental = pb_PreferencesInternal.GetBool(pb_Constant.pbEnableExperimental);
			showMissingLightmapUvWarning = pb_PreferencesInternal.GetBool(pb_Constant.pbShowMissingLightmapUvWarning, false);
			pbManageLightmappingStaticFlag = pb_PreferencesInternal.GetBool(pb_Constant.pbManageLightmappingStaticFlag, false);

			pbUseUnityColors = pb_PreferencesInternal.GetBool(pb_Constant.pbUseUnityColors);
			pbLineHandleSize = pb_PreferencesInternal.GetFloat(pb_Constant.pbLineHandleSize);
			pbWireframeSize = pb_PreferencesInternal.GetFloat(pb_Constant.pbWireframeSize);
			faceSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedFaceColor);
			pbWireframeColor = pb_PreferencesInternal.GetColor(pb_Constant.pbWireframeColor);
			pbPreselectionColor = pb_PreferencesInternal.GetColor(pb_Constant.pbPreselectionColor);
			pbSelectedFaceDither = pb_PreferencesInternal.GetBool(pb_Constant.pbSelectedFaceDither);
			pbSelectedEdgeColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedEdgeColor);
			pbUnselectedEdgeColor = pb_PreferencesInternal.GetColor(pb_Constant.pbUnselectedEdgeColor);
			vertexSelectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbSelectedVertexColor);
			vertexUnselectedColor = pb_PreferencesInternal.GetColor(pb_Constant.pbUnselectedVertexColor);

			pbUVGridSnapValue = pb_PreferencesInternal.GetFloat(pb_Constant.pbUVGridSnapValue);
			pbVertexHandleSize = pb_PreferencesInternal.GetFloat(pb_Constant.pbVertexHandleSize);

			defaultColliderType = pb_PreferencesInternal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider);
			pbToolbarLocation = pb_PreferencesInternal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation);
			pbShadowCastingMode = pb_PreferencesInternal.GetEnum<ShadowCastingMode>(pb_Constant.pbShadowCastingMode);
			pbDefaultStaticFlags = pb_PreferencesInternal.GetEnum<StaticEditorFlags>(pb_Constant.pbDefaultStaticFlags);

			pbDefaultMaterial = pb_PreferencesInternal.GetMaterial(pb_Constant.pbDefaultMaterial);

			defaultShortcuts = pb_PreferencesInternal.GetShortcuts().ToArray();
		}

		public static void SetPrefs()
		{
			pb_PreferencesInternal.SetBool(pb_Constant.pbStripProBuilderOnBuild, pbStripProBuilderOnBuild);
			pb_PreferencesInternal.SetBool(pb_Constant.pbDisableAutoUV2Generation, pbDisableAutoUV2Generation);
			pb_PreferencesInternal.SetBool(pb_Constant.pbShowSceneInfo, pbShowSceneInfo, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetInt(pb_Constant.pbToolbarLocation, (int) pbToolbarLocation, pb_PreferenceLocation.Global);

			pb_PreferencesInternal.SetBool(pb_Constant.pbUseUnityColors, pbUseUnityColors, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbSelectedFaceDither, pbSelectedFaceDither, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetFloat(pb_Constant.pbLineHandleSize, pbLineHandleSize, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetFloat(pb_Constant.pbVertexHandleSize, pbVertexHandleSize, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetFloat(pb_Constant.pbWireframeSize, pbWireframeSize, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbSelectedFaceColor, faceSelectedColor, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbWireframeColor, pbWireframeColor, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbPreselectionColor, pbPreselectionColor, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbSelectedVertexColor, vertexSelectedColor, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbUnselectedVertexColor, vertexUnselectedColor, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbSelectedEdgeColor, pbSelectedEdgeColor, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetColor(pb_Constant.pbUnselectedEdgeColor, pbUnselectedEdgeColor, pb_PreferenceLocation.Global);

			pb_PreferencesInternal.SetString(pb_Constant.pbDefaultShortcuts, pb_Shortcut.ShortcutsToString(defaultShortcuts), pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetMaterial(pb_Constant.pbDefaultMaterial, pbDefaultMaterial);
			pb_PreferencesInternal.SetInt(pb_Constant.pbDefaultCollider, (int) defaultColliderType);
			pb_PreferencesInternal.SetInt(pb_Constant.pbShadowCastingMode, (int) pbShadowCastingMode);
			pb_PreferencesInternal.SetInt(pb_Constant.pbDefaultStaticFlags, (int) pbDefaultStaticFlags);
			pb_PreferencesInternal.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, defaultOpenInDockableWindow, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbShowEditorNotifications, pbShowEditorNotifications, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbForceConvex, pbForceConvex);
			pb_PreferencesInternal.SetBool(pb_Constant.pbForceVertexPivot, pbForceVertexPivot, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbForceGridPivot, pbForceGridPivot, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbPerimeterEdgeBridgeOnly, pbPerimeterEdgeBridgeOnly, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbPBOSelectionOnly, pbPBOSelectionOnly, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbCloseShapeWindow, pbCloseShapeWindow, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbUVEditorFloating, pbUVEditorFloating, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbUniqueModeShortcuts, pbUniqueModeShortcuts, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbIconGUI, pbIconGUI, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbShiftOnlyTooltips, pbShiftOnlyTooltips, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbMeshesAreAssets, pbMeshesAreAssets);
			pb_PreferencesInternal.SetBool(pb_Constant.pbElementSelectIsHamFisted, pbElementSelectIsHamFisted, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbEnableExperimental, pbEnableExperimental, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbShowMissingLightmapUvWarning, showMissingLightmapUvWarning, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetBool(pb_Constant.pbManageLightmappingStaticFlag, pbManageLightmappingStaticFlag, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.SetFloat(pb_Constant.pbUVGridSnapValue, pbUVGridSnapValue, pb_PreferenceLocation.Global);

			if (pb_Editor.instance != null)
			{
				pb_MeshHandles.Destroy();
				pb_Editor.instance.OnEnable();
			}

			SceneView.RepaintAll();
		}
	}
}
