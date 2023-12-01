/**
 *  IMPORTANT
 *
 *  This is a generated file. Any changes will be overwritten.
 *  See Debug/GenerateMenuItems to make modifications.
 */

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.Actions;
using UnityEditor.EditorTools;

namespace UnityEditor.ProBuilder
{
    static class EditorToolbarMenuItem
    {
        internal const string k_MenuPrefix = "Tools/ProBuilder/";

		[MenuItem(k_MenuPrefix + "Editors/New Bezier Shape", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_NewBezierShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/New Bezier Shape", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_NewBezierShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Asset", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportAsset()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Asset", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportAsset()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Obj", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportObj()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportObj>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Obj", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportObj()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportObj>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Ply", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportPly()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportPly>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Ply", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportPly()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportPly>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Ascii", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportStlAscii()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Ascii", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportStlAscii()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Binary", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportStlBinary()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Binary", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportStlBinary()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_BevelEdges()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BevelEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges &B", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_BridgeEdges()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges &B", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BridgeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices &C", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_CollapseVertices()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices &C", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_CollapseVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_ConformFaceNormals()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ConformFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces _BACKSPACE", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DeleteFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces _BACKSPACE", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DeleteFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DetachFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DetachFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,false));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Duplicate Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DuplicateFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<DuplicateFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Duplicate Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DuplicateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DuplicateFaces>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,false));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude %E", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_Extrude()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<Extrude>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude %E", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_Extrude()
		{
			var instance = EditorToolbarLoader.GetInstance<Extrude>();
			if(instance != null && instance.enabled)
			{
                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<ExtrudeEdges>(), true));
                        break;
                    case SelectMode.Face:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<ExtrudeFaces>(), true));
                        break;
                }
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Fill Hole", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FillHole()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<FillHole>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Fill Hole", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FillHole()
		{
			var instance = EditorToolbarLoader.GetInstance<FillHole>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FlipFaceEdge()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceEdge()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &N", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FlipFaceNormals()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &N", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &U", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_InsertEdgeLoop()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &U", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_InsertEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_MergeFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_MergeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Offset Elements", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_OffsetElements()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<OffsetElements>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Offset Elements", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_OffsetElements()
		{
			var instance = EditorToolbarLoader.GetInstance<OffsetElements>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection %J", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SetPivotToSelection()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection %J", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SetPivotToSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect &E", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SmartConnect()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect &E", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartConnect()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide &S", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SmartSubdivide()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide &S", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartSubdivide()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			if(instance != null && instance.enabled)
			{
                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<SubdivideEdges>(), true));
                        break;
                    default:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<SubdivideFaces>(), true));
                        break;
                }
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices &X", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SplitVertices()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices &X", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SplitVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_TriangulateFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_TriangulateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices &V", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_WeldVertices()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices &V", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_WeldVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleDragRectMode()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragRectMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleDragSelectionMode()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragSelectionMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation _P", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleHandleOrientation()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation _P", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleHandleOrientation()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleSelectBackFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleSelectBackFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle X Ray &#X", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleXRay()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleXRay>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle X Ray &#X", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleXRay()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleXRay>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Mirror Objects", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Mirror Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Pro Builderize", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_ProBuilderize()
		{
			var instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Pro Builderize", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ProBuilderize()
		{
			var instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Set Collider", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_SetCollider()
		{
			var instance = EditorToolbarLoader.GetInstance<SetCollider>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Set Collider", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SetCollider()
		{
			var instance = EditorToolbarLoader.GetInstance<SetCollider>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Set Trigger", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_SetTrigger()
		{
			var instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Set Trigger", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SetTrigger()
		{
			var instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection &G", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_GrowSelection()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection &G", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_GrowSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectHole()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectHole()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Loop &L", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectLoop()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Loop &L", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			if(instance != null && instance.enabled)
			{
                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<SelectEdgeLoop>(), true));
                        break;
                    case SelectMode.Face:
                        EditorToolbarLoader.GetInstance<SelectFaceLoop>().PerformAction();
                        break;
                }
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Material", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectMaterial()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Material", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectMaterial()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Ring &R", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectRing()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectRing>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Ring &R", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectRing>();
			if(instance != null && instance.enabled)
			{
                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<SelectEdgeRing>(), true));
                        break;
                    case SelectMode.Face:
                        EditorToolbarLoader.GetInstance<SelectFaceRing>().PerformAction();
                        break;
                }
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectSmoothingGroup()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectSmoothingGroup()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectVertexColor()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectVertexColor()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#G", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_ShrinkSelection()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#G", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_ShrinkSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null && instance.enabled)
			{
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}
	}
}
