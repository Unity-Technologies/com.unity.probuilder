using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.ProBuilder;

/// <summary>
/// Tests that every icon path used in IconUtility.GetIcon() across the codebase loads a non-null texture.
/// Icon list is kept in sync with all call sites to IconUtility.GetIcon().
/// </summary>
class IconUtilityTests
{
    /// <summary>
    /// All literal icon paths passed to IconUtility.GetIcon() in the codebase (no runtime/variable paths).
    /// About the paths in comment, they are disabled because icons are not currently used in the UI,
    /// so they are not packed with the package.
    /// Re-enable these tests if/when the icons are used again.
    /// </summary>
    static readonly string[] k_AllLiteralIconPaths =
    {
            // SelectionSettingsButtons.cs
            "Modes/Mode_Face",
            "Modes/Mode_Edge",
            "Modes/Mode_Vertex",
            // ToggleHandleOrientation.cs
            "Modes/ToolHandleGlobal",
            "Modes/ToolHandleLocal",
            "Modes/ToolHandleElement",
            // PolyShapeTool.cs
            "Toolbar/CreatePolyShape.png",
            "Toolbar/CreatePolyShape",
            // ToggleXRay.cs
            "Toolbar/Selection_SelectHidden-Off",
            "Toolbar/Selection_SelectHidden-On",
            // ProBuilderMeshEditor.cs
            "EditableMesh/EditMeshContext",
            // ExtrudeFaces.cs
            //"Toolbar/ExtrudeFace_Individual",
            //"Toolbar/ExtrudeFace_VertexNormals",
            //"Toolbar/ExtrudeFace_FaceNormals",
            // NewBezierShape.cs 
            //"Toolbar/NewBezierSpline",
            // UVEditor.cs
            "UVEditor/ProBuilderGUI_UV_ShowTexture_On",
            "UVEditor/ProBuilderGUI_UV_ShowTexture_Off",
            "UVEditor/ProBuilderGUI_UV_Manip_On",
            "UVEditor/ProBuilderGUI_UV_Manip_Off",
            "UVEditor/camera-64x64",
            // CutTool.cs
            "Toolbar/CutTool",
            "Cursors/cutCursor",
            "Cursors/cutCursor-add",
            // SmoothGroupEditor.cs
            "Toolbar/Background/RoundedRect_Normal",
            "Toolbar/Background/RoundedRect_Hover",
            "Toolbar/Background/RoundedRect_Pressed",
            "Toolbar/Background/RoundedRect_Normal_Blue",
            "Toolbar/Background/RoundedRect_Hover_Blue",
            "Toolbar/Background/RoundedRect_Pressed_Blue",
            "Toolbar/Background/RoundedRect_Normal_BlueSteel",
            "Toolbar/Background/RoundedRect_Hover_BlueSteel",
            "Toolbar/Background/RoundedRect_Pressed_BlueSteel",
            "Toolbar/Background/RoundedRect_Normal_Orange",
            "Toolbar/Background/RoundedRect_Hover_Orange",
            "Toolbar/Background/RoundedRect_Pressed_Orange",
            "Toolbar/Help",
            "Toolbar/Face_BreakSmoothing",
            "Toolbar/Selection_SelectBySmoothingGroup",
            // ProBuilderEditor.cs
            "Scene/SelectionRect",
            // EditorStyles.cs
            "Toolbar/RoundedBorder",
            "Scene/TextBackground",
            // EditorGUIUtility.cs (ProBuilder)
            "Modes/Mode_Object",
            // EditShapeTool.cs
            "Tools/EditShape",
            // DrawShapeTool.cs
            "Tools/ShapeTool/Cube",
            "Tools/ShapeTool/Sphere",
            "Tools/ShapeTool/Plane",
            "Tools/ShapeTool/Cylinder",
            "Tools/ShapeTool/Cone",
            "Tools/ShapeTool/Prism",
            "Tools/ShapeTool/Stairs",
            "Tools/ShapeTool/Torus",
            "Tools/ShapeTool/Pipe",
            "Tools/ShapeTool/Arch",
            "Tools/ShapeTool/Door",
            "Tools/ShapeTool/Sprite",
            // BezierSplineEditor.cs
            "Toolbar/Bezier_Free",
            "Toolbar/Bezier_Aligned",
            "Toolbar/Bezier_Mirrored",
            //MenuAction iconPath values (used in GetIcon(iconPath))
            //"Toolbar/Face_Extrude",
            //"Toolbar/Selection_Shrink",
            //"Toolbar/Selection_SelectByVertexColor",
            //"Toolbar/Selection_SelectByMaterial",
            //"Toolbar/Selection_SelectHole",
            //"Toolbar/Selection_Ring_Face",
            //"Toolbar/Selection_Loop_Face",
            //"Toolbar/Selection_Ring_Edge",
            //"Toolbar/Selection_Loop_Edge",
            //"Toolbar/Selection_Grow",
            //"Toolbar/Object_Triangulate",
            //"Toolbar/Object_Subdivide",
            //"Toolbar/Object_ProBuilderize",
            //"Toolbar/Object_Mirror",
            //"Toolbar/Object_Merge",
            //"Toolbar/Object_GenerateUV2",
            //"Toolbar/Pivot_FreezeTransform",
            //"Toolbar/Object_FlipNormals",
            //"Toolbar/Object_ConformNormals",
            //"Toolbar/Pivot_CenterOnObject",
            "Toolbar/DragSelect_Off",
            "Toolbar/DragSelect_On",
            //"Toolbar/Vert_Weld",
            //"Toolbar/Face_Triangulate",
            //"Toolbar/Face_Subdivide",
            //"Toolbar/Edge_Subdivide",
            //"Toolbar/Vert_Split",
            //"Toolbar/Pivot_CenterOnElements",
            //"Toolbar/OffsetElements",
            //"Toolbar/Face_Merge",
            //"Toolbar/Edge_InsertLoop",
            //"Toolbar/Face_FlipNormals",
            //"Toolbar/Face_FlipTri",
            //"Toolbar/Edge_FillHole",
            //"Toolbar/Edge_Extrude",
            //"Toolbar/Face_Duplicate",
            //"Toolbar/Face_Detach",
            //"Toolbar/Face_Delete",
            //"Toolbar/Vert_Connect",
            //"Toolbar/Edge_Connect",
            //"Toolbar/Face_ConformNormals",
            //"Toolbar/Vert_Collapse",
            //"Toolbar/Edge_Bridge",
            //"Toolbar/Edge_Bevel",
            //"Toolbar/Object_Export",
            //"Toolbar/Panel_VertColors",
            //"Toolbar/Panel_UVEditor",
            //"Toolbar/Panel_Smoothing",
            //"Toolbar/Panel_Materials",
        };

    static IEnumerable<string> GetAllIconPaths()
    {
        var seen = new HashSet<string>();
        foreach (var path in k_AllLiteralIconPaths)
        {
            if (seen.Add(path))
                yield return path;
        }

        // Dynamic paths: EditorShapeUtility uses "Tools/ShapeTool/" + name for each shape type
        foreach (var name in EditorShapeUtility.shapeTypes)
        {
            var path = "Tools/ShapeTool/" + name;
            if (seen.Add(path))
                yield return path;
        }
    }

    [Test]
    [TestCaseSource(nameof(GetAllIconPaths))]
    public void GetIcon_ReturnsNonNull(string iconPath)
    {
        var texture = IconUtility.GetIcon(iconPath);
        Assert.That(texture, Is.Not.Null, $"IconUtility.GetIcon(\"{iconPath}\")");
    }

    [Test]
    public void GetIcon_WithInvalidPath_ReturnsNull()
    {
        var texture = IconUtility.GetIcon("NonExistent/Icon");
        Assert.That(texture, Is.Null);
    }
}
