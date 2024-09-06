# Select Vertex Color

To select all elements that use the same [vertex color](workflow-vertexcolors), use the **Select Vertex Color** action. This action is available in the [vertex, edge, and face modes](modes.md).

The __Select Vertex Color__ selects all faces on this object that have the same vertex color as the selected face. You can also extend the selection to other GameObjects if you disable the **Current Selection** option.
<!--can a face have more than one vertex color?-->
Even if the vertex color isn't currently visible (for example, if it has a Material that doesn't show colors <!--some materials won't because their shader doesn't-->, like the checkerboard Material), the colored faces are still selected.

![Select all orange faces on the Mesh](images/Example_SelectByVertexColor.png)

From the main menu, select **Tools** > **ProBuilder** > **Selection** > **Select Vertex Color**.

## Select Vertex Colors Options

Enable the **Current Selection** option to extend the selection to other faces on the currently selected GameObject(s). By default, this option is disabled. 

When disabled, ProBuilder selects every face with the currently selected vertex color on any GameObject in the scene.
