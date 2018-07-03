using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ExtrudeFaces : MenuAction
	{
		private ExtrudeMethod m_ExtrudeMethod;

		private static string GetExtrudeIconString(ExtrudeMethod m)
		{
			return m == ExtrudeMethod.VertexNormal ? "Toolbar/ExtrudeFace_VertexNormals"
				: m == ExtrudeMethod.FaceNormal ? "Toolbar/ExtrudeFace_FaceNormals"
				: "Toolbar/ExtrudeFace_Individual";
		}

		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon(GetExtrudeIconString(m_ExtrudeMethod), IconSkin.Pro); } }
		protected override Texture2D disabledIcon
		{
			get { return IconUtility.GetIcon(string.Format("{0}_disabled", GetExtrudeIconString(m_ExtrudeMethod)), IconSkin.Pro); }
		}

		public override TooltipContent tooltip { get { return _tooltip; } }
		protected override bool hasFileMenuEntry { get { return false; } }
		[SerializeField] Texture2D[] icons = null;

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Extrude Faces",
			"Extrude selected faces, either as a group or individually.\n\nAlt + Click this button to show additional Extrude options.",
			keyCommandSuper, 'E'
		);

		public ExtrudeFaces()
		{
			m_ExtrudeMethod = (ExtrudeMethod) PreferencesInternal.GetInt(PreferenceKeys.pbExtrudeMethod);

			icons = new Texture2D[3];
			icons[(int)ExtrudeMethod.IndividualFaces] = IconUtility.GetIcon("Toolbar/ExtrudeFace_Individual", IconSkin.Pro);
			icons[(int)ExtrudeMethod.VertexNormal] = IconUtility.GetIcon("Toolbar/ExtrudeFace_VertexNormals", IconSkin.Pro);
			icons[(int)ExtrudeMethod.FaceNormal] = IconUtility.GetIcon("Toolbar/ExtrudeFace_FaceNormals", IconSkin.Pro);
		}

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MeshSelection.TopInternal().Sum(x => x.selectedFaceCount) > 0;
			}
		}

		public override bool hidden
		{
			get
			{
				return editLevel != EditLevel.Geometry ||
					(PreferencesInternal.GetBool(PreferenceKeys.pbElementSelectIsHamFisted) && componentMode != ComponentMode.Face);
			}
		}

		protected override MenuActionState optionsMenuState
		{
			get { return MenuActionState.VisibleAndEnabled; }
		}

		protected override void OnSettingsGUI()
		{
			GUILayout.Label("Extrude Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Extrude Amount determines how far a face will be moved along it's normal when extruding.  This value can be negative.\n\nYou may also choose to Extrude by Face Normal, Vertex Normal, or as Individual Faces.", MessageType.Info);

			float extrudeAmount = PreferencesInternal.HasKey(PreferenceKeys.pbExtrudeDistance) ? PreferencesInternal.GetFloat(PreferenceKeys.pbExtrudeDistance) : .5f;

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
					GUILayout.Label(icons[(int) m_ExtrudeMethod]);
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();

			m_ExtrudeMethod = (ExtrudeMethod) EditorGUILayout.EnumPopup("Extrude By", m_ExtrudeMethod);
			extrudeAmount = EditorGUILayout.FloatField("Distance", extrudeAmount);

			if(EditorGUI.EndChangeCheck())
			{
				PreferencesInternal.SetFloat(PreferenceKeys.pbExtrudeDistance, extrudeAmount);
				PreferencesInternal.SetInt(PreferenceKeys.pbExtrudeMethod, (int) m_ExtrudeMethod);
			}

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Extrude Faces"))
				DoAction();
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuExtrude(MeshSelection.TopInternal(), false);
		}
	}
}

