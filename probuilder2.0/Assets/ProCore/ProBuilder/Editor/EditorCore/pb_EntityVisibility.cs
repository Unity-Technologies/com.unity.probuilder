using ProBuilder.Core;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Responsible for managing the visibility of entity types in the scene.
	/// </summary>
	[InitializeOnLoad]
	static class pb_EntityVisibility
	{
		private static bool show_Detail {
			get { return pb_PreferencesInternal.GetBool(pb_Constant.pbShowDetail); }
			set { pb_PreferencesInternal.SetBool(pb_Constant.pbShowDetail, value); }
		}

		private static bool show_Mover {
			get { return pb_PreferencesInternal.GetBool(pb_Constant.pbShowMover); }
			set { pb_PreferencesInternal.SetBool(pb_Constant.pbShowMover, value); }
		}

		private static bool show_Collider {
			get { return pb_PreferencesInternal.GetBool(pb_Constant.pbShowCollider); }
			set { pb_PreferencesInternal.SetBool(pb_Constant.pbShowCollider, value); }
		}

		private static bool show_Trigger {
			get { return pb_PreferencesInternal.GetBool(pb_Constant.pbShowTrigger); }
			set { pb_PreferencesInternal.SetBool(pb_Constant.pbShowTrigger, value); }
		}

		static pb_EntityVisibility()
		{
#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged += (x) => { OnPlayModeStateChanged(); };
#else
			EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
#endif
		}

		/// <summary>
		/// Set the visibility of an entity type in the sceneview.
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="isVisible"></param>
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

			foreach(var entity in Object.FindObjectsOfType<pb_Entity>())
			{
				if(entity.entityType == entityType)
				{
					MeshRenderer mr = entity.GetComponent<MeshRenderer>();
					if(mr != null) mr.enabled = isVisible;
				}
			}
		}

		/// <summary>
		/// Registered to EditorApplication.onPlaymodeStateChanged
		/// </summary>
		static void OnPlayModeStateChanged()
		{
			bool isPlaying = EditorApplication.isPlaying;
			bool orWillPlay = EditorApplication.isPlayingOrWillChangePlaymode;

			// if these two don't match, that means it's the call prior to actually engaging
			// whatever state. when entering play mode it doesn't make a difference, but on
			// exiting it's the difference between a scene reload and the reloaded scene.
			if(isPlaying != orWillPlay)
				return;

			bool isEntering = isPlaying && orWillPlay;

			foreach (var entityBehaviour in Resources.FindObjectsOfTypeAll<pb_EntityBehaviour>())
			{
				if (entityBehaviour.manageVisibility)
				{
					// skip OnExit because OnEnter is operating on an instanced new scene, no changes will affect the
					// actual scene
					if(isEntering)
						entityBehaviour.OnEnterPlayMode();
//					else
//						entityBehaviour.OnExitPlayMode();
				}
			}

			if(!isEntering)
				return;

			// deprecated pb_Entity path
			bool detailEnabled	 = show_Detail;
			bool moverEnabled	 = show_Mover;

			foreach(var entity in Resources.FindObjectsOfTypeAll<pb_Entity>())
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
