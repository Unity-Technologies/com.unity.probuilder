# Fill Hole

In ProBuilder, a hole is a missing face. The **Fill Hole** action creates a face between existing edges or vertices that touch the selected hole.

> **Tip:** You don't need to select all edges or vertices around a hole. Selecting only one edge or vertex is enough to fill the hole.

To fill a hole:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select either:
    * The **Vertex** edit mode.
    * The **Edge** edit mode.
1. Select an edge or vertex along the hole. <!--the tooltip on Fille Entire Hole says you can get all the holes at once by not selecting anything - same as Select Hole - but I can't get the Fill Hole option clickable without selecting something-->
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) select **Fill Hole**.
    * From the main menu, select **Tools** > **ProBuilder** > **Geometry** > **Select Hole**.
1. The **Fill Hole Options** overlay opens. 
    By default, ProBuilder fills the entire hole. If you want to fill only part of the hole, disable the **Fill Entire Hole** option and select the edges you want to build along. 
    For example, if you have a missing quad, and select to adjacent edges, ProBuilder creates a triangular polygon that covers half of the hole.

![On the right is a shape with a hole. On the left, the hole is filled.](images/FillHole_Example.png)
