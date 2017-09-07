#define FBX_EXPORTER_ENABLED

#if FBX_EXPORTER_ENABLED

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace ProBuilder2.Actions
{
	/**
	 * This class is responsible for loading the pb_MenuAction into the toolbar and menu.
	 */
	[InitializeOnLoad]
	static class RegisterCreatePrefabAction
	{
		/**
		 * Static initializer is called when Unity loads the assembly.
		 */
		static RegisterCreatePrefabAction()
		{
			if(pb_FbxListener.FbxEnabled)
				pb_EditorToolbarLoader.RegisterMenuItem(InitCustomAction);
		}

		/**
		 * Helper function to load a new menu action object.
		 */
		static pb_MenuAction InitCustomAction()
		{
			return new CreateFbxPrefab();
		}

		/**
		 * Usually you'll want to add a menu item entry for your action.
		 * https://docs.unity3d.com/ScriptReference/MenuItem.html
		 */
		[MenuItem("Tools/ProBuilder/Export/Export FBX Prefab", true)]
		static bool MenuVerifyDoSomethingWithPbObject()
		{
			// Using pb_EditorToolbarLoader.GetInstance keeps MakeFacesDoubleSided as a singleton.
			CreateFbxPrefab instance = pb_EditorToolbarLoader.GetInstance<CreateFbxPrefab>();
			return instance != null && instance.IsEnabled();
		}

		[MenuItem("Tools/ProBuilder/Export/Export FBX Prefab", false, pb_Constant.MENU_EXPORT)]
		static void MenuDoDoSomethingWithPbObject()
		{
			CreateFbxPrefab instance = pb_EditorToolbarLoader.GetInstance<CreateFbxPrefab>();

			if(instance != null)
				pb_EditorUtility.ShowNotification(instance.DoAction().notification);
		}
	}

	/**
	 * This is the actual action that will be executed.
	 */
	public class CreateFbxPrefab : pb_MenuAction
	{
		// Put this action in the Object category for better visibility. It's supposed to belong to "Export", but that's all the way
		// at the bottom of the toolbar (and not colored).
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }

		private bool m_FbxQuads = true;
		private bool m_KeepOriginal = false;

		/**
		 * What to show in the hover tooltip window.  pb_TooltipContent is similar to GUIContent, with the exception that it also
		 * includes an optional params[] char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
		 */
		static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Convert to Prefab",
			"Create a new prefab with FBX asset from the selection.",
			""	// Some combination of build settings can cause the compiler to not respection optional params in the pb_TooltipContent c'tor?
		);

		public CreateFbxPrefab()
		{
			m_FbxQuads = pb_PreferencesInternal.GetBool("Export::m_FbxQuads", true);
			m_KeepOriginal = pb_PreferencesInternal.GetBool("Export::m_ConvertToPrefabKeepsOriginal", false);
		}

		/**
		 * Determines if the action should be enabled or grayed out.
		 */
		public override bool IsEnabled()
		{
			// `selection` is a helper property on pb_MenuAction that returns a pb_Object[] array from the current selection.
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0;
		}

		/**
		 * Determines if the action should be loaded in the menu (ex, face actions shouldn't be shown when in vertex editing mode).
		 */
		public override bool IsHidden()
		{
			return pb_Editor.instance == null;
		}

		/**
		 * AltState determines wether or not the options button will be shown, and if shown, as enabled or disabled.
		 */
		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Create FBX Prefab Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			m_FbxQuads = EditorGUILayout.Toggle("Export Quads", m_FbxQuads);
			m_KeepOriginal = EditorGUILayout.Toggle("Keep Original", m_KeepOriginal);

			if(EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetBool("Export::m_ConvertToPrefabKeepsOriginal", m_KeepOriginal);
				pb_PreferencesInternal.SetBool("Export::m_FbxQuads", m_FbxQuads);
			}

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Create FBX Prefab"))
			{
				DoAction();
				SceneView.RepaintAll();
				pb_MenuOption.CloseAll();
			}
		}

		/**
		 * Do the thing. Return a pb_ActionResult indicating the success/failure of action.
		 */
		public override pb_ActionResult DoAction()
		{
			GameObject[] unityGameObjectsToConvert = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.TopLevel) as GameObject[];

			if(unityGameObjectsToConvert.Length < 1)
				return pb_ActionResult.NoSelection;
			
			// Delay call because otherwise OnGUI gets confused and throws a Stack.Pop exception
			EditorApplication.delayCall += () =>
			{
				Type modelExporterType = pb_Reflection.GetType("FbxExporters.Editor.ConvertToModel");

				UnityEngine.GameObject[] res = null;

				if(modelExporterType != null)
				{
					MethodInfo createPrefabMethod = modelExporterType.GetMethod("CreateInstantiatedModelPrefab");
					
					if(createPrefabMethod != null)
					{
						foreach(pb_Object pb in Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.Deep) as pb_Object[])
						{
							pb.ToMesh(m_FbxQuads ? MeshTopology.Quads : MeshTopology.Triangles);
							// don't refresh collisions because it throws errors when quads are enabled
							pb.Refresh(RefreshMask.UV | RefreshMask.Colors | RefreshMask.Normals | RefreshMask.Tangents);						
							// ...and also clear existing collisions (to be set again after export)
							MeshCollider mc = pb.transform.GetComponent<MeshCollider>();
							if(mc != null) mc.sharedMesh = null;
						}

						// would be nice to pass Type.Missing for the default parameters, but that throws an error for unknown reasons
						res = createPrefabMethod.Invoke(null, new object[] { unityGameObjectsToConvert, null, m_KeepOriginal }) as UnityEngine.GameObject[];

						if(res != null)
							Selection.objects = res;

						foreach(pb_Object pb in Selection.GetFiltered(typeof(pb_Object), SelectionMode.Editable | SelectionMode.Deep) as pb_Object[])
						{
							pb.ToMesh(MeshTopology.Triangles);
							pb.Refresh();
							pb.Optimize();
						}
					}
					else
					{
						pb_Log.Warning("FbxExporters.Editor.ConvertToModel.CreateInstantiatedModelPrefab method not found.");
					}
				}
				else
				{
					pb_Log.Warning("FbxExporters.Editor.ConvertToModel class not found.");
				}
			};

			return new pb_ActionResult(Status.Success, "Create FBX Prefab");
			// return new pb_ActionResult(Status.Failure, "Create FBX Prefab");
		}
	}
}
#endif

