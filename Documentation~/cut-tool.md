# ![Cut Tool icon](images/icons/Cut_Tool.png) Cut tool

Use the Cut tool to easily subdivide mesh faces with precision. To cut out a shape on a mesh, you activate the Cut tool which lets you define the cutout shape with points. The cutout shape becomes a new face on the mesh.

To create a new face on an existing mesh face:

1. In the ProBuilder toolbar, click **Cut Tool**. The **Cut Tool** overlay appears at the bottom of the Scene view. If the ProBuilder toolbar displays text buttons, the button text background becomes red.

  <span style="color:blue">**@DEV**: On the ProBuilder toolbar in Text Mode, the Cut tool appears as **Cut Tool** literally, whereas the New Shape tool appears as **New Shape** and the Poly Shape tool appears as **New Poly Shape**. For UX consistency, would you consider using **Cut** instead?</span>

  > **Tip:** You can also use the **XXX** hotkey to activate the Cut tool, or use the menu (**Tools** > **ProBuilder** > **Geometry** > **Cut Tool**).

  <span style="color:blue;">**@DEV**: <br/>It looks like there is no Cut tool shortcut/hotkey and it doesn't seem to be accessible from the Custom Tool menu. Are there any plans to add these? </span>

2. To control snapping, use the options on the [Cut Tool overlay](#cut_overlay).

3. Click on the mesh face where you want to the vertices for the new face to be. ProBuilder creates the cutout shape based on the the edges you draw with these points. For example, you can specify three points on the mesh to define a triangular shape and the fourth to close it:

	![Example of a triangular cutout on a face](images/cut-tool-example.png)

4. When you are satisfied with the shape you drew, click **Cut** to complete the cut. The new face appears selected in the Scene view and the Cut Tool overlay now displays a **Start** button.

	**Note**: If you click the **Cut** button and nothing happens, it is probably because the points you defined do not create a valid edge or face. For example, if the tool detected only one point, that does not make a valid edge. Define more points or exit the Cut tool to cancel the operation.

5. If you want to cut out another face on the same mesh, click the **Start** button.

6. To exit the Cut tool, select the Esc key or click **Cut Tool** on the ProBuilder toolbar.

	**Tip**: When you create a new face, the Cut tool creates extra edges. To simplify the geometry, select the meshes around the new face and merge them with the [Merge Faces](Face_Merge.md) tool. For example, if you cut out a diamond on the side of a cube, you can merge the other faces to leave only two faces.

<span style="color:blue;">@DEVQ3:<br/>Does this create overly complicated geometry to suggest this?</span>



<a name="cut_overlay"></a>

## Cut Tool overlay

When ProBuilder enters Cut mode, the following overlay appears at the bottom of the Scene view:

![Extra options for the Cut Tool](images/icons/Cut_Tool-overlay.png) 

Enable the **Snap to existing edges and vertices** option to snap the points you draw on the target face to the grid. When snapping is enabled, the **Snapping distance** value you set defines how closely those points follow the grid.

When you activate the Cut tool, overlay displays a **Cut** button. As soon as you complete the cut successfully, the **Start** button appears because the Cut tool is modal and you have the choice to either define another cutout or explicitly exit the tool:

* When you are ready to define points for a new cutout, click the **Start** button and draw more points on the mesh.
* When you want to exit the tool, click an action on the ProBuilder toolbar or select Esc.
