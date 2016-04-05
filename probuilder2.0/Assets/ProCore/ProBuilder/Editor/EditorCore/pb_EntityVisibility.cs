using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Static class responsible for managing the visibility of entity types in the scene.
	 */
	[InitializeOnLoad]
	public static class pb_EntityVisibility
	{
		private static bool show_Detail 	= true;
		// private static bool show_Occluder 	= true;
		private static bool show_Mover 		= true;
		private static bool show_Collider 	= true;
		private static bool show_Trigger 	= true;
		// private static bool show_NoDraw 	= true;

		static pb_EntityVisibility()
		{
			show_Detail		= pb_Preferences_Internal.GetBool(pb_Constant.pbShowDetail);
			// show_Occluder	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowOccluder);
			show_Mover		= pb_Preferences_Internal.GetBool(pb_Constant.pbShowMover);
			show_Collider	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowCollider);
			show_Trigger	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowTrigger);
			// show_NoDraw		= pb_Preferences_Internal.GetBool(pb_Constant.pbShowNoDraw);

			EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
		}

		/**
		 *	Set the visibility of an entity type in the sceneview.
		 */
		public static void SetEntityVisibility(EntityType entityType, bool isVisible)
		{
			foreach(pb_Entity sel in Object.FindObjectsOfType(typeof(pb_Entity)))
			{
				if(sel.entityType == entityType) {
					sel.GetComponent<MeshRenderer>().enabled = isVisible;
					if(sel.GetComponent<MeshCollider>())
						sel.GetComponent<MeshCollider>().enabled = isVisible;
				}
			}
		}

		/**
		 * Registered to EditorApplication.onPlaymodeStateChanged
		 */
		private static void OnPlayModeStateChanged()
		{
			if(EditorApplication.isPlaying)
			{
				foreach(pb_Entity entity in Resources.FindObjectsOfTypeAll<pb_Entity>())
				{
					switch(entity.entityType)
					{
						case EntityType.Occluder:
						case EntityType.Detail:
							if(!show_Detail)
							{
								entity.transform.GetComponent<MeshRenderer>().enabled = true;
								entity.transform.GetComponent<Collider>().enabled = true;
							}
							break;

						case EntityType.Mover:
							if(!show_Mover)
							{
								entity.transform.GetComponent<MeshRenderer>().enabled = true;
								entity.transform.GetComponent<Collider>().enabled = true;
							}
							break;

						case EntityType.Collider:
							if(!show_Collider)
							{
								entity.transform.GetComponent<Collider>().enabled = true;
							}
							break;

						case EntityType.Trigger:
							if(!show_Trigger)
							{
								entity.transform.GetComponent<Collider>().enabled = true;
							}
							break;
					}
				}

			}
			else
			{
				// Turn stuff back off that's not supposed to be on
				foreach(pb_Entity entity in Resources.FindObjectsOfTypeAll<pb_Entity>())
				{
					switch(entity.entityType)
					{
						case EntityType.Occluder:
						case EntityType.Detail:
							if(!show_Detail)
							{
								entity.transform.GetComponent<MeshRenderer>().enabled = false;
								entity.transform.GetComponent<Collider>().enabled = false;
							}
							break;

						case EntityType.Mover:
							if(!show_Mover)
							{
								entity.transform.GetComponent<MeshRenderer>().enabled = false;
								entity.transform.GetComponent<Collider>().enabled = false;
							}
							break;

						case EntityType.Collider:
							if(!show_Collider)
							{
								entity.transform.GetComponent<MeshRenderer>().enabled = false;
								entity.transform.GetComponent<Collider>().enabled = false;
							}
							break;

						case EntityType.Trigger:
							if(!show_Trigger)
							{
								entity.transform.GetComponent<MeshRenderer>().enabled = false;
								entity.transform.GetComponent<Collider>().enabled = false;
							}
							break;
					}
				}
			}
		}
	}
}
