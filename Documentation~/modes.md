# Edit modes and active contexts

You can use ProBuilder tools in two tool contexts:

* The GameObject context, which allows you to control the entire Mesh.
* The ProBuilder context, which allows you to control the individual elements of the mesh. The ProBuilder context is further divided into edit modes that define what you are selecting and editing:

    * Vertex Selection: Select vertices and perform detailed editing such as vertex splitting and connecting. For a complete list of actions you can perform in this mode, refer to [Vertex actions](vertex.md)
    * Edge selection: Select edges and perform semi-complex geometry editing, and edge loop modeling techniques. For a complete list of actions you can perform in this mode, refer to [Edge actions](edge.md).
    * Face selection: Select faces on an object to perform basic tasks like moving, extruding, or even deleting them. For a complete list of actions you can perform in this mode, refer to [Face actions](face.md).

To change context, in the **Tools** overlay, select the **Tool Context** button.

To change edit modes within the ProBuilder context, in the **Tool Settings** overlay, select the mode.

## Keyboard shortcuts

* **Escape** to return from the ProBuilder context to the GameObject context.
* **G** to cycle through the ProBuilder edit modes.
* **Shift+Drag** while using any of the standard Unity Transform controls to extrude or inset the element(s).
