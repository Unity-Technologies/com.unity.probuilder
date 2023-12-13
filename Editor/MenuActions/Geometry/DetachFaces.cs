using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class DetachFaces : MenuAction
    {
        Pref<DetachSetting> m_DetachSetting = new Pref<DetachSetting>("DetachFaces.target", DetachSetting.GameObject);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override string iconPath => "Toolbar/Face_Detach";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);
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

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var detachFace = new EnumField("Detach To", m_DetachSetting);
            detachFace.tooltip = "You can create a new Game Object with the selected face(s), or keep them as part of this object by using a Submesh.";
            detachFace.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out DetachSetting newValue);
                m_DetachSetting.SetValue(newValue);
            });
            root.Add(detachFace);

            return root;
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Detach Face Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_DetachSetting.value = EditorGUILayout.Toggle("Separate GameObject", m_DetachSetting.value == DetachSetting.GameObject)
                ? DetachSetting.GameObject
                : DetachSetting.Submesh;

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Detach Selection"))
                EditorUtility.ShowNotification(PerformAction().notification);
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Detach Face(s)");

            if (m_DetachSetting == DetachSetting.GameObject)
                return DetachFacesToObject();

            return DetachFacesToSubmesh();
        }

        static ActionResult DetachFacesToSubmesh()
        {
            int count = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                pb.ToMesh();
                List<Face> res = pb.DetachFaces(pb.selectedFacesInternal);
                pb.Refresh();
                pb.Optimize();

                pb.SetSelectedFaces(res.ToArray());

                count += pb.selectedFaceCount;
            }

            ProBuilderEditor.Refresh();

            if (count > 0)
                return new ActionResult(ActionResult.Status.Success, "Detach " + count + (count > 1 ? " Faces" : " Face"));

            return new ActionResult(ActionResult.Status.Success, "Detach Faces");
        }

        static ActionResult DetachFacesToObject()
        {
            int detachedFaceCount = 0;
            List<GameObject> detached = new List<GameObject>();

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                if (mesh.selectedFaceCount < 1 || mesh.selectedFaceCount == mesh.facesInternal.Length)
                    continue;

                var primary = mesh.selectedFaceIndexes;
                detachedFaceCount += primary.Count;

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

                Undo.RecordObject(mesh.renderer, "Update Renderer");

                mesh.DeleteFaces(primary);
                copy.DeleteFaces(inverse);

                mesh.Rebuild();
                copy.Rebuild();

                mesh.Optimize();
                copy.Optimize();

                mesh.ClearSelection();
                copy.ClearSelection();

                copy.SetSelectedFaces(copy.faces);

                Undo.RegisterCreatedObjectUndo(copy.gameObject, "Detach Selection");

                copy.gameObject.name = GameObjectUtility.GetUniqueNameForSibling(mesh.transform.parent, mesh.gameObject.name); ;
                detached.Add(copy.gameObject);
            }

            MeshSelection.SetSelection(detached);
            ProBuilderEditor.Refresh();

            if (detachedFaceCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Detach " + detachedFaceCount + " faces to new Object");

            return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
        }
    }
}
