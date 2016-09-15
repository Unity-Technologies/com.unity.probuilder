<!-- # Video: ProBuilder Fundamentals -->

First time using ProBuilder? Start here for an overview of creating your first mesh, editing geometry,
applying materials, and UV editing.

<!-- [![ProBuilder Fundamentals Video](../images/VideoLink_YouTube_768.png)](@todo "ProBuilder Fundamentals Video") -->

---

## The ProBuilder Toolbar

<div class="video-link-missing">
Quick Video: <a href="@todo link vid section">Fundamentals of ProBuilder: Toolbar</a>
</div>

All of ProBuilder's specialized editing functions are available via the [ProBuilder Toolbar](../toolbar/overview-toolbar), which dynamically adapts to your Edit Mode and selection.

![Toolbar Example](../images/ProBuilderToolbar_GeoActionsArea.png "Toolbar Example")

Each Toolbar button will display detailed information about it's use, options, and keyboard shortcuts when hovered over. Viewing tooltips is a great way to start learning about ProBuilder's functionality.

*More Info: [**ProBuilder Toolbar**](../toolbar/overview-toolbar)*

---

## Creating a New Mesh

<div class="video-link-missing">
Quick Video: <a href="@todo link vid section">Fundamentals of ProBuilder: Shape Tool</a>
</div>

<img src="../../images/Cube_120x120.png" align="right"> Press `CTRL K` on your keyboard to spawn in a new ProBuilder-editable cube.

To start with a more complex shape, ProBuilder also includes a library of shapes (cylinder, torus, stairs, etc), to begin modeling with. Which each of these shapes, you can customize both starting dimensions and unique parameters.

![Shape Tool Example](../images/Example_ShapeToolsWithCurvedStair.png)

*More Info: [**Shape Tool**](../toolbar/tool-panels/#shape-tool)*

---

## Editing Meshes
<div class="video-link-missing">
Quick Video: <a href="@todo link vid section">Fundamentals of ProBuilder: Basic Mesh Editing</a>
</div>

<a id="modes"></a>
### Object vs Element

![Editing Modes Example](ExampleImage_ObjectAndElementEditingModes.png "Editing Modes Example")

**Object Mode** is standard Unity mode, no surprises.

**Elements** are the individual parts that make up a mesh: **Vertices**, **Edges**, and **Faces**. If it helps, you can think of these like levels of detail on a map- for example, "city, state, country". Click in the [Edit Mode Toolbar](../toolbar/overview-toolbar/#edit-mode-toolbar) to change Mode, or use it's shortcut keys.

* **Vertex Editing**: Select and edit Vertices for detailed editing and functions like vertex splitting and connecting.
* **Edge Editing**: Select and edit Edges for semi-complex geometry editing, and Edge Loop Modeling techniques.
* **Face Editing**: Select and edit Faces on an object, performing basic tasks like deleting faces and extruding.

### Element Selection and Manipulation

First, choose which Element type you'd like to edit by clicking it's button in the [Edit Mode Toolbar](../toolbar/overview-toolbar/#edit-mode-toolbar).

Then, use any of the standard Unity selection methods (click, drag, etc) and manipulation controls (move, rotate, scale), just as you would on any other object(s).

### Building and Editing Complex Meshes

ProBuilder follows many of the same conventions as other 3D modeling applications, so experienced 3D artists will likely be able to jump right in after reading the [ProBuilder Toolbar](../toolbar/overview-toolbar) section.

If you are new to 3D modeling, ProBuilder is a great way to get your feet wet.  Now would be a good time to check out the tutorial videos on the ProCore [Youtube playlist](https://www.youtube.com/playlist?list=PLrJfHfcFkLM8PDioWg_5nmUqQycnVmi58).

<!-- @todo -->
<!-- For those seeking greater knowledge we highly recommend viewing the [3D Modeling](@todo) section. There, you can find:

* Step-by-step tutorials and videos
* Workflow suggestions
* Links to other useful sites, videos, etc

*More Info: [**3D Modeling**](@todo)*
 -->

---

## Texturing and UVs

<div class="video-link-missing">
Quick Video: <a href="@todo link vid section">Fundamentals of ProBuilder: Texturing and UVs</a>
</div>

![Materials Example](../images/Example_MaterialsOnLevel.png "Materials Example")

### Applying Materials

You can apply **any** material (including Substance) to ProBuilder meshes using the Unity drag and drop method or the [Material Palette](../toolbar/tool-panels/#material-tools).

Materials can also be applied to individual faces of a ProBuilder mesh, while in [Element Mode](../toolbar/overview-toolbar/#edit-mode-toolbar).

*More Info: [**Material Tools**](../toolbar/tool-panels/#material-tools)*

### Editing UVs

ProBuilder includes both [Auto UVs](../texturing/auto-uvs-actions) (default), and a complete [Manual UV Editing and Unwrapping](../texturing/manual-uvs-actions) system.

**Auto UV** mode lets you tweak basics like Offset, Tiling, and Rotation, while ProBuilder handles the complex UV work automatically.

**Manual UV** mode enables complete control of the UVs, including Projection Mapping, UV Stitching, and more, similar to UV editors in major tools like 3DS Max, Blender, etc.

**You may use whichever you prefer, or a mix of both, even on the same mesh.**

*More Info: [**Texturing and UVs**](../texturing/overview-texture-mapping)*

