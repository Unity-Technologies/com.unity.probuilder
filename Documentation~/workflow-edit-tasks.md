# Common editing tasks



## Object-specific tasks

* **Create a coffee mug**: start with a [cylinder](Cylinder.md), [select all faces](Selection_Grow.md) on the top and [merge them](Face_Merge.md). Next, create a slight [inset](Face_Inset.md) on the merged top, and [extrude it](Face_Extrude.md) all the way down. Finally, create a half-[torus](Torus.md), rotate it, move it next to the cylinder for the handle and [merge](Object_Merge.md) the torus and cylinder together.
* **Build a bed**: start with a rectangular [cube](Cube.md) for the mattress. Create smaller rectangles for the legs of the bed, and then [merge](Object_Merge.md) everything together to make one single Mesh.
* **Make a bottle**: start with a a [cylinder](Cylinder.md). [Merge the faces](Face_Merge.md) on the top end, then [extrude the face](Face_Extrude.md) up, scale it to the size of a neck, and extrude it up again.

## Building-specific tasks

- **Make a hole** (for a window or door): There are many methods you can use to do this, but some work better depending on what you are working on. For example, in an even and rectangular wall, such as on the first floor of a house, you could use this strategy:

  - Select the two faces of the wall (back and front), then **Shift**+**Scale** to create an [inset](Face_Inset.md) horizontally, and **Scale** vertically to make it an even border.
  - Press **Backspace** to delete the insets.

  On the other hand, if you are working on an uneven or multi-sided wall, such as a castle or church wall, this strategy is preferable:

  * Use the [Insert Edge Loop](Edge_InsertLoop.md) action to create two vertical edges. Do the same on the horizontal plane (two for a window, one for a door).
  * Adjust the loops so that the resulting hole is the right size and location for a window or a door.
  * Select the face of the hole and press **Backspace**. If necessary, from the other side of the wall, [delete the face](Face_Delete.md) on the other side.  

  Whichever strategy you start with, you need to weld the newly exposed edges and vertices together:

  * Select all the edges on one side only of the hole and use **Shift**+**Translate** until they connect up with the edge on the other side.
  * Then [weld the vertices](Vert_Weld.md) together where the edges meet.

- **Add a door**: follow the "Make a hole" procedure, but to start, create a [Door shape](Door.md). Fit the hole to match, [merge the two objects](Object_Merge.md), and then [weld the door to the frame](Vert_Weld.md) you created.

- **Make a tunnel with normals on both the inside and outside**: duplicate the tunnel object and scale it slightly so that you [bridge the edges](Edge_Bridge.md). Next, [flip the normals](Object_FlipNormals.md) on the smaller one.

- **Make a building with towers**: start with a [cube](Cube.md) and [inset](Face_Inset.md) on the top face, then [extrude](Face_Extrude.md) upward. Repeat this as many times as you like. You could also [subdivide the top face](Face_Subdivide.md) to create multiple extrusions.

<!--

<span style="color:red">@TODO: Follow up with Gabriel to get some good examples for using the Cut tool to make either objects or buildings for this section  </span>

-->

## Finding more inspiration

Use the Unity ProBuilder channel to find videos that demonstrate how to use the tools effectively:

* [ProBuilder Simple Objects - Crates and Barrels](https://www.youtube.com/watch?v=lmLG4nC9tm0)
* [ProBuilder Building Structures with Interior and Exterior](https://www.youtube.com/watch?v=CBa_opm3_GM)
* [Prototyping a "Medieval House" in Unity with ProBuilder3D](https://www.youtube.com/watch?v=xEEUhSyrq7M)
* [ProBuilder Greyboxing an Interior FPS Level](https://www.youtube.com/watch?v=dYBOBgfcTgY)
* [Unity at GDC - Rapid worldbuilding with ProBuilder](https://www.youtube.com/watch?v=7k-81UEluyg)



<!--

<span style="color:red">@TODO: Follow up with Gabriel to get some more up to date videos for this section. </span>

-->
