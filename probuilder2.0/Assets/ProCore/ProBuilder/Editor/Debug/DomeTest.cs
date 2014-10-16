using UnityEditor;
using UnityEngine;
using System.Collections;
using ProBuilder2.Common;

public class DomeTest : EditorWindow
{
	[MenuItem("Tools/ProBuilder/Debug/Dome Test")]
	public static void MenuImtds()
	{
		EditorWindow.GetWindow<DomeTest>().Show();
	}

	GameObject go;

	int latitudeCuts = 6, longitudeCuts = 6;
	float latitudeDeg = 360f, longitudeDeg = 360f;
	void OnGUI()
	{
		if(go == null)
		{
			go = GameObject.Find("Dome");
			if(go == null)
			{
				go = new GameObject();
				go.name = "Dome";
				go.AddComponent<MeshFilter>();
				go.AddComponent<MeshRenderer>();
				go.AddComponent<VisualizeMesh>();				
			}
		}

		EditorGUI.BeginChangeCheck();

			latitudeCuts =  (int)EditorGUILayout.Slider("Latitude Cuts", latitudeCuts, 3, 24);
			longitudeCuts = (int)EditorGUILayout.Slider("Longitude Cuts", longitudeCuts, 3, 24);

			latitudeDeg =  (int)EditorGUILayout.Slider("Latitude Deg", latitudeDeg, 1f, 360f);
			longitudeDeg = (int)EditorGUILayout.Slider("Longitude Deg", longitudeDeg, 1f, 360f);

		if(EditorGUI.EndChangeCheck() || GUILayout.Button("refresh"))
			go.GetComponent<MeshFilter>().sharedMesh = pb_Shape_Generator.DomeGeneratorMesh(2f, latitudeCuts, longitudeCuts, latitudeDeg, longitudeDeg);

		if(GUILayout.Button("shared verts"))
		{
			pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces( 
				new Vector3[]
				{
					new Vector3(0f,0f,0f),
					new Vector3(1f,0f,0f),
					new Vector3(2f,0f,0f),
					new Vector3(0f,1f,0f),
					new Vector3(1f,1f,0f),
					new Vector3(2f,1f,0f)
				},
				new pb_Face[]
				{
					new pb_Face( new int[] { 0,1,3,1,4,3 } ),
					new pb_Face( new int[] { 1,2,4,2,5,4 } )
				}
			);
			
			pb_Editor_Utility.InitObjectFlags(pb, pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider), EntityType.Detail);
		}
	}
}
