<div class="site"><a href="https://youtu.be/Ta3HkV_qHTc"><img src="images/VidLink_GettingStarted_Slim.png"></a></div>

---

<a name="shape-tool"></a>
## ![Shape Tool Icon](images/icons/Panel_Shapes.png) Shape Tool

**Keyboard Shortcut** : `CTRL SHIFT K`

**Purpose** : Create new editable shapes such as cylinder, arch, and stairs.

Each shape has specific properties which can be customized before creation. For example, the stairs shape lets you choose items like step height, arc, and which parts of the stairway to build.

![Shape Tool Example](images/Example_ShapeToolsWithCurvedStair.png)

**Usage**

1. Click **New Shape**.
1. If *Shape Preview* is enabled, you will see a blue preview object in the scene.
1. Choose the shape you'd like to create (ex, cube, cylinder, torus, etc).
1. Set the options (ex, width, height, radius, number of stairs, etc).
1. You may move or rotate the preview object within your scene.
1. Click **Build Shape** to create your final shape.


<a name="material-tools"></a>
## ![Material Tools Icon](images/icons/Panel_Materials.png) Material Tools

**Purpose** : Apply materials to objects or faces.

![Material Tools](images/MaterialTools_WithExample.png)

**Quick Material Usage**

- **(A)** The current *Quick Material*.
- **(B)** Apply the material to the selected faces.
- **(C)** Pick your material from the selected face.
- **(D)** Preview of the current *Quick Material*.

Hold `CTRL SHIFT` while clicking on a face to apply the *Quick Material*.

**Material Palette Usage**

- **(E)** Drag-and-drop your often-used materials to these slots.
- **(F)** Create additional Material Palette slots.

You can also press `ALT (number key)` to apply materials from the Palette.


<a name="texturing"></a>
## ![UV Editor Icon](images/icons/Panel_UVEditor.png) Texturing and UVs

**Purpose**: Opens the UV Editor.

![Materials Example](images/Example_MaterialsOnLevel.png)

The UV Editor Panel includes both [Auto UV](auto-uvs-actions) tools, and a complete [Manual UV Editing and Unwrapping](manual-uvs-actions) system.

For more information, see [Texturing and UVs](overview-texture-mapping).


<a name="vertex-colors"></a>
## ![Vertex Color Tools Icon](images/icons/Panel_VertColors.png) Vertex Color Tools

**Purpose** : Opens the Vertex Coloring controls, for applying or painting vertex colors onto meshes.

> **Warning:** <br/>Not all shaders will show vertex colors on a mesh.

![Vertex Coloring](images/VertexColor_WithLevelExample.png)

Applying Vertex Colors is a great way to colorize levels for prototyping, team layout, zones, etc.


<a name="smoothing-groups"></a>
## ![Smoothing Groups Icon](images/icons/Panel_Smoothing.png) Smoothing Groups

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=6bwZ9vN7uN0&index=4&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">ProBuilder Toolbar: Smoothing Groups</a>
</div>

**Purpose**: Create a smooth and rounded look, or sharp and hard cornered.

**Usage**:

![Smoothing Groups Example](images/Smoothing_Editor.png)

- Choose Face editing from the [Edit Mode Toolbar](overview-toolbar#edit-mode-toolbar)
- Select a group of faces that you want to have smooth adjoining edges
- Click an unused smooth group number in the Smooth Group Editor
	- *Note* Smooth groups already in use are shown with a light blue highlight
- To clear selected face smoothing groups, select the faces and click the ![break smooth groups](images/icons/Face_BreakSmoothing.png) icon
- To select all faces matching with the current smooth group index, use the ![select by smooth group](images/icons/Selection_SelectBySmoothingGroup.png) icon
