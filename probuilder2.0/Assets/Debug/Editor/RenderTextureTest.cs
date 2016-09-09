using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System.Reflection;

using Parabox.Debug;

public class RenderTextureTest : EditorWindow
{
	[MenuItem("Tools/Render Scene to Texture")]
	static void MenuInit()
	{
		SceneView scn = SceneView.lastActiveSceneView;
		Camera sceneCamera = scn.camera;
		int width = (int) scn.position.width;
		int height = (int) scn.position.height;

		Shader shader = (Shader) AssetDatabase.LoadAssetAtPath("Assets/ProCore/ProBuilder/Shader/SelectionPass.shader", typeof(Shader));

		GameObject go = new GameObject();
		Camera renderCam = go.AddComponent<Camera>();
		renderCam.CopyFrom(sceneCamera);
		renderCam.enabled = false;
		renderCam.clearFlags = CameraClearFlags.SolidColor;
		renderCam.backgroundColor = Color.black;

		RenderTexture rt = RenderTexture.GetTemporary(width, height, 16);
		renderCam.targetTexture = rt;
		renderCam.RenderWithShader(shader, string.Empty);

		RenderTexture prev = RenderTexture.active;
		RenderTexture.active = rt;

		Texture2D img = new Texture2D(width, height);

		img.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		img.Apply();

		RenderTexture.active = prev;
		RenderTexture.ReleaseTemporary(rt);

		pb_EditorUtility.SaveTexture(img, "Assets/test.png");

		GameObject.DestroyImmediate(img);
		GameObject.DestroyImmediate(go);
	}
}
