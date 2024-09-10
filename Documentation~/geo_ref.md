# Geometry options reference

This table lists all the geometry options you can use with a ProBuilder object, and indicates which [Edit modes](modes.md) support them. 

These actions are available:

* From the **main menu** > **Tools** > **ProBuilder** > **Geometry**.
* From the context menu: 
    1. In the **Tools** overlay, set the working context to ProBuilder. 
    1. In the **Tool Settings** overlay, select an edit mode.
    1. In the **Scene** view, right-click on a ProBuilder object.

| **Property** | **GameObject tool context** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- | ---- |
| [Bevel Edges](Edge_Bevel.md) (and faces) | | | x | x |
| [Bridge Edges](Edge_Bridge.md) | | | x | |
| [Collapse Vertices](Vert_Collapse.md) | | x | | |
| [Conform Face Normals](Face_ConformNormals.md) (Faces) | | | | x |
| [Delete Faces](Face_Delete.md) | | | | x |
| [Detach Faces](Face_Detach.md) | | | | x |
| [Duplicate Faces](Face_Duplicate.md) | | | | x |
| [Export](Object_Export.md) | x | | | |
| [Extrude Edges](Edge_Extrude.md) | | | x | |
| [Extrude Faces](Face_Extrude.md) | | | | x |
| [Fill Hole](FillHole.md) | | x | x | |
| [Flip Face Edge](Face_FlipTri.md)  | | | | x |
| [Flip Face Normals](Face_FlipNormals.md) | | | | x |
| [Insert Edge Loop](Edge_InsertLoop.md)  | | | | |
| [Merge Faces](Face_Merge.md) | | | | x |
| [Offset Elements](Offset_Elements.md) |  | x | x | x |
| [Set Pivot To Selection](SetPivot.md) | | x | x | x |
| [Smart Connect (Connect Edges)](Edge_Connect.md) | | | x | |
| [Smart Connect (Connect Vertices)](Vert_Connect.md) | | x | | |
| [Smart Subdivide (Subdivide Edges)](Edge_Subdivide.md) | | | x | |
| [Smart Subdivide (Subdivide Faces)](Face_Subdivide.md) | | | | x |
| [Split Vertices](Vert_Split.md) | | x | | |
| [Triangulate Faces](Face_Triangulate.md) | | | | x |
| [Weld Vertices](Vert_Weld.md) | | x | | |


<!--
The Editor:

* Has only one extrude - I should merge them
* Doesn't have Lightmap UVs
-->