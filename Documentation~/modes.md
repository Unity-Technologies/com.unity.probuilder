# Edit modes and active contexts

You can use ProBuilder tools in two tool contexts:

* The GameObject context, which allows you to control the entire Mesh.
* The ProBuilder context, which allows you to control the individual elements of the mesh. The ProBuilder context is further divided into edit modes that define what you are selecting and editing:

    * Vertex Selection: Select vertices and perform detailed editing such as vertex splitting and connecting. For a complete list of actions you can perform in this mode, refer to [Vertex actions](vertex.md)
    * Edge selection: Select edges and perform semi-complex geometry editing, and edge loop modeling techniques. For a complete list of actions you can perform in this mode, refer to [Edge actions](edge.md).
    * Face selection: Select faces on an object to perform basic tasks like moving, extruding, or even deleting them. For a complete list of actions you can perform in this mode, refer to [Face actions](face.md).

To change context, in the **Tools** overlay, select the **Tool Context** at the top of the overlay.

To change edit modes within the ProBuilder context, in the **Tool Settings** overlay, select the mode.

## The Probuilder context menu

To quickly access ProBuilder actions, right-click (macOS: **Ctrl**+click) on a ProBuilder object to open the context menu:

* In the GameObject context, the context menu's ProBuilder category lists actions that impact the mesh as a whole, such as Flip Normals and Export.
* In the ProBuilder context, the context menu lists actions that match your current selection and edit mode.

These actions are also available in the ProBuilder menu.

## Keyboard shortcut for edit modes and context

To change context or edit modes, use the following keyboard shortcuts:

* Press **Escape** to return from the ProBuilder context to the GameObject context.
* Press **G** to cycle through the ProBuilder edit modes.

For a list of all shortcuts, and to add or change shortcuts, go to **Edit** > **Shortcuts** (macOS: **Unity** > **Shortcuts**).
