<!-- # Video: Texture Mapping Overview

[![ProBuilder Texture Mapping Overview Video](../images/VideoLink_YouTube_768.png)](@todo)-->
---

## Texture Mapping 101

<div class="video-link-missing">
Section Video: <a href="@todo">Texture Mapping: Texture Mapping 101</a>
</div>

**Texture Mapping** is the process of applying materials ("textures") to an object, and adjusting the Offset, Rotation, and Tiling of the object's UVs.

<div style="text-align:center">
<img src="../../images/UVEditor_Example-BeforeAfter.png">
</div>

**UVs** are how the mesh stores this data. These are basically 2D "fold-outs" of the actual 3D mesh, like the image below.

<div style="text-align:center">
<img src="../../images/UVEditor_Example-123.png">
</div>

---

## Auto vs Manual UVs

<div class="video-link-missing">
Section Video: <a href="@todo">Texture Mapping: Auto vs Manual UVs</a>
</div>

ProBuilder provides both "Automatic" and "Manual" Texturing methods:

* [Auto UVs](auto-uvs-actions) : Use this for simple Texturing work, especially architectural or hard-surface items. Tiling, Offset, Rotation, and other controls are available, while ProBuilder automatically handles projection and updates as you work.

* [Manual UV Editing](manual-uvs-actions) : Use a full UV Editor to precisely unwrap and edit UVs, render UV Templates, project UVs, and more.

You can use a mix of Auto and Manual UVs, even on the same object. This is especially useful when some parts of a model need to have tiling textures, while others are unwrapped.

---

## The UV Editor Window

<div class="video-link-missing">
Section Video: <a href="@todo">Texture Mapping: The UV Editor Window</a>
</div>

Both [Auto-Texturing](auto-uvs-actions) and [Manual UV Editing](manual-uvs-actions) controls are located in UV Editor Window.

To open this window, click it's button in the [Main Toolbar](@todo).

* In Text Mode, this will be the button labeled **UV Editor**
* In Icon Mode, use the button with this icon: ![UV Editor Icon](../images/icons/Panel_UVeditor.png "UV Editor Icon")

<div style="text-align:center">
<img src="../../images/UVPanel_FullWindow_Letters.png">
</div>

### ![Item A](../images/LetterCircle_A.png) UV Editor Toolbar

General tools and shortcuts for working with UVs- see the [UV Editor Toolbar](uv-editor-toolbar) section for details.

### ![Item B](../images/LetterCircle_B.png) Actions Panel

This is a dynamic Panel, similar to the [Main Toolbar](../toolbar/overview-toolbar)- only actions available for the selected UV Element(s) type will be shown.

For full info, see the [Auto UVs Actions](auto-uvs-actions) and [Manual UVs Actions](manual-uvs-actions) sections.

### ![Item C](../images/LetterCircle_C.png) UV Viewer

Here you can view and edit the selected object's UV Elements directly- see the [Manual UVs](manual-uvs-actions) section for full details.
