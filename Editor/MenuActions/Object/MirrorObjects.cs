using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor.ProBuilder.UI;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class MirrorObjects : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Object; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Object_Mirror", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		[System.Flags]
		private enum MirrorSettings
		{
			X = 0x1,
			Y = 0x2,
			Z = 0x4,
			Duplicate = 0x8
		}

		MirrorSettings storedScale
		{
			get { return (MirrorSettings)PreferencesInternal.GetInt("pbMirrorObjectScale", (int)(0x1 | 0x8)); }
			set { PreferencesInternal.SetInt("pbMirrorObjectScale", (int)value); }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Mirror Objects",
			@"Mirroring objects will duplicate an flip objects on the specified axes."
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null && MeshSelection.Top().Length > 0;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Mirror Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Mirror objects on the selected axes.\n\nIf Duplicate is toggled a new object will be instantiated from the selection and mirrored, or if disabled the selection will be moved.", MessageType.Info);

			MirrorSettings scale = storedScale;

			bool x = (scale & MirrorSettings.X) != 0 ? true : false;
			bool y = (scale & MirrorSettings.Y) != 0 ? true : false;
			bool z = (scale & MirrorSettings.Z) != 0 ? true : false;
			bool d = (scale & MirrorSettings.Duplicate) != 0 ? true : false;

			EditorGUI.BeginChangeCheck();

			x = EditorGUILayout.Toggle("X", x);
			y = EditorGUILayout.Toggle("Y", y);
			z = EditorGUILayout.Toggle("Z", z);
			d = EditorGUILayout.Toggle("Duplicate", d);

			if(EditorGUI.EndChangeCheck())
				storedScale = (MirrorSettings)
				(x ? MirrorSettings.X : 0) |
				(y ? MirrorSettings.Y : 0) |
				(z ? MirrorSettings.Z : 0) |
				(d ? MirrorSettings.Duplicate : 0);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Mirror"))
				EditorUtility.ShowNotification( DoAction().notification );
		}

		public override ActionResult DoAction()
		{
			Vector3 scale = new Vector3(
				(storedScale & MirrorSettings.X) > 0 ? -1f : 1f,
				(storedScale & MirrorSettings.Y) > 0 ? -1f : 1f,
				(storedScale & MirrorSettings.Z) > 0 ? -1f : 1f );

			bool duplicate = (storedScale & MirrorSettings.Duplicate) > 0;

			List<GameObject> res  = new List<GameObject>();

			foreach(ProBuilderMesh pb in MeshSelection.Top())
				res.Add( Mirror(pb, scale, duplicate).gameObject );

			MeshSelection.SetSelection(res);

			ProBuilderEditor.Refresh();

			return res.Count > 0 ?
				new ActionResult(ActionResult.Status.Success, string.Format("Mirror {0} {1}", res.Count, res.Count > 1 ? "Objects" : "Object")) :
				new ActionResult(ActionResult.Status.NoChange, "No Objects Selected");
		}

		/**
		 *	\brief Duplicates and mirrors the passed pb_Object.
		 *	@param pb The donor pb_Object.
		 *	@param axe The axis to mirror the object on.
		 *	\returns The newly duplicated pb_Object.
		 *	\sa ProBuilder.Axis
		 */
		public static ProBuilderMesh Mirror(ProBuilderMesh pb, Vector3 scale, bool duplicate = true)
		{
			ProBuilderMesh mirredObject;

			if (duplicate)
			{
				mirredObject = Object.Instantiate(pb.gameObject, pb.transform.parent, false).GetComponent<ProBuilderMesh>();
				mirredObject.MakeUnique();
				mirredObject.transform.parent = pb.transform.parent;
				mirredObject.transform.localRotation = pb.transform.localRotation;
				Undo.RegisterCreatedObjectUndo(mirredObject.gameObject, "Mirror Object");
			}
			else
			{
				UndoUtility.RecordObject(pb, "Mirror");
				mirredObject = pb;
			}

			Vector3 lScale = mirredObject.gameObject.transform.localScale;
			mirredObject.transform.localScale = scale;

			// if flipping on an odd number of axes, flip winding order
			if ((scale.x * scale.y * scale.z) < 0)
			{
				foreach(var face in mirredObject.facesInternal)
					face.Reverse();
			}

			mirredObject.FreezeScaleTransform();
			mirredObject.transform.localScale = lScale;

			mirredObject.ToMesh();
			mirredObject.Refresh();
			mirredObject.Optimize();

			return mirredObject;
		}
	}
}
