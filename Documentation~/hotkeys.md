# ProBuilder hotkeys (keyboard shortcuts)

You can use **hotkeys** (keyboard shortcuts) to access many of the actions that are available from the toolbar and the menu. The menu items that have associated hotkeys display the key combinations in the menu. 

You can also change the default hotkeys on a few items in Unity's [Shortcut Manager](https://docs.unity3d.com/Manual/UnityHotkeys.html).

![Set hotkeys in Unity's Shortcut Manager and then see hotkeys assignments in the menu](images/pb_hotkeys.png)

This page gives an overview of the default ProBuilder keyboard shortcuts. Where a command has **Ctrl/Cmd** as part of the keystroke, use the **Control** key on Windows and the **Command** key on macOS. Similarly, where a command has **Alt/Opt** as part of the keystroke, use the **Alt** key on Windows and either the **Alt** or **Option** key on macOS, depending on your keyboard.

> **Note:** There are a few rare exceptions where a hotkey uses the **Control** key for both Windows and macOS. These exceptions are clearly indicated on the action reference page and in the list below.

| **Key combination:**                                         | **Action:**                                                  |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| **Alt/Opt+Click(action button)**                             | Open options for any action on the toolbar.                  |
| **Alt/Opt+B**                                                | [Create a new face between two selected edges](Edge_Bridge.md). |
| **Alt/Opt+C**                                                | [Collapse the selected vertices](Vert_Collapse.md).          |
| **Alt/Opt+E**                                                | Create a new edge that connects either the [selected vertices](Vert_Connect.md) or the [centers of each selected edge](Edge_Connect.md), depending on what editing mode you are in. |
| **Alt/Opt+G**                                                | [Increase the number of items in the selection](Selection_Grow.md). |
| **Alt/Opt+L**                                                | [Select edge loop](Selection_Loop_Edge.md).                  |
| **Alt/Opt+L**                                                | [Select face loop](Selection_Loop_Face.md).                  |
| **Alt/Opt+R**                                                | [Select edge ring](Selection_Ring_Edge.md).                  |
| **Alt/Opt+R**                                                | [Select face ring](Selection_Ring_Face.md).                  |
| **Alt/Opt+S**                                                | Divide the selected [edges](Edge_Subdivide.md) or [faces](Face_Subdivide.md). |
| **Alt/Opt+U**                                                | [Insert edge loop](Edge_InsertLoop.md).                      |
| **Alt/Opt+V**                                                | [Weld selected vertices](Vert_Weld.md).                      |
| **Alt/Opt+X**                                                | [Split the selected vertex](Vert_Split.md) into individual vertices (one per adjacent face). |
| **Alt/Opt+#**                                                | [Apply a specific Material](workflow-materials.md) to the selected object(s) or face(s). |
| **Alt/Opt+Shift+G**                                          | [Shrink the selection](Selection_Shrink.md).                 |
| **Backspace**/**Delete**                                     | Delete the selected [faces](Face_Delete.md) or [Bezier shape points](bezier.md). |
| **Esc**                                                      | Enable the Object edit mode.                                 |
| **G**                                                        | Toggle between the Object and Element (geometry) edit modes. |
| **H**                                                        | Cycle through Vertex, Edge, and Face edit modes.             |
| **P**                                                        | [Toggle the orientation](HandleAlign.md) of the ProBuilder selection handle. |
| **Ctrl+Click**                                               | [Align an adjacent face's UV coordinates](manual-uvs-actions.md#continue) to the current selection in the [UV Editor window](uv-editor.md).<br /><br />**Important:** Unlike many other hotkey combinations involving the **Ctrl** key in Windows and the **Cmd** key in macOS, this action works with only the **Ctrl** key for both platforms. |
| **Ctrl+Shift+Click**                                         | [Copy one face's UVs to another face](manual-uvs-actions.md#copy-uvs) to the current selection in the Scene view with the [UV Editor window](uv-editor.md) open.<br /><br />**Important:** Unlike many other hotkey combinations involving the **Ctrl** key in Windows and the **Cmd** key in macOS, this action works with only the **Ctrl** key for both platforms. |
| <a name="uv-snap"></a>**Ctrl/Cmd+Drag** (while moving, rotating, or scaling) | Snap to UV increments in the [UV Editor window](uv-editor.md). |
| **Ctrl/Cmd+E**                                               | Extrude [edges](Edge_Extrude.md) and [faces](Face_Extrude.md) using the default options. |
| **Ctrl/Cmd+J**                                               | Move the pivot to the center of the currently selected elements: <br /> - [Vertices](Vert_SetPivot.md)<br /> - [Edges](Edge_SetPivot.md)<br /> - [Faces](Face_SetPivot.md) |
| **Ctrl/Cmd+K**                                               | Create a [new Mesh cube](Cube.md).                           |
| **Ctrl/Cmd+Shift+K**                                         | Activate the [Shape tool](shape-tool).                       |
| **Shift+Drag** (while moving, rotating, or scaling)          | Extrude [edges](Edge_Extrude.md) or [faces](Face_Extrude.md). |
| **Shift+Hover**                                              | Show tooltips when hovering over an action icon in the ProBuilder toolbar. |


