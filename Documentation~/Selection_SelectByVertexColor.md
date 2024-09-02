# Select Vertex Color

To select all elements that use the same [vertex color](workflow-vertexcolors), use the **Select Vertex Color** action. This action is available in the [vertex, edge, and face modes](modes.md).

<!--you get very different results based on  your selection mode. If it's edge, you get all the edge of the same color. If it's face, you get all the faces that have a vertex of that color. If it's vertex, you get all the vertexes... wait, what does vertex color mean? does it paint all the edges that go out of that vertex?
Either way, it selects all the edges, faces, and vertexes at once-->
The __Select Vertex Color__ selects all faces on this object which have the same vertex color as the selected face. You can also extend the selection to other GameObjects if you disable the **Current Selection** option.
<!--can a face have more than one vertex color?-->
Even if the vertex color isn't currently visible (for example, if it has a Material that doesn't show colors <!--some materials won't because their shader doesn't-->, like the checkerboard Material), the colored faces are still selected.

![Select all orange faces on the Mesh](images/Example_SelectByVertexColor.png)

From the main menu, select **Tools** > **ProBuilder** > **Selection** > **Select Vertex Color**.

## Select by Colors Options

Enable the **Current Selection** option to extend the selection to other faces on the currently selected GameObject(s). By default, this option is disabled. 

When disabled, ProBuilder selects every face with the currently selected vertex color on any GameObject in the scene.

![Grow Selection Options](images/Selection_SelectByVertexColor_props.png)
