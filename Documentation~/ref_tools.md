# Tools reference

This table lists all the tools available in ProBuilder:

| **Tool:**                                     | **Description:**                                             |
| --------------------------------------------- | ------------------------------------------------------------ |
| [New Shape](shape-tool.md) tool               | Creates a new mesh with the Shape component, which defines the mesh's shape primitive. You define the same bounding box for all shape primitives. <br /><br />**Tip**: To modify the primitive or bounding box after creation, use the [Edit Shape](edit-shape.md) tool. |
| [Poly Shape](polyshape.md) tool               | Creates a new mesh with the Poly Shape component.            |
| [Edit Shape](edit-shape.md) tool              | tbd                                                          |
| [Cut](cut-tool.md) tool                       | Cuts a sub-face into an existing mesh face. You determine the shape of the new face by defining points on the mesh to create the edges of the new face. <br /><br />You can use this tool on any face, regardless of whether you created the mesh with the [New Shape](shape-tool.md) tool, the [Poly Shape](polyshape.md) tool, or by [probuilderizing](Object_ProBuilderize.md) a Unity mesh. |
| [Bezier Shape](bezier.md) tool (Experimental) | Define a Bezier spline (curve) to extrude a 3D version along the curve. You can fine-tune the shape by using the tangent handles on the control points to bend the shape. |

<span style="color:blue">**@DEV**: In the Inspector, the title of the Shape component is **Shape Component (Script)**, whereas the title of the Poly Shape component is **Poly Shape (Script)**. For UX consistency, would you consider using **Shape (Script)** instead?</span>