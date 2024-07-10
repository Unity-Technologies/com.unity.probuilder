# ProBuilder Preferences

To configure ProBuilder, To open the Preferences window, go to **Edit** > **Preferences** (macOS: **Unity** > **Settings**) in the main menu.

## Developer Mode

| **Property** | **Function** |
| :--- | :--- |
| **Pivot Mode** | When the active context is ProBuilder, the pivot   - bda wording. Set the pivot point to Center, Individual Origins, Active Element |
| **Show Internal Pivot and Orientation** | Displays the Pivot Point and Orientation controls in the Scene edit. |

[["Choose the default pivot location for new ProBuilder objects." was for Pivot Location, which isn't a thing at this point]]
[[First Corner: Use the "first corner" as the pivot point for the newly created Mesh. The first corner refers to where you first clicked in the Scene view to create it
Center: Use the center of the newly instantiated object as the pivot point]]

## Dimensions Overlay

By default, the [Dimensions overlay](menu-dimover.md) displays the dimensions of the selected mesh elements only. To display the dimension of the entire object, regardless of the selected elements (vertices, edges, or faces), change the **Bounds Display** property from **Object** to **Element**.

## Experimental

Use these settings to enable and disable ProBuilder experimental features.

> **Warning:** Experimental features are untested and might break your Project.

| **Property** | **Function** |
| :--- | :--- |
| **Experimental Features Enabled** | Enable this option to access the [New Bezier Shape](bezier.md) experimental feature in the ProBuilder toolbar, and the **Store Mesh as Asset** option. **Note:** This setting has no affect on access to the [Boolean (CSG) Tool](boolean.md), which is always available from the [Experimental menu](menu-experimental.md). |
| **Store Meshes as Asset** | Enable this option to store Mesh information in the Project instead of in each separate Scene level. |

> **Note**: When you toggle Experimental Features on or off, Unity has to recompile scripts because the changes add or remove functionality in Unity. This means that there is a delay before this option appears to change.

## General

Use these properties to set some basic options for ProBuilder.

| **Property** | **Function** |
| :--- | :--- |
| **Show Action Notifications** | Enable this option if you want ProBuilder to notify you when performing actions. |
| **Auto Lightmap UVs** | Enable this option to generate the UV2 channel after every geometry edit. This means you don't have to manually generate them every time the Mesh changes. UV2 channel generation for Meshes is necessary for lighting, but can be time-consuming. If you are editing objects with large numbers of vertices, disable this to save resources. |
| **Show Missing Lightmap UVs Warning** | Enable this option to show a warning in the console if ProBuilder shapes are missing a valid UV2 channel when Unity performs a lightmap bake. |
| **Show Handle Info** | Enable this option to display the information for moving, rotating, and scaling deltas in the bottom right of the Scene view. **Note**: If you have the [Component Editor **Tools** panel](https://docs.unity3d.com/Manual/UsingCustomEditorTools.html#ToolModesAccessSceneViewPanel) open in the Scene view, it covers this information. Close the panel to display the information. |
| **Show Scene Info** | Enable this option to display the Mesh information overlay in the top left of the Scene view. These details include overall face, vertex and triangle counts, and the number of elements currently selected. |
| **Script Stripping** | Enable this option to automatically remove the extra data ProBuilder stores in a Scene. This includes all ProBuilder scripts, so if you are using the runtime API you should disable this feature. |

## Graphics

Use these settings to customize the size and color of Mesh elements.

By default, the **Use Unity Colors** option is enabled. However, you can disable this option to set custom colors for a number of elements.

| **Property** | | **Function** |
| :--- | :--- | :--- |
| **Show Hover Highlight** | | Enable this option to highlight the closest Mesh elements when your cursor moves towards them. <br/><br />**Tip:** You can also set the color to use for highlighting with the [Preselection](#preselection_color) property. |
| **Selection X-Ray** | | Enable this option to display any selected hidden geometry. |
| **Use Unity Colors** | | Enable this property to use the [standard Unity Color preferences](https://docs.unity3d.com/Manual/Preferences.html#colors). By default, this property is enabled.<br /><br />When you disable this option, a number of properties appear below. These allow you to specify your own colors to use instead of the Unity colors. For example, you can specify different colors for selected and unselected faces, edges, and vertices. |
| | **Dither Face Overlay** | Enable this option to use dithering (dotted overlay) when you hover over or select items. If you disable this option, the overlay appears solid instead.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Wireframe** | Pick the color ProBuilder uses to display the Mesh's wireframe.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Preselection** | Pick the color ProBuilder uses to highlight the closest Mesh element. The [Show Preselection Highlight](#preselection) property must be enabled in order to display highlights.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Selected Face Color** | Pick the color ProBuilder uses to display the selected face(s) in a ProBuilder Mesh.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Unselected Edge Color** | Pick the color ProBuilder uses to display the unselected edges in a ProBuilder Mesh.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Selected Edge Color** | Pick the color ProBuilder uses to display the selected edge(s) in a ProBuilder Mesh.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Unselected Vertex Color** | Pick the color ProBuilder uses to display the unselected vertices in a ProBuilder Mesh.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| | **Selected Vertex Color** | Pick the color ProBuilder uses to display the selected vertex (or vertices) in a ProBuilder Mesh.<br/><br />This property is only available when **Use Unity Colors** is disabled. |
| **Vertex Size** | Set the size to render the vertex points on ProBuilder Meshes in the Scene view. |
| **Line Size** | Set the size to render the edges on ProBuilder Meshes in the Scene view. <br/><br />**Note:** On macOS, this property is only available if you use [OpenGL](https://www.opengl.org/) instead of Metal.|
| **Wireframe Size** | Set the size to render the ProBuilder Mesh wireframe in the Scene view. <br/><br />**Note:** On macOS, this property is only available if you use [OpenGL](https://www.opengl.org/) instead of Metal. |

## Mesh Editing

Use these settings to customize interacting with Meshes.

| **Property** | **Function**  |
| :--- | :--- |
| **Auto Resize Colliders** | Enable this option to automatically resize colliders according to Mesh bounds as you edit. |
| **Allow non-manifold actions** | Enable this option if you want to edit your Meshes with advanced techniques, such as [bridging closed edges](Edge_Bridge.md). Note that these complex actions can break your project unless you are familiar with their concepts and how to apply them. <br />By default, this option is disabled. |
| **Auto Update Action Preview** | |

## Mesh Settings

Use these settings to establish default behavior for some ProBuilder options.

| **Property** | | **Function** |
| :--- | :--- | :--- |
| **Material** | | Set a reference to the default Material you want to use for ProBuilder Meshes. By default, ProBuilder uses the ProBuilderDefault Material when creating new Meshes. |
| **Static Editor Flags** | | Choose one of the [Unity Static Settings](https://docs.unity3d.com/Manual/StaticObjects.html) as the default for new ProBuilder Meshes. The default value is **Nothing**. |
| **Mesh Collider is Convex** | | Enable this option to set the default convex collider state for new ProBuilder objects. |
| **Snap New Shape To Grid** | | Enable this option to snap a newly instantiated object to the nearest grid point (as determined by **ProGrids**. |
| **Shadow Casting Mode** | | Choose how new ProBuilder meshes cast shadows. The default value is **Two Sided** . See the **Cast Shadows** property on the [Mesh Renderer](https://docs.unity3d.com/Manual/class-MeshRenderer.html) component for more information on this setting. |
| **Collider Type** | | Set the default type of [collision primitive](https://docs.unity3d.com/Manual/CollidersOverview.html) to use for new ProBuilder objects. The default is **Mesh Collider**. **None**: Do not use a collider. **Box Collider**: Use a [basic cube](https://docs.unity3d.com/Manual/class-BoxCollider.html) for collider. **Mesh Collider**: Use a [custom shape collider](https://docs.unity3d.com/Manual/class-MeshCollider.html) to match the newly created Mesh. This is the default.|
| **Lightmap UVs Settings** | Set defaults for the standard [Lightmap UVs parameters](https://docs.unity3d.com/Manual/LightingGiUvs-GeneratingLightmappingUVs.html). To return to the default settings, click the **Reset** button | |
| | **Hard Angle** | |
| | **Pack Margin** | |
| | **Angle Error** | |
| | **Area Error** | |

## Snap Settings

Use these properties to customize how snapping behaves with ProBuilder.

| **Property** | **Function** |
| :--- | :--- |
| **Snap As Group** | Enable this option if you want each selected item to keep the same relative position to each other while snapping. This is the default. Disable this option to snap each selected item to the grid independently. |
| **Snap Axis** | Choose how vertices snap to the grid while moving. **Active Axis**: Vertices snap only along the currently active axis. This is the default. **All Axes**: Vertices snap to all axes simultaneously. |


## UV Editor

Use this setting to customize the [UV Editor window](uv-editor.md).

| **Property** | **Function** |
| :--- | :--- |
| **Grid Size** | Size of the grid in the UV Editor, for visual and functional purposes. |
