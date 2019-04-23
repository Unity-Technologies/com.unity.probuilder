#  Interacting with ProBuilder

![ProBuilder User Interface](images/overview-ui.png)

ProBuilder provides several ways to interact with the tools:

![A](images/LetterCircle_A.png) The [Scene Information](#sceneinfo)

![B](images/LetterCircle_B.png) The [ProBuilder menu](menu.md) 

![C](images/LetterCircle_C.png) [ProBuilder hotkeys](hotkeys.md)

![D](images/LetterCircle_D.png) [Editor windows](#pb_editors)

![E](images/LetterCircle_E.png) The [ProBuilder Preferences](preferences.md) window 

![F](images/LetterCircle_F.png) [Transform tools](#pb_transform) for ProBuilder

![G](images/LetterCircle_G.png) The [ProBuilder toolbar](toolbar.md) and the [Edit mode toolbar](edit-mode-toolbar.md) 

![H](images/LetterCircle_H.png) [ProBuilder tool options](toolbar.md#pb_options) 

![I](images/LetterCircle_I.png) [ProBuilder component](#pb_comp) windows





<a name="sceneinfo"></a>

## Scene Information

![Dimensions Overlay](images/dimoverlay.png)

**Scene Information** displays information about the Meshes in the Scene, and which elements are selected. 

To toggle this on or off, use the **Show Scene Info** setting in the [Preferences](preferences.md#info_overlay).



<a name="pb_editors"></a>

## Editor windows

![Editor windows](images/pb_editors.png)

Editor windows provide [tools or features](tool-panels.md) with extended functionality. For example, the UV Editor window (in the example image above) allows you to perform advanced texture manipulations, including texture mapping, UV unwrapping, and tiling. 

To access these windows, use the [Probuilder menu](menu.md), [hotkeys](hotkeys.md), or the tool panel section of the [ProBuilder toolbar](toolbar.md).



<a name="pb_comp"></a>

## Component windows

![Poly Shape component and Bezier Shape windows](images/pb_comp.png)

There are two component windows in ProBuilder that help define topology: 

* [Poly Shape](polyshape.md) 
* [Bezier shape](bezier.md) (Experimental)

These components provide the ability to re-edit the base shape as many times as necessary. However, using them discards any standard ProBuilder Mesh edits made previously. 

For example, imagine you create a new Poly Shape with five points, and then extrude one of the faces. Next, you decide to remove one of the points, so you enter Poly Shape editing mode again. The extrusion disappears as soon as you re-enter Poly Shape editing mode.

The [Pro Builder Mesh](ProBuilderMesh.md) component window appears on every ProBuilder object. It allows you to customize lightmap UV parameters for each object.



<a name="pb_transform"></a>

## Transform tools in ProBuilder

![Translating a Face in ProBuilder](images/pb_transform.png)

Most of the time, you interact with ProBuilder with translation, rotation, and scaling tools in much the same way that you interact with Unity. However, ProBuilder uses a combination of [Edit modes](modes.md) and special [key combinations](hotkeys.md) to interact at a much deeper level with your Meshes. 

For example, you can use the Shift key with the scaling and translation tools in [Face mode](modes.md) to create [insets](Face_Inset.md) and [extrusions](Face_Extrude.md). This allows you to build complex Meshes easily. 

For an overview of working with ProBuilder, see [Creating Meshes](workflow-create.md), [Editing Meshes](workflow-edit.md), and [Materials, Shaders, Textures, and UVs](workflow-texture-mapping.md).

