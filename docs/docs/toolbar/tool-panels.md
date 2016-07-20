# Video: Tool Panels

[![ProBuilder Toolbar: Tool Panels](../images/VideoLink_YouTube_768.png)](@todo "ProBuilder Toolbar: Tool Panels")

These **Tool Panels** contain important tools for [Shape Creation](@todo), [Materials](@todo), [Texturing and UVs](@todo), [Vertex Coloring](@todo), [Smoothing Groups](@todo), and [Mirroring](@todo). 

> Each panel can be docked, kept floating, or simply closed immediately after use.

---

<a id="shape-tool"></a>
## Shape Tool

<div class="info-box warning">
Section Video: <a href="@todo link vid section">ProBuilder Toolbar: Shape Tool</a>
</div> 

**Toolbar Icon** : ![Shape Tool Icon](Panel_Shapes.png "Create New Shape Icon")

**Keyboard Shortcut** : `CTRL SHIFT K`

**Purpose:** Create new editable shapes such as Cylinder, Arch, and Stairs.

Each shape has specific properties which can be customized before creation- for example, the Stairs shape lets you choose items like Step Height, Arc, and which parts of the Stairway to build.

![Shape Tool Example](../images/Example_ShapeToolsWithCurvedStair.png)

**Usage:** To create a new shape, do the following:
 
1. Open the New Shape Tool
1. If "Shape Preview" is enabled, you will see a blue Preview Object in the scene
1. Choose the Shape you'd like to create (ie, Cube, Cylinder, Torus, etc)
1. Set the options (ie, width, height, radius, number of stairs, etc)
1. You may Move or Rotate the Preview Object within your scene
1. Once ready, click "Build Shape" to create your final Shape

---

## Material Tools

<div class="info-box warning">
Section Video: <a href="@todo link vid section">ProBuilder Toolbar: Material Tools</a>
<br>
Related Video: <a href="@todo link vid section">Texturing and UVs</a>
</div> 

**Toolbar Icon:** ![Material Tools Icon](Panel_Materials.png "Material Tools Icon")

**Purpose:** Save and apply your most frequently used materials, using Quick Material one-click or Material Palette keyboard shortcuts.

![Material Tools](../images/MaterialTools_WithExample.png "Material Tools")

**Quick Material Usage:**

* **(A)** The current "Quick Material"
* **(B)** Apply the Quick Material to the selected faces **(G)**
* **(C)** Pick your Quick Material from the selected face **(G)**
* **(D)** Preview of the current Quick Material

You may also hold `CTRL SHIFT` while clicking on a face, to apply the Quick Material to just that face.

**Material Palette Usage:**

* **(E)** Drag-and-drop your often-used materials to these slots
* **(F)** Click to apply a material to the selected faces **or** objects
* **(G)** Create additional Material Palette slots

You can also press `ALT (number key)` to apply materials from the Palette.

---

## Texturing and UVs

<div class="info-box warning">
Section Video: <a href="@todo link vid section">ProBuilder Toolbar: Texturing and UVs</a>
</div> 

**Toolbar Icon:** ![UV Editor Icon](Panel_UVeditor.png "UV Editor Icon")
 
**Purpose:** Opens the UV Editor Panel, for controlling how materials are displayed on your mesh (tiling, offset, unwrapping, etc).

![Materials Example](../images/Example_MaterialsOnLevel.png "Materials Example")

The UV Editor Panel includes both [Auto UV](@todo) tools, and a complete [Manual UV Editing and Unwrapping](@todo) system.

> *More Info: [**Texturing and UVs**](@todo)*

---

## Vertex Color Tools

<div class="info-box warning">
Section Video: <a href="@todo link vid section">ProBuilder Toolbar: Vertex Color Tools</a>
</div>

**Toolbar Icon:** ![Vertex Color Tools Icon](Panel_VertColors.png "UV Editor Icon")
 
**Purpose:** Opens the Vertex Coloring controls, for applying or painting vertex colors onto meshes.

Not all shaders will show vertex colors on a mesh: [Vertex Colors FAQ](@todo)

![Vertex Coloring](../images/VertexColor_WithLevelExample.png "Vertex Coloring")

Applying Vertex Colors is a great way to colorize levels for prototyping, team layout, zones, etc.

> *More Info: [**Vertex Coloring**](@todo)* 

---

## Smoothing Groups

<div class="info-box warning">
Section Video: <a href="@todo link vid section">ProBuilder Toolbar: Smoothing Groups</a>
</div>

**Toolbar Icon:** ![Smoothing Groups Icon](Panel_Smoothing.png "Smoothing Groups Icon")

**Purpose:** Create a smooth and rounded look, or sharp-edged and faceted.

**Usage:**

First, choose [Face Editing](@todo) from the [Edit Mode Toolbar](@todo), and select the faces you wish to make smooth or faceted.

> *If no faces are selected, or you are in [Object Mode](@todo), any Smoothing actions will be applied to the entire object*

![Smoothing Groups Example](../images/SmoothingGroups-Panel_WithLettersAndExample.png "Smoothing Groups Example")

* **(A)** If any selected faces already have Smoothing Groups assigned, they will be highlighted. Clicking on a highlighted group will un-assign it.
* **(B)** To assign the selected faces to a new Smoothing Group, click any available group button
* **(C)** You can also assign Hard groups
* **(D)** Press the "Clear" button to clear all Smoothing from the selected faces

---

## Mirror Tool

<div class="info-box warning">
Section Video: <a href="@todo link vid section">ProBuilder Toolbar: Mirror Tool</a>
</div>

**Toolbar Icon:** ![Mirror Tool Icon](Object_Mirror.png "Mirror Tool Icon")

**Purpose:** Use the Mirror Tool to create mirrored copies of objects.

The Mirror Tool is especially useful when creating symetrical items- build one half, mirror it, then weld the two together
for a perfectly symetrical result.

![Mirror Tool Example](../images/Mirror-Panel_WithExample.png "Mirror Tool Example")

**Usage:**

1. Select the object to be mirrored
1. Open the Mirror Tools
1. Select which axis (or multiple axis) to mirror on
1. Click "Mirror"- a mirrored duplicate will be created


