using UnityEngine;
using UnityEditor;
#if !UNITY_4_7
using UnityEngine.Rendering;
#endif
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class pb_Preferences_Internal
{
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
	 * Checks if pref key exists in library, and if so return the value.  If not, return the default value.

	 */
	public static bool GetBool(string pref, bool forceDefault = false)
	{
		if(!forceDefault && EditorPrefs.HasKey(pref))
			return EditorPrefs.GetBool(pref);

		if(	pref == pb_Constant.pbForceConvex ||
			pref == pb_Constant.pbManifoldEdgeExtrusion ||
			pref == pb_Constant.pbPBOSelectionOnly ||
			pref == pb_Constant.pbCloseShapeWindow ||
			pref == pb_Constant.pbGrowSelectionUsingAngle ||
			pref == pb_Constant.pbNormalizeUVsOnPlanarProjection ||
			pref == pb_Constant.pbDisableAutoUV2Generation ||
			pref == pb_Constant.pbShowSceneInfo ||
			pref == pb_Constant.pbEnableBackfaceSelection ||
			pref == pb_Constant.pbVertexPaletteDockable ||
			pref == pb_Constant.pbGrowSelectionAngleIterative ||
			pref == pb_Constant.pbIconGUI ||
			pref == pb_Constant.pbUniqueModeShortcuts ||
			pref == pb_Constant.pbShiftOnlyTooltips ||
			pref == pb_Constant.pbCollapseVertexToFirst ||
			pref == pb_Constant.pbDragSelectWholeElement ||
			pref == pb_Constant.pbEnableExperimental ||
			pref == pb_Constant.pbMeshesAreAssets)
			return false;
		else
			return true;
	}

	/**
	 *	Get float value that is stored in preferences, or it's default value.
	 */
	public static float GetFloat(string pref, bool forceDefault = false)
	{
		switch(pref)
		{
			case pb_Constant.pbVertexHandleSize:
				return EditorPrefs.HasKey(pref) && !forceDefault ?  EditorPrefs.GetFloat(pref) : .5f;

			case pb_Constant.pbGrowSelectionAngle:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetFloat(pref) : 42f;

			case pb_Constant.pbExtrudeDistance:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetFloat(pref) : .5f;

			case pb_Constant.pbWeldDistance:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetFloat(pref) : Mathf.Epsilon;

			case pb_Constant.pbUVGridSnapValue:
				return EditorPrefs.HasKey(pref) && !forceDefault ? Mathf.Clamp(EditorPrefs.GetFloat(pref), .015625f, 2f) : .125f;

			case pb_Constant.pbUVWeldDistance:
				return EditorPrefs.HasKey(pref) && !forceDefault ? Mathf.Clamp(EditorPrefs.GetFloat(pref), Mathf.Epsilon, 10f) : .01f;

			case pb_Constant.pbBevelAmount:
				return EditorPrefs.HasKey(pref) && !forceDefault ? Mathf.Clamp(EditorPrefs.GetFloat(pref), Mathf.Epsilon, 1000f) : .05f;

			default:
				return 1f;
		}
	}

	/**
	 * Get int value if it exists, default value otherwise.
	 */
	public static int GetInt(string pref, bool forceDefault = false)
	{
		int key = 0;

		switch(pref)
		{
			case pb_Constant.pbDefaultEditLevel:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return key;

			case pb_Constant.pbDefaultSelectionMode:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return key;

			case pb_Constant.pbHandleAlignment:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return key;

			case pb_Constant.pbDefaultCollider:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) ColliderType.MeshCollider;
				return key;

			case pb_Constant.pbVertexColorTool:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) VertexColorTool.Painter;
				return key;

			case pb_Constant.pbToolbarLocation:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) SceneToolbarLocation.UpperCenter;
				return key;

			case pb_Constant.pbDefaultEntity:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) EntityType.Detail;
				return key;

			case pb_Constant.pbDragSelectMode:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) DragSelectMode.Difference;
				return key;

			case pb_Constant.pbExtrudeMethod:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) ExtrudeMethod.VertexNormal;
				return key;

			case pb_Constant.pbShadowCastingMode:
				#if !UNITY_4_7
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) ShadowCastingMode.TwoSided;
				#endif
				return key;

			default:
				return key;
		}
	}

	/**
	 *	Get an enum value from the stored preferences (or it's default value).
	 */
	public static T GetEnum<T>(string key, bool forceDefault = false) where T : struct, System.IConvertible
	{
		return (T) (object) GetInt(key, forceDefault);
	}

	/**
	 *	Get Color value stored in preferences.
	 */
	public static Color GetColor(string pref, bool forceDefault = false)
	{
		Color col = Color.white;

		if( !forceDefault && !pbUtil.ColorWithString( EditorPrefs.GetString(pref), out col) )

		switch(pref)
		{
			case pb_Constant.pbDefaultFaceColor:
					col = new Color(0f, .86f, 1f, .275f);
				break;

			case pb_Constant.pbDefaultEdgeColor:
					col = new Color(0f, .25f, 1f, 1f);
				break;

			case pb_Constant.pbDefaultVertexColor:
					col = new Color(.8f, .8f, .8f, 1f);
				break;

			case pb_Constant.pbDefaultSelectedVertexColor:
					col = Color.green;
				break;
		}

		return col;
	}

	/**
	 *	Get a material from preferences.
	 */
	public static Material GetMaterial(string pref, bool forceDefault = false)
	{
		Material mat = null;

		switch(pref)
		{
			case pb_Constant.pbDefaultMaterial:
				if(!forceDefault && EditorPrefs.HasKey(pref))
				{
					if(EditorPrefs.GetString(pref) == "Default-Diffuse")
						return pb_Constant.UnityDefaultDiffuse;

					mat = (Material) AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString(pref), typeof(Material));
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
	 *	Get the string value associated with this key.
	 */
	public static string GetString(string pref, bool forceDefault = false)
	{
		return preferences.GetString(pref);
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
