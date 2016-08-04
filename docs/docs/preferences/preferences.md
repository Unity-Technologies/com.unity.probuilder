ProBuilder contains many settings that can be customized to suit your needs.

![Preferences Window](images/preferences.png)

<h1>General Settings</h1>

## Strip PB Scripts on Build

If enabled ProBuilder will automatically remove the extra data ProBuilder stores in a scene.  This includes all ProBuilder scripts, so if you are making use of the runtime API you will need to disable this feature.

## Disable Auto UV2 Generation

Generating a UV2 channel for meshes is necessary for lighting, but can be a time consuming operation.  If you are editing objects with large vertex counts it is beneficial to skip generating the UV2 channel after every geometry edit and do it manually (using the [Generate UV2](../toolbar/object-actions/#generateuv2) toolbar item).

## Show Scene Info

Show or hide the mesh information overlay in the top left of the Scene View.

## Show Editor Notifications

This preference enables or disables notification popups when performing actions.

<h1>Toolbar Settings</h1>

## Use Icon GUI

Toggles the toolbar between using Icons or Text.

## Shift Key Tooltips

If enabled the ProBuilder toolbar will only show tooltips when the mouse is hovering an item and the `Shift` key is held.

By default tooltips are shown when the mouse hovers an action for more than a second.

## Toolbar Location

Controls where the [Element Mode Toolbar](../fundamentals/#edit-mode-toolbar) is shown in the Scene View.

## Unique Mode Shortcuts

If Unique Mode Shorcuts is enabled ProBuilder assigns the `G, H, J, K` keys to `Object`, `Vertex`, `Edge`, and `Face` modes respectively.  You can change which keys are mapped to these actions in the Shortcut Settings section.

By default ProBuilder assigns the `G` key to toggle between `Object` mode and `Vertex/Edge/Face` modes.  `H` toggles between the different element modes `Vertex/Edge/Face`.

## Open in Dockable Window

If enabled the ProBuilder toolbar will be opened as a dockable window.  If disabled the window will be floating and always on top.

<h1>Resource Defaults</h1>

## Default Material
## Default Entity
## Default Collider
## Force Convex Mesh Collider

<h1>Miscellaneous Settings</h1>

## Limit Drag Check to Selection
## Only PBO are Selectable
## Close Shape Window After Building
## Dimension Overlay Lines

<h1>Geometry Editing Settings</h1>

## Precise Element Selection
## Selected Face Color
## Edge Wireframe Color
## Vertex Color
## Selected Vertex Color
## Vertex Handle Size
## Force Pivot to Vertex Point
## Force Pivot to Grid
<a id="bridge-perimeter-edges"></a>
## Bridge Perimeter Edges Only

<h1>Experimental</h1>

## Meshes Are Assets

<h1>UV Editing Settings</h1>
## UV Snap Increment
## Editor Window Floating
