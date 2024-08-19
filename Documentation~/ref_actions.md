# Action reference

This table lists all the actions you can apply to a ProBuilder object, and indicates which [Edit modes](modes.md) support them. 

These actions are available:

* From the **main menu** > **Tools** > **ProBuilder**. The actions are grouped in **Selection**, **Interaction**, **Object**, and **Geometry**.
* From the context menu: 
    1. In the **Tools** overlay, set the working context to ProBuilder. 
    1. In the **Tool Settings** overlay, select an edit mode.
    1. In the **Scene** view, right-click on a ProBuilder object.

## Selection actions

These options are never available in the GameObject tool context.

| **Property** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- |
| [Grow Selection](Selection_Grow.md) | x | x | x |
| [Select Hole](Selection_SelectHole.md) | x | x | |
| [Select Loop (in Edge Selection mode)](Selection_Loop_Edge.md) | | x | |
| [Select Loop (in Face Selection mode)](Selection_Loop_Face.md) | | | x |
| [Select Material](Selection_SelectByMaterial.md) | | | x |
| [Select Ring (in Edge Selection mode)](Selection_Ring_Edge.md) | | x | |
| [Select Ring (in Face Selection mode)](Selection_Ring_Face.md) | | | x |
| [Select Vertex Color](Selection_SelectByVertexColor.md) | x | x | x |
| [Shrink Selection](Selection_Shrink.md) | | x | x | x |

The following action is available only through its keyboard shortcut, and never in the GameObject tool context:

| **Property** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- |
| [Select Path](SelectPath.md) | |  | x |


## Interaction actions

| **Property** | **GameObject tool context** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- | ---- |
| [Rect](Selection_Rect_Intersect.md) | | | x | x |
| [Toggle Handle Orientation](HandleAlign.md) | x | x | x | x |
| [Toggle Select Back Faces](Selection_SelectHidden.md) | x | x | x | x |
| [Toggle X Ray](Toggle_X_Ray.md) |  |  |  |  |

## Object actions

| **Property** | **GameObject tool context** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- | ---- |

## Geometry actions


| **Property** | **GameObject tool context** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- | ---- |
| [Bevel Edges](Edge_Bevel.md) (and faces) | | | x | x |
| [Bridge Edges](Edge_Bridge.md) | | | x | |
| [Center Pivot](CenterPivot.md) | x | | | |
| [Collapse Vertices](Vert_Collapse.md) | | x | | |
| [Conform Normals](Face_ConformNormals.md) (Faces) | | | | x |
| [Conform Normals](Object_ConformNormals.md) (Objects) | x | | | |
| [Connect Edges](Edge_Connect.md) | | | x | |
| [Connect Vertices](Vert_Connect.md) | | x | | |
| [Delete Faces](Face_Delete.md) | | | | x |
| [Detach Faces](Face_Detach.md) | | | | x |
| [Duplicate Faces](Face_Duplicate.md) | | | | x |
| [Export](Object_Export.md) | x | | | |
| [Extrude Edges](Edge_Extrude.md) | | | x | |
| [Extrude Faces](Face_Extrude.md) | | | | x |
| [Fill Hole](Edge_FillHole.md) (Edges) | | | x | |
| [Fill Hole](Vert_FillHole.md) (Vertices) | | x | | |
| [Flip Face Edge](Face_FlipTri.md)  | | | | x |
| [Flip Face Normals](Face_FlipNormals.md) | | | | x |
| [Flip Normals](Object_FlipNormals.md) | x | | | |
| [Freeze Transform](Freeze_Transform.md) | x | | | |
| **Handle** (refer to [Orientation](HandleAlign.md)) | x | x | x | x |
| [Lightmap UVs](Object_LightmapUVs.md) | x | | | |
| [Insert Edge Loop](Edge_InsertLoop.md)  | | | | |
| [Inset](Face_Inset.md) | | | | x |
| [Merge Faces](Face_Merge.md) | | | | x |
| [Merge Objects](Object_Merge.md) | x | | | |
| [Mirror Objects](Object_Mirror.md) | x | | | |
| [Offset Elements](Offset_Elements.md) |  | x | x | x |
| [ProBuilderize](Object_ProBuilderize.md) | x | | | |

| [Set Collider](Entity_Trigger.md#Collider) | x | | | |
| [Set Pivot To Selection](SetPivot.md) | | x | x | x |
| [Set Trigger](Entity_Trigger.md) | x | | | |
| [Shift](Selection_Shift.md) | | x | x | x |
| [Split Vertices](Vert_Split.md) | | x | | |
| [Subdivide Edges](Edge_Subdivide.md) | | | x | |
| [Subdivide Faces](Face_Subdivide.md) | | | | x |
| [Subdivide Object](Object_Subdivide.md) | x | | | |
| [Triangulate Faces](Face_Triangulate.md) | | | | x |
| [Triangulate](Object_Triangulate.md) (Object) | x | | | |
| **Turn Edges** (see [Flip Face Edge](Face_FlipTri.md)) |  | | | x |
| [Weld Vertices](Vert_Weld.md) | | x | | |