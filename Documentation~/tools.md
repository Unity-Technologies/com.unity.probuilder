# Tools vs. actions

Tools provide a modal environment where you can perform complex tasks, such as creating new Meshes or defining precise cuts on an existing mesh. For example, when you create Meshes, you can use the [New Shape](shape-tool.md) tool or the [Poly Shape](polyshape.md) tool. Both of these are modal tools so that you can define dimensions and set any shape-specific properties available before ProBuilder builds the final mesh. The [Cut](cut-tool.md) tool is also modal because you have to define several points on a mesh where you want to create a new edge before ProBuilder creates the new face.

> **Tip**: To use the tools, change the Active Context to ProBuilder from the Tools overlay. When you activate a tool, its Tools overlay button is highlighted. You can click the button again to close the tool.

Actions are immediate changes, such as selecting all faces with a specific color or splitting a single edge. As soon as you initiate an action, ProBuilder performs that action. For example, [Grow Selection](Selection_Grow.md) is an action that you initiate from the menu or a shortcut and it finishes immediately. You can modify its options to change the behavior of the action, but the options appear in a non-modal window.

Actions are often only available in specific [Edit modes](modes.md), whereas tools are generally available in all modes. For example, you can launch the Cut tool in every edit mode and it behaves exactly the same, but most actions are specific to the selected element and behave differently, such as subdividing an element:

* [Subdivide Edge](Edge_Subdivide.md) divides the selected edge(s) into multiple edges.
* [Subdivide Face](Face_Subdivide.md) adds a vertex at the center of each selected face and connects them in the center.
* [Subdivide Object](Object_Subdivide.md) divides every face on the selected objects.

For a list of tools available in this version of ProBuilder, refer to [Tools reference](ref_tools.md).

For a list of actions available in this version of ProBuilder, refer to [Action reference](ref_actions.md).
