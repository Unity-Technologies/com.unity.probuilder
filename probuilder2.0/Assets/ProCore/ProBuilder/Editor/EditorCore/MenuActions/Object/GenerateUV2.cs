using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class GenerateUV2 : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }
		public override bool hasMenuEntry { get { return false; } }

		private static bool generateUV2PerObject
		{
			get
			{
				return EditorPrefs.GetBool("pbGenerateUV2PerObject", true);
			}
			set
			{
				EditorPrefs.SetBool("pbGenerateUV2PerObject", value);
			}
		}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Generate UV2",
			@"Create UV2 maps for all selected objects.\n\nCan optionally be set to Generate UV2 for the entire scene in the options panel."
		);

		public override bool IsHidden()
		{
			return !pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation);
		}

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

			EditorGUILayout.HelpBox(
@"Generate Scene UV2s will rebuild all ProBuilder mesh UV2s when invoked, instead of just the selection.  Usually you'll want to leave this off.

You can use the button below to rebuild all scene UV2s quickly.", MessageType.Info);
			bool perSceneUV2s = !generateUV2PerObject;
			perSceneUV2s = EditorGUILayout.Toggle("Generate Scene UV2s", perSceneUV2s);
			generateUV2PerObject = !perSceneUV2s;

			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("Rebuild All ProBuilder UV2s"))
				pb_Editor_Utility.ShowNotification(DoGenerateUV2( GameObject.FindObjectsOfType<pb_Object>() ).notification);
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
				selected[i].GenerateUV2(true);
			}

			EditorUtility.ClearProgressBar();

			int l = selected.Length;
			return new pb_ActionResult(Status.Success, "Generate UV2\n" + (l > 1 ? string.Format("for {0} objects", l) : string.Format("for {0} object", l)) );
		}
	}
}
