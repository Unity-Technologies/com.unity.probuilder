using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEditor.Actions;
using UnityEngine.UIElements;

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
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;

            OnObjectSelectionChanged(); // invoke once as we might already have a selection in Hierarchy
        }

        private void OnObjectSelectionChanged()
        {
            // can't just check if any MeshFilter is present because we need to know whether or not it's already a
            // probuilder mesh
            int meshCount = Selection.transforms.SelectMany(x => x.GetComponentsInChildren<MeshFilter>()).Count();
            m_Enabled = meshCount > 0 && meshCount != MeshSelection.selectedObjectCount;
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }
        public override string iconPath => "Toolbar/Object_ProBuilderize";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

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

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;

        [MenuItem("CONTEXT/MeshFilter/ProBuilderize", true, 11)]
        static bool ProBuilderizeMeshAction_Validate(MenuCommand command)
        {
            return !MenuActionSettings.IsCurrentAction(new ProBuilderize());
        }

        [MenuItem("CONTEXT/MeshFilter/ProBuilderize", false, 11)]
        static void ProBuilderizeMeshAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                var filter = (MeshFilter)command.context;
                //Check if we are not trying to Probuilderize a PB mesh
                if (filter.GetComponent<ProBuilderMesh>() != null)
                    return;

                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorAction.Start(new MenuActionSettings(new ProBuilderize()));
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var quadsToggle = new Toggle(m_QuadsTooltip.text);
            quadsToggle.tooltip = m_QuadsTooltip.tooltip;
            quadsToggle.SetValueWithoutNotify(m_Quads);
            root.Add(quadsToggle);

            var smoothingToggle = new Toggle(m_SmoothingTooltip.text);
            smoothingToggle.tooltip = m_SmoothingTooltip.tooltip;
            smoothingToggle.SetValueWithoutNotify(m_Smoothing);
            root.Add(smoothingToggle);

            var line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            root.Add(line);

            var smoothingSlider = new Slider(m_SmoothingThresholdTooltip.text, 0.0001f, 45f);
            smoothingSlider.tooltip = m_SmoothingThresholdTooltip.tooltip;
            smoothingSlider.SetValueWithoutNotify(m_SmoothingAngle);
            smoothingSlider.style.flexGrow = 1;
            smoothingSlider.SetEnabled(m_Smoothing);
            line.Add(smoothingSlider);

            var smoothingSliderValue = new FloatField();
            smoothingSliderValue.SetValueWithoutNotify(m_SmoothingAngle);
            smoothingSliderValue.isDelayed = true;
            smoothingSliderValue.style.width = 50;
            line.Add(smoothingSliderValue);

            quadsToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                m_Quads.SetValue(evt.newValue);
            });
            smoothingToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                m_Smoothing.SetValue(evt.newValue);
                smoothingSlider.SetEnabled(m_Smoothing);
            });
            smoothingSlider.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                m_SmoothingAngle.SetValue(evt.newValue);
                smoothingSliderValue.SetValueWithoutNotify(m_SmoothingAngle);
            });
            smoothingSliderValue.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                m_SmoothingAngle.SetValue(evt.newValue);
                smoothingSlider.SetValueWithoutNotify(m_SmoothingAngle);
            });

            return root;
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
                EditorUtility.ShowNotification(PerformAction().notification);

            GUI.enabled = true;
        }

        protected override ActionResult PerformActionImplementation()
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

            return DoProBuilderize(all, settings);
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

            // Return immediately from the action so that the GUI can resolve. Displaying a progress bar interrupts the
            // event loop causing a layoutting error.
            EditorApplication.delayCall += () =>
            {
                foreach (var mf in selected)
                {
                    if (mf.sharedMesh == null)
                        continue;

                    GameObject go = mf.gameObject;
                    Mesh sourceMesh = mf.sharedMesh;
                    Material[] sourceMaterials = go.GetComponent<MeshRenderer>()?.sharedMaterials;

                    try
                    {
                        var destination = Undo.AddComponent<ProBuilderMesh>(go);
                        var meshImporter = new MeshImporter(sourceMesh, sourceMaterials, destination);
                        meshImporter.Import(settings);

                        destination.Rebuild();
                        destination.Optimize();

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
            };

            if (i < 1)
                return new ActionResult(ActionResult.Status.Canceled, "Nothing Selected");
            return new ActionResult(ActionResult.Status.Success, "ProBuilderize " + i + (i > 1 ? " Objects" : " Object").ToString());
        }
    }
}
