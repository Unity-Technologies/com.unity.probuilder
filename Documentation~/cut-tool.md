# ![Cut Tool icon](images/icons/Cut_Tool.png) Cut Tool

Use the Cut Tool to easily subdivide mesh faces with precision. To cut out a shape on a mesh, you switch to Cut mode where you can draw the edges that define the cutout shape by adding points to connect them. The cutout shape becomes a new face on the mesh.

To create a new face on an existing mesh face:

1. In the ProBuilder toolbar, click **Cut Tool**. The Cut Tool overlay appears at the bottom of the Scene view.

	> **Tip:** You can also use the **XXX** hotkey to switch to Cut mode, or use the menu (**Tools** > **ProBuilder** > **Geometry** > **Cut Tool**).

	<span style="color:blue;">**@DEVQ1**: <br/>It looks like there is no Cut Tool shortcut/hotkey and it doesn't seem to be accessible from the Custom Tool menu. Are there any plans to add these? </span>

	<span style="color:blue;">**@DEVQ2**: <br/>Also, its tooltip on the Custom Tool menu is Create Poly Shape. Do you want me to add an issue in Github for that? </span>

2. To control snapping, use the options on the [Cut Tool overlay](#cut_overlay).

3. Click on the mesh face where you want to define the vertices for the new face. For example, you can specify three points on the mesh to define a triangular shape and the fourth to close it:

	<span style="color:red;">[@TODO: image to demonstrate this]</span>

4. When you are satisfied with the shape you drew, click **Cut** to complete the cut. The new face appears selected in the Scene view and the Cut Tool overlay now displays a **Start** button. 

	<span style="color:red;">[@TODO: add warning about exiting cut mode if your cut doesn't work]</span>

5. If you want to cut out another face on the same mesh, click the **Start** button.

6. To exit Cut mode, press the ESC key or click Cut Tool on the ProBuilder toolbar.

	**Tip**: When you create a new face, the Cut Tool creates extra edges. To simplify the geometry, select the meshes around the new face and merge them with the [Merge Faces](Face_Merge.md) tool. For example, if you cut out a diamond on the side of a cube, you can merge the other faces to leave only two faces.

<span style="color:blue;">@DEVQ3:<br/>Does this create overly complicated geometry to suggest this?</span>



<a name="cut_overlay"></a>

## Cut Tool overlay

When ProBuilder enters Cut mode, the following overlay appears at the bottom of the Scene view:

![Extra options for the Cut Tool](images/icons/Cut_Tool-overlay.png) 

These options let you control whether and how closely the points you draw on the target face will snap to the grid:

| **Overlay control:**                    | **Description:**                                             |
| --------------------------------------- | ------------------------------------------------------------ |
| **Snap to existing edges and vertices** | Enable this option to snap points to the grid.               |
| **Snapping distance**                   | Enter how closely you want to the points you specify to follow the grid. |
| **Cut** or **Start** button             | While you specify points on the mesh, you can click the **Cut** button to complete the new face. As soon as you complete the cut, the **Start** button appears.<br /><br />When you are ready to define points for a new cutout, click the **Start** button and begin specifying points. |

