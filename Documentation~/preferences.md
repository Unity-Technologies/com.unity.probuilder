# ProBuilder Preferences 

To configure ProBuilder, from the main menu, go to **Edit** > **Preferences** (macOS: **Unity** > **Settings**). In the **Preferences** window, select the **ProBuilder** tab.

## Dimensions Overlay

By default, the [Dimensions overlay](menu-dimover.md) displays the dimensions of the selected mesh elements only. To display the dimension of the entire object, regardless of the selected elements (vertices, edges, or faces), change the **Bounds Display** property from **Object** to **Element**.

## Experimental

Enable or disable these ProBuilder experimental features:

* **Tools** > **ProBuilder** > **Editors** > **New Bezier Shape**.
* **Tools** > **ProBuilder** > **Experimental** > **Boolean (CSG)**.
* **Preferences** window > **ProBuilder** > **Store Mesh as Asset**.

> **Warning:** Experimental features can have unpredictable results.

| **Property** | **Function** |
| :--- | :--- |
| **Experimental Features Enabled** | Add the experimental features to the Editor. |
| **Store Meshes as Asset** | Store meshes as standalone assets in the Project folder, rather than as part of the Scene asset. This property is only visible when **Experimental Features Enabled** is enabled. |

> **Note**: When you activate or deactivate experimental features, the Unity Editor has to recompile scripts, which causes a delay before changes apply.
## General

Set some basic options for ProBuilder.

