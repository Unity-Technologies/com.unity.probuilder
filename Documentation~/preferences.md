# ProBuilder Preferences window

To define how you interact with ProBuilder, you can customize the user interface and how the tools work.

![Preferences Window](images/preferences.png)



ProBuilder provides the following preference sections, which you can change to suit your needs:

- [Experimental](#experimental)
- [General](#general)
- [Graphics](#graphics)
- [Mesh Editing](#editing)
- [Mesh Settings](#settings)
- [Shortcuts](#shortcuts)
- [Toolbar](#toolbar)
- [UV Editor](#uvs)




<a name="experimental"></a>

## Experimental 

Use these settings to enable and disable ProBuilder experimental features.

> **Warning:** Experimental features are untested and might break your Project.

![Experimental section](images/prefs_experimental.png)

| **Property:**                                            | **Description:**                                           |
| :--------------------------------------------------------- | :----------------------------------------------------------- |
| __Experimental Features Enabled__                          | Enable this option to access the [New Bezier Shape](bezier.md) experimental feature in the ProBuilder toolbar, and the __Store Mesh as Asset__ option. <br /><br />**Note:** This setting has no affect on the [Boolean (CSG) Tool](boolean.md), which is always available from the [Experimental menu](menu.md#experimental). |
| __Meshes Are Assets__                                      | Enable this option to store Mesh information in the Project instead of in each separate Scene level. |



<a name="general"></a>

## General

Use these properties to set some basic options for ProBuilder.

![General section](images/prefs_general.png)

|**Property:** |**Description:** |
|:---|:---|
|__Show Action Notifications__ |Enable this option if you want ProBuilder to notify you when performing actions. |
|<a name="autouvs"></a>__Auto Lightmap UVs__ |Enable this option to generate the UV2 channel after every geometry edit. This means you don't have to manually generate them every time the Mesh changes.<br/><br/>UV2 channel generation for Meshes is necessary for lighting, but can be time-consuming. If you are editing objects with large numbers of vertices, disable this to save resources. |
|__Show Missing Lightmap UVs Warning__ |Enable this option to show a warning in the console if ProBuilder shapes are missing a valid UV2 channel when Unity performs a lightmap bake. |
|<a name="info_overlay"></a>__Show Scene Info__ |Enable this option to display the Mesh information overlay in the top left of the Scene view. These details include overall face, vertex and triangle counts, and the number of elements currently selected:<br />![Scene information overlay](images/info_overlay.png) |
|__Script Stripping__ |Enable this option to automatically remove the extra data ProBuilder stores in a Scene. This includes all ProBuilder scripts, so if you are using the runtime API you should disable this feature. |



<a name="graphics"></a>

## Graphics

Use these settings to customize the size and color of Mesh elements.

![Graphics section](images/prefs_graphics.png)

By default, the **Use Unity Colors** option is enabled. However, you can disable this option to set custom colors for a number of elements.

|**Property:** |**Description:** |
|:---|:---|
| <a name="preselection"></a>__Show Hover Highlight__ | Enable this option to highlight the closest Mesh elements when your cursor moves towards them. <br/>**Tip:** You can also set the color to use for highlighting with the [Preselection](#preselection_color) property. |
|<a name="unitycolors"></a>__Use Unity Colors__ |Enable this property to use the [standard Unity Color preferences](https://docs.unity3d.com/Manual/Preferences.html#colors). By default, this property is enabled.<br />When you disable this option, a number of properties appear below. These allow you to specify your own colors to use instead of the Unity colors. For example, you can specify different colors for selected and unselected faces, edges, and vertices. |
|__Dither Face Overlay__ |Enable this option to use dithering (dotted overlay) when you hover over or select items. If you disable this option, the overlay appears solid instead.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Wireframe__ |Pick the color ProBuilder uses to display the Mesh's wireframe.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|<a name="preselection_color"></a>__Preselection__ |Pick the color ProBuilder uses to highlight the closest Mesh element. The [Show Preselection Highlight](#preselection) property must be enabled in order to display highlights.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Selected Face Color__ |Pick the color ProBuilder uses to display the selected face(s) in a ProBuilder Mesh.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Unselected Edge Color__ |Pick the color ProBuilder uses to display the unselected edges in a ProBuilder Mesh.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Selected Edge Color__ |Pick the color ProBuilder uses to display the selected edge(s) in a ProBuilder Mesh.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Unselected Vertex Color__ |Pick the color ProBuilder uses to display the unselected vertices in a ProBuilder Mesh.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Selected Vertex Color__ |Pick the color ProBuilder uses to display the selected vertex (or vertices) in a ProBuilder Mesh.<br/>This property is only available when __Use Unity Colors__ is disabled. |
|__Depth Test__ | Enable this option to render Vertex, Edge, and Face handles through geometry. |
|__Vertex Size__ |Set the size to render the vertex points on ProBuilder Meshes in the Scene view. |
|__Line Size__ |Set the size to render the edges on ProBuilder Meshes in the Scene view. <br/>**Note:** On macOS, this property is only available if you use [OpenGL](https://www.opengl.org/) instead of Metal.|
|__Wireframe Size__ |Set the size to render the ProBuilder Mesh wireframe in the Scene view. <br/>**Note:** On macOS, this property is only available if you use [OpenGL](https://www.opengl.org/) instead of Metal. |





<a name="prefs_editing"></a>

## Mesh Editing

Use these settings to customize interacting with Meshes.

![Mesh Editing section](images/prefs_editing.png)

| **Property:**                                     | **Description:**                                           |
| :-------------------------------------------------- | :----------------------------------------------------------- |
| __Auto Resize Colliders__                           | Enable this option to automatically resize colliders according to Mesh bounds as you edit. |
| <a name="bridge"></a>__Allow non-manifold actions__ | Enable this option if you want to edit your Meshes with advanced techniques, such as [bridging closed edges](Edge_Bridge.md). Note that these complex actions can break your project unless you are familiar with their concepts and how to apply them. <br />By default, this option is disabled. |





<a name="settings"></a>

## Mesh Settings

Use these settings to establish default behavior for some ProBuilder options.

![Mesh Settings section](images/prefs_settings.png)

|**Property:** |**Description:** |
|:---|:---|
| __Static Editor Flags__ |Choose one of the [Unity Static Settings](https://docs.unity3d.com/Manual/StaticObjects.html) as the default for new ProBuilder Meshes. The default value is **Nothing**. |
| <a name="defmat"></a>__Material__ |Set a reference to the default Material you want to use for ProBuilder Meshes. By default, ProBuilder uses the ProBuilderDefault Material when creating new Meshes. |
| __Mesh Collider is Convex__ |Enable this option to set the default convex collider state for new ProBuilder objects. |
| __Pivot Location__ | Choose the default pivot location for new ProBuilder objects. |
|| **First Vertex**<br/>The pivot point of the newly instantiated object coincides with a vertex position. |
|| **Center** <br/>Use the center of the newly instantiated object as the pivot point. |
| __Snap New Shape To Grid__ | Enable this option to snap a newly instantiated object to the nearest grid point (as determined by **ProGrids**). |
|__Shadow Casting Mode__ |Choose how new ProBuilder Meshes cast shadows. The default value is **Two Sided**.<br />See the **Cast Shadows** property on the [Mesh Renderer](https://docs.unity3d.com/Manual/class-MeshRenderer.html) component for more information on this setting. |
|__Collider Type__ |Set the default type of [collision primitive](https://docs.unity3d.com/Manual/CollidersOverview.html) to use for new ProBuilder objects. The default is **Mesh Collider**.|
||**None** <br/>Do not use a collider. |
||**Box Collider** <br/>Use a [basic cube](https://docs.unity3d.com/Manual/class-BoxCollider.html) for the collider. |
||**Mesh Collider** <br/>Use a [custom shape collider](https://docs.unity3d.com/Manual/class-MeshCollider.html) to match the newly created Mesh. This is the default. |
|__Lightmap UV Settings__ |Set defaults for the standard [Lightmap UVs parameters](https://docs.unity3d.com/Manual/LightingGiUvs-GeneratingLightmappingUVs.html). To return to the default settings, click the **Reset** button.|





<a name="shortcuts"></a>

## Shortcuts

Use these settings to change the key mappings for [ProBuilder hotkeys](hotkeys.md).

![Shortcut Settings section](images/prefs_shortcuts.png)

To change the key mapping for one of these settings:

1. Click the setting you want to change (for example, **Set Pivot**) in the list on the left. 

	The **Key** and **Modifiers** drop-down menus on the right display the current setting, and the **Description** displays any information about the setting.

2. Choose a new key value from the **Key** drop-down menu if you want to change the key itself. This can be anything you can press on the keyboard or console. For example, you can select joystick buttons or **Backspace** instead of alphanumeric characters.

3. Choose a new modifier value from the **Modifiers** drop-down menu if you want to change the modifier. 

	> **Caution:** Be careful if you choose the **Everything** value, because this might override another value that uses the same key in a specific context.

As soon as you change the **Key** or **Modifiers** value, ProBuilder saves the change. There is no Save button to commit the change and Undo does not affect these settings. 

> **Note:** If something goes wrong, you can click the **Reset All Preferences** button at the top of the window to return to the default values. However, this resets *all* ProBuilder preferences to their default, so use this only as a last resort.



|**Setting:** |**Description:** |**Default Key Assignment** |
|:---|:---|:--:|
|__Toggle Geometry Mode__ |Toggle between [the Object and Element selection modes](modes.md). |G|
|__Toggle Selection Mode__ |Toggle between [the three Element selection modes](modes.md) (Vertex, Edge, and Face). If [Unique Mode Shortcuts](#unique) is disabled, this shortcut does not apply. |H|
|__Set Trigger__ |Set all selected objects to entity type Trigger. |T|
|__Set Occluder__ |Set all selected objects to entity type Occluder. |O|
|__Set Collider__ |Set all selected objects to entity type Collider. |C|
|__Set Mover__ |Set all selected objects to entity type Mover. |M|
|__Set Detail__ |Set all selected objects to entity type Brush. |B|
|__Toggle Handle Orientation__ |Toggle the orientation of the ProBuilder selection handle. |P|
|__Set Pivot__ |Center pivot around current selection. |Cmd/Ctrl+J|
|__Delete Face__ |Delete all selected faces. |FN+Backspace|
|__Vertex Mode__ |Enter Vertex editing mode. Automatically swaps to Element level editing. |H|
|__Edge Mode__ |Enter Edge editing mode. Automatically swaps to Element level editing. |J|
|__Face Mode__ |Enter Face editing mode. Automatically swaps to Element level editing. |K|







<a name="toolbar"></a>

## Toolbar

Use these properties to set default behavior for the [ProBuilder toolbar](toolbar).

![Toolbar section](images/prefs_toolbar.png)

| **Property:** | **Description:** |
| :--- | :--- |
| __Shift Key Tooltips__ | Enable this option to only show tooltips when the mouse cursor is hovering over a button and you are holding down **Shift**.<br/>By default, tooltips appear when the mouse cursor hovers over a button for more than a second. |
| <a name="icongui"></a>__Icon GUI__ | Enable this option to use toolbar buttons that [display icons only](toolbar.md#buttonmode). <br />Disable this option to use toolbar buttons that [display text only](toolbar.md#buttonmode).<br />**Note:** You can also [use the context menu](customizing.md#buttons) to switch between icons and text. |
| <a name="unique"></a>__Unique Mode Shortcuts__ | Enable this option to assign **G**, **H**, **J**, and **K** to Object, Vertex, Edge, and Face [modes](modes.md) respectively.  You can change which keys are mapped to these actions in the [Shortcut Settings](#shortcuts) section.<br/><br/>By default, ProBuilder assigns **G** to toggle between Object mode and the Element modes, and **H** toggles between the different Element modes (Vertex, Edge, and Face). |
| <a name="toolbarloc"></a>__Toolbar Location__ | Choose the location where you want the [Edit Mode toolbar](edit-mode-toolbar.md) to appear in the Scene view. Possible locations are:<br />- **Upper Center**<br />- **Upper Left**<br />- **Upper Right**<br />- **Bottom Center**<br />- **Bottom Left**<br />- **Bottom Right** |
|__Close Shape Window after Build__ |Enable this option to automatically close the [Shape Tool](shape-tool.md) window after it creates a new Mesh object. |





<a name="uvs"></a>

## UV Editor

Use these settings to customize the [UV Editor window](uv-editor.md).

![UV Editor section](images/prefs_uvs.png)

| **Property:** | **Description:**                                           |
| :-------------- | :----------------------------------------------------------- |
| __Grid Size__   | Size of the grid in the UV Editor, for visual and functional purposes. |

