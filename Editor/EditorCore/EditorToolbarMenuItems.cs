/**
 *  IMPORTANT
 *
 *  This is a generated file. Any changes will be overwritten.
 *  See Debug/GenerateMenuItems to make modifications.
 */

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using UnityEditor.ShortcutManagement;

namespace UnityEditor.ProBuilder
{
    static class EditorToolbarMenuItem
    {
        const string k_MenuPrefix = "Tools/ProBuilder/";
        const string k_ShortcutPrefix = "ProBuilder/";

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
            if (instance != null && instance.enabled)
            {
                EditorUtility.ShowNotification(instance.PerformAction().notification);
                ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
            }
        }

        [Shortcut(k_ShortcutPrefix + "Create/New Poly Shape", typeof(SceneView))]
        [MenuItem(k_MenuPrefix + "Editors/New Poly Shape", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_NewPolyShapeToggle()
		{
			var instance = EditorToolbarLoader.GetInstance<NewPolyShapeAction>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut(k_ShortcutPrefix + "Create/New Shape", typeof(SceneView), KeyCode.K, ShortcutModifiers.Shift|ShortcutModifiers.Control)]
		[MenuItem(k_MenuPrefix + "Editors/New Shape", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_NewShapeToggle()
		{
			var instance = EditorToolbarLoader.GetInstance<NewShapeAction>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut(k_ShortcutPrefix + "Editors/Open Lightmap UV Editor", typeof(SceneView))]
		[MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenLightmapUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut(k_ShortcutPrefix + "Editors/Open Material Editor", typeof(SceneView))]
		[MenuItem(k_MenuPrefix + "Editors/Open Material Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenMaterialEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut(k_ShortcutPrefix + "Editors/Open Smoothing Editor", typeof(SceneView))]
		[MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenSmoothingEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut("ProBuilder/Editors/Open UV Editor", typeof(SceneView))]
		[MenuItem(k_MenuPrefix + "Editors/Open UV Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenUVEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut(k_ShortcutPrefix + "Editors/Open Vertex Color Editor", typeof(SceneView))]
        [MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexColorEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [Shortcut(k_ShortcutPrefix + "Editors/Open Vertex Position Editor", typeof(SceneView))]
        [MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor", false, PreferenceKeys.menuEditor + 1)]
		static void MenuPerform_OpenVertexPositionEditor()
		{
			var instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_BevelEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Bevel Edges", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Bevel Edges", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BevelEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BevelEdges>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_BridgeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Bridge Edges", typeof(ProBuilderContext), KeyCode.B, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Bridge Edges", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_BridgeEdges()
		{
			var instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_CollapseVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Collapse Vertices", typeof(ProBuilderContext), KeyCode.C, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_CollapseVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals", true, PreferenceKeys.menuGeometry + 3)]
        static bool MenuVerify_ConformFaceNormals()
        {
            var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
            return instance != null && instance.enabled;
        }

        [Shortcut(k_ShortcutPrefix +"Geometry/Conform Face Normals", typeof(ProBuilderContext), KeyCode.C, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_ConformFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DeleteFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			return instance != null && instance.enabled;
		}

		[Shortcut(k_ShortcutPrefix + "Geometry/Delete Faces", typeof(ProBuilderContext), KeyCode.Backspace)]
		[MenuItem(k_MenuPrefix + "Geometry/Delete Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DeleteFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DetachFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Detach Faces", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Detach Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DetachFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DetachFaces>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Duplicate Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_DuplicateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DuplicateFaces>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Duplicate Faces", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Duplicate Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_DuplicateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<DuplicateFaces>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Extrude", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_Extrude()
		{
			var instance = EditorToolbarLoader.GetInstance<Extrude>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Extrude", typeof(ProBuilderContext), KeyCode.E, ShortcutModifiers.Control)]
		[MenuItem(k_MenuPrefix + "Geometry/Extrude", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_Extrude()
		{
			var instance = EditorToolbarLoader.GetInstance<Extrude>();
			if(instance != null && instance.enabled)
			{
                Debug.Log("Extrude");
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Fill Hole", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FillHole()
		{
			var instance = EditorToolbarLoader.GetInstance<FillHole>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Fill Hole", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Fill Hole", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FillHole()
		{
			var instance = EditorToolbarLoader.GetInstance<FillHole>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FlipFaceEdge()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Flip Face Edge", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceEdge()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_FlipFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Flip Face Normals", typeof(ProBuilderContext), KeyCode.N, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_FlipFaceNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_InsertEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Insert Edge Loop", typeof(ProBuilderContext), KeyCode.U, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_InsertEdgeLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_MergeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Merge Faces", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Merge Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_MergeFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeFaces>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Offset Elements", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_OffsetElements()
		{
			var instance = EditorToolbarLoader.GetInstance<OffsetElements>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Offset Elements", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Offset Elements", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_OffsetElements()
		{
			var instance = EditorToolbarLoader.GetInstance<OffsetElements>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SetPivotToSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Set Pivot To Selection", typeof(ProBuilderContext), KeyCode.J, ShortcutModifiers.Control)]
		[MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SetPivotToSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SmartConnect()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Smart Connect", typeof(ProBuilderContext), KeyCode.E, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Smart Connect", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartConnect()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartConnect>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SmartSubdivide()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Smart Subdivide", typeof(ProBuilderContext), KeyCode.S, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SmartSubdivide()
		{
			var instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_SplitVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Split Vertices", typeof(ProBuilderContext), KeyCode.X, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Split Vertices", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_SplitVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<SplitVertices>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_TriangulateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Split Vertices", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_TriangulateFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices", true, PreferenceKeys.menuGeometry + 3)]
		static bool MenuVerify_WeldVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Geometry/Weld Vertices", typeof(ProBuilderContext), KeyCode.V, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Geometry/Weld Vertices", false, PreferenceKeys.menuGeometry + 3)]
		static void MenuPerform_WeldVertices()
		{
			var instance = EditorToolbarLoader.GetInstance<WeldVertices>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleDragRectMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragRectMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleDragSelectionMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleDragSelectionMode()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleHandleOrientation()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			return instance != null && instance.enabled;
		}

		[Shortcut(k_ShortcutPrefix + "Interaction/Toggle Handle Orientation", typeof(ProBuilderContext), KeyCode.P)]
		[MenuItem(k_MenuPrefix + "Interaction/Toggle Handle Orientation", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleHandleOrientation()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleHandleOrientation>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleSelectBackFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			return instance != null && instance.enabled;
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleSelectBackFaces()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Interaction/Toggle X Ray", true, PreferenceKeys.menuSelection + 1)]
		static bool MenuVerify_ToggleXRay()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleXRay>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Interaction/Toggle X Ray", typeof(ProBuilderContext), KeyCode.X, ShortcutModifiers.Alt|ShortcutModifiers.Shift)]
		[MenuItem(k_MenuPrefix + "Interaction/Toggle X Ray", false, PreferenceKeys.menuSelection + 1)]
		static void MenuPerform_ToggleXRay()
		{
			var instance = EditorToolbarLoader.GetInstance<ToggleXRay>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Center Pivot", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Center Pivot", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Center Pivot", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_CenterPivot()
		{
			var instance = EditorToolbarLoader.GetInstance<CenterPivot>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Conform Object Normals", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Conform Object Normals", typeof(ProBuilderContext))]
        [MenuItem(k_MenuPrefix + "Object/Conform Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ConformObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Flip Object Normals", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Flip Object Normals", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FlipObjectNormals()
		{
			var instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Freeze Transform", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Freeze Transform", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_FreezeTransform()
		{
			var instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Merge Objects", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Merge Objects", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Merge Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MergeObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MergeObjects>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Mirror Objects", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Mirror Objects", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Mirror Objects", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_MirrorObjects()
		{
			var instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Pro Builderize", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_ProBuilderize()
		{
			var instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			return instance != null && instance.enabled;
		}
        [Shortcut(k_ShortcutPrefix + "Object/ProBuilderize", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Pro Builderize", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_ProBuilderize()
		{
			var instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
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
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Subdivide Object", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Subdivide Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_SubdivideObject()
		{
			var instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", true, PreferenceKeys.menuGeometry + 2)]
		static bool MenuVerify_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Object/Triangulate Object", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Object/Triangulate Object", false, PreferenceKeys.menuGeometry + 2)]
		static void MenuPerform_TriangulateObject()
		{
			var instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Grow Selection", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_GrowSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Grow Selection", typeof(ProBuilderContext), KeyCode.G, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Selection/Grow Selection", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_GrowSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<GrowSelection>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Hole", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectHole()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Select Hole", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Selection/Select Hole", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectHole()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectHole>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [MenuItem(k_MenuPrefix + "Selection/Select Loop", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Select Loop", typeof(ProBuilderContext), KeyCode.L, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Selection/Select Loop", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectLoop()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectLoop>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Material", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectMaterial()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Select Material", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Selection/Select Material", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectMaterial()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

        [MenuItem(k_MenuPrefix + "Selection/Select Ring", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectRing>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Select Ring", typeof(ProBuilderContext), KeyCode.R, ShortcutModifiers.Alt)]
		[MenuItem(k_MenuPrefix + "Selection/Select Ring", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectRing()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectRing>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectSmoothingGroup()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Select Smoothing Group", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectSmoothingGroup()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_SelectVertexColor()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Select Vertex Color", typeof(ProBuilderContext))]
		[MenuItem(k_MenuPrefix + "Selection/Select Vertex Color", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_SelectVertexColor()
		{
			var instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}

		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection", true, PreferenceKeys.menuSelection + 0)]
		static bool MenuVerify_ShrinkSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			return instance != null && instance.enabled;
		}

        [Shortcut(k_ShortcutPrefix + "Selection/Shrink Selection", typeof(ProBuilderContext), KeyCode.G, ShortcutModifiers.Alt|ShortcutModifiers.Shift)]
		[MenuItem(k_MenuPrefix + "Selection/Shrink Selection", false, PreferenceKeys.menuSelection + 0)]
		static void MenuPerform_ShrinkSelection()
		{
			var instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
			if(instance != null && instance.enabled)
			{
				EditorUtility.ShowNotification(instance.PerformAction().notification);
				ProBuilderAnalytics.SendActionEvent(instance, ProBuilderAnalytics.TriggerType.MenuOrShortcut);
			}
		}
	}
}
