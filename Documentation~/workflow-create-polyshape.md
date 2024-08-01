# Create a mesh with the Poly Shape tool

To create a custom shape, use the **Poly Shape** tool. Draw a 2D custom shape (depth and width) and then extrude that shape into a 3D mesh to define its height. 

![A Poly Shape is a 3D shape extruded from any 2D polygon](images/PolyShape_HeaderImage.png)

To create a custom mesh:

1. Do one of the following:
    * In the **Scene** view, in the **Tools** overlay, select **Create PolyShape**.
    * In the main menu, go to **Tools** > **ProBuilder** > **Editors** > **New PolyShape**.
1. To create the outer bounds of your mesh, click in the **Scene** view to create control points.
    > **Note:** You can click directly on another surface to create an outgrowth, even when that surface is on the y-axis. 
    ![Making a 2D shape on the wall](images/PolyShape_Draw1.png)
1. To finish placing control points, press **Enter** or **Space**.
    ![Extruding the 2D shape into a 3D Mesh](images/PolyShape_Draw2.png)
1. To extrude a 3D shape, move your mouse up or down.
1. To finish the shape, press **Enter** or **Space**. Your new mesh is now [in editing mode](polyshape.md) so you can continue to change it.

![Finished Poly Shape](images/PolyShape_Draw3.png)

To edit a shape you created with the **Poly Shape** tool, in the **Scene** view:

1. Select the shape.
1. From the **Tools** overlay, select **Edit Poly Shape**.

> **Note**: If you apply procedural edits to the mesh, any manual changes are overwritten.
