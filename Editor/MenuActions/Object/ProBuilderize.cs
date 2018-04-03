using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using System.Collections.Generic;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.MeshOperations;

namespace ProBuilder.Actions
{
	class ProBuilderize : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Object_ProBuilderize", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }
		public override bool isProOnly { get { return true; } }

		private GUIContent m_QuadsTooltip = new GUIContent("Import Quads", "Create ProBuilder mesh using quads where " +
			"possible instead of triangles.");
		private GUIContent m_SmoothingTooltip = new GUIContent("Import Smoothing", "Import smoothing groups by " +
			"testing adjacent faces against an angle thresold.");
		private GUIContent m_SmoothingThresholdTooltip = new GUIContent("Smoothing Threshold", "When importing " +
			"smoothing groups any adjacent faces with an adjoining angle difference of less than this value will be " +
			"grouped together in a smoothing group.");

		private static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"ProBuilderize",
			@"Creates ProBuilder-modifiable objects from meshes."
		);

		public override bool IsEnabled()
		{
			int meshCount = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Count();

			return	meshCount > 0 &&
					meshCount != selection.Length;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("ProBuilderize Options", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("When Preserve Faces is enabled ProBuilder will try to group adjacent triangles into faces.", MessageType.Info);

			bool quads = pb_PreferencesInternal.GetBool("pb_MeshImporter::quads", true);
			bool smoothing = pb_PreferencesInternal.GetBool("pb_MeshImporter::smoothing", true);
			float smoothingThreshold = pb_PreferencesInternal.GetFloat("pb_MeshImporter::smoothingThreshold", 1f);

			EditorGUI.BeginChangeCheck();

			quads = EditorGUILayout.Toggle(m_QuadsTooltip, quads);
			smoothing = EditorGUILayout.Toggle(m_SmoothingTooltip, smoothing);
			GUI.enabled = smoothing;
			EditorGUILayout.PrefixLabel(m_SmoothingThresholdTooltip);
			smoothingThreshold = EditorGUILayout.Slider(smoothingThreshold, 0.0001f, 45f);
			GUI.enabled = true;

			if (EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetBool("pb_MeshImporter::quads", quads);
				pb_PreferencesInternal.SetBool("pb_MeshImporter::smoothing", smoothing);
				pb_PreferencesInternal.SetFloat("pb_MeshImporter::smoothingThreshold", smoothingThreshold);
			}

			GUILayout.FlexibleSpace();

			GUI.enabled = IsEnabled();

			if(GUILayout.Button("ProBuilderize"))
				pb_EditorUtility.ShowNotification(DoAction().notification);

			GUI.enabled = true;
		}

		public override pb_ActionResult DoAction()
		{
			IEnumerable<MeshFilter> top = Selection.transforms.Select(x => x.GetComponent<MeshFilter>()).Where(y => y != null);
			IEnumerable<MeshFilter> all = Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Where(x => x != null);

			pb_MeshImporter.Settings settings = new pb_MeshImporter.Settings()
			{
				quads = pb_PreferencesInternal.GetBool("pb_MeshImporter::quads", true),
				smoothing = pb_PreferencesInternal.GetBool("pb_MeshImporter::smoothing", true),
				smoothingThreshold = pb_PreferencesInternal.GetFloat("pb_MeshImporter::smoothingThreshold", 1f)
			};

			if(top.Count() != all.Count())
			{
				int result = EditorUtility.DisplayDialogComplex("ProBuilderize Selection",
					"ProBuilderize children of selection?",
					"Yes",
					"No",
					"Cancel");

				if(result == 0)
					return DoProBuilderize(all, settings);
				else if(result == 1)
					return DoProBuilderize(top, settings);
				else
					return pb_ActionResult.UserCanceled;
			}
			else
			{
				return DoProBuilderize(all, settings);
			}
		}

		[System.Obsolete("Please use DoProBuilderize(IEnumerable<MeshFilter>, pb_MeshImporter.Settings")]
		public static pb_ActionResult DoProBuilderize(
			IEnumerable<MeshFilter> selected,
			bool preserveFaces)
		{
			return DoProBuilderize(selected, new pb_MeshImporter.Settings()
			{
				quads = preserveFaces,
				smoothing = false,
				smoothingThreshold = 1f
			});
		}

		/// <summary>
		/// Adds pb_Object component without duplicating the objcet. Is undo-able.
		/// </summary>
		/// <param name="selected"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static pb_ActionResult DoProBuilderize(
			IEnumerable<MeshFilter> selected,
			pb_MeshImporter.Settings settings)
		{
			int i = 0;
			float count = selected.Count();

			foreach(var mf in selected)
			{
				if(mf.sharedMesh == null)
					continue;

				GameObject go = mf.gameObject;
				Mesh originalMesh = mf.sharedMesh;

				try
				{
					pb_Object pb = Undo.AddComponent<pb_Object>(go);

					pb_MeshImporter meshImporter = new pb_MeshImporter(pb);
					meshImporter.Import(go, settings);

					// if this was previously a pb_Object, or similarly any other instance asset, destroy it.
					// if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the
					// renderer so that we don't modify the asset.
					if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(originalMesh)))
						Undo.DestroyObjectImmediate(originalMesh);
					else
						go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();

					i++;
				}
				catch(System.Exception e)
				{
					Debug.LogWarning("Failed ProBuilderizing: " + go.name + "\n" + e.ToString());
				}

				EditorUtility.DisplayProgressBar("ProBuilderizing", mf.gameObject.name, i / count);
			}

			EditorUtility.ClearProgressBar();
			pb_Selection.OnSelectionChanged();
			pb_Editor.Refresh(true);

			if(i < 1)
				return new pb_ActionResult(Status.Canceled, "Nothing Selected");
			else
				return new pb_ActionResult(Status.Success, "ProBuilderize " + i + (i > 1 ? " Objects" : " Object").ToString());
		}
	}
}
