<div class="site"><a href="https://youtu.be/Ta3HkV_qHTc"><img src="images/VidLink_GettingStarted_Slim.png"></a></div>

---

<!-- # Video: UV Editor Toolbar

[![UV Editor Toolbar Video](images/VideoLink_YouTube_768.png)](@todo)
 -->

![](images/UVToolbar.png)


## Select, Move, Rotate, Scale

<!--div class="video-link-missing">
Section Video: <a href="@todo">UV Editor Toolbar: Manipulators</a>
</div-->

This first group of buttons contains shortcuts to the standard Unity manipulation modes. Clicking these will have the exact same effect as clicking on the main Unity toolbar buttons.


## Vertex, Edge Face

<!--div class="video-link-missing">
Section Video: <a href="@todo">UV Editor Toolbar: Element Selection</a>
</div-->

The second button group contains shortcuts to ProBuilder's [Element Editing Modes](fundamentals#modes). When using [Manual UV Editing](manual-uvs-actions), this allows you to select and manipulate UVs by Vertex, Edge, or Face.  

> **Note:** <br/>When using [Auto UVs](auto-uvs-actions) you may only edit UVs by face. Editing an Edge or Vertex will convert the selected UVs to [Manual UVs](manual-uvs-actions).


## ![In-Scene Controls Toggle](images/icons/ProBuilderGUI_UV_Manip_On.png) In-Scene Controls

<!--div class="video-link-missing">
Section Video: <a href="@todo">UV Editor Toolbar: In-Scene Controls</a>
</div-->

When **On**, you can use Unity's standard Move, Rotate, and Scale tools to directly manipulate UVs in the scene, without affecting geometry.

|**Toolbar Icon:** |**Description:** |
|:---|:---|
| ![In-Scene ON](images/icons/ProBuilderGUI_UV_Manip_On.png) | **On** : Move, Rotate, and Scale tools will affect UVs, geometry will not be affected |
| ![In-Scene OFF](images/icons/ProBuilderGUI_UV_Manip_On.png) |  **Off** : Move, Rotate, and Scale tools will return to normal geometry actions |

![](images/UV_InSceneControls.png)

Snap to increments by holding `CTRL` . You can customize these increment values via the [ProBuilder Preferences](preferences)


## ![Texture Preview Toggle](images/icons/ProBuilderGUI_UV_ShowTexture_On.png) Texture Preview

<!--div class="video-link-missing">
Section Video: <a href="@todo">UV Editor Toolbar: Texture Preview</a>
</div-->

When **On**, the selected face's main texture will be displayed in the UV Viewer.

|**Toolbar Icon:** |**Description:** |
|:---|:---|
| ![In-Scene ON](images/icons/ProBuilderGUI_UV_ShowTexture_On.png) | **On** : Selected element's Texture will be displayed in the UV Viewer |
| ![In-Scene OFF](images/icons/ProBuilderGUI_UV_ShowTexture_Off.png) | **Off** : No texture will be displayed in the UV Viewer |

![](images/ShowTexturePreview_Example.png)


## ![Render UV Template Button](images/icons/ProBuilderGUI_UV_Manip_On.png) Render UV Template

<!--div class="video-link-missing">
Section Video: <a href="@todo">UV Editor Toolbar: Render UV Template</a>
</div-->

Opens the Render UVs tool panel, for rendering UV Templates to be used with texture map painting, atlasing, sprite sheets, etc.

![](images/RenderUVsPanel.png)

* __Image Size__ : Total size of the rendered template (always square)
* __Hide Grid__ : Should the grid be hidden in the render?
* __Line Color__ : What color should UV lines be rendered as?
* __Transparent Background__ : Should the background be rendered transparent?
* __Background Color__ : If you want a non-transparent background, set the color here
* __Save UV Template__ : Click to render the UV Template - a file dialog will be opened to save the file.


