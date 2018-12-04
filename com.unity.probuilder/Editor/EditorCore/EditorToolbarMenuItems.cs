/**
 *  IMPORTANT
 *
 *  This is a generated file. Any changes will be overwritten.
 *  See Debug/GenerateMenuItems to make modifications.
 */

using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.Actions;
using System.Collections.Generic;

namespace UnityEditor.ProBuilder
{
    static class EditorToolbarMenuItem
    {
        const string k_MenuPrefix = "Tools/ProBuilder/";

        [MenuItem(k_MenuPrefix + "Editors/New Bezier Shape ", true)]
        static bool MenuVerifyNewBezierShape()
        {
            NewBezierShape instance = EditorToolbarLoader.GetInstance<NewBezierShape>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/New Bezier Shape ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoNewBezierShape()
        {
            NewBezierShape instance = EditorToolbarLoader.GetInstance<NewBezierShape>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/New Poly Shape ", true)]
        static bool MenuVerifyNewPolyShape()
        {
            NewPolyShape instance = EditorToolbarLoader.GetInstance<NewPolyShape>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/New Poly Shape ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoNewPolyShape()
        {
            NewPolyShape instance = EditorToolbarLoader.GetInstance<NewPolyShape>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor ", true)]
        static bool MenuVerifyOpenLightmapUVEditor()
        {
            OpenLightmapUVEditor instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Lightmap UV Editor ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenLightmapUVEditor()
        {
            OpenLightmapUVEditor instance = EditorToolbarLoader.GetInstance<OpenLightmapUVEditor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Material Editor ", true)]
        static bool MenuVerifyOpenMaterialEditor()
        {
            OpenMaterialEditor instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Material Editor ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenMaterialEditor()
        {
            OpenMaterialEditor instance = EditorToolbarLoader.GetInstance<OpenMaterialEditor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Shape Editor Menu Item %#k", true)]
        static bool MenuVerifyOpenShapeEditorMenuItem()
        {
            OpenShapeEditorMenuItem instance = EditorToolbarLoader.GetInstance<OpenShapeEditorMenuItem>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Shape Editor Menu Item %#k", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenShapeEditorMenuItem()
        {
            OpenShapeEditorMenuItem instance = EditorToolbarLoader.GetInstance<OpenShapeEditorMenuItem>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor ", true)]
        static bool MenuVerifyOpenSmoothingEditor()
        {
            OpenSmoothingEditor instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Smoothing Editor ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenSmoothingEditor()
        {
            OpenSmoothingEditor instance = EditorToolbarLoader.GetInstance<OpenSmoothingEditor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open UV Editor ", true)]
        static bool MenuVerifyOpenUVEditor()
        {
            OpenUVEditor instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open UV Editor ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenUVEditor()
        {
            OpenUVEditor instance = EditorToolbarLoader.GetInstance<OpenUVEditor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor ", true)]
        static bool MenuVerifyOpenVertexColorEditor()
        {
            OpenVertexColorEditor instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Vertex Color Editor ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenVertexColorEditor()
        {
            OpenVertexColorEditor instance = EditorToolbarLoader.GetInstance<OpenVertexColorEditor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor ", true)]
        static bool MenuVerifyOpenVertexPositionEditor()
        {
            OpenVertexPositionEditor instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Editors/Open Vertex Position Editor ", false, PreferenceKeys.menuEditor + 1)]
        static void MenuDoOpenVertexPositionEditor()
        {
            OpenVertexPositionEditor instance = EditorToolbarLoader.GetInstance<OpenVertexPositionEditor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Export/Export Asset ", true)]
        static bool MenuVerifyExportAsset()
        {
            ExportAsset instance = EditorToolbarLoader.GetInstance<ExportAsset>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Export/Export Asset ", false, PreferenceKeys.menuExport + 0)]
        static void MenuDoExportAsset()
        {
            ExportAsset instance = EditorToolbarLoader.GetInstance<ExportAsset>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Export/Export Obj ", true)]
        static bool MenuVerifyExportObj()
        {
            ExportObj instance = EditorToolbarLoader.GetInstance<ExportObj>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Export/Export Obj ", false, PreferenceKeys.menuExport + 0)]
        static void MenuDoExportObj()
        {
            ExportObj instance = EditorToolbarLoader.GetInstance<ExportObj>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Export/Export Ply ", true)]
        static bool MenuVerifyExportPly()
        {
            ExportPly instance = EditorToolbarLoader.GetInstance<ExportPly>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Export/Export Ply ", false, PreferenceKeys.menuExport + 0)]
        static void MenuDoExportPly()
        {
            ExportPly instance = EditorToolbarLoader.GetInstance<ExportPly>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Export/Export Stl Ascii ", true)]
        static bool MenuVerifyExportStlAscii()
        {
            ExportStlAscii instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Export/Export Stl Ascii ", false, PreferenceKeys.menuExport + 0)]
        static void MenuDoExportStlAscii()
        {
            ExportStlAscii instance = EditorToolbarLoader.GetInstance<ExportStlAscii>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Export/Export Stl Binary ", true)]
        static bool MenuVerifyExportStlBinary()
        {
            ExportStlBinary instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Export/Export Stl Binary ", false, PreferenceKeys.menuExport + 0)]
        static void MenuDoExportStlBinary()
        {
            ExportStlBinary instance = EditorToolbarLoader.GetInstance<ExportStlBinary>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Bevel Edges ", true)]
        static bool MenuVerifyBevelEdges()
        {
            BevelEdges instance = EditorToolbarLoader.GetInstance<BevelEdges>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Bevel Edges ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoBevelEdges()
        {
            BevelEdges instance = EditorToolbarLoader.GetInstance<BevelEdges>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Bridge Edges &b", true)]
        static bool MenuVerifyBridgeEdges()
        {
            BridgeEdges instance = EditorToolbarLoader.GetInstance<BridgeEdges>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Bridge Edges &b", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoBridgeEdges()
        {
            BridgeEdges instance = EditorToolbarLoader.GetInstance<BridgeEdges>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices &c", true)]
        static bool MenuVerifyCollapseVertices()
        {
            CollapseVertices instance = EditorToolbarLoader.GetInstance<CollapseVertices>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Collapse Vertices &c", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoCollapseVertices()
        {
            CollapseVertices instance = EditorToolbarLoader.GetInstance<CollapseVertices>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals ", true)]
        static bool MenuVerifyConformFaceNormals()
        {
            ConformFaceNormals instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Conform Face Normals ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoConformFaceNormals()
        {
            ConformFaceNormals instance = EditorToolbarLoader.GetInstance<ConformFaceNormals>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Delete Faces  [delete]", true)]
        static bool MenuVerifyDeleteFaces()
        {
            DeleteFaces instance = EditorToolbarLoader.GetInstance<DeleteFaces>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Delete Faces  [delete]", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoDeleteFaces()
        {
            DeleteFaces instance = EditorToolbarLoader.GetInstance<DeleteFaces>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Detach Faces ", true)]
        static bool MenuVerifyDetachFaces()
        {
            DetachFaces instance = EditorToolbarLoader.GetInstance<DetachFaces>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Detach Faces ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoDetachFaces()
        {
            DetachFaces instance = EditorToolbarLoader.GetInstance<DetachFaces>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Extrude %e", true)]
        static bool MenuVerifyExtrude()
        {
            Extrude instance = EditorToolbarLoader.GetInstance<Extrude>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Extrude %e", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoExtrude()
        {
            Extrude instance = EditorToolbarLoader.GetInstance<Extrude>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Fill Hole ", true)]
        static bool MenuVerifyFillHole()
        {
            FillHole instance = EditorToolbarLoader.GetInstance<FillHole>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Fill Hole ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoFillHole()
        {
            FillHole instance = EditorToolbarLoader.GetInstance<FillHole>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge ", true)]
        static bool MenuVerifyFlipFaceEdge()
        {
            FlipFaceEdge instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Flip Face Edge ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoFlipFaceEdge()
        {
            FlipFaceEdge instance = EditorToolbarLoader.GetInstance<FlipFaceEdge>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &n", true)]
        static bool MenuVerifyFlipFaceNormals()
        {
            FlipFaceNormals instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Flip Face Normals &n", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoFlipFaceNormals()
        {
            FlipFaceNormals instance = EditorToolbarLoader.GetInstance<FlipFaceNormals>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &u", true)]
        static bool MenuVerifyInsertEdgeLoop()
        {
            InsertEdgeLoop instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Insert Edge Loop &u", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoInsertEdgeLoop()
        {
            InsertEdgeLoop instance = EditorToolbarLoader.GetInstance<InsertEdgeLoop>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Merge Faces ", true)]
        static bool MenuVerifyMergeFaces()
        {
            MergeFaces instance = EditorToolbarLoader.GetInstance<MergeFaces>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Merge Faces ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoMergeFaces()
        {
            MergeFaces instance = EditorToolbarLoader.GetInstance<MergeFaces>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection %j", true)]
        static bool MenuVerifySetPivotToSelection()
        {
            SetPivotToSelection instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Set Pivot To Selection %j", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoSetPivotToSelection()
        {
            SetPivotToSelection instance = EditorToolbarLoader.GetInstance<SetPivotToSelection>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Smart Connect &e", true)]
        static bool MenuVerifySmartConnect()
        {
            SmartConnect instance = EditorToolbarLoader.GetInstance<SmartConnect>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Smart Connect &e", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoSmartConnect()
        {
            SmartConnect instance = EditorToolbarLoader.GetInstance<SmartConnect>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide &s", true)]
        static bool MenuVerifySmartSubdivide()
        {
            SmartSubdivide instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Smart Subdivide &s", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoSmartSubdivide()
        {
            SmartSubdivide instance = EditorToolbarLoader.GetInstance<SmartSubdivide>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Split Vertices &x", true)]
        static bool MenuVerifySplitVertices()
        {
            SplitVertices instance = EditorToolbarLoader.GetInstance<SplitVertices>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Split Vertices &x", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoSplitVertices()
        {
            SplitVertices instance = EditorToolbarLoader.GetInstance<SplitVertices>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces ", true)]
        static bool MenuVerifyTriangulateFaces()
        {
            TriangulateFaces instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Triangulate Faces ", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoTriangulateFaces()
        {
            TriangulateFaces instance = EditorToolbarLoader.GetInstance<TriangulateFaces>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Geometry/Weld Vertices &v", true)]
        static bool MenuVerifyWeldVertices()
        {
            WeldVertices instance = EditorToolbarLoader.GetInstance<WeldVertices>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Geometry/Weld Vertices &v", false, PreferenceKeys.menuGeometry + 3)]
        static void MenuDoWeldVertices()
        {
            WeldVertices instance = EditorToolbarLoader.GetInstance<WeldVertices>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode ", true)]
        static bool MenuVerifyToggleDragRectMode()
        {
            ToggleDragRectMode instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Rect Mode ", false, PreferenceKeys.menuSelection + 1)]
        static void MenuDoToggleDragRectMode()
        {
            ToggleDragRectMode instance = EditorToolbarLoader.GetInstance<ToggleDragRectMode>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode ", true)]
        static bool MenuVerifyToggleDragSelectionMode()
        {
            ToggleDragSelectionMode instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Interaction/Toggle Drag Selection Mode ", false, PreferenceKeys.menuSelection + 1)]
        static void MenuDoToggleDragSelectionMode()
        {
            ToggleDragSelectionMode instance = EditorToolbarLoader.GetInstance<ToggleDragSelectionMode>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }
        
        [MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces ", true)]
        static bool MenuVerifyToggleSelectBackFaces()
        {
            ToggleSelectBackFaces instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Interaction/Toggle Select Back Faces ", false, PreferenceKeys.menuSelection + 1)]
        static void MenuDoToggleSelectBackFaces()
        {
            ToggleSelectBackFaces instance = EditorToolbarLoader.GetInstance<ToggleSelectBackFaces>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Center Pivot ", true)]
        static bool MenuVerifyCenterPivot()
        {
            CenterPivot instance = EditorToolbarLoader.GetInstance<CenterPivot>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Center Pivot ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoCenterPivot()
        {
            CenterPivot instance = EditorToolbarLoader.GetInstance<CenterPivot>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Conform Object Normals ", true)]
        static bool MenuVerifyConformObjectNormals()
        {
            ConformObjectNormals instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Conform Object Normals ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoConformObjectNormals()
        {
            ConformObjectNormals instance = EditorToolbarLoader.GetInstance<ConformObjectNormals>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Flip Object Normals ", true)]
        static bool MenuVerifyFlipObjectNormals()
        {
            FlipObjectNormals instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Flip Object Normals ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoFlipObjectNormals()
        {
            FlipObjectNormals instance = EditorToolbarLoader.GetInstance<FlipObjectNormals>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Freeze Transform ", true)]
        static bool MenuVerifyFreezeTransform()
        {
            FreezeTransform instance = EditorToolbarLoader.GetInstance<FreezeTransform>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Freeze Transform ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoFreezeTransform()
        {
            FreezeTransform instance = EditorToolbarLoader.GetInstance<FreezeTransform>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Merge Objects ", true)]
        static bool MenuVerifyMergeObjects()
        {
            MergeObjects instance = EditorToolbarLoader.GetInstance<MergeObjects>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Merge Objects ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoMergeObjects()
        {
            MergeObjects instance = EditorToolbarLoader.GetInstance<MergeObjects>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Mirror Objects ", true)]
        static bool MenuVerifyMirrorObjects()
        {
            MirrorObjects instance = EditorToolbarLoader.GetInstance<MirrorObjects>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Mirror Objects ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoMirrorObjects()
        {
            MirrorObjects instance = EditorToolbarLoader.GetInstance<MirrorObjects>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Pro Builderize ", true)]
        static bool MenuVerifyProBuilderize()
        {
            ProBuilderize instance = EditorToolbarLoader.GetInstance<ProBuilderize>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Pro Builderize ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoProBuilderize()
        {
            ProBuilderize instance = EditorToolbarLoader.GetInstance<ProBuilderize>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Set Collider ", true)]
        static bool MenuVerifySetCollider()
        {
            SetCollider instance = EditorToolbarLoader.GetInstance<SetCollider>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Set Collider ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoSetCollider()
        {
            SetCollider instance = EditorToolbarLoader.GetInstance<SetCollider>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Set Trigger ", true)]
        static bool MenuVerifySetTrigger()
        {
            SetTrigger instance = EditorToolbarLoader.GetInstance<SetTrigger>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Set Trigger ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoSetTrigger()
        {
            SetTrigger instance = EditorToolbarLoader.GetInstance<SetTrigger>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Subdivide Object ", true)]
        static bool MenuVerifySubdivideObject()
        {
            SubdivideObject instance = EditorToolbarLoader.GetInstance<SubdivideObject>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Subdivide Object ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoSubdivideObject()
        {
            SubdivideObject instance = EditorToolbarLoader.GetInstance<SubdivideObject>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Object/Triangulate Object ", true)]
        static bool MenuVerifyTriangulateObject()
        {
            TriangulateObject instance = EditorToolbarLoader.GetInstance<TriangulateObject>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Object/Triangulate Object ", false, PreferenceKeys.menuGeometry + 2)]
        static void MenuDoTriangulateObject()
        {
            TriangulateObject instance = EditorToolbarLoader.GetInstance<TriangulateObject>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Grow Selection &g", true)]
        static bool MenuVerifyGrowSelection()
        {
            GrowSelection instance = EditorToolbarLoader.GetInstance<GrowSelection>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Grow Selection &g", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoGrowSelection()
        {
            GrowSelection instance = EditorToolbarLoader.GetInstance<GrowSelection>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Invert Selection %#i", true)]
        static bool MenuVerifyInvertSelection()
        {
            InvertSelection instance = EditorToolbarLoader.GetInstance<InvertSelection>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Invert Selection %#i", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoInvertSelection()
        {
            InvertSelection instance = EditorToolbarLoader.GetInstance<InvertSelection>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Hole ", true)]
        static bool MenuVerifySelectHole()
        {
            SelectHole instance = EditorToolbarLoader.GetInstance<SelectHole>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Hole ", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoSelectHole()
        {
            SelectHole instance = EditorToolbarLoader.GetInstance<SelectHole>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Loop &l", true)]
        static bool MenuVerifySelectLoop()
        {
            SelectLoop instance = EditorToolbarLoader.GetInstance<SelectLoop>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Loop &l", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoSelectLoop()
        {
            SelectLoop instance = EditorToolbarLoader.GetInstance<SelectLoop>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Material ", true)]
        static bool MenuVerifySelectMaterial()
        {
            SelectMaterial instance = EditorToolbarLoader.GetInstance<SelectMaterial>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Material ", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoSelectMaterial()
        {
            SelectMaterial instance = EditorToolbarLoader.GetInstance<SelectMaterial>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Ring &r", true)]
        static bool MenuVerifySelectRing()
        {
            SelectRing instance = EditorToolbarLoader.GetInstance<SelectRing>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Ring &r", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoSelectRing()
        {
            SelectRing instance = EditorToolbarLoader.GetInstance<SelectRing>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group ", true)]
        static bool MenuVerifySelectSmoothingGroup()
        {
            SelectSmoothingGroup instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Smoothing Group ", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoSelectSmoothingGroup()
        {
            SelectSmoothingGroup instance = EditorToolbarLoader.GetInstance<SelectSmoothingGroup>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Vertex Color ", true)]
        static bool MenuVerifySelectVertexColor()
        {
            SelectVertexColor instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Select Vertex Color ", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoSelectVertexColor()
        {
            SelectVertexColor instance = EditorToolbarLoader.GetInstance<SelectVertexColor>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }

        [MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#g", true)]
        static bool MenuVerifyShrinkSelection()
        {
            ShrinkSelection instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();

            return instance != null && instance.enabled;
        }

        [MenuItem(k_MenuPrefix + "Selection/Shrink Selection &#g", false, PreferenceKeys.menuSelection + 0)]
        static void MenuDoShrinkSelection()
        {
            ShrinkSelection instance = EditorToolbarLoader.GetInstance<ShrinkSelection>();
            if (instance != null)
                UnityEditor.ProBuilder.EditorUtility.ShowNotification(instance.DoAction().notification);
        }
    }
}
