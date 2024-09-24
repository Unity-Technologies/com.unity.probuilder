# Bridge Edges

The __Bridge Edges__ action creates a new face between two selected edges.

![Bridge edges between two planes](images/BridgeEdges_Example.png)

To bridge edges:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay the **Edge** edit mode.
1. Hold **Shift** to select the edges to bridge.
1. Do one of the following:
    * Press **Alt+B** (macOs: **Option**+**Shift**+**B**).
    * Right-click (macOS: **Ctrl**+click) on the selected edge and select **Bridge Edges**.
    * From the main menu, select **Tools** > **ProBuilder** > **Geometry** > **Bridge Edges**.

## Open and closed edges

By default, the Bridge Edges action bridges only open edges, which are edges that have only one face. This is because an edge that has more than two faces on the same plane is a non-manifold geometry, which can lead to errors.

For example, if you have a door shape with an open archway and open external sides, by default you:

* Can bridge the external sides, because those edges each have only one face - the front or back of the door.
* Can't bridge the archway, because those edges have two faces each - the front (or back) and inside wall of the door.

To bridge closed edges, from the main menu, go to **Unity** > **Settings** > **ProBuilder** and select **Allow non-manifold actions**.




