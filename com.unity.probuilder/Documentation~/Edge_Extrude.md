# ![Extrude Edges icon](images/icons/Edge_Extrude.png) Extrude Edges

The __Extrude Edges__ tool pushes a new edge out from each selected edge, connected by a new face for each edge. This tool only works on open edges (that is, an edge that has no connected face on one side). However, you can override this restriction with the **Manifold Edge Extrusion** option.

Each new face follows the direction of the normals of the face that is adjacent to the selected edge. 

![Extrude 4 edges of 9](images/ExtrudeEdges_Example.png)

You can invoke this tool in either way:

- Select one or more edge(s) and click **Extrude Edges**. By default, the distance of the extrusion is **0.5**, but you can change that with the **Distance** option.

	> **Tip:** You can also use the **Ctrl/Cmd+E** hotkey instead of the button with this method.

- Select one or more edge(s) and then hold **Shift** while you move, rotate, or scale the selected edge(s). This method ignores the options but provides greater control, especially with the direction of the extrusion.



## Extrude Edges Options

These options apply only if you use the **Extrude Edges** button or the **Ctrl/Cmd+E** hotkey.

![Extrude Edges icon](images/Edge_Extrude_props.png) 



| **Property:**             | **Description:**                                           |
| :-------------------------- | :----------------------------------------------------------- |
| **As Group**                | Enable this option to keep the sides of extruded edges attached to each other if you select more than one edge to extrude. |
| **Manifold Edge Extrusion** | Enable this option to extrude edges that are not free (that is, edges that are connected to faces on all sides). This option can produce unexpected results, so use with caution. |
| __Distance__                | Distance to extrude the edge(s). <br />Both positive and negative values are valid. |



