using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Stl;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;
#if UNITY_2023_2_OR_NEWER
using UnityEditor.Actions;
using UnityEngine.UIElements;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    sealed class Export : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_Export"); } }
        public override TooltipContent tooltip { get { return m_Tooltip; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        GUIContent gc_ExportFormat = new GUIContent("Export Format", "The type of file to export the current selection as.");
        GUIContent gc_ExportRecursive = new GUIContent("Include Children", "Should the exporter include children of the current selection when searching for meshes to export?");
        GUIContent gc_ObjExportRightHanded = new GUIContent("Right Handed", "Unity coordinate space is left handed, where most other major 3D modeling softwares are right handed. Usually this option should be left enabled.");
        GUIContent gc_ExportAsGroup = new GUIContent("Export As Group", "If enabled all selected meshes will be combined to a single model. If not, each mesh will be exported individually.");
        GUIContent gc_ObjApplyTransform = new GUIContent("Apply Transforms", "If enabled each mesh will have it's Transform applied prior to export. This is useful when you want to retain the correct placement of objects when re-importing to Unity (just set the imported mesh to { 0, 0, 0 }). If not enabled meshes are exported in local space.");
        GUIContent gc_ObjExportCopyTextures = new GUIContent("Copy Textures", "With Copy Textures enabled the exporter will copy material textures to the destination directory. If false the material library will point to the texture path within the Unity project. If you're exporting models with the intention of editing in an external 3D modeler then re-importing, disable this option to avoid duplicate textures in your project.");
        GUIContent gc_ObjExportVertexColors = new GUIContent("Vertex Colors", "Some 3D modeling applications will read and write vertex colors as an unofficial extension to the OBJ format.\n\nWarning! Enabling this can break compatibility with some other 3D modeling applications.");
        GUIContent gc_ObjTextureOffsetScale = new GUIContent("Texture Offset, Scale", "Write texture map offset and scale to the material library. Not all 3D modeling applications support this specificiation, and some will fail to load materials that define these values.");
        GUIContent gc_ObjQuads = new GUIContent("Export Quads", "Where possible, faces will be exported as quads instead of triangles. Note that this can result in a larger exported mesh (ProBuilder will not merge shared vertices with this option enabled).");
        GUIContent gc_ExportAssetInPlace = new GUIContent("Replace Source", "Remove the ProBuilder component and replace the MeshFilter mesh with the exported asset.");

        Pref<ExportFormat> m_ExportFormat = new Pref<ExportFormat>("export.format", k_DefaultFormat);
        Pref<bool> m_ExportRecursive = new Pref<bool>("export.exportRecursive", false);
        Pref<bool> m_ExportAsGroup = new Pref<bool>("export.exportAsGroup", false);

        // obj specific
        Pref<bool> m_ObjExportRightHanded = new Pref<bool>("export.objExportRightHanded", true);
        Pref<bool> m_ObjExportCopyTextures = new Pref<bool>("export.objExportCopyTextures", true);
        Pref<bool> m_ObjApplyTransform = new Pref<bool>("export.objApplyTransform", false);
        Pref<bool> m_ObjExportVertexColors = new Pref<bool>("export.objExportVertexColors", false);
        Pref<bool> m_ObjTextureOffsetScale = new Pref<bool>("export.objTextureOffsetScale", false);
        Pref<bool> m_ObjQuads = new Pref<bool>("export.objQuads", true);

        // stl specific
        Pref<FileType> m_StlExportFormat = new Pref<FileType>("export.stlExportFormat", FileType.Ascii);

        // ply specific
        Pref<bool> m_PlyExportIsRightHanded = new Pref<bool>("export.plyExportIsRightHanded", true);
        Pref<bool> m_PlyApplyTransform = new Pref<bool>("export.plyApplyTransform", true);
        Pref<bool> m_PlyQuads = new Pref<bool>("export.plyQuads", true);
        Pref<bool> m_PlyNGons = new Pref<bool>("export.plyNGons", false);

        public enum ExportFormat
        {
            Obj,
            Stl,
            Ply,
            Asset,
            Prefab
        }

        const ExportFormat k_DefaultFormat = ExportFormat.Obj;

        static readonly TooltipContent m_Tooltip = new TooltipContent
            (
                "Export",
                "Export the selected ProBuilder objects as a model file."
            );

        public override bool hidden
        {
            get { return false; }
        }

        public override bool enabled
        {
            get { return MeshSelection.selectedObjectCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Export", true)]
        static bool ValidateExportAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Export", false, 12)]
        static void MergeObjectsAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<Export>(), false));
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        VisualElement m_ObjOptions;
        VisualElement m_StlOptions;
        VisualElement m_PlyOptions;
        VisualElement m_AssetOptions;
        Toggle m_ExportAsGroupElement;

        protected internal override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var exportFormat = new EnumField(gc_ExportFormat.text, m_ExportFormat);
            exportFormat.tooltip = gc_ExportFormat.tooltip;
            root.Add(exportFormat);

            var toggle = new Toggle(gc_ExportRecursive.text);
            toggle.tooltip = gc_ExportRecursive.tooltip;
            toggle.SetValueWithoutNotify(m_ExportRecursive);
            root.Add(toggle);

            m_ExportAsGroupElement = new Toggle(gc_ExportAsGroup.text);
            m_ExportAsGroupElement.tooltip = gc_ExportAsGroup.tooltip;
            var displayed = m_ExportFormat != ExportFormat.Asset
                && m_ExportFormat != ExportFormat.Prefab
                && m_ExportFormat != ExportFormat.Stl;
            m_ExportAsGroupElement.style.display = displayed ? DisplayStyle.Flex : DisplayStyle.None;
            m_ExportAsGroupElement.SetValueWithoutNotify(m_ExportAsGroup);
            root.Add(m_ExportAsGroupElement);

            m_ObjOptions = new VisualElement();
            root.Add(m_ObjOptions);
            m_StlOptions = new VisualElement();
            root.Add(m_StlOptions);
            m_PlyOptions = new VisualElement();
            root.Add(m_PlyOptions);
            m_AssetOptions = new VisualElement();
            root.Add(m_AssetOptions);

            exportFormat.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out ExportFormat newValue);
                if (m_ExportFormat.value == newValue)
                    return;

                m_ExportFormat.value = newValue;
                if (m_ExportFormat.value == ExportFormat.Asset || m_ExportFormat.value == ExportFormat.Prefab)
                {
                    var opt = ExportAsset.s_ExportAssetOptions.value;
                    opt.makePrefab = m_ExportFormat.value == ExportFormat.Prefab;
                    ExportAsset.s_ExportAssetOptions.SetValue(opt);
                }

                var d = m_ExportFormat == ExportFormat.Obj || m_ExportFormat == ExportFormat.Ply;
                m_ExportAsGroupElement.style.display = d ? DisplayStyle.Flex : DisplayStyle.None;

                UpdateOptions();
                ProBuilderSettings.Save();
            });
            toggle.RegisterValueChangedCallback(evt =>
            {
                m_ExportRecursive.value = evt.newValue;
                ProBuilderSettings.Save();
            });
            m_ExportAsGroupElement.RegisterValueChangedCallback(evt =>
            {
                m_ExportAsGroup.value = evt.newValue;
                ProBuilderSettings.Save();
            });

            DoObjOptions();
            DoStlOptions();
            DoPlyOptions();
            DoAssetOptions();
            UpdateOptions();

            return root;
        }

        void UpdateOptions()
        {
            m_ObjOptions.style.display = m_ExportFormat == ExportFormat.Obj ? DisplayStyle.Flex : DisplayStyle.None;
            m_StlOptions.style.display = m_ExportFormat == ExportFormat.Stl ? DisplayStyle.Flex : DisplayStyle.None;
            m_PlyOptions.style.display = m_ExportFormat == ExportFormat.Ply ? DisplayStyle.Flex : DisplayStyle.None;
            m_AssetOptions.style.display = (m_ExportFormat == ExportFormat.Asset || m_ExportFormat == ExportFormat.Prefab) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void DoObjOptions()
        {
            var root = m_ObjOptions;
            var applyTrsToggle = new Toggle(gc_ObjApplyTransform.text);
            applyTrsToggle.tooltip = gc_ObjApplyTransform.tooltip;
            applyTrsToggle.SetValueWithoutNotify(m_ExportAsGroup ? true : m_ObjApplyTransform);
            applyTrsToggle.SetEnabled(!m_ExportAsGroup);
            applyTrsToggle.RegisterValueChangedCallback(evt =>
            {
                if (m_ObjApplyTransform.value != evt.newValue)
                {
                    m_ObjApplyTransform.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            root.Add(applyTrsToggle);
            m_ExportAsGroupElement.RegisterValueChangedCallback(evt =>
            {
                applyTrsToggle.SetValueWithoutNotify(m_ExportAsGroup ? true : m_ObjApplyTransform);
                applyTrsToggle.SetEnabled(!m_ExportAsGroup);
            });

            var toggle = new Toggle(gc_ObjExportRightHanded.text);
            toggle.tooltip = gc_ObjExportRightHanded.tooltip;
            toggle.SetValueWithoutNotify(m_ObjExportRightHanded);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_ObjExportRightHanded.value != evt.newValue)
                {
                    m_ObjExportRightHanded.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            root.Add(toggle);

            toggle = new Toggle(gc_ObjExportCopyTextures.text);
            toggle.tooltip = gc_ObjExportCopyTextures.tooltip;
            toggle.SetValueWithoutNotify(m_ObjExportCopyTextures);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_ObjExportCopyTextures.value != evt.newValue)
                {
                    m_ObjExportCopyTextures.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            root.Add(toggle);

            toggle = new Toggle(gc_ObjExportVertexColors.text);
            toggle.tooltip = gc_ObjExportVertexColors.tooltip;
            toggle.SetValueWithoutNotify(m_ObjExportVertexColors);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_ObjExportVertexColors.value != evt.newValue)
                {
                    m_ObjExportVertexColors.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            root.Add(toggle);

            toggle = new Toggle(gc_ObjTextureOffsetScale.text);
            toggle.tooltip = gc_ObjTextureOffsetScale.tooltip;
            toggle.SetValueWithoutNotify(m_ObjTextureOffsetScale);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_ObjTextureOffsetScale.value != evt.newValue)
                {
                    m_ObjTextureOffsetScale.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            root.Add(toggle);

            toggle = new Toggle(gc_ObjQuads.text);
            toggle.tooltip = gc_ObjQuads.tooltip;
            toggle.SetValueWithoutNotify(m_ObjQuads);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_ObjQuads.value != evt.newValue)
                {
                    m_ObjQuads.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            root.Add(toggle);
        }

        void DoStlOptions()
        {
            var exportFormat = new EnumField("Stl Format", m_StlExportFormat);
            m_StlOptions.Add(exportFormat);
            exportFormat.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Enum.TryParse(evt.newValue, out FileType newValue);
                if (m_StlExportFormat.value == newValue)
                    return;

                m_StlExportFormat.value = newValue;
                ProBuilderSettings.Save();
            });
        }

        void DoPlyOptions()
        {
            var applyTrsToggle = new Toggle(gc_ObjApplyTransform.text);
            applyTrsToggle.tooltip = gc_ObjApplyTransform.tooltip;
            applyTrsToggle.SetValueWithoutNotify(m_ExportAsGroup ? true : m_PlyApplyTransform);
            applyTrsToggle.SetEnabled(!m_ExportAsGroup);
            applyTrsToggle.RegisterValueChangedCallback(evt =>
            {
                if (m_PlyApplyTransform.value != evt.newValue)
                {
                    m_PlyApplyTransform.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            m_PlyOptions.Add(applyTrsToggle);
            m_ExportAsGroupElement.RegisterValueChangedCallback(evt =>
            {
                applyTrsToggle.SetValueWithoutNotify(m_ExportAsGroup ? true : m_PlyApplyTransform);
                applyTrsToggle.SetEnabled(!m_ExportAsGroup);
            });

            var toggle = new Toggle("Right Handed");
            toggle.SetValueWithoutNotify(m_PlyExportIsRightHanded);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_PlyExportIsRightHanded.value != evt.newValue)
                {
                    m_PlyExportIsRightHanded.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            m_PlyOptions.Add(toggle);

            toggle = new Toggle("Quads");
            toggle.SetValueWithoutNotify(m_PlyQuads);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (m_PlyQuads.value != evt.newValue)
                {
                    m_PlyQuads.value = evt.newValue;
                    ProBuilderSettings.Save();
                }
            });
            m_PlyOptions.Add(toggle);
        }

        void DoAssetOptions()
        {
            var options = ExportAsset.s_ExportAssetOptions.value;

            var toggle = new Toggle(gc_ExportAssetInPlace.text);
            toggle.tooltip = gc_ExportAssetInPlace.tooltip;
            toggle.SetValueWithoutNotify(options.replaceOriginal);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (options.replaceOriginal != evt.newValue)
                {
                    options.replaceOriginal = evt.newValue;
                    ExportAsset.s_ExportAssetOptions.SetValue(options);
                    ProBuilderSettings.Save();
                }
            });
            m_AssetOptions.Add(toggle);
        }
#endif
        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Export Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            m_ExportFormat.value = (ExportFormat)EditorGUILayout.EnumPopup(gc_ExportFormat, m_ExportFormat);
            if (EditorGUI.EndChangeCheck())
            {
                if (m_ExportFormat.value == ExportFormat.Asset || m_ExportFormat.value == ExportFormat.Prefab)
                {
                    var opt = ExportAsset.s_ExportAssetOptions.value;
                    opt.makePrefab = m_ExportFormat.value == ExportFormat.Prefab;
                    ExportAsset.s_ExportAssetOptions.SetValue(opt);
                }
            }

            m_ExportRecursive.value = EditorGUILayout.Toggle(gc_ExportRecursive, m_ExportRecursive);

            if (m_ExportFormat != ExportFormat.Asset
                && m_ExportFormat != ExportFormat.Prefab
                && m_ExportFormat != ExportFormat.Stl)
            {
                m_ExportAsGroup.value = EditorGUILayout.Toggle(gc_ExportAsGroup, m_ExportAsGroup);
            }

            if (m_ExportFormat == ExportFormat.Obj)
                ObjExportOptions();
            else if (m_ExportFormat == ExportFormat.Stl)
                StlExportOptions();
            else if (m_ExportFormat == ExportFormat.Ply)
                PlyExportOptions();
            else if (m_ExportFormat == ExportFormat.Asset || m_ExportFormat == ExportFormat.Prefab)
                AssetExportOptions();

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Export"))
                EditorUtility.ShowNotification(PerformAction().notification);
        }

        void ObjExportOptions()
        {
            EditorGUI.BeginDisabledGroup(m_ExportAsGroup);

            if (m_ExportAsGroup)
                EditorGUILayout.Toggle("Apply Transforms", true);
            else
                m_ObjApplyTransform.value = EditorGUILayout.Toggle(gc_ObjApplyTransform, m_ObjApplyTransform);

            EditorGUI.EndDisabledGroup();

            m_ObjExportRightHanded.value = EditorGUILayout.Toggle(gc_ObjExportRightHanded, m_ObjExportRightHanded);
            m_ObjExportCopyTextures.value = EditorGUILayout.Toggle(gc_ObjExportCopyTextures, m_ObjExportCopyTextures);
            m_ObjExportVertexColors.value = EditorGUILayout.Toggle(gc_ObjExportVertexColors, m_ObjExportVertexColors);
            m_ObjTextureOffsetScale.value = EditorGUILayout.Toggle(gc_ObjTextureOffsetScale, m_ObjTextureOffsetScale);
            m_ObjQuads.value = EditorGUILayout.Toggle(gc_ObjQuads, m_ObjQuads);
        }

        void StlExportOptions()
        {
            m_StlExportFormat.value = (UnityEngine.ProBuilder.Stl.FileType)EditorGUILayout.EnumPopup("Stl Format", m_StlExportFormat);
        }

        void PlyExportOptions()
        {
            EditorGUI.BeginDisabledGroup(m_ExportAsGroup);

            if (m_ExportAsGroup)
                EditorGUILayout.Toggle("Apply Transforms", true);
            else
                m_PlyApplyTransform.value = EditorGUILayout.Toggle(gc_ObjApplyTransform, m_PlyApplyTransform);
            EditorGUI.EndDisabledGroup();

            m_PlyExportIsRightHanded.value = EditorGUILayout.Toggle("Right Handed", m_PlyExportIsRightHanded);
            m_PlyQuads.value = EditorGUILayout.Toggle("Quads", m_PlyQuads);
        }

        void AssetExportOptions()
        {
            var options = ExportAsset.s_ExportAssetOptions.value;

            EditorGUI.BeginChangeCheck();
            options.replaceOriginal = EditorGUILayout.Toggle(gc_ExportAssetInPlace, options.replaceOriginal);
            if(EditorGUI.EndChangeCheck())
                ExportAsset.s_ExportAssetOptions.SetValue(options);
        }

        protected override ActionResult PerformActionImplementation()
        {
            string res = null;

            List<ProBuilderMesh> meshes = m_ExportRecursive ? MeshSelection.deep.ToList() : MeshSelection.topInternal;

            if (meshes == null || meshes.Count == 0)
                return new ActionResult(ActionResult.Status.Canceled, "No ProBuilder Mesh");

            if (m_ExportFormat == ExportFormat.Obj)
            {
                res = ExportObj.ExportWithFileDialog(meshes,
                        m_ExportAsGroup,
                        m_ObjQuads,
                        new ObjOptions() {
                    handedness = m_ObjExportRightHanded ? ObjOptions.Handedness.Right : ObjOptions.Handedness.Left,
                    copyTextures = m_ObjExportCopyTextures,
                    applyTransforms = m_ExportAsGroup || m_ObjApplyTransform.value,
                    vertexColors = m_ObjExportVertexColors,
                    textureOffsetScale = m_ObjTextureOffsetScale
                });
            }
            else if (m_ExportFormat == ExportFormat.Stl)
            {
                res = ExportStlAscii.ExportWithFileDialog(meshes.Select(x => x.gameObject).ToArray(), m_StlExportFormat);
            }
            else if (m_ExportFormat == ExportFormat.Ply)
            {
                res = ExportPly.ExportWithFileDialog(meshes, m_ExportAsGroup, new PlyOptions() {
                    isRightHanded = m_PlyExportIsRightHanded,
                    applyTransforms = m_PlyApplyTransform,
                    quads = m_PlyQuads,
                    ngons = m_PlyNGons
                });
            }
            else if (m_ExportFormat == ExportFormat.Asset || m_ExportFormat == ExportFormat.Prefab)
            {
                res = ExportAsset.ExportWithFileDialog(meshes, ExportAsset.s_ExportAssetOptions.value);
            }

            if (string.IsNullOrEmpty(res))
                return new ActionResult(ActionResult.Status.Canceled, "Canceled");

            PingExportedModel(res);

            return new ActionResult(ActionResult.Status.Success, "Exported " + m_ExportFormat.value);
        }

        internal static void PingExportedModel(string path)
        {
            var local = path.Replace("\\", "/");
            var dataPath = Application.dataPath.Replace("\\", "/");

            if (local.Contains(dataPath))
            {
                AssetDatabase.Refresh();
                var assetPath = "Assets" + local.Replace(dataPath, "");
                var o = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (o != null)
                    EditorGUIUtility.PingObject(o);
            }
        }
    }
}
