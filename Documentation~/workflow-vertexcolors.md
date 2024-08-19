# Setting vertex colors

Applying vertex colors is a great way to colorize levels for prototyping, team layout, zones, and more. You can apply unique vertex colors to faces or objects in order to easily identify where they begin and end. You can also apply vertex colors to vertices and edges for visual effects.

![Vertex Coloring](images/VertexColor_WithLevelExample.png)



> **Warning:** Not all shaders display vertex colors on a Mesh. However, you can see vertex colors on your Meshes as long as you use a Material that supports vertex colors (like the default ProBuilder Material).



## Editing modes

Depending on what [editing mode](modes.md) you are in and what you select on your Mesh, the vertex colors appear differently. For example, if you select a single vertex or edge, the color you apply is intense on that element and fades outward from it. However, if you select a face or the entire Mesh object, the color covers the face or Mesh evenly:

![Vertex Colors window](images/VertexColors_bymodes.png)



<a name="apply"></a>

## Apply a color

To apply a vertex color:

1. In the **Scene** view > **Tools** overlay, enable the **ProBuilder** tool context.
1. In the **Tool settings** overlay, select an [Editing mode](modes.md).
1. Select the object or element that you want to apply a color to.
1. In the main menu, go to **Tools** > **ProBuilder** > **Editors** > [**Open Vertex Color Editor**](vertex-colors.md).
1. To apply a color, do one of the following:
    * Select **Apply** next to the color. 
	* Press **Alt**+**Shift**+**&lt;number&gt;** (macOS: **Opt**+**Shift**+**&lt;number&gt;**) 
	* Select the preset from the ProBuilder menu (**Tools** > **ProBuilder** > **Vertex Colors** > **Set Selected Faces to Preset &lt;number&gt;**).

> **Note:** To remove a vertex color, apply the white vertex color (**#FFFFFF**).