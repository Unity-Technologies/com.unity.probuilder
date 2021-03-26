# ![Collapse Vertices icon](images/icons/Vert_Collapse.png) Collapse Vertices

The __Collapse Vertices__ action collapses all selected vertices to a single point, regardless of distance.

![Cube becomes a pyramid](images/CollapseVerts_Example.png)

> **Tip:** You can also use this action with the **Alt/Opt+C** shortcut, or from the ProBuilder menu (**Tools** > **ProBuilder** > **Geometry** > **Collapse Vertices**).

ProBuilder uses the **Collapse To First** option to decide whether to collapse the vertices to a central point, or to the first selected vertex.

## Collapse Vertices Options

![Collapse Vertices options](images/Vert_Collapse_props.png)

The **Collapse Vertices** action determines where to locate the collapsed vertex in this way:

- If **Collapse To First** option is enabled, ProBuilder uses the location of the vertex that you selected first.
- Otherwise, ProBuilder calculates the center position between all selected vertices to use as the new location.

By default, the **Collapse To First** option is disabled, so ProBuilder uses the center position as in the example image above.
