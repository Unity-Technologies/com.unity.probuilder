# Select Vertex Color

To select all elements (faces, edges, or vertices) from faces that use the same vertex color, use the **Select Vertex Color** action. This action is available in the [vertex, edge, and face modes](modes.md).

> **Note:** Selection works even on vertex colors that aren't currently visible, for example if their material shader doesn't display colors.

![On the left, a single orange face is selected. On the right, all orange faces are selected.](images/Example_SelectByVertexColor.png)

To select faces by material:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select an edit mode. All three modes support this action.
1. Select an element. To select more than one color, hold **Shift** and select more elements. <!--this isn't as simple as I make it sound - when you select an edge or a vertex, it's not obvious which face it will reference for the color - it's honestly easier to use only Face mode-->
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) and click **Select** > **ProBuilder Select** > **Select Vertex Color**.
    * From the main menu, select **Tools** > **ProBuilder** > **Selection** > **Select Vertex Color**.
<!--1. By default, ProBuilder selects matching elements from all GameObjects in the scene. To limit your selection to the current GameObject, in the **Select Material Options** overlay, select **Current Selection**. 

This doesn't seem to exist anymore; I never get an overlay



## Select Vertex Colors Options

Enable the **Current Selection** option to extend the selection to other faces on the currently selected GameObject(s). By default, this option is disabled. 

When disabled, ProBuilder selects every face with the currently selected vertex color on any GameObject in the scene.
-->