# Set Pivot To Selection

Use the __Set Pivot__ action to move the pivot point of a mesh to the average center of the selected faces, edges, or vertices.

To set a pivot point:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select an edit mode. All three modes support this action.
1. Hold **Shift** to select the faces, edges, or vertices to average. 
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) on the selected face or edge and select **Set Pivot to Selection**.
    * From the main menu, select **Tools** > **ProBuilder** > **Geometry** > **Set Pivot To Selection**.

![Centering the pivot on selected faces of a tower made of three merged blocks. The left panel shows the pivot at the center of the mesh. The middle panel shows the faces on the top of the tower selected. The right panel shows the pivot point at the top of the tower.](images/Face_SetPivot.png)

This example sets a new pivot point in a tower made of three merged blocks:

* **Left panel**: The pivot point of the tower is at the center of the entire mesh.
* **Middle panel**: The top faces are selected. The Set Pivot action changes the pivot to the center of those top faces.
* **Right panel**: The pivot point is now at the top.
