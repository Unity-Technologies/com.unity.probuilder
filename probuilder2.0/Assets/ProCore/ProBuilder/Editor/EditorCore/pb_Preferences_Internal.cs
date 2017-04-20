using UnityEngine;
using UnityEditor;
#if !UNITY_4_7
using UnityEngine.Rendering;
#endif
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Manage ProBuilder preferences.
	 */
	public class pb_Preferences_Internal
	{
		private static Dictionary<string, bool> m_BoolDefaults = new Dictionary<string, bool>()
		{
			{ pb_Constant.pbForceConvex, false },
			{ pb_Constant.pbManifoldEdgeExtrusion, false },
			{ pb_Constant.pbPBOSelectionOnly, false },
			{ pb_Constant.pbCloseShapeWindow, false },
			{ pb_Constant.pbGrowSelectionUsingAngle, false },
			{ pb_Constant.pbNormalizeUVsOnPlanarProjection, false },
			{ pb_Constant.pbDisableAutoUV2Generation, false },
			{ pb_Constant.pbShowSceneInfo, false },
			{ pb_Constant.pbEnableBackfaceSelection, false },
			{ pb_Constant.pbVertexPaletteDockable, false },
			{ pb_Constant.pbGrowSelectionAngleIterative, false },
			{ pb_Constant.pbIconGUI, false },
			{ pb_Constant.pbUniqueModeShortcuts, false },
			{ pb_Constant.pbShiftOnlyTooltips, false },
			{ pb_Constant.pbCollapseVertexToFirst, false },
			{ pb_Constant.pbDragSelectWholeElement, false },
			{ pb_Constant.pbEnableExperimental, false },
			{ pb_Constant.pbMeshesAreAssets, false }
		};

		private static Dictionary<string, float> m_FloatDefaults = new Dictionary<string, float>()
		{
			{ pb_Constant.pbVertexHandleSize, .5f },
			{ pb_Constant.pbGrowSelectionAngle, 42f },
			{ pb_Constant.pbExtrudeDistance, .5f },
			{ pb_Constant.pbWeldDistance, Mathf.Epsilon },
			{ pb_Constant.pbUVGridSnapValue, .125f },
			{ pb_Constant.pbUVWeldDistance, .01f },
			{ pb_Constant.pbBevelAmount, .05f }
		};

		private static Dictionary<string, int> m_IntDefaults = new Dictionary<string, int>()
		{
			{ pb_Constant.pbDefaultEditLevel, 0 },
			{ pb_Constant.pbDefaultSelectionMode, 0 },
			{ pb_Constant.pbHandleAlignment, 0 },
			{ pb_Constant.pbDefaultCollider, (int) ColliderType.MeshCollider },
			{ pb_Constant.pbVertexColorTool, (int) VertexColorTool.Painter },
			{ pb_Constant.pbToolbarLocation, (int) SceneToolbarLocation.UpperCenter },
			{ pb_Constant.pbDefaultEntity, (int) EntityType.Detail },
			{ pb_Constant.pbDragSelectMode, (int) DragSelectMode.Difference },
			{ pb_Constant.pbExtrudeMethod, (int) ExtrudeMethod.VertexNormal },
#if !UNITY_4_7
			{ pb_Constant.pbShadowCastingMode, (int) ShadowCastingMode.TwoSided },
#endif
		};

		private static Dictionary<string, Color> m_ColorDefaults = new Dictionary<string, Color>()
		{
			{ pb_Constant.pbDefaultFaceColor, new Color(0f, .86f, 1f, .275f) },
			{ pb_Constant.pbDefaultEdgeColor, new Color(0f, .25f, 1f, 1f) },
			{ pb_Constant.pbDefaultVertexColor, new Color(.8f, .8f, .8f, 1f) },
			{ pb_Constant.pbDefaultSelectedVertexColor, Color.green },
		};

		private static Dictionary<string, string> m_StringDefaults = new Dictionary<string, string>()
		{
		};

		private static pb_PreferenceDictionary m_Preferences = null;

		/**
		 *	Access the project local preferences asset.
		 */
		public static pb_PreferenceDictionary preferences
		{
			get
			{
				if(m_Preferences == null)
					m_Preferences = pb_FileUtil.LoadRequiredRelative<pb_PreferenceDictionary>("Data/ProBuilderPreferences.asset");

				return m_Preferences;
			}
		}

		/**
		 * Checks if pref key exists in library, and if so return the value.  If not, return the default value (true).
		 */
		public static bool GetBool(string pref)
		{
			// Backwards compatibility reasons dictate that default bool value is true.
			if(m_BoolDefaults.ContainsKey(pref))
				return GetBool(pref, m_BoolDefaults[pref]);

			return GetBool(pref, true);
		}

		/**
		 *	Get a preference bool value. Local preference has priority over EditorPref.
		 */
		public static bool GetBool(string key, bool fallback)
		{
			if(preferences.HasKey<bool>(key))
				return preferences.GetBool(key, fallback);
			else if(EditorPrefs.HasKey(key))
				return EditorPrefs.GetBool(key);

			return fallback;
		}

		/**
		 *	Get float value that is stored in preferences, or it's default value.
		 */
		public static float GetFloat(string key)
		{
			if(m_FloatDefaults.ContainsKey(key))
				return GetFloat(key, m_FloatDefaults[key]);
			return GetFloat(key, 1f);
		}

		public static float GetFloat(string key, float fallback)
		{
			if(preferences.HasKey<float>(key))
				return preferences.GetFloat(key, fallback);
			return EditorPrefs.GetFloat(key, fallback);
		}

		/**
		 *	Get int value that is stored in preferences, or it's default value.
		 */
		public static int GetInt(string key)
		{
			if(m_IntDefaults.ContainsKey(key))
				return GetInt(key, m_IntDefaults[key]);
			return GetInt(key, 0);
		}

		public static int GetInt(string key, int fallback)
		{
			if(preferences.HasKey<int>(key))
				return preferences.GetInt(key, fallback);
			return EditorPrefs.GetInt(key, fallback);
		}

		/**
		 *	Get an enum value from the stored preferences (or it's default value).
		 */
		public static T GetEnum<T>(string key) where T : struct, System.IConvertible
		{
			// @todo
			return (T) (object) GetInt(key);
		}

		/**
		 *	Get Color value stored in preferences.
		 */
		public static Color GetColor(string key)
		{
			if(m_ColorDefaults.ContainsKey(key))
				return GetColor(key, m_ColorDefaults[key]);
			return GetColor(key, Color.white);
		}

		public static Color GetColor(string key, Color fallback)
		{
			if(preferences.HasKey<Color>(key))
				return preferences.GetColor(key, fallback);
			pbUtil.TryParseColor(EditorPrefs.GetString(key), ref fallback);
			return fallback;
		}

		/**
		 *	Get the string value associated with this key.
		 */
		public static string GetString(string key)
		{
			if(m_StringDefaults.ContainsKey(key))
				return GetString(key, m_StringDefaults[key]);
			return GetString(key, "");
		}

		public static string GetString(string key, string fallback)
		{
			if(preferences.HasKey<string>(key))
				return preferences.GetString(key, fallback);
			return EditorPrefs.GetString(key, fallback);
		}

		/**
		 *	Get a material from preferences.
		 */
		public static Material GetMaterial(string key)
		{
			if(preferences.HasKey<Material>(key))
				return preferences.GetMaterial(key);

			Material mat = null;

			switch(key)
			{
				case pb_Constant.pbDefaultMaterial:
					if(EditorPrefs.HasKey(key))
					{
						if(EditorPrefs.GetString(key) == "Default-Diffuse")
							return pb_Constant.UnityDefaultDiffuse;

						mat = (Material) AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString(key), typeof(Material));
					}
					break;

				default:
					return pb_Constant.DefaultMaterial;
			}

			if(!mat)
				mat = pb_Constant.DefaultMaterial;

			return mat;
		}

		/**
		 *	Retrieve stored shortcuts from preferences in an IEnumerable format.
		 */
		public static IEnumerable<pb_Shortcut> GetShortcuts()
		{
			return EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts) ?
				pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts)) :
				pb_Shortcut.DefaultShortcuts();													// Key not found, return the default
		}
	}
}
