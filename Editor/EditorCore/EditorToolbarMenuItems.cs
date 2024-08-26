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

		[MenuItem(k_MenuPrefix + "Editors/Create Bezier Shape", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_NewBezierShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Create Bezier Shape", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Create Bezier Shape", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_NewBezierShape()
		{
			var instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Open Lightmap UV Editor", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Open Material Editor", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Open Smoothing Editor", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Open UV Editor", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Open Vertex Color Editor", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor", true, PreferenceKeys.menuEditor + 1)]
		static bool MenuVerify_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			Menu.SetChecked(k_MenuPrefix + "Editors/Open Vertex Position Editor", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Asset", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportAsset()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			Menu.SetChecked(k_MenuPrefix + "Export/Export Asset", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Asset", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportAsset()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportAsset>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Obj", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportObj()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportObj>();
			Menu.SetChecked(k_MenuPrefix + "Export/Export Obj", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Obj", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportObj()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportObj>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Ply", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportPly()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportPly>();
			Menu.SetChecked(k_MenuPrefix + "Export/Export Ply", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Ply", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportPly()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportPly>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Ascii", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportStlAscii()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			Menu.SetChecked(k_MenuPrefix + "Export/Export Stl Ascii", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Ascii", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportStlAscii()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Binary", true, PreferenceKeys.menuExport + 0)]
		static bool MenuVerify_ExportStlBinary()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			Menu.SetChecked(k_MenuPrefix + "Export/Export Stl Binary", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Export/Export Stl Binary", false, PreferenceKeys.menuExport + 0)]
		static void MenuPerform_ExportStlBinary()
		{
			var instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_BevelEdges()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Bevel Edges", instance.IsMenuItemChecked() );
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

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_BridgeEdges()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Bridge Edges", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BridgeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_CollapseVertices()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Collapse Vertices", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices", false, PreferenceKeys.menuGeometry + 3)]
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
			Menu.SetChecked(k_MenuPrefix + "Geometry/Conform Face Normals", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ConformFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces _BACKSPACE", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DeleteFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Delete Faces _BACKSPACE", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces _BACKSPACE", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DeleteFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DetachFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Detach Faces", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Geometry/Duplicate Faces", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Geometry/Extrude %E", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Geometry/Fill Hole", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Geometry/Flip Face Edge", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceEdge()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &N", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FlipFaceNormals()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Flip Face Normals &N", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &N", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &U", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_InsertEdgeLoop()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Insert Edge Loop &U", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &U", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_InsertEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_MergeFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Merge Faces", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_MergeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Offset Elements", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_OffsetElements()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<OffsetElements>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Offset Elements", instance.IsMenuItemChecked() );
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

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SetPivotToSelection()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Set Pivot To Selection", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SetPivotToSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect &E", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SmartConnect()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Smart Connect &E", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect &E", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartConnect()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide &S", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SmartSubdivide()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Smart Subdivide &S", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Geometry/Split Vertices &X", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices &X", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SplitVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_TriangulateFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Triangulate Faces", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_TriangulateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices &V", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_WeldVertices()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			Menu.SetChecked(k_MenuPrefix + "Geometry/Weld Vertices &V", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragRectMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleHandleOrientation()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			Menu.SetChecked(k_MenuPrefix + "Interaction/Toggle Handle Orientation", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleHandleOrientation()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleSelectBackFaces()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			Menu.SetChecked(k_MenuPrefix + "Interaction/Toggle Select Back Faces", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleSelectBackFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle X Ray &#X", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleXRay()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ToggleXRay>();
			Menu.SetChecked(k_MenuPrefix + "Interaction/Toggle X Ray &#X", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle X Ray &#X", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleXRay()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleXRay>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			Menu.SetChecked(k_MenuPrefix + "Object/Center Pivot", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			Menu.SetChecked(k_MenuPrefix + "Object/Conform Object Normals", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			Menu.SetChecked(k_MenuPrefix + "Object/Flip Object Normals", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			Menu.SetChecked(k_MenuPrefix + "Object/Freeze Transform", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			Menu.SetChecked(k_MenuPrefix + "Object/Merge Objects", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Mirror Objects", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			Menu.SetChecked(k_MenuPrefix + "Object/Mirror Objects", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Object/Pro Builderize", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Object/Set Collider", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Set Collider", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SetCollider()
		{
			var instance = EditorToolbarLoader.GetInstance<SetCollider>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Set Trigger", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_SetTrigger()
		{
			var instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			Menu.SetChecked(k_MenuPrefix + "Object/Set Trigger", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Set Trigger", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SetTrigger()
		{
			var instance = EditorToolbarLoader.GetInstance<SetTrigger>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			Menu.SetChecked(k_MenuPrefix + "Object/Subdivide Object", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			Menu.SetChecked(k_MenuPrefix + "Object/Triangulate Object", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection &G", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_GrowSelection()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			Menu.SetChecked(k_MenuPrefix + "Selection/Grow Selection &G", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection &G", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_GrowSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null && instance.enabled)
			{
				EditorAction.Start(new MenuActionSettings(instance,true));
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectHole()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			Menu.SetChecked(k_MenuPrefix + "Selection/Select Hole", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectHole()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Loop &L", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectLoop()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			Menu.SetChecked(k_MenuPrefix + "Selection/Select Loop &L", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Selection/Select Material", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Selection/Select Ring &R", instance.IsMenuItemChecked() );
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
			Menu.SetChecked(k_MenuPrefix + "Selection/Select Smoothing Group", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectSmoothingGroup()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectVertexColor()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			Menu.SetChecked(k_MenuPrefix + "Selection/Select Vertex Color", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectVertexColor()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#G", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_ShrinkSelection()
		{
			if (ToolManager.activeContextType != typeof(PositionToolContext)) return false;
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			Menu.SetChecked(k_MenuPrefix + "Selection/Shrink Selection &#G", instance.IsMenuItemChecked() );
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#G", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_ShrinkSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null && instance.enabled)
			{
				PreviewActionManager.EndPreview();
				instance.PerformAction();
				ProBuilderAnalytics.SendActionEvent(instance);
			}
		}
	}
}
