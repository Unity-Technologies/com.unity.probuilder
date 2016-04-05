using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class EntityType_Detail : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string MenuTitle { get { return "Set Detail"; } }

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

		private bool visible = true;
		Texture2D isVisibleIcon, isNotVisibleIcon;

		public EntityType_Detail() : base()
		{
			visible = pb_Preferences_Internal.GetBool(pb_Constant.pbShowDetail);
			isVisibleIcon = pb_IconUtility.GetIcon("Toolbar/Eye_On", IconSkin.Pro); 
			isNotVisibleIcon = pb_IconUtility.GetIcon("Toolbar/Eye_Off", IconSkin.Pro);
		}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Entity Type: Detail",
			@"Set the selected objects to Detail entity types.

A Detail type is marked with all static flags except Occluding and Reflection Probes."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Detail);
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
			EditorPrefs.SetBool(pb_Constant.pbShowDetail, visible);
			pb_EntityVisibility.SetEntityVisibility(EntityType.Detail, visible);
		}
	}
}
