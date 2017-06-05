using UnityEngine;
using UnityEditor;
 
public class TakeScreenshotInEditor : ScriptableObject
{

	[MenuItem ("Tools/Debug/Screenshot/Take Screenshot of Game View %^s")]
	static void TakeScreenshot()
	{
		// string path = EditorUtility.SaveFilePanel("Save GameView Screenshot",
		// 	"",
		// 	"Screenshot",
		// 	"png");

		// Application.CaptureScreenshot(path);
	}


	[MenuItem ("Tools/Debug/Screenshot/Take Screenshot of Scene View")]
	static void TakeSceneViewScreenshot()
	{
		Camera cam = (Camera)SceneView.lastActiveSceneView.camera;
		cam = Camera.current;
		
		if(cam == null)
		{
			Debug.Log("Cam is null");
			return;
		}

		string path = EditorUtility.SaveFilePanelInProject("Save SceneView Screenshot",
			"SceneView Screenshot",
			"png",
			"Enter screenshot name");

		Texture2D tex = new Texture2D ( (int)cam.pixelWidth, (int)cam.pixelHeight, TextureFormat.RGB24, false);
		// Read screen contents into the texture
		// TextureImporter ti = new TextureImporter(tex);
		// Debug.Log(ti.isReadable);
		tex.ReadPixels (new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);
		tex.Apply ();

		// Encode texture into PNG
		byte[] bytes = tex.EncodeToPNG();
		DestroyImmediate (tex);

		// For testing purposes, also write to a file in the project folder
		System.IO.File.WriteAllBytes(path, bytes);

		AssetDatabase.Refresh();
	}
}
