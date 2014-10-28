using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

/**
 * When building the project, remove all references to pb_Objects.
 */
public class pb_ScenePostProcessor
{
	[PostProcessScene]
	public static void OnPostprocessScene()
	{ 
		if(EditorApplication.isPlayingOrWillChangePlaymode)
			return;

		foreach(pb_Object pb in GameObject.FindObjectsOfType(typeof(pb_Object)))
		{
			pb.ToMesh(true);

			pb_Entity entity = pb.gameObject.GetComponent<pb_Entity>();
			GameObject.DestroyImmediate(pb);
			if(entity != null)
				GameObject.DestroyImmediate(entity);
		}
	}
}