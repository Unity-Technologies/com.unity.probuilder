# New PolyShape and Edit PolyShape tools

Use the PolyShape tool to [create a custom polygonal shape](workflow-create-polyshape.md). When you activate the PolyShape tool to create a new shape, the **PolyShape Tool** panel appears in the bottom of the Scene view. After you initially [create a PolyShape](workflow-create-polyshape.md), the PolyShape is still active, but in editing mode:

![The PolyShape tool in editing mode](images/Experimental_PolyShapeInspector.png) 

**(A)** You can toggle the ![](images/icons/tlbx-icon-polyshape.png) Edit PolyShape button on the **Tools** panel to toggle the PolyShape editing mode on and off.

**(B)** You can modify the **Extrusion** (height of the Mesh) and the **Flip Normals** properties on the **PolyShape** component.

**(C)** The **PolyShape Tool** panel lets you the modify the **Extrusion** and the **Flip Normals** properties too. The **Quit Editing** button exits the PolyShape editing mode.

While the PolyShape editing mode is active, you can also modify the base shape by adding, deleting, or moving any of the points that define the PolyShape.

>  **Note**: If you are in editing mode immediately after creating a new PolyShape Mesh, you can also click **New PolyShape** (![PolyShape Icon](images/icons/NewPolyShape.png)) on the ProBuilder toolbar to exit the PolyShape tool.




## Editing a PolyShape

If you exited the PolyShape tool, you can re-activate it to modify the **Extrusion** and the **Flip Normals** properties. To re-activate the PolyShape editing mode: 

1. Select the PolyShape you want to modify. The **Tools** panel displays the ![](images/icons/tlbx-icon-polyshape.png) Edit PolyShape button and the **PolyShape** component appears in the Inspector with the **Edit PolyShape** button.

	![The Tools panel in the Scene view (A) and the PolyShape component in the Inspector](images/Experimental_PolyShapeInspector-edit.png)

2. Click either button to activate the PolyShape editing mode.

> **Note:** You can modify Mesh elements on the PolyShape Mesh with the standard ProBuilder editing tools. However, each time you re-enter PolyShape editing mode, you lose any element changes.

To modify the shape in PolyShape editing mode, perform the following tasks in the Scene view:

- Click and drag existing control points to move them around.
- Click existing control points to select them, then use **Backspace** (Windows) or **Delete** (macOS) to remove the points from the shape.
- Click along the perimeter line to add new control points.
- Click and drag the handle in the center of the Mesh to set the height.

You can also use the controls in the **PolyShape** component in the Inspector to:

- Enter a value to use for the height of the Mesh in the **Extrusion** property.
- Enable or disable the **Flip Normals** option to toggle whether the Camera displays the interior or exterior of the Mesh.

