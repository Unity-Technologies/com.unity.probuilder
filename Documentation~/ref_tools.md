# Tools reference

Use the ProBuilder tools to create and edit meshes in the Scene view. 

## Tools

This table lists all the tools available in ProBuilder:

| **Tool** | **Description** |
| --- | --- |
| [New Shape and Edit Shape](shape-tool.md) | Creates a new mesh with the Shape component, which defines the mesh's shape primitive. |
| [New PolyShape and Edit PolyShape](polyshape.md) | Creates a new mesh with the PolyShape component. You then draw a 2D shape, and extrude it to a 3D shape. |
| [Cut](cut-tool.md) | Creates a subface in an existing mesh face. You design the shape of the new face by defining points on the mesh, then move the face as you do any other face. <br /><br />You can use this tool on any face, regardless of whether you created the mesh with the [New Shape](shape-tool.md) tool, the [New PolyShape](polyshape.md) tool, or by [probuilderizing](Object_ProBuilderize.md) a standard Unity mesh. |

> **Note:** For documentation on the [Bezier Shape](bezier.md) tool, refer to [Experimental features](experimental.md).

## Active context in the Tools overlay

The ProBuilder tools are available in the **Tools** overlay in the **Scene** view. The tools available depend on the active context: GameObject or ProBuilder.

* You can create and edit shapes and PolyShapes from both the ProBuilder and GameObject contexts.
* The Cut tool is available only when the context is ProBuilder.

**Tip:** While a tool is active, performing any other task that doesn't use the tool deactivates the tool. For example, if while using the [Cut](cut-tool.md) tool on an object, you click on a different object, the tool exits without performing the cut on the original object. 
