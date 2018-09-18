using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ExtrudeFaces : MenuAction
	{
		Pref<float> m_ExtrudeDistance = new Pref<float>("ExtrudeFaces.distance", .5f);

		ExtrudeMethod extrudeMethod
		{
			get { return ProBuilderEditor.instance.m_ExtrudeMethod; }
			set { ProBuilderEditor.instance.m_ExtrudeMethod.value = value; }
		}

		static string GetExtrudeIconString(ExtrudeMethod m)
		{
			return m == ExtrudeMethod.VertexNormal ? "Toolbar/ExtrudeFace_VertexNormals"
				: m == ExtrudeMethod.FaceNormal ? "Toolbar/ExtrudeFace_FaceNormals"
				: "Toolbar/ExtrudeFace_Individual";
		}

		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon(GetExtrudeIconString(extrudeMethod), IconSkin.Pro); } }
		protected override Texture2D disabledIcon
		{
			get { return IconUtility.GetIcon(string.Format("{0}_disabled", GetExtrudeIconString(extrudeMethod)), IconSkin.Pro); }
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
				return editLevel != EditLevel.Geometry || componentMode != ComponentMode.Face;
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

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
					GUILayout.Label(icons[(int) extrudeMethod]);
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();

			extrudeMethod = (ExtrudeMethod) EditorGUILayout.EnumPopup("Extrude By", extrudeMethod);
			m_ExtrudeDistance.value = EditorGUILayout.FloatField("Distance", m_ExtrudeDistance);

			if(EditorGUI.EndChangeCheck())
				Settings.Save();

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Extrude Faces"))
				DoAction();
		}

		public override ActionResult DoAction()
		{
			var editor = ProBuilderEditor.instance;
			var selection = MeshSelection.TopInternal();

			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Extrude");

			int extrudedFaceCount = 0;

			foreach(ProBuilderMesh mesh in selection)
			{
				mesh.ToMesh();
				mesh.Refresh(RefreshMask.Normals);

				if (mesh.selectedFaceCount < 1)
					continue;

				extrudedFaceCount += mesh.selectedFaceCount;
				var selectedFaces = mesh.GetSelectedFaces();

				mesh.Extrude(selectedFaces,
					ProBuilderEditor.instance.m_ExtrudeMethod,
					m_ExtrudeDistance);

				mesh.SetSelectedFaces(selectedFaces);

				mesh.Rebuild();
				mesh.Optimize();
			}

			if(editor != null)
				ProBuilderEditor.Refresh();

			SceneView.RepaintAll();

			if( extrudedFaceCount > 0 )
				return new ActionResult(ActionResult.Status.Success, "Extrude");

			return new ActionResult(ActionResult.Status.Canceled, "Extrude\nEmpty Selection");
		}
	}
}