| **Property** | **Function** |
| :--- | :--- |
| **Show Action Notifications** | Display a notification in the Scene view after you perform an action with ProBuilder. |
| **Auto Lightmap UVs** | Generate the UV2 channel after every geometry edit. This means you don't have to manually generate them every time the mesh changes. UV2 channel generation for meshes is necessary for lighting, but can be time-consuming. If you are editing objects with many vertices, disable this option to save resources. |
| **Show Missing Lightmap UVs Warning** | Display a warning in the console if ProBuilder shapes are missing a valid UV2 channel when Unity performs a lightmap bake. |
| **Show Handle Info** | Display the information for moving, rotating, and scaling deltas in the **Scene** view when the active context is ProBuilder. **Note**: If you have the [Component Editor **Tools** panel](https://docs.unity3d.com/Manual/UsingCustomEditorTools.html#ToolModesAccessSceneViewPanel) open in the **Scene** view, it covers this information and you need to close it. |
| **Show Scene Info** | Display the Mesh Information overlay in the **Scene** view when the active context is ProBuilder. It includes overall face, vertex, and triangle counts, and the number of elements currently selected. |
| **Script Stripping** |  Automatically remove the extra data ProBuilder stores in a scene. This includes all ProBuilder scripts, so if you're using the runtime API you need to disable this feature. |

## Graphics

Customize the appearance of mesh elements to make editing more visually accessible. 

| **Property** | | **Function** |
| :--- | :--- | :--- |
| **Show Hover Highlight** | | Enable this option to highlight the closest Mesh elements when your cursor moves towards them. <br/><br />**Tip:** You can also set the color to use for highlighting with the [Preselection](#preselection_color) property. |
| **Selection X-Ray** | | Enable this option to display any selected hidden geometry. |
| **Use Unity Colors** | | Use the [standard Unity Color preferences](https://docs.unity3d.com/Manual/Preferences.html#colors). To change the colors, disable this option to access the color settings. |
| | **Dither Face Overlay** | Use a dotted overlay when you hover over or select items. If you disable this option, the overlay appears solid instead. |
| | **Wireframe** | Pick the color ProBuilder uses to display the mesh's wireframe. |
| | **Preselection** | Pick the color ProBuilder uses to highlight the mesh element closest to your mouse. Note that this highlight appears only if you enable the **Show Hover Highlight** property. |
| | **Selected Face Color** | Pick the color ProBuilder uses to display the selected face(s) in a ProBuilder mesh. |
| | **Unselected Edge Color** | Pick the color ProBuilder uses to display the unselected edges in a ProBuilder mesh. |
| | **Selected Edge Color** | Pick the color ProBuilder uses to display the selected edge(s) in a ProBuilder mesh. |
| | **Unselected Vertex Color** | Pick the color ProBuilder uses to display the unselected vertices in a ProBuilder mesh. |
| | **Selected Vertex Color** | Pick the color ProBuilder uses to display the selected vertex (or vertices) in a ProBuilder mesh. |
| **Vertex Size** | Set the size to render the vertex points on ProBuilder meshes in the **Scene** view. |
| **Line Size** | Set the size to render the edges on ProBuilder meshes in the **Scene** view. **Note:** On macOS, this property is only available if you use [OpenGL](https://www.opengl.org/) instead of Metal.|
| **Wireframe Size** | Set the size to render the ProBuilder mesh wireframe in the **Scene** view. **Note:** On macOS, this property is only available if you use [OpenGL](https://www.opengl.org/) instead of Metal. |

## Mesh Editing

Customize how meshes behave when you edit them.

| **Property** | **Function**  |
| :--- | :--- |
| **Auto Resize Colliders** | As you edit mesh bounds, colliders automatically resize to match the new bounds. |
| **Allow non-manifold actions** | Allow advanced behaviours of editing actions so you can create [non-manifold geometry](gloss.html#manifold-and-non-manifold-geometry), such as [bridging closed edges](Edge_Bridge.md). Note that this can destabilize your project; use them only if you are sure of their concepts and how to apply them. |
| **Auto Update Action Preview** | Smooth update for the preview of any final result as you edit. This option can lead to a slower editing experience when editing large selections. |

## Mesh Settings

Establish default behaviors for some ProBuilder options.

| **Property** | | **Function** |
| :--- | :--- | :--- |
| **Material** | Select a default material for ProBuilder meshes. If no material is selected, ProBuilder uses the ProBuilderDefault Material when creating new meshes. |
| **Static Editor Flags** | Choose one of the [Unity Static Settings](https://docs.unity3d.com/Manual/StaticObjects.html) as the default for new ProBuilder meshes. The default value is **Nothing**. |
| **Mesh Collider is Convex** | Makes new ProBuilder object colliders [convex](https://docs.unity3d.com/Manual/class-MeshCollider.html) by default. |
| **Snap New Shape To Grid** | Snap a newly instantiated object to the nearest grid point (as determined by **ProGrids**). |
| **Shadow Casting Mode** | Choose how new ProBuilder meshes cast shadows. The default value is **Two Sided**. For more information, refer to the **Cast Shadows** property on the [Mesh Renderer](https://docs.unity3d.com/Manual/class-MeshRenderer.html) component. |
| **Collider Type** | Set the default type of [collision primitive](https://docs.unity3d.com/Manual/CollidersOverview.html) to use for new ProBuilder objects. The default is a [**Mesh Collider**](https://docs.unity3d.com/Manual/class-MeshCollider.html), which is a custom shape. To use a basic cube, select [**Box Collider**](https://docs.unity3d.com/Manual/class-BoxCollider.html). To create objects without a default collider, select **None**. |
| **Lightmap UVs Settings** | Set defaults for the standard Lightmap UVs parameters. [For details, refer to [Generating lightmap UVs](https://docs.unity3d.com/Manual/LightingGiUvs-GeneratingLightmappingUVs.html). To return to the default settings, click the **Reset** button | 

## Snap Settings

Customize how snapping behaves with ProBuilder.

| **Property** | **Function** |
| :--- | :--- |
| **Snap As Group** | All selected items keep the same relative position to each other while snapping. This is the default. Disable this option to snap each selected item to the grid independently. |
| **Snap Axis** | Choose how mesh elements snap to the grid when you move them (if Snap to Grid is active). **Active Axis**: When you move an element along an axis, the element snaps to the next grid point on that axis. This is the default. **All Axes**: An element snaps to the nearest grid point, even if it's on a different axis to the one you're moving on. |

## UV Editor

Set the size of the grid in the **UV Editor** window. Smaller squares make precision work easier. 

The range is from 0.02 to 2. You can have the **UV Editor** window open while you adjust this setting to see the changes in real time.
