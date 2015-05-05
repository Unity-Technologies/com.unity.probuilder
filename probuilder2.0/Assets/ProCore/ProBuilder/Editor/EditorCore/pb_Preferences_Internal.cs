using UnityEngine;
using UnityEditor;
using System.Collections;
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
	public static bool GetBool(string pref) { return GetBool(pref, false); }
	public static bool GetBool(string pref, bool forceDefault)
	{

		switch(pref)
		{
			case pb_Constant.pbDefaultOpenInDockableWindow:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;
		
			case pb_Constant.pbShowEditorNotifications:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbDragCheckLimit:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbForceGridPivot:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbForceVertexPivot:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbForceConvex:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;

			case pb_Constant.pbManifoldEdgeExtrusion:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbPerimeterEdgeBridgeOnly:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbPBOSelectionOnly:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbCloseShapeWindow:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbUVEditorFloating:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;
			
			case pb_Constant.pbGrowSelectionUsingAngle:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbUVMaterialPreview:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;
			
			case pb_Constant.pbShowSceneToolbar:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;
			
			case pb_Constant.pbShowUVEditorTooltip:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;
			
			case pb_Constant.pbShowDetail:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbShowOccluder:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbShowMover:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbShowCollider:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbShowTrigger:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbShowNoDraw:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			case pb_Constant.pbNormalizeUVsOnPlanarProjection:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
				
			case pb_Constant.pbStripProBuilderOnBuild:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;
				
			case pb_Constant.pbDisableAutoUV2Generation:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
				
			case pb_Constant.pbShowSceneInfo:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbEnableBackfaceSelection:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbVertexPaletteDockable:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbGrowSelectionAngleIterative:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : false;
			
			case pb_Constant.pbExtrudeAsGroup:
				return EditorPrefs.HasKey(pref) && !forceDefault ? EditorPrefs.GetBool(pref) : true;

			// When in doubt, say yes!
			default:
				return true;

		}
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
					col = Color.blue;
				break;
			
			case pb_Constant.pbDefaultSelectedVertexColor:
					col = Color.green;
				break;
		}

		return col;
	}

	public static pb_Shortcut[] GetShortcuts()
	{
		return EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts) ?
			pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts))
			:
			pb_Shortcut.DefaultShortcuts();													// Key not found, return the default
	}


	public static Material GetMaterial(string pref) { return GetMaterial(pref, false); }
	public static Material GetMaterial(string pref, bool forceDefault)
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

		if(!mat) mat = pb_Constant.DefaultMaterial;
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
			
			default:
				return (T)System.Convert.ChangeType( 0, typeof(T));
		}
	}
}
