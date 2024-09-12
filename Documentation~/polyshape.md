# New PolyShape and Edit PolyShape tools

To create custom shapes, use the **New PolyShape** tool. Draw a 2D shape with as many points as you need, then extrude it to 3D. You can continue to edit the shape once you've extruded it to make it more complex. 

**Tip:** To subdivide a face, refer to [**The Cut** tool](cut-tool.md).

## Create a PolyShape

To create a new PolyShape:

1. Activate the **New PolyShape** tool in one of the following ways:
	* In the **Tools** overlay, select **New PolyShape**.
	* In the main menu, go to **Tools** > **ProBuilder** > **Editors** > **New PolyShape**.

	For information about the **PolyShape Settings** panel, refer to [PolyShape Settings panel](#the-polyshape-settings-pnael).
1. Draw points in the **Scene** view to create the 2D outline of the shape. To help you draw in an exact location, use [Snap to Grid](snap-to-grid.md).
    > **Tip:** You can add shapes next to each other, but they stay separate objects. To merge them, use the [Merge Objects](Object_Merge.md) tool.
1. When you connect the last point to the first point, you can extrude the shape to create a 3D object.
	
The **New PolyShape** tool stays active until you deactivate it, so you can create many shapes in a sequence. 

## Edit a PolyShape

To edit an existing PolyShape:

1. Select the PolyShape in the **Scene** view.
1. In the **Tools** overlay, set the active context to ProBuilder. 
1. In the **Tools** overlay, click **Edit ProBuilder Shape** to:
    * Move existing control points.
    * Delete control points. Select a point and press **Backspace** (macOS: **Delete**).
    * Click along the perimeter line to add new control points.
    * Click and drag the handle in the center of the mesh to set the height.

## The PolyShape Settings panel

Use the **PolyShape Settings panel** to:

| **Property** | **Description** |
| --- | --- |
| **Extrusion** | Set a height value for the mesh. |
| **Flip Normals** | Toggle to display the interior or exterior of the mesh. |

**Tip:** These options are also available in the **Inspector** window.
