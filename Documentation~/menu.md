# The ProBuilder menu

Use the ProBuilder menu to access most of the ProBuilder editors, actions, tools, and features that are only available through this menu. For example, you can repair and debug ProBuilder, or access the experimental [Boolean operations](boolean.md).

## Editors

Create new shapes and open editor windows.

| **Menu item** | **Description** |
| --- | --- |
| **New Bezier Shape** | Use the **Bezier Shape** tool to define a Bezier curve around which ProBuilder extrudes a Mesh. **Warning:** Use **New Bezier Shape** with caution. This tool is experimental and still under development. If you use this tool, it might reduce ProBuilder's stability. For more information, refer to [Bezier Shape tool](bezier.md). |
| **New Poly Shape** | Create custom ProBuilder mesh shapes. For more information, refer to [Poly Shape tool](shape-tool.md). | 
| **New Shape** | Create new ProBuilder mesh shapes such as cylinders, arches, and stairs. For more information, refer to [Shape tool](shape-tool.md). |
| **Open Lightmap UV Editor** | Change the settings for generating lightmap UVs. For more information, refer to [Lightmap UV Editor](lightmap-uv.md). |
| **Open Material Editor** | Apply materials to objects or faces. For more information, refer to [Material Editor](material-tools.md). |
| **Open Smoothing Editor** | Create smooth and rounded or sharp and hard cornered edges. For more information, refer to [Smooth Group Editor](smoothing-groups.md). |
| **Open UV Editor** | Apply textures to objects or faces. For more information, refer to [UV Editor](uv-editor.md). |
| **Open Vertex Color Editor** | Apply or paint vertex colors onto meshes. For more information, refer to [Vertex Colors](vertex-colors.md). |
| **Open Vertex Position Editor** | Enter specific translation coordinates to modify vertex positions. For more information, refer to [Positions Editor](vertex-positions.md). |

## Edit

Edit shapes.

| **Menu item** | **Description** |
| --- | --- |
| **Edit PolyShape** | Edit a poly shape you created with **Tools** > **ProBuilder** > **Editors** > **New PolyShape**.  |
| **Edit shape** |  Edit a shape you created with **Tools** > **ProBuilder** > **Editors** > **New Shape**. |

## Dimensions Overlay

Show or hide the dimensions for all three axes. 

This overlay appears on all mesh objects, not just ProBuilder meshes.

## Selection

Change your selections.

| **Menu item** | **Description** |
| --- | --- |
| **Grow Selection** | Expand the selection outward to adjacent faces, edges, or vertices. For more information, refer to [Grow Selection](Selection_Grow.md). |
| **Select Hole** | Select all elements along the selected open vertex or edge. For more information, refer to [Select Holes](Selection_SelectHole.md). |
| **Select Loop** | Select an edge loop from each selected edge in Edge editing mode, or a face loop from each selected face in Face editing mode. |
| **Select Material** | Select all faces that have the same material as the selected face(s). For more information, refer to [Select by Material](Selection_SelectByMaterial.md). |
| **Select Ring** | Select a ring from each selected edge in Edge editing mode, or a face ring from each selected face in Face editing mode. |
| **Select Smoothing Group** | Select all faces that belong to the currently selected smoothing group. For more information, refer to [Select Smoothing Group](Selection_SmoothingGroup.md). |
| **Select Vertex Color** | Select all faces on this object that have the same vertex color as the selected face. For more information, refer to [Select by Colors](Selection_SelectByVertexColor.md). |
| **Shrink Selection** | Remove the elements on the perimeter of the current selection. For more information, refer to [Shrink Selection](Selection_Shrink.md). |

## Interaction

Interact with your selections.

