# Tools vs. actions

Tools are mechanisms that let you create new meshes or make significant changes to the topology of an existing mesh through a scripting component, whereas actions are immediate changes. As soon as you initiate an action, ProBuilder performs that action. Tools are modal instead of immediate, so while the tool mode is active, you can't perform another action, including transforming the mesh or its elements. 

For example, when you create meshes, you can use the [New Shape](shape-tool.md) tool or the [Poly Shape](polyshape.md) tool. Both of these are modal tools so that you can define its dimensions and set any shape-specific properties available  before ProBuilder builds it. The [Cut](cut-tool.md) tool is also modal because you have to define several points on a mesh where you want to create a new edge before ProBuilder creates the new face. By contrast, [Grow Selection](Selection_Grow.md) is an action that you initiate from the menu, toolbar button, or hotkey and it finishes immediately. You can modify its options to change the behavior of the action, but the options are in a non-modal window.

Another difference is that actions are often only available in specific [Edit modes](modes.md), whereas tools are generally available in all modes. For example, you can launch the [Cut](cut-tool.md) tool in every mode except the Object mode and it behaves exactly the same, but most actions operate are specific to the selected element and behave differently, such as the Subdivide actions: 

* [Subdivide Edge](Edge_Subdivide.md) divides the selected edge(s) into multiple edges.
* [Subdivide Face](Face_Subdivide) adds a vertex at the center of each selected face and connects them in the center.
* [Subdivide Object](Object_Subdivide) divides every face on the selected objects.

Because tools are modal, sometimes initiating another action might exit the tool mode before performing the modifications in some cases. For example, if you are using the Cut tool and you click on a different object in the Scene view, the Cut tool exits without performing the cut.

Some tools are available from the [Component Editor Tools panel](https://docs.unity3d.com/Manual/UsingCustomEditorTools.html#ToolModesAccessSceneViewPanel) in the Scene view when the selection meets the tool's criterion. For example, if you select a mesh you created with the Poly Shape tool, the **Create PolyShape** icon appears in the tools panel, which you can click to activate the Edit Poly Shape tool. If you select a mesh you created with the New Shape tool, the Edit Shape Tool icon appears in the tools panel, which you can click to activate the Edit Shape tool.  

<span style="color:blue">**@DEVQ**: The tooltips for the tools panel are inconsistent (**Create PolyShape** appears instead of **Edit Poly Shape** and **Edit Shape Tool** appears instead of **Edit Shape**). Would you be willing to consider changing these to be consistent with naming/UX conventions?</span>

For a list of tools available in this version of ProBuilder, see [Tools reference](ref_tools.md).

For a list of actions available in this version of ProBuilder, see [Action reference](ref_action.md).





