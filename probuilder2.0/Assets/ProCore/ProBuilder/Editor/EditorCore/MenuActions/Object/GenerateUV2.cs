using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class GenerateUV2 : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Object_GenerateUV2", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }
		public override bool hasFileMenuEntry { get { return false; } }

		private Editor uv2Editor = null;

		private static bool generateUV2PerObject
		{
			get
			{
				return pb_PreferencesInternal.GetBool("pbGenerateUV2PerObject", false);
			}
			set
			{
				pb_PreferencesInternal.SetBool("pbGenerateUV2PerObject", value);
			}
		}

		private static bool disableAutoUV2Generation
		{
			get
			{
				return pb_PreferencesInternal.GetBool(pb_Constant.pbDisableAutoUV2Generation);
			}
			set
			{
				pb_PreferencesInternal.SetBool(pb_Constant.pbDisableAutoUV2Generation, value);
			}
		}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Generate UV2",
			@"Create UV2 maps for all selected objects.\n\nCan optionally be set to Generate UV2 for the entire scene in the options panel."
		);

		public override bool IsEnabled()
		{
			if(generateUV2PerObject)
				return selection != null && selection.Length > 0;

			return true;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Generate UV2 Options", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Generate Scene UV2s will rebuild all ProBuilder mesh UV2s when invoked, instead of just the selection.", MessageType.Info);
			bool perSceneUV2s = !generateUV2PerObject;
			perSceneUV2s = EditorGUILayout.Toggle("Generate Scene UV2s", perSceneUV2s);
			generateUV2PerObject = !perSceneUV2s;

			EditorGUI.BeginChangeCheck();
			bool enableAutoUV2 = !disableAutoUV2Generation;
			enableAutoUV2 = EditorGUILayout.Toggle("Enable Auto UV2", enableAutoUV2);
			if(EditorGUI.EndChangeCheck())
				disableAutoUV2Generation = !enableAutoUV2;

			pb_EditorUtility.CreateCachedEditor<pb_UnwrapParametersEditor>(selection, ref uv2Editor);

			if(uv2Editor != null)
			{
				GUILayout.Space(4);
				uv2Editor.OnInspectorGUI();
			}

			GUILayout.FlexibleSpace();

			if(GUILayout.Button( generateUV2PerObject ? "Rebuild Selected UV2s" : "Rebuild Scene UV2s"))
				pb_EditorUtility.ShowNotification( DoAction().notification);
		}

		public override pb_ActionResult DoAction()
		{
			pb_Object[] selected = generateUV2PerObject ? selection : GameObject.FindObjectsOfType<pb_Object>();
			return DoGenerateUV2(selected);
		}

		private static pb_ActionResult DoGenerateUV2(pb_Object[] selected)
		{
			if(selected == null || selected.Length < 1)
				return pb_ActionResult.NoSelection;

			for(int i = 0; i < selected.Length; i++)
			{
				if(selected.Length > 3)
				{
					if( EditorUtility.DisplayCancelableProgressBar(
						"Generating UV2 Channel",
						"pb_Object: " + selected[i].name + ".",
						(((float)i+1) / selected.Length)))
					{
						EditorUtility.ClearProgressBar();
						Debug.LogWarning("User canceled UV2 generation.  " + (selected.Length-i) + " pb_Objects left without lightmap UVs.");
						return pb_ActionResult.UserCanceled;
					}
				}

				// True parameter forcibly generates UV2.  Otherwise if pbDisableAutoUV2Generation is true then UV2 wouldn't be built.
				selected[i].Optimize(true);
			}

			EditorUtility.ClearProgressBar();

			int l = selected.Length;
			return new pb_ActionResult(Status.Success, "Generate UV2\n" + (l > 1 ? string.Format("for {0} objects", l) : string.Format("for {0} object", l)) );
		}
	}
}
