using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder.Stl;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class Export : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_Export", IconSkin.Pro); } }
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

        Pref<ExportFormat> m_ExportFormat = new Pref<ExportFormat>("export.format", k_DefaultFormat);

        Pref<bool> m_ExportRecursive = new Pref<bool>("export.exportRecursive", false);
        Pref<bool> m_ExportAsGroup = new Pref<bool>("export.exportAsGroup", true);

        // obj specific
        Pref<bool> m_ObjExportRightHanded = new Pref<bool>("export.objExportRightHanded", true);
        Pref<bool> m_ObjExportCopyTextures = new Pref<bool>("export.objExportCopyTextures", true);
        Pref<bool> m_ObjApplyTransform = new Pref<bool>("export.objApplyTransform", true);
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
            Asset
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
            get { return Selection.gameObjects != null && Selection.gameObjects.Length > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Export Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_ExportFormat.value = (ExportFormat)EditorGUILayout.EnumPopup(gc_ExportFormat, m_ExportFormat);

            m_ExportRecursive.value = EditorGUILayout.Toggle(gc_ExportRecursive, m_ExportRecursive);

            if (m_ExportFormat != ExportFormat.Asset &&
                m_ExportFormat != ExportFormat.Stl)
            {
                m_ExportAsGroup.value = EditorGUILayout.Toggle(gc_ExportAsGroup, m_ExportAsGroup);
            }

            if (m_ExportFormat == ExportFormat.Obj)
                ObjExportOptions();
            else if (m_ExportFormat == ExportFormat.Stl)
                StlExportOptions();
            else if (m_ExportFormat == ExportFormat.Ply)
                PlyExportOptions();

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Export"))
                DoAction();
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

        public override ActionResult DoAction()
        {
            string res = null;

            IEnumerable<ProBuilderMesh> meshes = m_ExportRecursive ? MeshSelection.deep : MeshSelection.topInternal;

            if (meshes == null || !meshes.Any())
            {
                return new ActionResult(ActionResult.Status.Canceled, "No Meshes Selected");
            }
            else if (m_ExportFormat == ExportFormat.Obj)
            {
                res = ExportObj.ExportWithFileDialog(meshes,
                        m_ExportAsGroup,
                        m_ObjQuads,
                        new ObjOptions() {
                    handedness = m_ObjExportRightHanded ? ObjOptions.Handedness.Right : ObjOptions.Handedness.Left,
                    copyTextures = m_ObjExportCopyTextures,
                    applyTransforms = m_ExportAsGroup || m_ObjApplyTransform,
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
            else if (m_ExportFormat == ExportFormat.Asset)
            {
                res = ExportAsset.ExportWithFileDialog(meshes);
            }

            if (string.IsNullOrEmpty(res))
                return new ActionResult(ActionResult.Status.Canceled, "User Canceled");

            PingExportedModel(res);

            return new ActionResult(ActionResult.Status.Success, "Export " + m_ExportFormat);
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
