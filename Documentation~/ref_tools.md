# Tools reference

Some tools are available from the [Tools panel](https://docs.unity3d.com/Manual/UsingCustomEditorTools.html#ToolModesAccessSceneViewPanel) in the Scene view when the selection meets the tool's criterion. For example, if you select a mesh you created with the [Shape](shape-tool.md) tool, the Edit Shape icon appears in the **Tools** panel, which you can click to activate the Shape tool and edit the bounding box or choose a different shape primitive. Similarly, if you select a mesh you created with the [Poly Shape](polyshape.md) tool, the Edit Poly Shape icon appears in the **Tools** panel, which you can click to activate the Poly Shape tool and move and add the points that define the polygon shape. 

While a tool is active, you can't perform a different task without exiting the tool mode. For example, if you click on a different object in the Scene view while using the [Cut](cut-tool.md) tool, the tool exits without performing the cut. Some tools add a specific scripting component to the GameObject which creates and manages changes to the mesh asset.

This table lists all the tools available in ProBuilder:

| **Tool:**                                     | **Description:**                                             |
| --------------------------------------------- | ------------------------------------------------------------ |
| [Shape](shape-tool.md) tool                   | Creates a new mesh with the Shape component, which defines the mesh's shape primitive. You define the same bounding box for all shape primitives. <br /><br />**Tip**: To modify the primitive or bounding box after creation, select the mesh and then click the![img](images/icons/tlbx-icon-shape.png) Edit Shape button on the **Tools** panel in the Scene view. |
| [Poly Shape](polyshape.md) tool               | Creates a new mesh with the Poly Shape component.<br /><br />**Tip**: To modify the original after creation, select the mesh and then click the ![](images/icons/tlbx-icon-polyshape.png) Edit Poly Shape button on the **Tools** panel in the Scene view. |
| [Cut](cut-tool.md) tool                       | Cuts a sub-face into an existing mesh face. You determine the shape of the new face by defining points on the mesh to create the edges of the new face. <br /><br />You can use this tool on any face, regardless of whether you created the mesh with the [New Shape](shape-tool.md) tool, the [Poly Shape](polyshape.md) tool, or by [probuilderizing](Object_ProBuilderize.md) a Unity mesh. |
| [Bezier Shape](bezier.md) tool (Experimental) | Define a Bezier spline (curve) to extrude a 3D version along the curve. You can fine-tune the shape by using the tangent handles on the control points to bend the shape. |

