using UnityEngine;
using UnityEditor;
#if !UNITY_4_7
using UnityEngine.Rendering;
#endif
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

#if PB_DEBUG
using Parabox.Debug;
#endif

public class pb_Preferences_Internal
{
	public static Material DefaultDiffuse
	{
		get
		{
			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Material mat = go.GetComponent<MeshRenderer>().sharedMaterial;
			GameObject.DestroyImmediate(go);
			return mat;
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
			pref == pb_Constant.pbMeshesAreAssets)
			return false;
		else
			return true;
	}

	public static float GetFloat(string pref) { return GetFloat(pref, false); }
	public static float GetFloat(string pref, bool forceDefault)
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

	public static Color GetColor(string pref) { return GetColor(pref, false); }
	public static Color GetColor(string pref, bool forceDefault)
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

	public static IEnumerable<pb_Shortcut> GetShortcuts()
	{
		return EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts) ?
			pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts))
			:
			pb_Shortcut.DefaultShortcuts();													// Key not found, return the default
	}


	public static Material GetMaterial(string pref, bool forceDefault = false)
	{
		Material mat = null;

		switch(pref)
		{
			case pb_Constant.pbDefaultMaterial:
				if(!forceDefault && EditorPrefs.HasKey(pref))
				{
					if(EditorPrefs.GetString(pref) == "Default-Diffuse")
						return pb_Preferences_Internal.DefaultDiffuse;

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
	 * Checks for pref key in EditorPrefs, and return stored value or the default.
	 */
	public static T GetEnum<T>(string pref) { return GetEnum<T>(pref, false); }
	public static T GetEnum<T>(string pref, bool forceDefault)
	{
		int key = 0;

		switch(pref)
		{
			case pb_Constant.pbDefaultEditLevel:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return (T)System.Convert.ChangeType( (EditLevel)key, typeof(T));

			case pb_Constant.pbDefaultSelectionMode:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return (T)System.Convert.ChangeType( (SelectMode)key, typeof(T));

			case pb_Constant.pbHandleAlignment:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : 0;
				return (T)System.Convert.ChangeType( (HandleAlignment)key , typeof(T));

			case pb_Constant.pbDefaultCollider:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int)ColliderType.MeshCollider;
				return (T)System.Convert.ChangeType( (ColliderType)key, typeof(T));

			case pb_Constant.pbVertexColorTool:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int)VertexColorTool.Painter;
				return (T)System.Convert.ChangeType( (VertexColorTool) key, typeof(T));

			case pb_Constant.pbToolbarLocation:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int)SceneToolbarLocation.UpperCenter;
				return (T)System.Convert.ChangeType( (SceneToolbarLocation) key, typeof(T));

			case pb_Constant.pbDefaultEntity:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int)EntityType.Detail;
				return (T)System.Convert.ChangeType( (EntityType) key, typeof(T));

			case pb_Constant.pbDragSelectMode:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int)DragSelectMode.Difference;
				return (T)System.Convert.ChangeType( (DragSelectMode) key, typeof(T));

#if !UNITY_4_7
			case pb_Constant.pbShadowCastingMode:
				key = !forceDefault && EditorPrefs.HasKey(pref) ? EditorPrefs.GetInt(pref) : (int) ShadowCastingMode.TwoSided;
				return (T)System.Convert.ChangeType( (ShadowCastingMode) key, typeof(T));
#endif

			default:
				return (T)System.Convert.ChangeType( 0, typeof(T));
		}
	}

	/**
	 * Returns all preferences as a hashtable where key matches the pb_Constant name.
	 */
	public static Hashtable ToHashtable()
	{
		Hashtable table = new Hashtable();

		table.Add(pb_Constant.pbStripProBuilderOnBuild,			pb_Preferences_Internal.GetBool(pb_Constant.pbStripProBuilderOnBuild));
		table.Add(pb_Constant.pbDisableAutoUV2Generation,		pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation));
		table.Add(pb_Constant.pbShowSceneInfo,					pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneInfo));
		table.Add(pb_Constant.pbEnableBackfaceSelection,		pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection));
		table.Add(pb_Constant.pbDefaultOpenInDockableWindow,	pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow));
		table.Add(pb_Constant.pbDragCheckLimit,					pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit));
		table.Add(pb_Constant.pbForceConvex,					pb_Preferences_Internal.GetBool(pb_Constant.pbForceConvex));
		table.Add(pb_Constant.pbForceGridPivot,					pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot));
		table.Add(pb_Constant.pbForceVertexPivot,				pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot));
		table.Add(pb_Constant.pbManifoldEdgeExtrusion,			pb_Preferences_Internal.GetBool(pb_Constant.pbManifoldEdgeExtrusion));
		table.Add(pb_Constant.pbPerimeterEdgeBridgeOnly,		pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly));
		table.Add(pb_Constant.pbPBOSelectionOnly,				pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly));
		table.Add(pb_Constant.pbCloseShapeWindow,				pb_Preferences_Internal.GetBool(pb_Constant.pbCloseShapeWindow));
		table.Add(pb_Constant.pbUVEditorFloating,				pb_Preferences_Internal.GetBool(pb_Constant.pbUVEditorFloating));
		table.Add(pb_Constant.pbShowSceneToolbar,				pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar));
		table.Add(pb_Constant.pbShowEditorNotifications,		pb_Preferences_Internal.GetBool(pb_Constant.pbShowEditorNotifications));
		table.Add(pb_Constant.pbDefaultFaceColor,				pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultFaceColor ));
		table.Add(pb_Constant.pbDefaultEdgeColor,				pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultEdgeColor ));
		table.Add(pb_Constant.pbDefaultSelectedVertexColor,		pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultSelectedVertexColor ));
		table.Add(pb_Constant.pbDefaultVertexColor,				pb_Preferences_Internal.GetColor(pb_Constant.pbDefaultVertexColor ));
		table.Add(pb_Constant.pbUVGridSnapValue,				pb_Preferences_Internal.GetFloat(pb_Constant.pbUVGridSnapValue));
		table.Add(pb_Constant.pbVertexHandleSize,				pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize));
		table.Add(pb_Constant.pbDefaultMaterial,				pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial));

		table.Add(pb_Constant.pbDefaultSelectionMode,			pb_Preferences_Internal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode));
		table.Add(pb_Constant.pbDefaultCollider,			 	pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider));
		table.Add(pb_Constant.pbToolbarLocation,			 	pb_Preferences_Internal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation));

		return table;
	}
}
