# Edit modes (Object vs Element)

ProBuilder uses *Edit modes* to define what you are selecting and editing. 

![Editing Modes Example](images/ExampleImage_ObjectAndElementEditingModes.png)

The **Object** mode is the standard Unity edit mode: when you make a selection in Object mode, you are selecting the entire Mesh.

The other three modes are also known collectively as the **Element** modes. These allow you to select and modify the individual elements of the geometry that make up a Mesh: __Vertices__, __Edges__, and __Faces__. 

An edge is made up of two vertices, a face is composed of three or more edges, and an object is the sum of all parts (like the city, state, and country levels of detail on a map).

To changes Edit modes, you can either click one of the mode buttons on the [Edit mode toolbar](edit-mode-toolbar.md), or use a [mode shortcut key](hotkeys.md).



## Selecting and manipulating 

The ProBuilder Edit modes allow you to access either the elements of your Mesh or the Mesh as a whole. 

1. Click the button matching the object or element mode you'd like to edit in from the [Edit mode toolbar](edit-mode-toolbar.md).

  | ***Icon Mode***                                        | ***Description***                                            |
  | ------------------------------------------------------ | ------------------------------------------------------------ |
  | ![Object edit mode](images/icons/EditModes_Object.png) | Select objects, modify the normals and the pivot, and merge objects together. For a complete list of actions you can perform in this mode, see [Object tools](object-actions.md). |
  | ![Vertex edit mode](images/icons/EditModes_Vertex.png) | Select vertices and perform detailed editing such as vertex splitting and connecting. For a complete list of actions you can perform in this mode, see [Vertex tools](vertex.md). |
  | ![Edge edit mode](images/icons/EditModes_Edge.png)     | Select edges and perform semi-complex geometry editing, and edge loop modeling techniques. For a complete list of actions you can perform in this mode, see [Edge tools](edge.md). |
  | ![Face edit mode](images/icons/EditModes_Face.png)     | Select faces on an object to perform basic tasks like moving, extruding, or even deleting them. For a complete list of actions you can perform in this mode, see [Face tools](face.md). |

2. Click or drag to select the element.

3. Once you have selected the object(s) or element(s) you want to use, you can: 

  * Use one of the tools on the [ProBuilder toolbar](toolbar.md) to apply an action to the selected element(s).
  * Use one of the tools on the [ProBuilder menu](menu.md) to apply an action to the selected element(s).
  * Use one of the [ProBuilder hotkeys](hotkeys.md) to run one of the [ProBuilder tools](ref_tools.md) on the selected element(s).
  * Use any of the standard Unity transformation controls to move, rotate, or scale the element(s).
  * **Shift+Drag** while using any of the standard Unity transformation controls to extrude or inset the element(s).

