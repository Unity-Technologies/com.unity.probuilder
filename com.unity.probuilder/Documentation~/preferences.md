<div class="site"><a href="https://youtu.be/Ta3HkV_qHTc"><img src="images/VidLink_GettingStarted_Slim.png"></a></div>

---

ProBuilder contains many settings that can be customized to suit your needs.

![Preferences Window](images/preferences.png)


|**Setting:** ||**Description:** |
|:---|:---|:---|
|**General Settings** |||
|__Strip PB Scripts on Build__ ||If enabled ProBuilder will automatically remove the extra data ProBuilder stores in a scene.  This includes all ProBuilder scripts, so if you are making use of the runtime API you will need to disable this feature. |
|__Disable Auto UV2 Generation__ ||Generating a UV2 channel for meshes is necessary for lighting, but can be a time consuming operation.  If you are editing objects with large vertex counts it is beneficial to skip generating the UV2 channel after every geometry edit and do it manually (using the [Generate UV2](object-actions#generateuv2) toolbar item). |
|__Show Scene Info__ ||Show or hide the mesh information overlay in the top left of the Scene View. |
|__Show Editor Notifications__ ||This preference enables or disables notification popups when performing actions. |
|**Toolbar Settings** |||
|__Use Icon GUI__ ||Toggles the toolbar between using Icons or Text. |
|__Shift Key Tooltips__ ||If enabled the ProBuilder toolbar will only show tooltips when the mouse is hovering an item and the `Shift` key is held.<br/><br/>By default tooltips are shown when the mouse hovers an action for more than a second. |
|__Toolbar Location__ ||Controls where the [Element Mode Toolbar](overview-toolbar#edit-mode-toolbar) is shown in the Scene View. |
||Upper Center |Display toolbar in the top of the window in the center |
||Upper Left |Display toolbar in the top of the window on the left |
||Upper Right |Display toolbar in the top of the window on the right |
||Bottom Center |Display toolbar at the bottom of the window in the center |
||Bottom Left |Display toolbar at the bottom of the window on the left |
||Bottom Right |Display toolbar at the bottom of the window on the right |
|__Unique Mode Shortcuts__ ||If enabled ProBuilder assigns the `G, H, J, K` keys to `Object`, `Vertex`, `Edge`, and `Face` modes respectively.  You can change which keys are mapped to these actions in the Shortcut Settings section.<br/><br/>By default ProBuilder assigns the `G` key to toggle between `Object` mode and `Vertex/Edge/Face` modes.  `H` toggles between the different element modes `Vertex/Edge/Face`.<br/><br/>For more information on Modes, see [Object and Element Modes](fundamentals#modes). |
|Open in Dockable Window ||If enabled the ProBuilder toolbar will be opened as a dockable window.  If disabled the window will be floating and always on top. |
|**Resource Defaults** |||
|__Default Material__ ||Default Material |
|__Default Entity__ ||What [Entity Type](object-actions#entity-type-tools) new shapes will be instantiated as. |
|__Default Collider__ ||The type of collider new shapes will be instantiated with. |
|__Force Convex Mesh Collider__ ||If the default collider is `Mesh Collider`, this setting controls the `Is Convex` setting of the collider. |
|**Miscellaneous Settings** |||
|__Limit Drag Check to Selection__ ||When enabled ProBuilder will restrict drag selection of elements to the current selection.  If disabled drag selecting in a scene will test every ProBuilder object, which may be slow in larger scenes. |
|__Only PBO are Selectable__ ||If enabled while ProBuilder is open only ProBuilder created GameObjects may be selected. |
|__Close Shape Window After Building__ ||If enabled the [Shape Tool](tool-panels#shape-tool) will automatically close itself after a new object is created. |
|__Dimension Overlay Lines__ ||Hide or show the bounding lines in the Dimensions Overlay. |
|**Geometry Editing Settings** |||
|__Precise Element Selection__ ||This controls how close to an element the cursor must be to register for selection.  By default this is disabled, meaning that vertex and edge selection are very forgiving when clicking.<br/><br/>If enabled, the selectable area for vertices and edges is smaller, but you may also select faces by clicking outside of vertices. |
|__Colors__ ||ProBuilder allows users to set the colors for element selections. |
|__Vertex Handle Size__ ||Determines how large vertex points are rendered in the scene.  This setting does not affect selection. |
|__Force Pivot to Vertex Point__ ||When instantiating new shapes ProBuilder will guarantee that the pivot point of the object coincides with a vertex position. |
|__Force Pivot to Grid__ ||When instantiating a new object ProBuilder will snap the object to the nearest grid point (as determined by ProGrids). |
|__Bridge Perimeter Edges Only__ <a id="bridge-perimeter-edges"></a> ||When enabled ProBuilder will not allow users to bridge closed edges.  Disable to remove this restriction. |
|**Experimental** |||
|__Meshes Are Assets__ ||ProBuilder will store mesh information in the Project instead of per-scene.<br/><br/> > **Warning:** <br/>Enabling experimental features may break your project!  Please exercise caution. |
|**UV Editing Settings** |||
|__UV Snap Increment__ ||Set the snap increment in the UV Editor window. |
|__Editor Window Floating__ ||Make the UV Editor window floating or dock-able. |


