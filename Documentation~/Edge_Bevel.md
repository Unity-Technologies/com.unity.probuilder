# Bevel Edges

The __Bevel Edge__ action splits the selected edge(s) into two edges, with a new face between.

![Bevel 3 edges on cube](images/BevelEdges_Example.png)

To bevel edges:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select either:
    * The **Face** edit mode to bevel all edges on that face.
    * The **Edge** edit mode to bevel selected edges.
1. Select the face or edge to bevel. Hold **Shift** to select multiple faces or edges.
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) on the selected face or edge and select **Bevel Edges**.
    * From the main menu, select **Tools** > **ProBuilder** > **Geometry** > **Bevel Edges**.

## Bevel Edges options

To change the width of the new face, change the __Distance__ value, which defines the distance in meters between the original and new bevels. 

The minimum width is `0.0001`. The maximum width depends on the original shape, because the new face has to fit within it.

Select **Live Preview** to move the bevels as you change the distance value with the mouse. When **Live Preview** is off, the bevels update only when you release the mouse.