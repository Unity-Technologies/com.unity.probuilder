# Select Edge Loop or Edge Ring

To add edges to a selection, you can use the **Select Edge Loop** or **Select Edge Ring** actions:

* The **Select Edge Loop** action adds edges that touch your selected edge.
    
    ![On the left, a single edge is selected. On the right, all touching edges are selected, forming a closed loop.](images/Selection_LoopExample.png)

* The **Select Edge Ring** action adds edges that touch the same faces as your selected edge, but that don't touch the selected edge itself.

    ![On the left, a single edge is selected along two faces. On the right, all edges that touch the same two faces are selected.](images/Selection_RingExample.png)

To grow an edge selection:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select the **Edge** edit mode.
1. Select an edge.
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) and click **Select** > **Select Edge Loop** or **Select Edge Ring**.
    * From the main menu, select **Tools** > **ProBuilder** > **Selection** >  **Select Loop** or **Select Ring** (these option names don't include the word "Edge").
1. The **Select Edge Ring** or **Select Edge Loop** overlay opens and the selection is expanded to match the default settings. 
    The **Iterative Selection** option is available for both actions. When you select it, the selection grows to include edges that are next to the edges it just added, and not just the edges next to the original selection.

