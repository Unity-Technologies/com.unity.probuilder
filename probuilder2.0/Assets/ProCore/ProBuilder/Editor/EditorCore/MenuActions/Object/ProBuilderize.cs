using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.Actions
{
	public class ProBuilderize : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Object_ProBuilderize"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
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

			bool preserveFaces = pb_PreferencesInternal.GetBool(pb_Constant.pbPreserveFaces);

			EditorGUI.BeginChangeCheck();

			preserveFaces = EditorGUILayout.Toggle("Preserve Faces", preserveFaces);

			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetBool(pb_Constant.pbPreserveFaces, preserveFaces);

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
			bool preserveFaces = pb_PreferencesInternal.GetBool(pb_Constant.pbPreserveFaces);

			if(top.Count() != all.Count())
			{
				int result = EditorUtility.DisplayDialogComplex("ProBuilderize Selection",
					"ProBuilderize children of selection?",
					"Yes",
					"No",
					"Cancel");

				if(result == 0)
					return DoProBuilderize(all, preserveFaces);
				else if(result == 1)
					return DoProBuilderize(top, preserveFaces);
				else
					return pb_ActionResult.UserCanceled;
			}
			else
			{
				return DoProBuilderize(all, preserveFaces);
			}
		}

		/**
		 * Adds pb_Object and pb_Entity to object without duplicating the objcet.  Is undo-able.
		 */
		public static pb_ActionResult DoProBuilderize(IEnumerable<MeshFilter> selected, bool preserveFaces)
		{
			if(selected.Count() < 1)
				return new pb_ActionResult(Status.Canceled, "Nothing Selected");

			int i = 0;
			float count = selected.Count();

			foreach(MeshFilter mf in selected)
			{
				if(mf.sharedMesh == null)
					continue;

				GameObject go = mf.gameObject;
				MeshRenderer mr = go.GetComponent<MeshRenderer>();
				Mesh originalMesh = mf.sharedMesh;

				try
				{
					pb_Object pb = Undo.AddComponent<pb_Object>(go);

					pb_MeshImporter meshImporter = new pb_MeshImporter(pb);
					meshImporter.Import(go);

					EntityType entityType = EntityType.Detail;

					if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x != null && x.name.Contains("Collider")))
						entityType = EntityType.Collider;
					else
					if(mr != null && mr.sharedMaterials != null && mr.sharedMaterials.Any(x => x != null && x.name.Contains("Trigger")))
						entityType = EntityType.Trigger;

					// if this was previously a pb_Object, or similarly any other instance asset, destroy it.
					// if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the
					// renderer so that we don't modify the asset.
					if(AssetDatabase.GetAssetPath(originalMesh) == "" )
						Undo.DestroyObjectImmediate(originalMesh);
					else if(mf != null)
						go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

					pb.ToMesh();
					pb.Refresh();
					pb.Optimize();

					i++;

					// Don't call the editor version of SetEntityType because that will
					// reset convexity and trigger settings, which we can assume are user
					// set already.
					if( !pb.gameObject.GetComponent<pb_Entity>() )
						Undo.AddComponent<pb_Entity>(pb.gameObject).SetEntity(entityType);
					else
						pb.gameObject.GetComponent<pb_Entity>().SetEntity(entityType);
				}
				catch(System.Exception e)
				{
					Debug.LogWarning("Failed ProBuilderizing: " + go.name + "\n" + e.ToString());
				}

				EditorUtility.DisplayProgressBar("ProBuilderizing", mf.gameObject.name, i/count);
			}

			EditorUtility.ClearProgressBar();

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "ProBuilderize " + i + (i > 1 ? " Objects" : " Object").ToString());
		}
	}
}
