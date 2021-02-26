# Tools reference

Tools provide a modal environment where you can perform complex tasks, such as creating new Meshes or defining precise cuts on an existing Mesh. While a tool is active, you can't perform a different task without exiting the tool mode. For example, if you click on a different object in the Scene view while using the [Cut](cut-tool.md) tool, the tool exits without performing the cut.

Mesh creation tools add a specific scripting component to the GameObject which creates the initial Mesh. ProBuilder uses this component to manage any changes you make to its initial definition. For example, after you use the [Shape](shape-tool.md) tool to create a pipe-shaped Mesh, you can re-edit that Mesh to change the pipe primitive to an arch primitive, although you lose any refinements, such as extrusions or face merges and splits.

Some tools are available from the [Tools panel](https://docs.unity3d.com/Manual/UsingCustomEditorTools.html#ToolModesAccessSceneViewPanel) in the Scene view when the selection meets the tool's criterion. For example, if you select a Mesh you created with the [Shape](shape-tool.md) tool, the Edit Shape icon appears in the **Tools** panel, which you can click to activate the Shape tool and edit the bounding box or choose a different shape primitive.

This table lists all the tools available in ProBuilder:

| **Tool:**                       | **Description:**                                             |
| ------------------------------- | ------------------------------------------------------------ |
| [Shape](shape-tool.md) tool     | Creates a new Mesh with the Shape component, which defines the Mesh's shape primitive. You define the same bounding box for all shape primitives. <br /><br />**Tip**: To modify the primitive or bounding box after creation, select the Mesh and then click the![img](images/icons/tlbx-icon-shape.png) Edit Shape icon on the **Tools** panel in the Scene view. |
| [Poly Shape](polyshape.md) tool | Creates a new Mesh with the Poly Shape component.<br /><br />**Tip**: To modify the original after creation, select the Mesh and then click the ![](images/icons/tlbx-icon-polyshape.png) Edit Poly Shape button on the **Tools** panel in the Scene view. |
| [Cut](cut-tool.md) tool         | Cuts a sub-face into an existing Mesh face. You determine the shape of the new face by defining points on the Mesh to create the edges of the new face. <br /><br />You can use this tool on any face, regardless of whether you created the Mesh with the [New Shape](shape-tool.md) tool, the [Poly Shape](polyshape.md) tool, or by [probuilderizing](Object_ProBuilderize.md) a Unity Mesh. |


> **Note:** For documentation on the [Bezier Shape](bezier.md) tool, see the [Experimental features](experimental.md) section of the documentation.
