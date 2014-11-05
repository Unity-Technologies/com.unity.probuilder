using UnityEngine;
using System.Collections;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.GUI;

public class ShowEditorPrefKeys : EditorWindow
{
	[MenuItem("Tools/ProBuilder/Debug/Show Editor Prefs")]
	public static void Init()
	{
		EditorWindow.GetWindow<ShowEditorPrefKeys>().ShowUtility();
	}

	Color SEPARATOR_COLOR = new Color(.6f, .6f, .6f, .3f);

	string[] enums = new string[]
	{
		pb_Constant.pbDefaultEditLevel,
		pb_Constant.pbDefaultSelectionMode,
		pb_Constant.pbHandleAlignment
	};

	string[] colors = new string[]
	{
		pb_Constant.pbDefaultFaceColor,
		pb_Constant.pbDefaultSelectedVertexColor,
		pb_Constant.pbDefaultVertexColor
	};

	string[] booleans = new string[]
	{
		pb_Constant.pbDefaultOpenInDockableWindow,
		pb_Constant.pbEditorPrefVersion,
		pb_Constant.pbDefaultCollider,
		pb_Constant.pbForceConvex,
		pb_Constant.pbVertexColorPrefs,
		pb_Constant.pbShowEditorNotifications,
		pb_Constant.pbDragCheckLimit,
		pb_Constant.pbForceVertexPivot,
		pb_Constant.pbForceGridPivot,
		pb_Constant.pbManifoldEdgeExtrusion,
		pb_Constant.pbPerimeterEdgeBridgeOnly,
		pb_Constant.pbPBOSelectionOnly,
		pb_Constant.pbCloseShapeWindow,
		pb_Constant.pbHideWireframe,
		pb_Constant.pbUVEditorFloating,
		pb_Constant.pbShowDetail,
		pb_Constant.pbShowOccluder,
		pb_Constant.pbShowMover,
		pb_Constant.pbShowCollider,
		pb_Constant.pbShowTrigger,
		pb_Constant.pbShowNoDraw,
		pb_Constant.pbNormalizeUVsOnPlanarProjection
	};

	string[] floats = new string[] 
	{
		pb_Constant.pbVertexHandleSize,
		pb_Constant.pbExtrudeDistance,
		pb_Constant.pbUVGridSnapValue
	};

	void OnGUI()
	{
		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical();
			GUILayout.Label("Preference", EditorStyles.boldLabel);
			GUI.backgroundColor = SEPARATOR_COLOR;
			pb_GUI_Utility.DrawSeparator(1);
			GUILayout.Space(2);

			foreach(string e in enums)
				GUILayout.Label(e);
			pb_GUI_Utility.DrawSeparator(1);
			
			
			foreach(string c in colors)
				GUILayout.Label(c);

			pb_GUI_Utility.DrawSeparator(1);

			foreach(string b in booleans)
				GUILayout.Label( b );

			pb_GUI_Utility.DrawSeparator(1);

			foreach(string f in floats)	
				GUILayout.Label( f );
			
			pb_GUI_Utility.DrawSeparator(1);

			GUILayout.Label("pbDefaultShortcuts");
			GUILayout.Label("pbDefaultMaterial");

			GUI.backgroundColor = Color.white;
		GUILayout.EndVertical();
	
		GUILayout.BeginVertical();

			GUILayout.Label("User Set?", EditorStyles.boldLabel);
			GUI.backgroundColor = SEPARATOR_COLOR;
			pb_GUI_Utility.DrawSeparator(1);
			GUILayout.Space(2);

			foreach(string e in enums)
				GUILayout.Label( EditorPrefs.HasKey(e).ToString() );

			pb_GUI_Utility.DrawSeparator(1);

			foreach(string c in colors)
				GUILayout.Label( EditorPrefs.HasKey(c).ToString() );

			pb_GUI_Utility.DrawSeparator(1);

			foreach(string b in booleans)
				GUILayout.Label( EditorPrefs.HasKey(b).ToString() );
	
			pb_GUI_Utility.DrawSeparator(1);

			foreach(string f in floats)
				GUILayout.Label( EditorPrefs.HasKey(f).ToString() );

			pb_GUI_Utility.DrawSeparator(1);

			GUILayout.Label( EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts).ToString() );
			GUILayout.Label( EditorPrefs.HasKey(pb_Constant.pbDefaultMaterial).ToString() );

			GUI.backgroundColor = Color.white;
		GUILayout.EndVertical();

		GUILayout.BeginVertical();
			GUILayout.Label("Value", EditorStyles.boldLabel);

			GUI.backgroundColor = SEPARATOR_COLOR;
			pb_GUI_Utility.DrawSeparator(1);
			GUILayout.Space(2);

			GUILayout.Label( pb_Preferences_Internal.GetEnum<EditLevel>( pb_Constant.pbDefaultEditLevel).ToString() );
			GUILayout.Label( pb_Preferences_Internal.GetEnum<SelectMode>( pb_Constant.pbDefaultSelectionMode).ToString()		 );
			GUILayout.Label( pb_Preferences_Internal.GetEnum<HandleAlignment>( pb_Constant.pbHandleAlignment).ToString() );
			
			pb_GUI_Utility.DrawSeparator(1);
				
			foreach(string c in colors)
				GUILayout.Label( pb_Preferences_Internal.GetColor(c).ToString() );
	
			pb_GUI_Utility.DrawSeparator(1);

			foreach(string b in booleans)
				GUILayout.Label( pb_Preferences_Internal.GetBool(b).ToString() );

			pb_GUI_Utility.DrawSeparator(1);

			foreach(string f in floats)
				GUILayout.Label( pb_Preferences_Internal.GetFloat(f).ToString() );
			
			pb_GUI_Utility.DrawSeparator(1);

			GUILayout.Label("");
			GUILayout.Label( pb_Preferences_Internal.GetMaterial( pb_Constant.pbDefaultMaterial).name );

			GUI.backgroundColor = Color.white;
		GUILayout.EndVertical();


		GUILayout.BeginVertical();
			GUILayout.Label("Default", EditorStyles.boldLabel);

			GUI.backgroundColor = SEPARATOR_COLOR;
			pb_GUI_Utility.DrawSeparator(1);
			GUILayout.Space(2);

			GUILayout.Label( pb_Preferences_Internal.GetEnum<EditLevel>( pb_Constant.pbDefaultEditLevel, true).ToString() 		);
			GUILayout.Label( pb_Preferences_Internal.GetEnum<SelectMode>( pb_Constant.pbDefaultSelectionMode, true).ToString()	);
			GUILayout.Label( pb_Preferences_Internal.GetEnum<HandleAlignment>( pb_Constant.pbHandleAlignment, true).ToString() 	);
			
			pb_GUI_Utility.DrawSeparator(1);
			
			foreach(string c in colors)
				GUILayout.Label( pb_Preferences_Internal.GetColor(c, true).ToString()						 );

			pb_GUI_Utility.DrawSeparator(1);

			foreach(string b in booleans)
				GUILayout.Label( pb_Preferences_Internal.GetBool(b, true).ToString() );
	
			pb_GUI_Utility.DrawSeparator(1);

			foreach(string f in floats)
				GUILayout.Label( pb_Preferences_Internal.GetFloat(f, true).ToString()						 );
			
			pb_GUI_Utility.DrawSeparator(1);

			GUILayout.Label("");
			GUILayout.Label( pb_Preferences_Internal.GetMaterial( pb_Constant.pbDefaultMaterial, true).name 	  	 		 );

			GUI.backgroundColor = Color.white;
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
	}
}

