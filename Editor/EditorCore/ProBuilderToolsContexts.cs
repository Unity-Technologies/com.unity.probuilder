using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.UIElements;
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
        ProBuilderEditor m_Editor;
        ProBuilderEditor editor => m_Editor ??= new ProBuilderEditor();

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
            typeof(Actions.ToggleDragSelectionMode),
            typeof(Actions.ToggleDragRectMode),
            typeof(Actions.ToggleXRay)
        };

        static string BuildMenuTitle()
        {
            var title = "ProBuilder";
            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Vertex:
                    title = MeshSelection.selectedSharedVertexCount.ToString();
                    title += MeshSelection.selectedSharedVertexCount == 1 ? " Vertex" : " Vertices";
                    break;
                case SelectMode.Edge:
                    title = MeshSelection.selectedEdgeCount.ToString();
                    title += MeshSelection.selectedEdgeCount == 1 ? " Edge" : " Edges";
                    break;
                case SelectMode.Face:
                    title = MeshSelection.selectedFaceCount.ToString();
                    title += MeshSelection.selectedFaceCount == 1 ? " Face" : " Faces";
                    break;
            }

            return title;
        }


        public override void PopulateMenu(DropdownMenu menu)
        {
            menu.SetDescriptor(new DropdownMenuDescriptor()
                {
                    title = BuildMenuTitle()
                }
            );

            //Headers area is for ProBuilder modes
            menu.AppendHeaderAction(UI.EditorGUIUtility.Styles.VertexIcon,
                x => { ProBuilderEditor.selectMode = SelectMode.Vertex; },
                x => ProBuilderEditor.selectMode == SelectMode.Vertex
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
            menu.AppendHeaderAction(UI.EditorGUIUtility.Styles.EdgeIcon,
                x => { ProBuilderEditor.selectMode = SelectMode.Edge; },
                x => ProBuilderEditor.selectMode == SelectMode.Edge
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
            menu.AppendHeaderAction(UI.EditorGUIUtility.Styles.FaceIcon,
                x => { ProBuilderEditor.selectMode = SelectMode.Face; },
                x => ProBuilderEditor.selectMode == SelectMode.Face
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);

            var actions = EditorToolbarLoader.GetActions();
            var group = ToolbarGroup.Tool;

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

                if (action.hasFileMenuEntry)
                {
                    string path = EditorToolbarMenuItem.k_MenuPrefix+action.group+"/"+title;
                    if (GetStatus(action) == DropdownMenuAction.Status.Normal || GetStatus(action) == DropdownMenuAction.Status.Disabled)
                    {
                        ContextMenuUtility.AddMenuItem(menu, path, GetMenuTitle(action, title));
                    }
                }else if (action.optionsEnabled)
                {
                    title = GetMenuTitle(action, title);
                    if(HasPreview(action))
                        menu.AppendAction(title, _ => EditorAction.Start(new MenuActionSettingsWithPreview(action)), GetStatus(action), action.icon);
                    else
                        menu.AppendAction(title, _ => EditorAction.Start(new MenuActionSettings(action)), GetStatus(action), action.icon);
                }
                else
                    menu.AppendAction(GetMenuTitle(action, title), _ => action.PerformAction(), GetStatus(action), action.icon);
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
                var groupName = action.group == ToolbarGroup.Selection ? "Select" : action.group.ToString();
                title = $"{groupName}/{action.menuTitle}";
            }
            return title;
        }

        public override void OnActivated()
        {
            if(m_Editor == null)
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

        [MenuItem("CONTEXT/ProBuilderMesh/Conform Normals", false, 11, "Conform object normals","Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_ConformNormals.png")]
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

        [MenuItem("CONTEXT/ProBuilderMesh/Export", false, 12, "Export ProBuilder mesh to another format", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_Export.png")]
        public static void ExportAction(MenuCommand command)
        {
            EditorToolbarLoader.GetInstance<Export>().PerformAltAction();
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Triangulate", true)]
        public static bool ValidateTriangulateObjectAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Triangulate", false, 13,  "Triangulate ProBuilder mesh", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_Triangulate.png")]
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

        [MenuItem("CONTEXT/ProBuilderMesh/Center Pivot", false, 14, "Center object pivot", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Pivot_CenterOnObject.png")]
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

        [MenuItem("CONTEXT/ProBuilderMesh/Flip Normals", false, 16, "Invert object normals", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_FlipNormals.png")]
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

        [MenuItem("CONTEXT/ProBuilderMesh/Subdivide Object", false, 15, "Subdivide ProBuilder mesh", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_Subdivide.png")]
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

        [MenuItem("CONTEXT/ProBuilderMesh/Mirror Objects", false, 17, "Mirror object faces", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_Mirror.png")]
        static void MirrorObjectAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorAction.Start(new MenuActionSettingsWithPreview(EditorToolbarLoader.GetInstance<MirrorObjects>()));
                    s_ActionAlreadyTriggered = false;
                };
            }
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Merge Objects", true)]
        static bool ValidateMergeObjectsAction()
        {
            return MeshSelection.selectedObjectCount > 1 && MeshSelection.activeMesh != null;
        }

        [MenuItem("CONTEXT/ProBuilderMesh/Merge Objects", false, 18, "Merge ProBuilder meshes", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Object_Merge.png")]
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

        [MenuItem("CONTEXT/ProBuilderMesh/Freeze Transform", false, 19, "Set pivot point to (0,0,0)", "Packages/com.unity.probuilder/Content/Icons/Toolbar/Pivot_FreezeTransform.png")]
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
    }
}
