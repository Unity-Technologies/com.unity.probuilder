using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class DuplicateFaces : MenuAction
    {
        Pref<DuplicateFaceSetting> m_DuplicateFaceSetting = new Pref<DuplicateFaceSetting>("DuplicateFaces.target", DuplicateFaceSetting.GameObject);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Detach", IconSkin.Pro); } } //icon to be replaced
        public override TooltipContent tooltip { get { return s_Tooltip; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Duplicate Faces",
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

        internal enum DuplicateFaceSetting
        {
            GameObject,
            Submesh
        };

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Duplicate Face Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Duplicate Faces can separate the selection into either a new GameObject or a submesh.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_DuplicateFaceSetting.value = (DuplicateFaceSetting)EditorGUILayout.EnumPopup("Duplicate To", m_DuplicateFaceSetting);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Duplicate Selection"))
                EditorUtility.ShowNotification(DoAction().notification);
        }

        public override ActionResult DoAction()
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
            int DuplicateedFaceCount = 0;
            List<GameObject> Duplicateed = new List<GameObject>();

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                if (mesh.selectedFaceCount < 1 || mesh.selectedFaceCount == mesh.facesInternal.Length)
                    continue;

                var primary = mesh.selectedFaceIndexes.ToArray();
                DuplicateedFaceCount += primary.Length;

                List<int> inverse = new List<int>();

                for (int i = 0; i < mesh.facesInternal.Length; i++)
                    if (!primary.Contains(i))
                        inverse.Add(i);

                ProBuilderMesh copy = Object.Instantiate(mesh.gameObject, mesh.transform.parent).GetComponent<ProBuilderMesh>();
                copy.MakeUnique();

#if !UNITY_2018_3_OR_NEWER
                // if is prefab, break connection and destroy children
                if (EditorUtility.IsPrefabInstance(copy.gameObject) || EditorUtility.IsPrefabAsset(copy.gameObject))
                    PrefabUtility.DisconnectPrefabInstance(copy.gameObject);
#endif

                if (copy.transform.childCount > 0)
                {
                    for (int i = copy.transform.childCount - 1; i > -1; i--)
                        Object.DestroyImmediate(copy.transform.GetChild(i).gameObject);

                    foreach (var child in mesh.transform.GetComponentsInChildren<ProBuilderMesh>())
                        EditorUtility.SynchronizeWithMeshFilter(child);
                }

                Undo.RegisterCreatedObjectUndo(copy.gameObject, "Duplicate Selection");

                copy.transform.position = mesh.transform.position;
                copy.transform.localScale = mesh.transform.localScale;
                copy.transform.localRotation = mesh.transform.localRotation;

                copy.DeleteFaces(inverse);

                mesh.Rebuild();
                copy.Rebuild();

                mesh.Optimize();
                copy.Optimize();

                mesh.ClearSelection();
                copy.ClearSelection();

                copy.gameObject.name = mesh.gameObject.name + "-Duplicate";
                Duplicateed.Add(copy.gameObject);
            }

            MeshSelection.SetSelection(Duplicateed.ToArray());
            ProBuilderEditor.Refresh();

            if (DuplicateedFaceCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Duplicate " + DuplicateedFaceCount + " faces to new Object");

            return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
        }
    }
}
