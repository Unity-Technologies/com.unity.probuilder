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
		private static bool show_Detail {
			get { return pb_Preferences_Internal.GetBool(pb_Constant.pbShowDetail); }
			set { EditorPrefs.SetBool(pb_Constant.pbShowDetail, value); }
		}

		private static bool show_Mover {
			get { return pb_Preferences_Internal.GetBool(pb_Constant.pbShowMover); }
			set { EditorPrefs.SetBool(pb_Constant.pbShowMover, value); }
		}

		private static bool show_Collider {
			get { return pb_Preferences_Internal.GetBool(pb_Constant.pbShowCollider); }
			set { EditorPrefs.SetBool(pb_Constant.pbShowCollider, value); }
		}

		private static bool show_Trigger {
			get { return pb_Preferences_Internal.GetBool(pb_Constant.pbShowTrigger); }
			set { EditorPrefs.SetBool(pb_Constant.pbShowTrigger, value); }
		}

		static pb_EntityVisibility()
		{
			EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
		}

		/**
		 *	Set the visibility of an entity type in the sceneview.
		 */
		public static void SetEntityVisibility(EntityType entityType, bool isVisible)
		{
			switch(entityType)
			{
				case EntityType.Detail:
					show_Detail = isVisible;
					break;
				case EntityType.Mover:
					show_Mover = isVisible;
					break;
				case EntityType.Collider:
					show_Collider = isVisible;
					break;
				case EntityType.Trigger:
					show_Trigger = isVisible;
					break;
			}

			foreach(pb_Entity entity in Object.FindObjectsOfType(typeof(pb_Entity)))
			{
				if(entity.entityType == entityType)
				{
					MeshRenderer mr = entity.GetComponent<MeshRenderer>();
					if(mr != null) mr.enabled = isVisible;
				}
			}
		}

		/**
		 * Registered to EditorApplication.onPlaymodeStateChanged
		 */
		private static void OnPlayModeStateChanged()
		{
			bool isPlaying = EditorApplication.isPlaying;
			bool orWillPlay = EditorApplication.isPlayingOrWillChangePlaymode;

			// if these two don't match, that means it's the call prior to actually engaging
			// whatever state.  when entering play mode it doesn't make a difference, but on
			// exiting it's the difference between a scene reload and the reloaded scene.
			if(isPlaying != orWillPlay)
				return;

			bool isEntering = isPlaying && orWillPlay;

			if(!isEntering)
				return;

			bool detailEnabled	 = show_Detail;
			bool moverEnabled	 = show_Mover;

			foreach(pb_Entity entity in Resources.FindObjectsOfTypeAll(typeof(pb_Entity)))
			{
				MeshRenderer mr = entity.gameObject.GetComponent<MeshRenderer>();

				if(mr == null)
					continue;

				switch(entity.entityType)
				{
					case EntityType.Detail:
						if(!detailEnabled)
							mr.enabled = true;
						break;

					case EntityType.Mover:
						if(!moverEnabled)
							mr.enabled = true;
						break;
				}
			}
		}
	}
}