| **Menu item** | **Description** |
| --- | --- |
| **Toggle Drag Rect Mode** | To limit the drag selection to elements that are fully inside the drag rectangle, set the **Rect** action to **Complete**. For more information, refer to [Rect](Selection_Rect_Intersect.md). |
| **Toggle Handle Orientation** | Toggle between the three orientation states for Scene handles (**Global**, **Local**, or **Normal**). For more information, refer to [Orientation](HandleAlign.md). |
| **Toggle Select Back Faces** | Define whether drag selection selects or ignores hidden elements. For more information, refer to [Select Hidden](Selection_SelectHidden.md). |
| **Toggle X Ray** | Show or hide any selected hidden geometry. For more information, refer to [Selection X-Ray](preferences.md#sel-xray). |

**Note:** The **Toggle Drag Selection Mode** option is now in the **Tool Settings** overlay.

## Object

Edit a ProBuilder object.

| **Menu item** | **Description** |
| --- | --- |
| **Center Pivot** | Move the pivot point for the mesh to the center of the object’s bounds. For more information, refer to [Center Pivot](CenterPivot.md). |
| **Conform Object Normals** | Set all face normals to the same relative direction. For more information, refer to [Conform Normals](Object_ConformNormals.md). |
| **Flip Object Normals** | Flip the normals of **all** faces on the selected object(s). For more information, refer to [Flip Normals](Object_FlipNormals.md). |
| **Freeze Transform** | Set the selected object's position, rotation, and scale to world-relative origin. For more information, refer to [Freeze Transform](Freeze_Transform.md). |
| **Merge Objects** | Merge two or more selected objects. For more information, refer to [Merge Objects](Object_Merge.md). |
| **Mirror Objects** | Create mirrored copies of objects. For more information, refer to [Mirror Objects](Object_Mirror.md). |
| **Pro Builderize** | Convert the selected object(s) into objects you can edit in ProBuilder. For more information, refer to [ProBuilderize](Object_ProBuilderize.md). |
| **Set Collider** | Assign the **Collider Behaviour** script to selected objects. For more information, refer to [Set Collider](Entity_Trigger.md#Collider). |
| **Set Trigger** | Assign the **Trigger Behaviour** script to selected objects. For more information, refer to [Set Trigger](Entity_Trigger.md). |
| **Subdivide Object** | Divide every face on selected objects. For more information, refer to [Subdivide Object](Object_Subdivide.md). |
| **Triangulate Object** | Reduce all polygons to their base triangles. For more information, refer to [Triangulate](Object_Triangulate.md). |

## Geometry

Use Vertex, Edge, and Face edit mode actions.

| **Menu item** | **Description** |
| --- | --- |
| **Bevel Edges** | Bevel every edge on the selected face(s). For more information, refer to [Bevel](Face_Bevel.md). |
| **Bridge Edges** | Create a new face between two selected edges. For more information, refer to [Bridge Edges](Edge_Bridge.md). |
| **Collapse Vertices** | Collapse all selected vertices to a single point, regardless of distance. For more information, refer to [Collapse Vertices](Vert_Collapse.md). |
| **Conform Face Normals** | Set all selected face normals to the same relative direction. For more information, refer to [Conform Normals](Face_ConformNormals.md). |
| **Delete Faces** | Delete the selected face(s). For more information, refer to [Delete Faces](Face_Delete.md). |
| **Detach Faces** | Detach the selected face(s) from the rest of the mesh. For more information, refer to [Detach Faces](Face_Detach.md). |
| **Duplicate Faces** | Copy each selected face and either move it to a new mesh or leave it as a submesh. For more information, refer to [Duplicate Faces](Face_Duplicate.md). |
| **Extrude** | Push a new edge out from each selected edge in Edge edit mode, or pull out the currently selected face and attach sides to each edge in Face edit mode. For more information, refer to [Extrude Edges](Edge_Extrude.md) and [Extrude Faces](Face_Extrude.md). |
| **Fill Hole** | Create a new face that fills any holes that touch the selected vertices or edges. For more information, refer to [Fill Hole (vertices)](Vert_FillHole.md) and [Fill Hole (edges)](Edge_FillHole.md). |
| **Flip Face Edge** | Swap the triangle orientation on the selected face(s) with four sides. For more information, refer to [Flip Face Edge](Face_FlipTri.md). |
| **Flip Face Normals** | Flip the normals only on the selected face(s). For more information, refer to [Flip Face Normals](Face_FlipNormals.md). |
| **Insert Edge Loop** | Add a new edge loop from the selected edge(s). For more information, refer to [Insert Edge Loop](Edge_InsertLoop.md). |
| **Merge Faces** | Merge selected faces into a single face, and remove any dividing edges. For more information, refer to [Merge Faces](Face_Merge.md). |
| **Offset Elements** | Move the selected vertex or vertices in Vertex edit mode, the selected edge(s) in Edge edit mode, or the selected face(s) in Face edit mode. For more information, refer to [Offset Elements](Offset_Elements.md). |
| **Set Pivot To Selection** | Move the pivot point of the mesh to the average center of the selected faces. For more information, refer to [Set Pivot](Face_SetPivot.md). |
| **Smart Connect** | Create a new edge connecting the selected vertices in Vertex edit mode, or insert an edge connecting the centers of each selected edge in Edge edit mode. For more information, refer to [Connect Vertices](Vert_Connect.md) and [Connect Edges](Edge_Connect.md). |
| **Smart Subdivide** | Divide the selected edge(s) in Edge edit mode, or add a vertex at the center of each edge and connect them in the center in Face edit mode. For more information, refer to [Subdivide Edges](Edge_Subdivide.md) and [Subdivide Faces](Face_Subdivide.md). |
| **Split Vertices** | Split a single vertex into multiple vertices (one per adjacent face). For more information, refer to [Split Vertices](Vert_Split.md). |
| **Triangulate Faces** | Reduce selected faces to their base triangles. For more information, refer to [Triangulate Faces](Face_Triangulate.md). |
| **Weld Vertices** | Merge selected vertices within a specific distance of one another. For more information, refer to [Weld Vertices](Vert_Weld.md). |

## Materials

Apply material presets to the selected object or element. 

To define presets, refer to [Material Editor window](material-tools.md).

## Vertex Colors

Apply Vertex Color presets to the selected object or element.

To define presets, refer to [Vertex Colors window](vertex-colors.md).

## Experimental

Activate the [Boolean](boolean.md) experimental feature. It lets you create new meshes from intersection, union, and subtraction Boolean operations.

> **Note**: This submenu is only available when you enable the [experimental features preference](preferences.md#experimental). Experimental features aren’t ready for public use, but are included for users to try out early, and report issues/feedback. 

##  Repair

Repair problems with ProBuilder meshes in the scene.

| **Menu item** | **Description** |
| --- | --- |
| **Fix Meshes in Selection** | Checks for degenerate triangles and removes them. A degenerate triangle is a triangle that has collinear vertices or one where two or more vertices are occupy the same point in space. |
| **Rebuild All ProBuilder Objects** | Rebuilds mesh representations from stored ProBuilder data for each GameObject in the scene. If you have a lot of GameObjects in a scene, this can take a while. |
| **Rebuild Shared Indexes Cache** | Discards all shared vertex position data and rebuilds based on proximity. |
| **Check for Broken ProBuilder References** | Checks for and repairs any missing or broken ProBuilder references in the scene. |


## Export

[Export selected ProBuilder Meshes](workflow-exporting.md) in various formats. 

| **Menu item** | **Description** |
| --- | --- |
| **Export Asset** | Saves the selection as a Unity mesh `.asset` files. This format is only readable in Unity. |
| **Export Obj** | Exports the selected object(s) as `.obj` files (Wavefront OBJ format). This is a widely supported model format. It supports multiple Textures and Mesh groups. |
| **Export Ply** | Exports the selected object(s) as `.ply` files (Stanford PLY, or Polygon File Format). This format is generally supported and very extensible. It supports quads and vertex colors, but not multiple materials. |
| **Export Stl Ascii** | Exports the selected object(s) as ASCII `.stl` files (stereolithography, standard tessellation, or standard triangle format). This is a widely supported format, generally used in CAD software or 3D printing. It only supports Triangle geometry. |
| **Export Stl Binary** | Exports the selected object(s) as Binary `.stl` files (stereolithography, standard tessellation, or standard triangle format). This is a widely supported format, generally used in CAD software or 3D printing. It only supports Triangle geometry. |

## Actions

Strip out ProBuilder scripts and leave only the models.

| **Menu item** | **Description** |
| --- | --- |
| **Strip All ProBuilder Scripts in Scene** | Remove all ProBuilder scripts from all GameObjects in the scene. |
| **Strip ProBuilder Scripts in Selection** | Remove all ProBuilder scripts from selected GameObjects. |


## Debug

Change logging preferences. 

| **Menu item** | | **Description** |
| --- | --- | --- |
| **Log Output** | | Define where ProBuilder writes messages to: the Unity Console or to a log File. |
| | **Console** |  Write to the Console. |
| | **File** |  Write to a file. When you select **File**, you can also set the **Log Path** and **open** the log file. |
| | **Log Path** | Set the log file path (select **...***). |
| | **Open** | Open the saved **ProBuilderLog.txt** log file. |
| **Chatty-ness** | | Define which kind of messages ProBuilder logs: Errors, Warnings, General information, Debug messages, or everything. |
| **Clear Log File** | | Reset the saved log file. This action deletes all previously logged messages. |

