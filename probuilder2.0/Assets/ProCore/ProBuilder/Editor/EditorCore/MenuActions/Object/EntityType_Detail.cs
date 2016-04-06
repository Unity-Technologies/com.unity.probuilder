using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public abstract class SetEntityType : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Object; } }
		public override Texture2D icon { get { return null; } }

		protected static GUIStyle _eyeconStyle = null;
		protected static GUIStyle eyeconStyle
		{
			get
			{
				if(_eyeconStyle == null)
				{
					_eyeconStyle = new GUIStyle();
					_eyeconStyle.alignment 			= TextAnchor.MiddleCenter;
					_eyeconStyle.border 			= new RectOffset(1,1,1,1);
					_eyeconStyle.stretchWidth 		= false;
					_eyeconStyle.stretchHeight 		= false;
					_eyeconStyle.margin 			= new RectOffset(4,4,4,4);
					_eyeconStyle.padding 			= new RectOffset(0,0,0,0);
				}

				return _eyeconStyle;
			}
		}

		protected abstract EntityType entityType { get; }
		protected abstract string entityPref { get; }

		private bool visible = true;

		static Texture2D isVisibleIcon, isNotVisibleIcon;

		public SetEntityType() : base()
		{
			visible = pb_Preferences_Internal.GetBool(entityPref);

			if(!isVisibleIcon) 
				isVisibleIcon = pb_IconUtility.GetIcon("Toolbar/Eye_On"); 

			if(!isNotVisibleIcon)
				isNotVisibleIcon = pb_IconUtility.GetIcon("Toolbar/Eye_Off");
		}

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuSetEntityType(selection, entityType);
		}

		protected override bool DoAltButton(params GUILayoutOption[] options)
		{
			return GUILayout.Button(visible ? isVisibleIcon : isNotVisibleIcon, eyeconStyle, options);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void DoAlt()
		{
			visible = !visible;
			EditorPrefs.SetBool(entityPref, visible);
			pb_EntityVisibility.SetEntityVisibility(entityType, visible);
		}
	}

	public class SetEntityType_Detail : SetEntityType
	{
		public override string MenuTitle { get { return "Set Detail"; } }
		protected override string entityPref { get { return pb_Constant.pbShowDetail; } }
		protected override EntityType entityType { get { return EntityType.Detail; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Entity Type: Detail",
			@"Set the selected objects to Detail entity types.

A Detail type is marked with all static flags except Occluding and Reflection Probes."
		);
	}

	public class SetEntityType_Mover : SetEntityType
	{
		public override string MenuTitle { get { return "Set Mover"; } }
		protected override string entityPref { get { return pb_Constant.pbShowMover; } }
		protected override EntityType entityType { get { return EntityType.Mover; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent(
			"Set Entity Type: Mover",
			"Sets all objects in selection to the entity type Mover.\n\nMover types have no static flags, so they may be moved during play mode.");
	}

	public class SetEntityType_Collider : SetEntityType
	{
		public override string MenuTitle { get { return "Set Collider"; } }
		protected override string entityPref { get { return pb_Constant.pbShowCollider; } }
		protected override EntityType entityType { get { return EntityType.Collider; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent(
			"Set Entity Type: Collider",
			"Sets all objects in selection to the entity type Collider.\n\nCollider types have Navigation and Off-Link Nav static flags set by default, and will have their MeshRenderer disabled on entering play mode.");
	}

	public class SetEntityType_Trigger : SetEntityType
	{
		public override string MenuTitle { get { return "Set Trigger"; } }
		protected override string entityPref { get { return pb_Constant.pbShowTrigger; } }
		protected override EntityType entityType { get { return EntityType.Trigger; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent(
			"Set Entity Type: Trigger",
			"Sets all objects in selection to the entity type Trigger.\n\nTrigger types have no static flags, and have a convex collider marked as Is Trigger added.  The MeshRenderer is turned off on entering play mode.");
	}
}
