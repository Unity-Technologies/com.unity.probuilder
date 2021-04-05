# Selection actions

This part of the ProBuilder toolbar provides access to selection modifiers and actions.

![Selection buttons on the ProBuilder toolbar](images/tool_selection.png)



Depending on whether you have the [Toolbar display mode](toolbar.md#buttonmode) set to **text** mode or **icon** mode, the selection action button displays the following to indicate what state the action is in:

* In **text** mode, the button displays the label text followed by a colon and the name of the state. For example, the [Select Hidden](Selection_SelectHidden.md) action displays either **Select Hidden: On** or **Select Hidden: Off**.
* In **icon** mode, the button displays a visual cue that reveals the action's current state. For example, the [Orientation](HandleAlign.md) action uses these three icons to indicate the handle alignment states: ![Orientation: Local](images/icons/HandleAlign_Local.png) (*Local*), ![Orientation: Global](images/icons/HandleAlign_World.png) (*Global*), and ![Orientation: Normal](images/icons/HandleAlign_Plane.png) (*Normal*).

The documentation for each action includes information about these visual indicators.

> **Note:** Some actions also have extra options or custom settings available. These action buttons have a special indicator in the top-right corner. The documentation for each action includes information about these options.

## ![Selection Rect icon](images/icons/Selection_Rect_Intersect.png) Rect

Use the **Rect** action to define whether drag selection should only select elements inside the drag-rect, or any intersected elements.

For more information, see the [Rect](Selection_Rect_Intersect.md) action documentation.

## ![Shift Modifier icon](images/icons/Selection_ShiftDifference.png) Shift

Use the **Shift** action to define how holding the **Shift** key affects selection.

For more information, see the [Shift](Selection_Shift.md) action documentation.

## ![Orientation](images/icons/HandleAlign_Local.png) Orientation

Use the **Orientation** action to set the orientation for Scene handles (__Global__, __Local__, or __Normal__).

For more information, see the [Orientation](HandleAlign.md) action documentation.

## ![Select Hidden ON](images/icons/Selection_SelectHidden-ON.png) Select Hidden

Use the **Select Hidden** action to define whether hidden elements are selected or ignored when drag-selecting.  

For more information, see the [Select Hidden](Selection_SelectHidden.md) action documentation.

## ![Select Edge Loop icon](images/icons/Selection_Loop.png) Select Edge Loop

Use the **Select Edge Loop** action to select an edge loop from each selected edge.

For more information, see the [Select Edge Loop](Selection_Loop_Edge.md) action documentation.

## ![Select Edge Ring icon](images/icons/Selection_Ring.png) Select Edge Ring

Use the **Select Edge Ring** action to select a ring from each selected edge.

For more information, see the [Select Edge Ring](Selection_Ring_Edge.md) action documentation.

## ![Select Face Loop icon](images/icons/Selection_Loop_Face.png) Select Face Loop

Use the **Select Face Loop** action to select a face loop from each selected face.

For more information, see the [Select Face Loop](Selection_Loop_Face.md) action documentation.

## ![Select Face Ring icon](images/icons/Selection_Ring_Face.png) Select Face Ring

Use the **Select Face Ring** action to select a face ring from each selected face.

For more information, see the [Select Face Ring](Selection_Ring_Face.md) action documentation.

## ![Select by Material icon](images/icons/Selection_SelectByMaterial.png) Select by Material

Use the **Select by Material** action to select all faces which have the same Material. 

For more information, see the [Select by Material](Selection_SelectByMaterial.md) action documentation.

## ![Select by Vertex Color icon](images/icons/Selection_SelectByVertexColor.png) Select by Colors

Use the **Select by Colors** action to select all faces on this object which have the same vertex color. 

For more information, see the [Select by Colors](Selection_SelectByVertexColor.md) action documentation.

## ![Shrink Selection](images/icons/Selection_Shrink.png) Shrink Selection

Use the **Shrink Selection** action to remove the elements on the perimeter of the current selection ([Grow Selection](Selection_Grow.md) in reverse).

For more information, see the [Shrink Selection](Selection_Shrink.md) action documentation.

## ![Grow Selection](images/icons/Selection_Grow.png) Grow Selection

Use the **Grow Selection** action to expand the selection outward to adjacent faces, edges, or vertices.

For more information, see the [Grow Selection](Selection_Grow.md) action documentation.

## ![Select Hole icon](images/icons/Selection_SelectHole.png) Select Holes

Use the **Select Holes** action to select all elements along the selected open vertex or edge.

For more information, see the [Select Holes](Selection_SelectHole.md) action documentation.

