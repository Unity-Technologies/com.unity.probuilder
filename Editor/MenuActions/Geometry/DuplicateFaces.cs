using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class DuplicateFaces : MenuAction
    {
        Pref<DuplicateFaceSetting> m_DuplicateFaceSetting = new Pref<DuplicateFaceSetting>("DuplicateFaces.target", DuplicateFaceSetting.GameObject);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override string iconPath => "Toolbar/Face_Duplicate";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Duplicate Faces",
                "Makes an exact copy of the selected faces, and either adds them to this mesh or creates a new Game Object"
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

        internal enum DuplicateFaceSetting
        {
            GameObject,
            Submesh
        };

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var duplicateType = new EnumField("Duplicate To", m_DuplicateFaceSetting);
            duplicateType.tooltip = "You can create a new Game Object with the selected face(s), or keep them as part of this object by using a Submesh.";
            duplicateType.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out DuplicateFaceSetting newValue);
                if (m_DuplicateFaceSetting.value == newValue)
                    return;

                m_DuplicateFaceSetting.value = newValue;
                ProBuilderSettings.Save();
            });
            root.Add(duplicateType);

            return root;
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Duplicate Face Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("You can create a new Game Object with the selected face(s), or keep them as part of this object by using a Submesh.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_DuplicateFaceSetting.value = (DuplicateFaceSetting)EditorGUILayout.EnumPopup("Duplicate To", m_DuplicateFaceSetting);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Duplicate Selection"))
                EditorUtility.ShowNotification(PerformAction().notification);
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Duplicate Face(s)");

            if (m_DuplicateFaceSetting == DuplicateFaceSetting.GameObject)
                return DuplicateFacesToObject();

            return DuplicateFacesToSubmesh();
        }

        static ActionResult DuplicateFacesToSubmesh()
        {
            int count = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                pb.ToMesh();
                List<Face> res = pb.DetachFaces(pb.selectedFacesInternal, false);
                pb.Refresh();
                pb.Optimize();

                pb.SetSelectedFaces(res.ToArray());

                count += pb.selectedFaceCount;
            }

            ProBuilderEditor.Refresh();

            if (count > 0)
                return new ActionResult(ActionResult.Status.Success, "Duplicate " + count + (count > 1 ? " Faces" : " Face"));

            return new ActionResult(ActionResult.Status.Success, "Duplicate Faces");
        }

        static ActionResult DuplicateFacesToObject()
        {
            int duplicatedFaceCount = 0;
            List<GameObject> duplicated = new List<GameObject>();

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                if (mesh.selectedFaceCount < 1)
                    continue;

                var primary = mesh.selectedFaceIndexes;
                duplicatedFaceCount += primary.Count;

                List<int> inverse = new List<int>();

                for (int i = 0; i < mesh.facesInternal.Length; i++)
                    if (!primary.Contains(i))
                        inverse.Add(i);

                ProBuilderMesh copy = Object.Instantiate(mesh.gameObject, mesh.transform.parent).GetComponent<ProBuilderMesh>();
                copy.MakeUnique();
                EditorUtility.SynchronizeWithMeshFilter(copy);

                if (copy.transform.childCount > 0)
                {
                    for (int i = copy.transform.childCount - 1; i > -1; i--)
                        Object.DestroyImmediate(copy.transform.GetChild(i).gameObject);

                    foreach (var child in mesh.transform.GetComponentsInChildren<ProBuilderMesh>())
                        EditorUtility.SynchronizeWithMeshFilter(child);
                }

                Undo.RegisterCreatedObjectUndo(copy.gameObject, "Duplicate Selection");

                copy.DeleteFaces(inverse);
                copy.Rebuild();
                copy.Optimize();
                mesh.ClearSelection();
                copy.ClearSelection();
                copy.SetSelectedFaces(copy.faces);

                copy.gameObject.name = GameObjectUtility.GetUniqueNameForSibling(mesh.transform.parent, mesh.gameObject.name);
                duplicated.Add(copy.gameObject);
            }

            MeshSelection.SetSelection(duplicated);
            ProBuilderEditor.Refresh();

            if (duplicatedFaceCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Duplicate " + duplicatedFaceCount + " faces to new Object");

            return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
        }
    }
}
