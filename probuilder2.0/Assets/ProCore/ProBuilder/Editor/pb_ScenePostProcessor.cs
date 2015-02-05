using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ProBuilder2.Common;

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
			// /**
			//  * Hide nodraw faces if present.  Don't call 'ToMesh' on objects that are statically batched since
			//  * batching runs pre-PostProcessScene and will break the combined mesh.
			//  */
			// if(pb.containsNodraw && !pb.GetComponent<MeshRenderer>().isPartOfStaticBatch)
			// {
			// 	pb.ToMesh(true);
			// 	pb.Refresh();
			// }

			GameObject go = pb.gameObject;

			pb_Entity entity = pb.gameObject.GetComponent<pb_Entity>();

			if(entity.entityType == EntityType.Collider || entity.entityType == EntityType.Trigger)	
				go.GetComponent<MeshRenderer>().enabled = false;

			if(!pb_Preferences_Internal.GetBool(pb_Constant.pbStripProBuilderOnBuild))
			   return;

			pb.dontDestroyMeshOnDelete = true;
			
			GameObject.DestroyImmediate( pb );
			GameObject.DestroyImmediate( go.GetComponent<pb_Entity>() );

		}
	}
}