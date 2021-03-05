using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class MirrorObjects : MenuAction
    {
        Pref<MirrorSettings> m_MirrorAxes = new Pref<MirrorSettings>("MirrorObjects.mirrorAxes", MirrorSettings.X);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Object_Mirror", IconSkin.Pro); }
        }

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

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Mirror Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Mirror objects on the selected axes.\n\nIf Duplicate is toggled a new object will be instantiated from the selection and mirrored, or if disabled the selection will be moved.", MessageType.Info);

            MirrorSettings scale = m_MirrorAxes;

            bool x = (scale & MirrorSettings.X) != 0 ? true : false;
            bool y = (scale & MirrorSettings.Y) != 0 ? true : false;
            bool z = (scale & MirrorSettings.Z) != 0 ? true : false;
            bool d = (scale & MirrorSettings.Duplicate) != 0 ? true : false;

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

            bool duplicate = (m_MirrorAxes & MirrorSettings.Duplicate) > 0;

            List<GameObject> res  = new List<GameObject>();

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
                res.Add(Mirror(pb, scale, duplicate).gameObject);

            MeshSelection.SetSelection(res);

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
            ProBuilderMesh mirredObject;

            if (duplicate)
            {
                mirredObject = Object.Instantiate(pb.gameObject, pb.transform.parent, false).GetComponent<ProBuilderMesh>();
                mirredObject.MakeUnique();
                mirredObject.transform.parent = pb.transform.parent;
                mirredObject.transform.localRotation = pb.transform.localRotation;
                Undo.RegisterCreatedObjectUndo(mirredObject.gameObject, "Mirror Object");
            }
            else
            {
                UndoUtility.RecordObject(pb, "Mirror");
                mirredObject = pb;
            }

            Vector3 lScale = mirredObject.gameObject.transform.localScale;
            mirredObject.transform.localScale = scale;

            // if flipping on an odd number of axes, flip winding order
            if ((scale.x * scale.y * scale.z) < 0)
            {
                foreach (var face in mirredObject.facesInternal)
                    face.Reverse();
            }

            mirredObject.FreezeScaleTransform();
            mirredObject.transform.localScale = lScale;

            mirredObject.ToMesh();
            mirredObject.Refresh();
            mirredObject.Optimize();

            return mirredObject;
        }
    }
}
