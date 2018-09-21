using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class DetachFaces : MenuAction
	{
		Pref<DetachSetting> m_DetachSetting = new Pref<DetachSetting>("DetachFaces.target", DetachSetting.GameObject);

		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Detach", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return s_Tooltip; } }

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Detach Faces",
			"Creates a new object (or submesh) from the selected faces."
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Face; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
		}

		protected override MenuActionState optionsMenuState
		{
			get { return MenuActionState.VisibleAndEnabled; }
		}

		enum DetachSetting
		{
			GameObject,
			Submesh
		};

		protected override void OnSettingsGUI()
		{
			GUILayout.Label("Detach Face Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Detach Faces can separate the selection into either a new GameObject or a submesh.", MessageType.Info);

			EditorGUI.BeginChangeCheck();

			m_DetachSetting.value = (DetachSetting) EditorGUILayout.EnumPopup("Detach To", m_DetachSetting);

			if(EditorGUI.EndChangeCheck())
				Settings.Save();

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Detach Selection"))
				EditorUtility.ShowNotification( DoAction().notification );
		}

		public override ActionResult DoAction()
		{
			if (m_DetachSetting == DetachSetting.GameObject)
				return DetachFacesToObject(MeshSelection.TopInternal());

			return DetachFacesToSubmesh(MeshSelection.TopInternal());
		}

		static ActionResult DetachFacesToSubmesh(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Detach Face(s)");

			int count = 0;

			foreach(ProBuilderMesh pb in selection)
			{
				pb.ToMesh();
				List<Face> res = pb.DetachFaces(pb.selectedFacesInternal);
				pb.Refresh();
				pb.Optimize();

				pb.SetSelectedFaces(res.ToArray());

				count += pb.selectedFaceCount;
			}

			ProBuilderEditor.Refresh();

			if(count > 0)
				return new ActionResult(ActionResult.Status.Success, "Detach " + count + (count > 1 ? " Faces" : " Face"));

			return new ActionResult(ActionResult.Status.Success, "Detach Faces");
		}

		static ActionResult DetachFacesToObject(ProBuilderMesh[] selection)
		{
			if(selection == null || selection.Length < 1)
				return ActionResult.NoSelection;

			UndoUtility.RegisterCompleteObjectUndo(selection, "Detach Selection to GameObject");

			int detachedFaceCount = 0;
			List<GameObject> detached = new List<GameObject>();

			foreach(ProBuilderMesh mesh in selection)
			{
				if(mesh.selectedFaceCount < 1 || mesh.selectedFaceCount == mesh.facesInternal.Length)
					continue;

				var primary = mesh.selectedFaceIndexes.ToArray();
				detachedFaceCount += primary.Length;

				List<int> inverse = new List<int>();

				for(int i = 0; i < mesh.facesInternal.Length; i++)
					if(!primary.Contains(i))
						inverse.Add(i);

				ProBuilderMesh copy = Object.Instantiate(mesh.gameObject).GetComponent<ProBuilderMesh>();
				copy.MakeUnique();

#if !UNITY_2018_3_OR_NEWER
				// if is prefab, break connection and destroy children
				if( EditorUtility.IsPrefabInstance(copy.gameObject) || EditorUtility.IsPrefabAsset(copy.gameObject) )
					PrefabUtility.DisconnectPrefabInstance(copy.gameObject);
#endif

				if(copy.transform.childCount > 0)
				{
					for(int i = copy.transform.childCount - 1; i > -1; i--)
						Object.DestroyImmediate(copy.transform.GetChild(i).gameObject);

					foreach(var child in mesh.transform.GetComponentsInChildren<ProBuilderMesh>())
						EditorUtility.SynchronizeWithMeshFilter(child);
				}

				Undo.RegisterCreatedObjectUndo(copy.gameObject, "Detach Selection");

				copy.transform.position = mesh.transform.position;
				copy.transform.localScale = mesh.transform.localScale;
				copy.transform.localRotation = mesh.transform.localRotation;

				mesh.DeleteFaces(primary);
				copy.DeleteFaces(inverse);

				mesh.Rebuild();
				copy.Rebuild();

				mesh.Optimize();
				copy.Optimize();

				mesh.ClearSelection();
				copy.ClearSelection();

				copy.gameObject.name = mesh.gameObject.name + "-detach";
				detached.Add(copy.gameObject);
			}

			MeshSelection.SetSelection(detached.ToArray());
			ProBuilderEditor.Refresh();

			if(detachedFaceCount > 0)
				return new ActionResult(ActionResult.Status.Success, "Detach " + detachedFaceCount + " faces to new Object");

			return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
		}
	}
}

