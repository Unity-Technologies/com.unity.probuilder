#if UNITY_5_0_0
#define UNITY_5
#endif

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using System.Collections;

public class pb_SceneExplorer : EditorWindow
{
#region Initialization

	[MenuItem("Tools/ProBuilder/Debug/Scene Explorer")]
	public static void MenuInitSceneExplorer()
	{
		EditorWindow.GetWindow<pb_SceneExplorer>(true, "Scene Explorer", true).Show();
	}

	void OnEnable()
	{
		OnHierarchyChange();
	}
#endregion

#region Meat

	Object[] all 			= new Object[0] {};
	Object[] textures 		= new Texture2D[0] {};
	Object[] audioclips 	= new AudioClip[0] {};
	Object[] meshes 		= new Mesh[0] {};
	Object[] materials 		= new Material[0] {};
	Object[] gameobjects 	= new GameObject[0] {};
	Object[] components		= new Component[0] {};

	Vector2 scroll = Vector2.zero;

	bool[] show = new bool[6] { false, false, false, false, false, false };

	void OnGUI()
	{
		if(EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
			return;

		if(GUILayout.Button("Clean Unused Assets"))
		{
			#if UNITY_5
			EditorUtility.UnloadUnusedAssetsImmediate();
			#else
			EditorUtility.UnloadUnusedAssets();
			// EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
			#endif
		}

		// if(GUILayout.Button("Disable Editor Graphics"))
		// 	pb_Editor_Graphics.OnDisable();

		scroll = GUILayout.BeginScrollView(scroll);

		Object[] obj = System.Array.FindAll(gameobjects, x => x != null && x.name.Contains("ProBuilder"));
		GUILayout.Label("ProBuilder Preview Objects: " + (obj == null ? 0 : obj.Length).ToString(), EditorStyles.boldLabel);

		GUILayout.Space(12);

		GUILayout.Label("All: " + all.Length, EditorStyles.boldLabel);
		int s = 0;

		show[s] = EditorGUILayout.Foldout(show[s++], "Texture2D: " + textures.Length);

		if(show[s-1])
		{
			for(int i = 0; i < textures.Length; i++)
				GUILayout.Label("\t" + textures[i] == null || textures[i].name.Length < 1 ? "null" : textures[i].name);
		}

		show[s] = EditorGUILayout.Foldout(show[s++], "AudioClip: " + audioclips.Length);

		if(show[s-1])
		{
			for(int i = 0; i < audioclips.Length; i++)
				GUILayout.Label("\t" + audioclips[i].ToString());
		}

		show[s] = EditorGUILayout.Foldout(show[s++], "Mesh: " + meshes.Length);

		if(show[s-1])
		{
			for(int i = 0; i < meshes.Length; i++)
				GUILayout.Label("\t" + meshes[i].ToString());
		}

		show[s] = EditorGUILayout.Foldout(show[s++], "Material: " + materials.Length);

		if(show[s-1])
		{
			for(int i = 0; i < materials.Length; i++)
				GUILayout.Label("\t" + materials[i].ToString());
		}

		show[s] = EditorGUILayout.Foldout(show[s++], "GameObject: " + gameobjects.Length);

		if(show[s-1])
		{
			for(int i = 0; i < gameobjects.Length; i++)
				GUILayout.Label("\t" + gameobjects[i].ToString());
		}

		show[s] = EditorGUILayout.Foldout(show[s++], "Component: " + components.Length);

		if(show[s-1])
		{
			for(int i = 0; i < components.Length; i++)
				GUILayout.Label("\t" + components[i].ToString());
		}


		GUILayout.EndScrollView();
	}

	void OnHierarchyChange()
	{
		all 		= Resources.FindObjectsOfTypeAll(typeof(Object));
		textures 	= Resources.FindObjectsOfTypeAll(typeof(Texture));
		audioclips 	= Resources.FindObjectsOfTypeAll(typeof(AudioClip));
		meshes 		= Resources.FindObjectsOfTypeAll(typeof(Mesh));
		materials 	= Resources.FindObjectsOfTypeAll(typeof(Material));
		gameobjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
		components 	= Resources.FindObjectsOfTypeAll(typeof(Component));

		Repaint();
	}
#endregion
}
