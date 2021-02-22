# Creating a Mesh with the Poly Shape tool

Use the **Poly Shape** tool to define a 2-dimensional custom shape (depth and width) and then extrude a 3-dimensional shape to define its height. Like the [Shape tool](shape-tool.md), this is a modal tool that creates a new Mesh but also lets you re-activate the tool to edit the initial shape. 

> **Note**: When you modify control points or settings, you lose any changes you made in editing the Mesh.

![A Poly Shape is a 3-dimensional shape extruded from any 2-dimensional polygon](images/PolyShape_HeaderImage.png)

To define a custom Mesh:

1. Open the ProBuilder window (in Unity's top menu: **Tools** > **ProBuilder window**).

  The [Edit Mode toolbar](edit-mode-toolbar.md) and the [ProBuilder toolbar](toolbar.md) appear.

2. From the ProBuilder toolbar, click **New Poly Shape** (![Poly Shape Icon](images/icons/NewPolyShape.png)). 

  > **Tip:** You can also access this tool from the ProBuilder menu (**Tools** > **ProBuilder** > **Editors** > **New Poly Shape**).

3. Click in the Scene view to create control points. Control points form the outer bounds of your Mesh.

	You can click directly on another surface to create an outgrowth, even when that surface is on the y-axis. 

	![Making a 2D shape on the wall](images/PolyShape_Draw1.png)

4. When you finish placing points, use either **Enter** (Windows), **Return** (macOS), or **Spacebar** to finalize the shape.

	![Extruding the 2D shape into a 3D Mesh](images/PolyShape_Draw2.png)

5. Move the mouse up and down to set the Mesh height. This extrudes a 3D Mesh from the 2D shape you defined with your control points.

6. Click to exit out of create mode. Your new Mesh is now [in editing mode](polyshape.md) so you can continue to modify it.

	![Finished Poly Shape](images/PolyShape_Draw3.png)

