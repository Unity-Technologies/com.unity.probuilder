using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class MirrorObjects : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Object_Mirror"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		[System.Flags]
		private enum MirrorSettings {
			X = 0x1,
			Y = 0x2,
			Z = 0x4,
			Duplicate = 0x8
		}

		MirrorSettings storedScale
		{
			get { return (MirrorSettings) pb_PreferencesInternal.GetInt("pbMirrorObjectScale", (int)(0x1 | 0x8)); }
			set { pb_PreferencesInternal.SetInt("pbMirrorObjectScale", (int) value); }
		}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Mirror Objects",
			@"Mirroring objects will duplicate an flip objects on the specified axes."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
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
				pb_EditorUtility.ShowNotification( DoAction().notification );
		}

		public override pb_ActionResult DoAction()
		{
			Vector3 scale = new Vector3(
				(storedScale & MirrorSettings.X) > 0 ? -1f : 1f,
				(storedScale & MirrorSettings.Y) > 0 ? -1f : 1f,
				(storedScale & MirrorSettings.Z) > 0 ? -1f : 1f );

			bool duplicate = (storedScale & MirrorSettings.Duplicate) > 0;

			List<GameObject> res  = new List<GameObject>();

			foreach(pb_Object pb in selection)
				res.Add( Mirror(pb, scale, duplicate).gameObject );

			pb_Selection.SetSelection(res);

			pb_Editor.Refresh();

			return res.Count > 0 ?
				new pb_ActionResult(Status.Success, string.Format("Mirror {0} {1}", res.Count, res.Count > 1 ? "Objects" : "Object")) :
				new pb_ActionResult(Status.NoChange, "No Objects Selected");
		}

		/**
		 *	\brief Duplicates and mirrors the passed pb_Object.
		 *	@param pb The donor pb_Object.
		 *	@param axe The axis to mirror the object on.
		 *	\returns The newly duplicated pb_Object.
		 *	\sa ProBuilder.Axis
		 */
		public static pb_Object Mirror(pb_Object pb, Vector3 scale, bool duplicate = true)
		{
			pb_Object p;

			if(duplicate)
			{
				p = pb_Object.InitWithObject(pb);
				p.MakeUnique();
				p.transform.parent = pb.transform.parent;
				p.transform.localRotation = pb.transform.localRotation;
			}
			else
			{
				pbUndo.RecordObject(pb, "Mirror");
				p = pb;
			}

			Vector3 lScale = pb.gameObject.transform.localScale;
			p.transform.localScale = scale;

			// if flipping on an odd number of axes, flip winding order
			if( (scale.x * scale.y * scale.z) < 0)
				p.ReverseWindingOrder(p.faces);

			p.FreezeScaleTransform();

			p.transform.localScale = lScale;

			if(duplicate)
			{
				Collider col = pb.GetComponent<Collider>();
				ColliderType colType = ColliderType.None;

				if(col != null)
				{
					if(col is MeshCollider)
						colType = ColliderType.MeshCollider;
					else
						colType = ColliderType.BoxCollider;
				}

				pb_EditorUtility.InitObject(p, colType, pb.GetComponent<pb_Entity>().entityType);
				p.transform.position = pb.transform.position;

				Undo.RegisterCreatedObjectUndo(p.gameObject, "Mirror Object");
			}

			p.ToMesh();
			p.Refresh();
			p.Optimize();

			return p;
		}
	}
}
