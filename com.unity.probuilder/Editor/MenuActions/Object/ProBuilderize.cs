using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ProBuilderize : MenuAction
    {
        bool m_Enabled;
        Pref<bool> m_Quads = new Pref<bool>("meshImporter.quads", true);
        Pref<bool> m_Smoothing = new Pref<bool>("meshImporter.smoothing", true);
        Pref<float> m_SmoothingAngle = new Pref<float>("meshImporter.smoothingAngle", 1f);

        public ProBuilderize()
        {
            MeshSelection.objectSelectionChanged += () =>
                {
                    // can't just check if any MeshFilter is present because we need to know whether or not it's already a
                    // probuilder mesh
                    int meshCount = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Count();
                    m_Enabled = meshCount > 0 && meshCount != MeshSelection.selectedObjectCount;
                };
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Object_ProBuilderize", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        GUIContent m_QuadsTooltip = new GUIContent("Import Quads", "Create ProBuilder mesh using quads where " +
                "possible instead of triangles.");
        GUIContent m_SmoothingTooltip = new GUIContent("Import Smoothing", "Import smoothing groups by " +
                "testing adjacent faces against an angle threshold.");
        GUIContent m_SmoothingThresholdTooltip = new GUIContent("Smoothing Threshold", "When importing " +
                "smoothing groups any adjacent faces with an adjoining angle difference of less than this value will be " +
                "grouped together in a smoothing group.");

        private static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "ProBuilderize",
                @"Creates ProBuilder-modifiable objects from meshes."
            );

        public override bool enabled
        {
            get { return base.enabled && m_Enabled; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("ProBuilderize Options", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("When Preserve Faces is enabled ProBuilder will try to group adjacent triangles into faces.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_Quads.value = EditorGUILayout.Toggle(m_QuadsTooltip, m_Quads);
            m_Smoothing.value = EditorGUILayout.Toggle(m_SmoothingTooltip, m_Smoothing);
            GUI.enabled = m_Smoothing;
            EditorGUILayout.PrefixLabel(m_SmoothingThresholdTooltip);
            m_SmoothingAngle.value = EditorGUILayout.Slider(m_SmoothingAngle, 0.0001f, 45f);

            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            GUI.enabled = enabled;

            if (GUILayout.Button("ProBuilderize"))
                EditorUtility.ShowNotification(DoAction().notification);

            GUI.enabled = true;
        }

        public override ActionResult DoAction()
        {
            IEnumerable<MeshFilter> top = Selection.transforms.Select(x => x.GetComponent<MeshFilter>()).Where(y => y != null);
            IEnumerable<MeshFilter> all = Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Where(x => x != null);

            MeshImportSettings settings = new MeshImportSettings()
            {
                quads = m_Quads,
                smoothing = m_Smoothing,
                smoothingAngle = m_SmoothingAngle
            };

            if (top.Count() != all.Count())
            {
                int result = UnityEditor.EditorUtility.DisplayDialogComplex("ProBuilderize Selection",
                        "ProBuilderize children of selection?",
                        "Yes",
                        "No",
                        "Cancel");

                if (result == 0)
                    return DoProBuilderize(all, settings);
                else if (result == 1)
                    return DoProBuilderize(top, settings);
                else
                    return ActionResult.UserCanceled;
            }
            else
            {
                return DoProBuilderize(all, settings);
            }
        }

        [System.Obsolete("Please use DoProBuilderize(IEnumerable<MeshFilter>, pb_MeshImporter.Settings")]
        public static ActionResult DoProBuilderize(
            IEnumerable<MeshFilter> selected,
            bool preserveFaces)
        {
            return DoProBuilderize(selected, new MeshImportSettings()
            {
                quads = preserveFaces,
                smoothing = false,
                smoothingAngle = 1f
            });
        }

        /// <summary>
        /// Adds pb_Object component without duplicating the objcet. Is undo-able.
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static ActionResult DoProBuilderize(
            IEnumerable<MeshFilter> selected,
            MeshImportSettings settings)
        {
            int i = 0;
            float count = selected.Count();

            foreach (var mf in selected)
            {
                if (mf.sharedMesh == null)
                    continue;

                GameObject go = mf.gameObject;
                Mesh originalMesh = mf.sharedMesh;

                try
                {
                    ProBuilderMesh pb = Undo.AddComponent<ProBuilderMesh>(go);

                    MeshImporter meshImporter = new MeshImporter(pb);
                    meshImporter.Import(go, settings);

                    // if this was previously a pb_Object, or similarly any other instance asset, destroy it.
                    // if it is backed by saved asset, leave the mesh asset alone but assign a new mesh to the
                    // renderer so that we don't modify the asset.
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(originalMesh)))
                        Undo.DestroyObjectImmediate(originalMesh);
                    else
                        go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

                    pb.ToMesh();
                    pb.Refresh();
                    pb.Optimize();

                    i++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("Failed ProBuilderizing: " + go.name + "\n" + e.ToString());
                }

                UnityEditor.EditorUtility.DisplayProgressBar("ProBuilderizing", mf.gameObject.name, i / count);
            }

            UnityEditor.EditorUtility.ClearProgressBar();
            MeshSelection.OnObjectSelectionChanged();
            ProBuilderEditor.Refresh();

            if (i < 1)
                return new ActionResult(ActionResult.Status.Canceled, "Nothing Selected");
            else
                return new ActionResult(ActionResult.Status.Success, "ProBuilderize " + i + (i > 1 ? " Objects" : " Object").ToString());
        }
    }
}
