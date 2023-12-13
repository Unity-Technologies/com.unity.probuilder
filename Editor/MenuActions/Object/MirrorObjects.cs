using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Object = UnityEngine.Object;


namespace UnityEditor.ProBuilder.Actions
{
    sealed class MirrorObjects : MenuAction
    {
        Pref<MirrorSettings> m_MirrorAxes = new Pref<MirrorSettings>("MirrorObjects.mirrorAxes", MirrorSettings.X);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }
        public override string iconPath => "Toolbar/Object_Mirror";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        [System.Flags]
        enum MirrorSettings
        {
            X = 0x1,
            Y = 0x2,
            Z = 0x4,
            Duplicate = 0x8
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Mirror Objects",
                @"Mirroring objects will duplicate and flip objects on the specified axes."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            MirrorSettings scale = m_MirrorAxes;
            bool x = (scale & MirrorSettings.X) != 0;
            bool y = (scale & MirrorSettings.Y) != 0;
            bool z = (scale & MirrorSettings.Z) != 0;
            bool d = (scale & MirrorSettings.Duplicate) != 0;

            var tooltip = "Mirror objects on the selected axes";
            var toggle = new Toggle("X");
            toggle.tooltip = tooltip;
            toggle.SetValueWithoutNotify(x);
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_MirrorAxes.SetValue((evt.newValue ? MirrorSettings.X : 0) | m_MirrorAxes & MirrorSettings.Y | m_MirrorAxes & MirrorSettings.Z | m_MirrorAxes & MirrorSettings.Duplicate);
                PreviewActionManager.UpdatePreview();
            });
            root.Add(toggle);

            toggle = new Toggle("Y");
            toggle.tooltip = tooltip;
            toggle.SetValueWithoutNotify(y);
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_MirrorAxes.SetValue(m_MirrorAxes & MirrorSettings.X | (evt.newValue ? MirrorSettings.Y : 0) | m_MirrorAxes & MirrorSettings.Z | m_MirrorAxes & MirrorSettings.Duplicate);
                PreviewActionManager.UpdatePreview();
            });
            root.Add(toggle);

            toggle = new Toggle("Z");
            toggle.tooltip = tooltip;
            toggle.SetValueWithoutNotify(z);
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_MirrorAxes.SetValue(m_MirrorAxes & MirrorSettings.X | m_MirrorAxes & MirrorSettings.Y | (evt.newValue ? MirrorSettings.Z : 0) | m_MirrorAxes & MirrorSettings.Duplicate);
                PreviewActionManager.UpdatePreview();
            });
            root.Add(toggle);

            toggle = new Toggle("Duplicate");
            toggle.tooltip = "If Duplicate is toggled a new object will be instantiated from the selection and mirrored, or if disabled the selection will be moved.";
            toggle.SetValueWithoutNotify(d);
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_MirrorAxes.SetValue(m_MirrorAxes & MirrorSettings.X | m_MirrorAxes & MirrorSettings.Y | m_MirrorAxes & MirrorSettings.Z | (evt.newValue ? MirrorSettings.Duplicate : 0));
                PreviewActionManager.UpdatePreview();
            });
            root.Add(toggle);

            return root;
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Mirror Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Mirror objects on the selected axes.\n\nIf Duplicate is toggled a new object will be instantiated from the selection and mirrored, or if disabled the selection will be moved.", MessageType.Info);

            MirrorSettings scale = m_MirrorAxes;

            bool x = (scale & MirrorSettings.X) != 0;
            bool y = (scale & MirrorSettings.Y) != 0;
            bool z = (scale & MirrorSettings.Z) != 0;
            bool d = (scale & MirrorSettings.Duplicate) != 0;

            EditorGUI.BeginChangeCheck();

            x = EditorGUILayout.Toggle("X", x);
            y = EditorGUILayout.Toggle("Y", y);
            z = EditorGUILayout.Toggle("Z", z);
            d = EditorGUILayout.Toggle("Duplicate", d);

            if (EditorGUI.EndChangeCheck())
                m_MirrorAxes.SetValue((MirrorSettings)
                    (x ? MirrorSettings.X : 0) |
                    (y ? MirrorSettings.Y : 0) |
                    (z ? MirrorSettings.Z : 0) |
                    (d ? MirrorSettings.Duplicate : 0));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Mirror"))
                EditorUtility.ShowNotification(PerformAction().notification);
        }

        protected override ActionResult PerformActionImplementation()
        {
            Vector3 scale = new Vector3(
                (m_MirrorAxes & MirrorSettings.X) > 0 ? -1f : 1f,
                (m_MirrorAxes & MirrorSettings.Y) > 0 ? -1f : 1f,
                (m_MirrorAxes & MirrorSettings.Z) > 0 ? -1f : 1f);

            bool duplicate = (m_MirrorAxes & MirrorSettings.Duplicate) != 0;
            List<GameObject> res = new List<GameObject>();

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
                res.Add(Mirror(pb, scale, duplicate).gameObject);

            MeshSelection.SetSelection(res);
            PreviewActionManager.selectionUpdateDisabled = true;
            ProBuilderEditor.Refresh();

            return res.Count > 0 ?
                new ActionResult(ActionResult.Status.Success, string.Format("Mirror {0} {1}", res.Count, res.Count > 1 ? "Objects" : "Object")) :
                new ActionResult(ActionResult.Status.NoChange, "No Objects Selected");
        }

        /**
         *  \brief Duplicates and mirrors the passed pb_Object.
         *  @param pb The donor pb_Object.
         *  @param axe The axis to mirror the object on.
         *  \returns The newly duplicated pb_Object.
         *  \sa ProBuilder.Axis
         */
        public static ProBuilderMesh Mirror(ProBuilderMesh pb, Vector3 scale, bool duplicate = true)
        {
            ProBuilderMesh mirroredObject;

            if (duplicate)
            {
                mirroredObject = Object.Instantiate(pb.gameObject, pb.transform.parent, false).GetComponent<ProBuilderMesh>();
                mirroredObject.MakeUnique();
                mirroredObject.transform.parent = pb.transform.parent;
                mirroredObject.transform.localRotation = pb.transform.localRotation;
                Undo.RegisterCreatedObjectUndo(mirroredObject.gameObject, "Mirror Object");
            }
            else
            {
                UndoUtility.RecordObject(pb, "Mirror");
                mirroredObject = pb;
            }

            Vector3 lScale = mirroredObject.gameObject.transform.localScale;
            mirroredObject.transform.localScale = scale;

            // if flipping on an odd number of axes, flip winding order
            if ((scale.x * scale.y * scale.z) < 0)
            {
                foreach (var face in mirroredObject.facesInternal)
                    face.Reverse();
            }

            mirroredObject.FreezeScaleTransform();
            mirroredObject.transform.localScale = lScale;

            mirroredObject.ToMesh();
            mirroredObject.Refresh();
            mirroredObject.Optimize();

            return mirroredObject;
        }
    }
}
