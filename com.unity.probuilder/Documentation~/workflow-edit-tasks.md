# Common editing tasks

This section provides some ideas of how you can use the ProBuilder tools to create geometry, but there any many more possibilities. Use these ideas to get started and develop your own workflows.

## General modeling tasks

- **Transforming:** By translating, rotating, and scaling geometry elements you can change the shape of a basic Mesh. For example, if you used the basic Stair shape but you want to give it a crooked look, you could move edges or vertices.

- **Extruding:** Click the ![Face edit mode](images/icons/EditModes_Face.png) [Face editing mode](modes.md) button on the Edit modes toolbar, select a face, hold Shift, and use the Transform controls to [drag the face out](Face_Extrude.md) from the Mesh.

  Alternatively, you can [extrude edges](Edge_Extrude.md) (for example, if you wanted to build a ski slope, you could extrude an edge and then [smooth it](smoothing-groups.md)).

- **Insetting**: Select a face, hold Shift, and use the Scale controls to drag the edges of the face inside the Mesh. Then drag in the opposite direction without the Shift modifier to complete the [inset](Face_Inset.md).

- **Subdividing edges**: If you have an irregular shape (for example, a wall with a peaked top) and you need to split it into four faces (for example, to insert windows), you can [insert an edge loop](Edge_InsertLoop.md) to split the whole wall into two mirrored pieces and then select only those edges along the front and back of the wall. 

	[Subdividing](Edge_Subdivide.md) the two selected edges then creates a third vertex which you can connect up with the vertices at the base of the peaked section to create four perfectly even sections on the front and the back. This approach is much easier than trying to subdivide a five-sided polygon.

- **Boolean operations**: Some geometry is hard to create just by moving faces, edges, and vertices. The [Boolean](boolean.md) tool allows you to quickly combine two Meshes together to create a new Mesh. The final Mesh is either the addition of the two, the difference between the two, or only the common geometry between them, depending on the mode.

- **Selecting**: Selecting elements is the first step in changing your Mesh. ProBuilder provides a whole range of [selection tools](selection-tools.md) that make it easier to target the elements you need to access. As a shortcut when working in the Face editing mode, you can double-click any face to select all faces on the Mesh.

## Object-specific tasks

* **Create a coffee mug** by starting with a [cylinder](Cylinder.md), [selecting all faces](Selection_Grow.md) on the top and [merging them](Face_Merge.md). Then create a slight [inset](Face_Inset.md) on the merged top, and [extrude it](Face_Extrude.md) all the way down. Finally, create a half-[torus](Torus.md), rotate it, move it next to the cylinder for the handle and [merge](Object_Merge.md) the torus and cylinder together.
* **Build a bed** using a rectangular [cube](Cube.md) for the mattress, creating smaller rectangles for the legs of the bed, and then [merge](Object_Merge.md) everything together to make one single Mesh.
* **Make a bottle** from a [cylinder](Cylinder.md), [merge the faces](Face_Merge.md) on the top end, then [extrude the face](Face_Extrude.md) up, scale it to the size of a neck, and extrude it up again.

## Building-specific tasks

- **Make a hole** (for a window or door). There are many methods you can use to do this, but some work better depending on what you are working on. For example, in an even and rectangular wall, such as on the first floor of a house, you could use this strategy: 

  - Select the wall's two faces (back and front), then **Shift**+**Scale** to create an [inset](Face_Inset.md) horizontally and **Scale** vertically to make it an even border. 
  - Press **Backspace** to delete the insets. 

  On the other hand, if you are working on an uneven or multi-sided wall, such as a castle or church wall, this strategy is preferable: 

  * Use the [Insert Edge Loop](Edge_InsertLoop.md) tool to create two vertical edges. Do the same on the horizontal plane (two for a window, one for a door). 
  * Adjust the loops so that the resulting hole is the right size and location for a window or a door. 
  * Select the face of the hole and press **Backspace**. If necessary, from the other side of the wall, [delete the face](Face_Delete.md) on the other side.  

  Whichever strategy you start with, you need to weld the newly exposed edges and vertices together:

  * Select all the edges on one side only of the hole and use **Shift**+**Translate** until they connect up with the edge on the other side. 
  * Then [weld the vertices](Vert_Weld.md) together where the edges meet.

- **Add a door** by following the *Make a hole* procedure but start by creating a [Door shape](Door.md), fit the hole to match, [merge the two objects](Object_Merge.md), and then [weld the door to the frame](Vert_Weld.md) you created.

- **Make a tunnel with normals on both the inside and outside** by duplicating the tunnel object, scaling it slightly, [bridging the edges](Edge_Bridge.md), and then [flipping the normals](Object_FlipNormals.md) on the smaller one. 

- **Make a building with towers** by starting with a [cube](Cube.md), and then [insetting](Face_Inset.md) on the top face, followed by [extruding](Face_Extrude.md) upward. Repeat this as many times as you like. You could also [subdivide the top face](Face_Subdivide.md) to create multiple extrusions.

## Finding more inspiration

Use the Unity ProBuilder channel to find lots of great videos that demonstrate how to use the tools effectively:

* [ProBuilder Simple Objects - Crates and Barrels](https://www.youtube.com/watch?v=lmLG4nC9tm0)
* [ProBuilder Building Structures with Interior and Exterior](https://www.youtube.com/watch?v=CBa_opm3_GM)
* [Prototyping a "Medieval House" in Unity with ProBuilder3D](https://www.youtube.com/watch?v=xEEUhSyrq7M)
* [ProBuilder Greyboxing an Interior FPS Level](https://www.youtube.com/watch?v=dYBOBgfcTgY)
* [Unity at GDC - Rapid worldbuilding with ProBuilder](https://www.youtube.com/watch?v=7k-81UEluyg)