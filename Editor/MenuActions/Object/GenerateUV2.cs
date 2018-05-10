using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class GenerateUV2 : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Object_GenerateUV2", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		protected override bool hasFileMenuEntry
		{
			get { return false; }
		}

		private Editor uv2Editor = null;

		private static bool generateUV2PerObject
		{
			get { return PreferencesInternal.GetBool("pbGenerateUV2PerObject", false); }
			set { PreferencesInternal.SetBool("pbGenerateUV2PerObject", value); }
		}

		private static bool disableAutoUV2Generation
		{
			get { return PreferencesInternal.GetBool(PreferenceKeys.pbDisableAutoUV2Generation); }
			set { PreferencesInternal.SetBool(PreferenceKeys.pbDisableAutoUV2Generation, value); }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Generate UV2",
			@"Create UV2 maps for all selected objects.\n\nCan optionally be set to Generate UV2 for the entire scene in the options panel."
		);

		public override bool IsEnabled()
		{
			if (generateUV2PerObject)
				return MeshSelection.Top().Length > 0;

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
			if (EditorGUI.EndChangeCheck())
				disableAutoUV2Generation = !enableAutoUV2;

			EditorUtility.CreateCachedEditor<UnwrapParametersEditor>(MeshSelection.Top(), ref uv2Editor);

			if (uv2Editor != null)
			{
				GUILayout.Space(4);
				uv2Editor.OnInspectorGUI();
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button(generateUV2PerObject ? "Rebuild Selected UV2s" : "Rebuild Scene UV2s"))
				EditorUtility.ShowNotification(DoAction().notification);
		}

		public override ActionResult DoAction()
		{
			ProBuilderMesh[] selected = generateUV2PerObject ? MeshSelection.Top() : GameObject.FindObjectsOfType<ProBuilderMesh>();
			return DoGenerateUV2(selected);
		}

		private static ActionResult DoGenerateUV2(ProBuilderMesh[] selected)
		{
			if (selected == null || selected.Length < 1)
				return ActionResult.NoSelection;

			for (int i = 0; i < selected.Length; i++)
			{
				if (selected.Length > 3)
				{
					if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(
						"Generating UV2 Channel",
						"pb_Object: " + selected[i].name + ".",
						(((float)i + 1) / selected.Length)))
					{
						UnityEditor.EditorUtility.ClearProgressBar();
						Debug.LogWarning("User canceled UV2 generation.  " + (selected.Length - i) + " pb_Objects left without lightmap UVs.");
						return ActionResult.UserCanceled;
					}
				}

				// True parameter forcibly generates UV2.  Otherwise if pbDisableAutoUV2Generation is true then UV2 wouldn't be built.
				selected[i].Optimize(true);
			}

			UnityEditor.EditorUtility.ClearProgressBar();

			int l = selected.Length;
			return new ActionResult(ActionResult.Status.Success, "Generate UV2\n" + (l > 1 ? string.Format("for {0} objects", l) : string.Format("for {0} object", l)));
		}
	}
}
