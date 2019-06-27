using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using System.Linq;
using System.Reflection;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Responsible for loading menu actions in to the pb_Toolbar.
    /// </summary>
    static class EditorToolbarLoader
    {
        static List<MenuAction> s_LoadedMenuActions;

        public static T GetInstance<T>() where T : MenuAction, new()
        {
            T instance = (T)GetActions().FirstOrDefault(x => x is T);

            if (instance == null)
            {
                instance = new T();
                if (s_LoadedMenuActions != null)
                    s_LoadedMenuActions.Add(instance);
                else
                    s_LoadedMenuActions = new List<MenuAction>() { instance };
            }

            return instance;
        }

        internal static List<MenuAction> GetActions(bool forceReload = false)
        {
            if (s_LoadedMenuActions != null && !forceReload)
                return s_LoadedMenuActions;

            s_LoadedMenuActions = new List<MenuAction>()
            {
                // tools
                new Actions.OpenShapeEditor(),
                new Actions.NewBezierShape(),
                new Actions.NewPolyShape(),
                new Actions.OpenMaterialEditor(),
                new Actions.OpenUVEditor(),
                new Actions.OpenVertexColorEditor(),
                new Actions.OpenSmoothingEditor(),

                new Actions.ToggleSelectBackFaces(),
                new Actions.ToggleHandleOrientation(),
                new Actions.ToggleDragSelectionMode(),
                new Actions.ToggleDragRectMode(),

                // selection
                new Actions.GrowSelection(),
                new Actions.ShrinkSelection(),
                new Actions.InvertSelection(),
                new Actions.SelectEdgeLoop(),
                new Actions.SelectEdgeRing(),
                new Actions.SelectFaceLoop(),
                new Actions.SelectFaceRing(),
                new Actions.SelectHole(),
                new Actions.SelectVertexColor(),
                new Actions.SelectMaterial(),
                new Actions.SelectSmoothingGroup(),

                // object
                new Actions.MergeObjects(),
                new Actions.MirrorObjects(),
                new Actions.FlipObjectNormals(),
                new Actions.SubdivideObject(),
                new Actions.FreezeTransform(),
                new Actions.CenterPivot(),
                new Actions.ConformObjectNormals(),
                new Actions.TriangulateObject(),
                new Actions.GenerateUV2(),
                new Actions.ProBuilderize(),
                new Actions.Export(),
                // new Actions.ExportFbx(),
                new Actions.ExportObj(),
                new Actions.ExportAsset(),
                new Actions.ExportPly(),
                new Actions.ExportStlAscii(),
                new Actions.ExportStlBinary(),

                // All
                new Actions.SetPivotToSelection(),

                // Faces (All)
                new Actions.DeleteFaces(),
                new Actions.DetachFaces(),
                new Actions.DuplicateFaces(),
                new Actions.ExtrudeFaces(),

                // Face
                new Actions.ConformFaceNormals(),
                new Actions.FlipFaceEdge(),
                new Actions.FlipFaceNormals(),
                new Actions.MergeFaces(),
                new Actions.SubdivideFaces(),
                new Actions.TriangulateFaces(),

                // Edge
                new Actions.BridgeEdges(),
                new Actions.BevelEdges(),
                new Actions.ConnectEdges(),
                new Actions.ExtrudeEdges(),
                new Actions.InsertEdgeLoop(),
                new Actions.SubdivideEdges(),

                // Vertex
                new Actions.CollapseVertices(),
                new Actions.WeldVertices(),
                new Actions.ConnectVertices(),
                new Actions.FillHole(),
                // new Actions.CreatePolygon(),
                new Actions.SplitVertices(),

                // Entity
#if ENABLE_ENTITY_TYPES
                new Actions.SetEntityType_Detail(),
                new Actions.SetEntityType_Mover(),
                new Actions.SetEntityType_Collider(),
                new Actions.SetEntityType_Trigger(),
#endif
                new Actions.SetTrigger(),
                new Actions.SetCollider(),
            };

            SearchForMenuAttributes(s_LoadedMenuActions);

            s_LoadedMenuActions.Sort(MenuAction.CompareActionsByGroupAndPriority);

            return s_LoadedMenuActions;
        }

        static void SearchForMenuAttributes(List<MenuAction> list)
        {
            var actions = TypeCache.GetTypesWithAttribute<ProBuilderMenuActionAttribute>();

            foreach (var action in actions)
            {
                if (!typeof(MenuAction).IsAssignableFrom(action) || action.IsAbstract)
                    continue;

                try
                {
                    var instance = Activator.CreateInstance(action) as MenuAction;
                    if (instance != null)
                        list.Add(instance);
                }
                catch
                {
                    Debug.LogWarning($"Failed initializing menu item \"{action.ToString()}\". Is the constructor private?");
                }
            }
        }
    }
}
