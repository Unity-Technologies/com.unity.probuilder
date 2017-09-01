<div class="site"><a href="https://youtu.be/Ta3HkV_qHTc"><img src="../../images/VidLink_GettingStarted_Slim.png"></a></div>

---

These **Tool Panels** contain important tools for [Shape Creation](#shape-tool), [Materials](#material-tools), [Texturing and UVs](#texturing), [Vertex Coloring](#vertex-colors), and [Smoothing Groups](#smoothing-groups).

**Note:** Each panel can be docked, kept floating, or simply closed immediately after use.

---

<div style="text-align:center">
<img src="../../images/Toolbar_ToolPanels.png">
</div>

---

<a id="shape-tool"></a>
##![Shape Tool Icon](../images/icons/Panel_Shapes.png "Create New Shape Icon") Shape Tool

**Keyboard Shortcut** : `CTRL SHIFT K`

**Purpose** : Create new editable shapes such as Cylinder, Arch, and Stairs.

Each shape has specific properties which can be customized before creation- for example, the Stairs shape lets you choose items like Step Height, Arc, and which parts of the Stairway to build.

![Shape Tool Example](../images/Example_ShapeToolsWithCurvedStair.png)

**Usage** : To create a new shape, do the following:

1. Open the New Shape Tool
1. If "Shape Preview" is enabled, you will see a blue Preview Object in the scene
1. Choose the Shape you'd like to create (ex, Cube, Cylinder, Torus, etc)
1. Set the options (ex, width, height, radius, number of stairs, etc)
1. You may Move or Rotate the Preview Object within your scene
1. Once ready, click "Build Shape" to create your final Shape

---

<a id="material-tools"></a>
##![Material Tools Icon](../images/icons/Panel_Materials.png "Material Tools Icon") Material Tools

**Purpose** : Save and apply your most frequently used materials, using Quick Material one-click or Material Palette keyboard shortcuts.

![Material Tools](../images/MaterialTools_WithExample.png "Material Tools")

**Quick Material Usage**:

* **(A)** The current "Quick Material"
* **(B)** Apply the Quick Material to the selected faces
* **(C)** Pick your Quick Material from the selected face
* **(D)** Preview of the current Quick Material

Hold `CTRL SHIFT` while clicking on a face to apply the Quick Material.

**Material Palette Usage** :

* **(E)** Drag-and-drop your often-used materials to these slots
* **(F)** Create additional Material Palette slots

You can also press `ALT (number key)` to apply materials from the Palette.

---

<a id="texturing"></a>
##![UV Editor Icon](../images/icons/Panel_UVEditor.png "UV Editor Icon") Texturing and UVs

**Purpose** : Opens the UV Editor Panel, for controlling how materials are displayed on your mesh (tiling, offset, unwrapping, etc).

![Materials Example](../images/Example_MaterialsOnLevel.png "Materials Example")

The UV Editor Panel includes both [Auto UV](../texturing/auto-uvs-actions) tools, and a complete [Manual UV Editing and Unwrapping](../texturing/manual-uvs-actions) system.

*More Info: [**Texturing and UVs**](../texturing/overview-texture-mapping)*

---

<a id="vertex-colors"></a>
##![Vertex Color Tools Icon](../images/icons/Panel_VertColors.png "UV Editor Icon") Vertex Color Tools

**Purpose** : Opens the Vertex Coloring controls, for applying or painting vertex colors onto meshes.

<div class="alert-box warning">
<strong>Warning!</strong>  Not all shaders will show vertex colors on a mesh.
</div>

![Vertex Coloring](../images/VertexColor_WithLevelExample.png "Vertex Coloring")

Applying Vertex Colors is a great way to colorize levels for prototyping, team layout, zones, etc.

<!-- *More Info: [**Vertex Coloring**](@todo)*  -->

---

<a id="smoothing-groups"></a>
##![Smoothing Groups Icon](../images/icons/Panel_Smoothing.png "Smoothing Groups Icon") Smoothing Groups

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=6bwZ9vN7uN0&index=4&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">ProBuilder Toolbar: Smoothing Groups</a>
</div>

**Purpose**: Create a smooth and rounded look, or sharp-edged and faceted.

**Usage**:

![Smoothing Groups Example](../images/SmoothingGroups-Panel_WithLettersAndExample.png "Smoothing Groups Example")

- Choose Face editing from the [Edit Mode Toolbar](../toolbar/overview-toolbar/#edit-mode-toolbar)
- Select a group of faces that you want to be smooth

> *If no faces are selected, or you are in [Object Mode](../general/fundamentals/#object-vs-element), any Smoothing actions will be applied to the entire object*

- **(A)** If any selected faces already have Smoothing Groups assigned, their group number will be highlighted. Clicking on a highlighted group will un-assign it.
- **(B)** To assign the selected faces to a new Smoothing Group, click any available group button
- **(C)** You can also assign Hard groups
- **(D)** Press the "Clear" button to clear all Smoothing from the selected faces
