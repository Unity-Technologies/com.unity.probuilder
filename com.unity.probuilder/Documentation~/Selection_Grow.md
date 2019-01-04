# ![Grow Selection](images/icons/Selection_Grow.png) Grow Selection

Expands the selection outward to adjacent faces, edges, or vertices.

![Grow Selection Examples](images/GrowSelection_Example.png)

This tool is available in the [vertex, edge, and face modes](modes.md).

> ***Tip:*** You can also use this tool with the **Alt+G** (Windows) or **Opt+G** (Mac) hotkey or from the ProBuilder menu (**Tools** > **ProBuilder** > **Selection** > **Grow Selection**).



## Grow Selection Options

![Grow Selection Options](images/Selection_Grow_props.png) 

| ***Property:***       | ***Description:***                                           |
| --------------------- | ------------------------------------------------------------ |
| **Restrict To Angle** | Enable this property to grow the selection only to those faces within a specified angle. |
| **Max Angle**         | Set the maximum angle allowed when growing the selection.<br />This property is ignored (and is not editable) unless the __Restrict to Angle__ property is enabled. |
| **Iterative**         | Enable this property to grow the selection one adjacent face at a time, each time you press the **Grow Selection** button.<br />This property is enabled automatically (and is not editable) if the __Restrict to Angle__ property is disabled. |





