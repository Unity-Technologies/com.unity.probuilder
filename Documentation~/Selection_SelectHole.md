# Select Hole

In ProBuilder, a hole is a missing face. The **Select Hole** action is a version of Grow Selection that selects all elements along an open vertex or edge. 

> **Tip:** To create a face from the hole, refer to [Fill Hole](FillHole.md).
<!--fill hole is currently two pages; I will not allow that to stand-->

To select a hole:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select either:
    * The **Vertex** edit mode.
    * The **Edge** edit mode.
1. Select an edge or vertex along a hole. To select all the holes in the mesh at once, don't select any vertex or edges.
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) select **ProBuilder Select** > **Select Hole**.
    * From the main menu, select **Tools** > **ProBuilder** > **Selection** > **Select Hole**.
1. All the edges or vertexes along the hole are selected.

![On the left is a mesh with a single selected edge along a missing face. On the right, the same mesh has all edges around the missing face selected.](images/Example_SelectHole.png)

