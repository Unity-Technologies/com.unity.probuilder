using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.ProBuilder.Overlays;
using UnityEditor.Actions;
using UnityEditor.ShortcutManagement;

namespace UnityEditor.ProBuilder
{
    static class ProBuilderToolManager
    {
        public static Tool activeTool => Tools.current;
    }

    [Icon("Packages/com.unity.probuilder/Content/Icons/EditableMesh/EditMeshContext.png")]
    [EditorToolContext("ProBuilder", typeof(ProBuilderMesh))]
    class PositionToolContext : EditorToolContext
    {
        internal class ProBuilderShortcutContext : IShortcutContext
        {
            public bool active
                => EditorWindow.focusedWindow is SceneView
                   && ProBuilderEditor.instance != null;
        }

        ProBuilderEditor m_Editor;
        ProBuilderEditor editor => m_Editor ??= new ProBuilderEditor();

        ProBuilderShortcutContext m_ShortcutContext;

        SceneInformationOverlay m_SceneInfoOverlay;

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(ProbuilderMoveTool);
                case Tool.Rotate:
                    return typeof(ProbuilderRotateTool);
                case Tool.Scale:
                    return typeof(ProbuilderScaleTool);
                default:
                    return null;
            }
        }

        static DropdownMenuAction.Status GetStatus(MenuAction action)
        {
            if(action.hidden)
                return DropdownMenuAction.Status.Hidden;
            if (action.enabled)
                return DropdownMenuAction.Status.Normal;
            return DropdownMenuAction.Status.Disabled;
        }

        static readonly HashSet<Type> k_ContextMenuBlacklist = new HashSet<Type>()
        {
            typeof(Actions.OpenSmoothingEditor),
            typeof(Actions.OpenMaterialEditor),
            typeof(Actions.OpenUVEditor),
            typeof(Actions.OpenVertexColorEditor),

            typeof(Actions.ToggleSelectBackFaces),
            typeof(Actions.ToggleHandleOrientation),
            typeof(Actions.ToggleDragRectMode),
            typeof(Actions.ToggleXRay)
        };

        public override void PopulateMenu(DropdownMenu menu)
        {
            var actions = EditorToolbarLoader.GetActions();
            var group = ToolbarGroup.Tool;

            ContextMenuUtility.AddMenuItem(menu, "Edit/Select All","Select/Select All");
            ContextMenuUtility.AddMenuItem(menu, "Edit/Deselect All","Select/Deselect All");
            ContextMenuUtility.AddMenuItem(menu, "Edit/Invert Selection","Select/Invert Selection");
            group = actions[0].group;
            // grouping and filtering is bespoke for demo reasons
            foreach (var action in actions)
            {
                if (k_ContextMenuBlacklist.Contains(action.GetType()))
                    continue;

                if (action.group == ToolbarGroup.Entity || action.group == ToolbarGroup.Object || action.group == ToolbarGroup.Tool)
                    continue;

                if (action.group != group)
                {
                    menu.AppendSeparator();
                    group = action.group;
                }

                var title = action.menuTitle;

                if (GetStatus(action) == DropdownMenuAction.Status.Normal || GetStatus(action) == DropdownMenuAction.Status.Disabled)
                {
                    if (action.hasFileMenuEntry)
                    {
                        string path = EditorToolbarMenuItem.k_MenuPrefix + action.group + "/" + title;
                        ContextMenuUtility.AddMenuItem(menu, path, GetMenuTitle(action, title));
                    }
                    else if (action.optionsEnabled)
                    {
                        title = GetMenuTitle(action, title);
                        menu.AppendAction(title, _ => EditorAction.Start(new MenuActionSettings(action, HasPreview(action))));
                    }
                    else
                        menu.AppendAction(GetMenuTitle(action, title), _ => action.PerformAction());
                }
            }

            var trs = Selection.transforms;
            if (trs.GetComponents<MeshFilter>().Length > trs.GetComponents<ProBuilderMesh>().Length)
                ContextMenuUtility.AddMenuItemsForType(menu, typeof(MeshFilter), targets, "Mesh Filter");
        }

        static bool HasPreview(MenuAction action)
        {
            return !(action is DetachFaces || action is DuplicateFaces);
        }

        string GetMenuTitle(MenuAction action, string title)
        {
            // Geometry and Tool groups are not displayed in the menu
            if (action.group != ToolbarGroup.Geometry && action.group != ToolbarGroup.Tool)
            {
                //STO-3001: For a better UX, Selection group is renamed to Select so that users don't think this is
                //acting on the current selection
                var groupName = action.group == ToolbarGroup.Selection ? "ProBuilder Select" : action.group.ToString();
                title = $"{groupName}/{action.menuTitle}";
            }
            return title;
        }

        public override void OnActivated()
        {
            m_Editor = new ProBuilderEditor();

            ProBuilderSettings.instance.afterSettingsSaved += UpdateSceneInfoOverlay;
            UpdateSceneInfoOverlay();

            ShortcutManager.RegisterContext(m_ShortcutContext ??= new ProBuilderShortcutContext());
        }

        public override void OnWillBeDeactivated()
        {
            m_Editor.Dispose();
            ProBuilderSettings.instance.afterSettingsSaved -= UpdateSceneInfoOverlay;

            if(m_SceneInfoOverlay != null)
                SceneView.RemoveOverlayFromActiveView(m_SceneInfoOverlay);

            ShortcutManager.UnregisterContext(m_ShortcutContext);
        }

        void UpdateSceneInfoOverlay()
        {
            if (ProBuilderEditor.s_ShowSceneInfo)
            {
                if(m_SceneInfoOverlay == null)
                    m_SceneInfoOverlay = new SceneInformationOverlay();

                SceneView.AddOverlayToActiveView(m_SceneInfoOverlay);
            }
            else if(m_SceneInfoOverlay != null)
            {
                SceneView.RemoveOverlayFromActiveView(m_SceneInfoOverlay);
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView view))
                return;
            editor.OnSceneGUI(view);
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;

        [MenuItem("CONTEXT/ProBuilderMesh/Conform Normals", true)]
        static bool ValidateConformObjectNormalsAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Conform Normals", false, 11)]
        static void ConformObjectNormalsAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<ConformObjectNormals>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Export", true)]
        public static bool ValidateExportAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Export", false, 12)]
        public static void ExportAction(MenuCommand command)
        {
            EditorToolbarLoader.GetInstance<Export>().PerformAltAction();
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Triangulate", true)]
        public static bool ValidateTriangulateObjectAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Triangulate", false, 13)]
        public static void TriangulateObjectAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<TriangulateObject>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Center Pivot", true)]
        static bool ValidateCenterPivotAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Center Pivot", false, 14)]
        static void CenterPivotAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<CenterPivot>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Flip Normals", true)]
        static bool ValidateFlipNormalsAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Flip Normals", false, 16)]
        static void FlipNormalsAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<FlipObjectNormals>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Subdivide Object", true)]
        public static bool ValidateSubdivideObjectAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Subdivide Object", false, 15)]
        public static void SubdivideObjectAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<SubdivideObject>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Mirror Objects", true)]
        static bool ValidateMirrorObjectAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Mirror Objects", false, 17)]
        static void MirrorObjectAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<MirrorObjects>(), true));
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Merge Objects", true)]
        static bool ValidateMergeObjectsAction()
        {
            return MeshSelection.selectedObjectCount > 1 && MeshSelection.activeMesh != null;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Merge Objects", false, 18)]
        static void MergeObjectsAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<MergeObjects>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Freeze Transform", true)]
        static bool ValidateFreezeTransformAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Freeze Transform", false, 19)]
        static void FreezeTransformAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<FreezeTransform>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Set Trigger", true)]
        static bool ValidateSetTriggerAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Set Trigger", false, 20)]
        static void SetTriggerAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<SetTrigger>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Set Collider", true)]
        static bool ValidateSetColliderAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Set Collider", false, 21)]
        static void SetColliderAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<SetCollider>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
    }

    class TextureToolContext : EditorToolContext
    {
        ProBuilderEditor m_Editor;

        TextureToolContext() { }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(TextureMoveTool);
                case Tool.Rotate:
                    return typeof(TextureRotateTool);
                case Tool.Scale:
                    return typeof(TextureScaleTool);
                default:
                    return null;
            }
        }

        public override void OnActivated()
        {
            m_Editor = new ProBuilderEditor();
        }

        public override void OnWillBeDeactivated()
        {
            m_Editor.Dispose();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView view))
                return;
            m_Editor.OnSceneGUI(view);
        }

    }
}
