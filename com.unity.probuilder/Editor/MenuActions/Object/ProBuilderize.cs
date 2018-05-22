using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ProBuilderize : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Object_ProBuilderize", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		GUIContent m_QuadsTooltip = new GUIContent("Import Quads", "Create ProBuilder mesh using quads where " +
			"possible instead of triangles.");
		GUIContent m_SmoothingTooltip = new GUIContent("Import Smoothing", "Import smoothing groups by " +
			"testing adjacent faces against an angle thresold.");
		GUIContent m_SmoothingThresholdTooltip = new GUIContent("Smoothing Threshold", "When importing " +
			"smoothing groups any adjacent faces with an adjoining angle difference of less than this value will be " +
			"grouped together in a smoothing group.");

		private static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"ProBuilderize",
			@"Creates ProBuilder-modifiable objects from meshes."
		);

		public override bool IsEnabled()
		{
			int meshCount = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Count();

			return meshCount > 0 &&
				meshCount != MeshSelection.Top().Length;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("ProBuilderize Options", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("When Preserve Faces is enabled ProBuilder will try to group adjacent triangles into faces.", MessageType.Info);

			bool quads = PreferencesInternal.GetBool("pb_MeshImporter::quads", true);
			bool smoothing = PreferencesInternal.GetBool("pb_MeshImporter::smoothing", true);
			float smoothingThreshold = PreferencesInternal.GetFloat("pb_MeshImporter::smoothingThreshold", 1f);

			EditorGUI.BeginChangeCheck();

			quads = EditorGUILayout.Toggle(m_QuadsTooltip, quads);
			smoothing = EditorGUILayout.Toggle(m_SmoothingTooltip, smoothing);
			GUI.enabled = smoothing;
			EditorGUILayout.PrefixLabel(m_SmoothingThresholdTooltip);
			smoothingThreshold = EditorGUILayout.Slider(smoothingThreshold, 0.0001f, 45f);
			GUI.enabled = true;

			if (EditorGUI.EndChangeCheck())
			{
				PreferencesInternal.SetBool("pb_MeshImporter::quads", quads);
				PreferencesInternal.SetBool("pb_MeshImporter::smoothing", smoothing);
				PreferencesInternal.SetFloat("pb_MeshImporter::smoothingThreshold", smoothingThreshold);
			}

			GUILayout.FlexibleSpace();

			GUI.enabled = IsEnabled();

			if (GUILayout.Button("ProBuilderize"))
				EditorUtility.ShowNotification(DoAction().notification);

			GUI.enabled = true;
		}

		public override ActionResult DoAction()
		{
			IEnumerable<MeshFilter> top = Selection.transforms.Select(x => x.GetComponent<MeshFilter>()).Where(y => y != null);
			IEnumerable<MeshFilter> all = Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Where(x => x != null);

			MeshImportSettings settings = new MeshImportSettings()
			{
				quads = PreferencesInternal.GetBool("pb_MeshImporter::quads", true),
				smoothing = PreferencesInternal.GetBool("pb_MeshImporter::smoothing", true),
				smoothingAngle = PreferencesInternal.GetFloat("pb_MeshImporter::smoothingThreshold", 1f)
			};

			if (top.Count() != all.Count())
			{
				int result = UnityEditor.EditorUtility.DisplayDialogComplex("ProBuilderize Selection",
					"ProBuilderize children of selection?",
					"Yes",
					"No",
					"Cancel");

				if (result == 0)
					return DoProBuilderize(all, settings);
				else if (result == 1)
					return DoProBuilderize(top, settings);
				else
					return ActionResult.UserCanceled;
			}
			else
			{
				return DoProBuilderize(all, settings);
			}
		}

		[System.Obsolete("Please use DoProBuilderize(IEnumerable<MeshFilter>, pb_MeshImporter.Settings")]
		public static ActionResult DoProBuilderize(
			IEnumerable<MeshFilter> selected,
			bool preserveFaces)
		{
			return DoProBuilderize(selected, new MeshImportSettings()
			{
				quads = preserveFaces,
				smoothing = false,
				smoothingAngle = 1f
			});
		}

		/// <summary>
		/// Adds pb_Object component without duplicating the objcet. Is undo-able.
		/// </summary>
		/// <param name="selected"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static ActionResult DoProBuilderize(
			IEnumerable<MeshFilter> selected,
			MeshImportSettings settings)
		{
			int i = 0;
			float count = selected.Count();

			foreach (var mf in selected)
			{
				if (mf.sharedMesh == null)
					continue;

				GameObject go = mf.gameObject;
				Mesh originalMesh = mf.sharedMesh;

				try
				{
					ProBuilderMesh pb = Undo.AddComponent<ProBuilderMesh>(go);

					MeshImporter meshImporter = new MeshImporter(pb);
					meshImporter.Import(go, settings);

					// if this was previously a pb_Object, or similarly any other instance asset, destroy it.
					// if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the
					// renderer so that we don't modify the asset.
					if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(originalMesh)))
						Undo.DestroyObjectImmediate(originalMesh);
					else
						go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();

					i++;
				}
				catch (System.Exception e)
				{
					Debug.LogWarning("Failed ProBuilderizing: " + go.name + "\n" + e.ToString());
				}

				UnityEditor.EditorUtility.DisplayProgressBar("ProBuilderizing", mf.gameObject.name, i / count);
			}

			UnityEditor.EditorUtility.ClearProgressBar();
			MeshSelection.OnSelectionChanged();
			ProBuilderEditor.Refresh(true);

			if (i < 1)
				return new ActionResult(ActionResult.Status.Canceled, "Nothing Selected");
			else
				return new ActionResult(ActionResult.Status.Success, "ProBuilderize " + i + (i > 1 ? " Objects" : " Object").ToString());
		}
	}
}
