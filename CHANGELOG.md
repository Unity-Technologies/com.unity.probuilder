Nu# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [5.0.7] - 2023-04-04

### Fixed

- [case: PBLD-48] Fixed a bug where the minimum size of a shape did not take into account snap and grid sizes.
- [case: PBLD-34] Fixed a bug where `Experimental Features Enabled` was not activating when using `Dedicated Server` platform.
- [case: PBLD-41] Fixed an issue where UV distribution metric would not recalculate after the mesh optimization step.
- [case: PBLD-32] Fixed `New Shape` start location being incorrect after using right mouse button.
- [case: PBLD-19] Fixed shape creation when the camera perspective is set to Top.
- Made minor performance improvements and reduced allocations when querying for components.
- [case: PBLD-38] Fixed an issue where exported assets would incorrectly use the UInt32 mesh index format.
- [case: PBLD-43] Fixed an issue where activating the **Edit Shape** tool would prevent the Editor tool context from switching. 
- [case: PBLD-57] Fixed error when building player with `EntityBehaviour` applied to prefabs.

### Changes

- Updated `Object.FindObjectsOfType` to use the new `Objects.FindObjectsByType` in Unity 2023.1.


## [5.0.6] - 2022-06-30

### Bug Fixes

- [case: 1407518] Fixed issue where 'Detach Faces' action would not undo correctly.
- [case: 1393809] Fixed move tool when working with small scales.
- [case: 1395936] Fixed Editor crash when opening a EditorWindow dropdown (MacOS).
- [case: 1389642] Fixed Grid snapping not working properly with EditShape Tool.
- [case: 1368465] Fixed issue where extruding an element orthogonally to their normal would result in some additional extrusion along the normal.
- [case: 1369443] Fixed SerializedObjectNotCreatableException errors in the console with Shape Tool.
- [case: 1348463] Fixed issue where instantiating a prefab would not build UV2s.
- [case: 1348434] Added more detailed instructions in the missing UV2 warning log.
- Fix some styling issues with Overlays in 21.2
- [case: 1350635] Fixed crash when using CSG operations.
- [case: 1405226] Fixed tooltips inconsistently showing and hiding.
- [case: 1407039] Fixed stair creation tool missing the inner radius parameter.
- [case: PBLD-3] Fixed vertex manipulation tools locking grid snapping settings on activation.
- [case: PBLD-9] Fixed issue where adding and then removing Collider or Trigger behaviours would cause meshes to not render in builds.
- [case: PBLD-7] Fixed ProBuilderize action not handling redo operation correctly.
- [case: PBLD-6] Fixed unnecessary reimport of all project textures on package install.
- [case: PBLD-11] Fixed Poly Shape Tool missing an undo step after setting height.
- [case: PBLD-13] Fixed a bug where a newly created shape would not be redrawn with Redo. 
- [case: PBLD-15] Fixed a bug with URP that prevented some items from being selectable in the Game view. 

### Changes

- Updates to API documentation
- Cylinder shape is allowed to have 3 sides, and can have an odd number of sides.

## [5.0.4] - 2021-06-08

### Bug Fixes

- [case: 1334017] Fixed errors while exporting a PBShape using FBX Exporter and cleaning export.
- [case: 1332226] Fixed issue where some Gizmos menu items would be missing in projects that have ProBuilder package installed.
- [case: 1324374] Fixed incorrect vertex/edge/face rect selection when mesh's parent is rotated and/or scaled.

## [5.0.3] - 2021-04-01

### Bug Fixes

- [case: 1324399] Fixing errors when building with prefabs in scene.
- [case: 1323666] Preventing to assign an empty mesh as MeshCollider.
- [case: 1322032] Fixing wrong ProBuilderMesh colors on domain reload when null.
- [case: 1322150] Fixing torus and sphere UV generation when creating New Shape.
- [case: 1320936] Fixing 'ProBuilderize' action creating empty submeshes.

### Changes

- Property `Experimental Feature` can now be reset with the rest of ProBuilder preferences.
- Add `GameObject/ProBuilder` menu to create primitives with default dimensions.

## [5.0.2] - 2021-03-11

### Bug Fixes

- Fixed `Draw Shape` tool showing incorrect UVs when setting shape bounding box.
- Fixed `Draw Shape` tool not clearing selection when changing the active shape type.
- Fixed `Cut Tool` error when pressing `Backspace` with no vertices placed.
- Fixed `Cut Tool` error when finishing a cut segment with less than 3 vertices.
- Fixed `Draw Shape` tool truncating shape property fields in the Scene View Overlay.

### Changes

- Moved contents of warning box in `Draw Shape` tool to tooltips.
- Updated manual documentation.

## [5.0.1] - 2021-03-09

### Bug Fixes

- Disable unstable test on Linux.

## [5.0.0] - 2021-03-08

### Features

- Redesigned shape creation workflow. Shapes are now interactively drawn in the Scene View, and remain configurable after the point of creation via the `ShapeComponent` Inspector. Default shapes can still be created through the `GameObject/ProBuilder` menu.
- Added `Point to Point Cut` tool.
- Added a selection preview when using the `Select Path` tool.
- Added `Selection X Ray` option to highlight occluded element selections with a muted color. Default shortcut is `Alt + Shift + X` (modifiable in Shortcut Manager).
- Added Analytics for Actions and Menu Shortcuts

### Bug Fixes

- [case: 1304442] Update package description for SRPs : warning to users to add samples projects.
- [case: 1300329] Fixing Undo completely reverting all actions.
- [case: 1299638] Fixed missing dependency on com.unity.modules.physics.
- [case: 1296104] Fixing freeze transform with negative scales.
- [case: 1296494] Fixing audio visualizer sample.
- [case: 1296428] Cleaning Polyshape tool when leaving the EditorTool.
- [case: 1296427] Removing NullRef Exception on Undo/redo with CutTool.
- [case: 1296422] Closing MenuToolToggles actions when closing ProBuilder window.
- [case: 1296520] Fixing `New Poly Shape` menu entry not working
- [case: 1254339] Correct offset when rendering UVs and correct export when UV Editor is docked.
- Fixed PolyShape in Prefab mode: PolyShapeMode was not serialized when exiting prefab mode. Update point insertion visualization.
- [case: 1259845] Fixed dimension overlay being hidden on playmode or reboot of the editor.
- [case: 1267383] Fixed `Bezier Shape` and `Poly Shape` component preventing build when `Script Stripping` was enabled.
- [case: 1256246] Ensuring edges subdivision is not creating hole and that arch shapes does not create degenerated triangles
- Scaled Auto UVs now remain in place when modifying geometry.
- [case: 1265296] Add tooltips to UV Actions window.
- [case: 1265623] Ensure that ProGrids snapping is enabled (not only active) when using snapping in ProBuilder
- [case: 1256154] Fixed StackOverflow Error when smoothing mesh in Probuilderize, also fixed an error in the display of edges when count > ushort.maxValue.
- [case: 1252668] Replaced mesh template tests for `Connect Edges` with more stable methods.
- [case: 1262236] Ensure PolyShape is not empty to avoid nullref while exporting.
- [case: 1184921] Add a custom preview for `ProBuilderMesh` prefabs.
- [case: 1276085] Fixed `UV Actions` window allowing resize events outside of the containing window.
- [case: 1277788] Take into account the default parent object in Unity 2021 and up.
- [case: 1276074] Fixed a case where `Fit UVs` action could result in `NaN` values.
- [case: 1281254] Fixed shader warning in URP for UNITY_PASS_FORWARDBASE macro redefinition.
- Fixed rect selection not working with Universal Render Pipeline.
- [case: 1283107] Fixed `Bevel` settings slider disappearing when values exceed 1.
- [case: 1283103] Fixed typo in `Center Pivot` tooltip.
- [case: 1283067] Fixed `Export Prefab` throwing an error when overwriting the root asset of an exported prefab instance.
- [case: 1284735] Fixed a possible exception when creating a shape, undoing, then redoing while the `Smooth Group Editor` window is open.
- [case: 1283111] Fixed `Poly Shape` tool not snapping placed vertices with grid snapping enabled.
- [case: 1284741] Fixed missing tooltips for some items in the `Smooth Group Editor` window.
- [case: 1283167] Fixed `Mesh Collider` mesh value not updating with modifications.
- [case: 1285651] Fixed tooltip going out of screen when screen display is scale up
- [case: 1285654] Fixed selected faces highlight for isometric camera mode in sceneview.
- [case: 1286045] Fixed selection cleaning problem after scene restart.
- [case: 1266769] Fixed tooltip window not rendering correctly on Linux.
- [case: 1281658] Fixed warning when modifying a PBMesh with particule effect using PBMesh as shape emitter.
- [case: 1317148] Fixed edge selection returning incorrect results with some Unity versions.
- [case: 1312537] Fixed script stripping on disabled objects when building.
- [case: 1311258] Fixed material reverting when subdividing edge.
- [case: 1317773] Fixed undo after shape creation.

### Changes

- Modified `VertexManipulationTool` to inherit from EditorTool.
- Adding a new MenuAction in Samples to merge faces from edges selection.
- Removing preprocessor directives for Unity 2018 and below for Probuilder 5.0.
- Modified the AppendVerticesToEdge to handle edges split for non-convex faces.
- Removed unused "About" images.
- Removed unused HDRP shader variants.
- `MergeElements.Merge` moved to public API.
- Upgraded `PolyShape` tool to EditorTool and correct some features in it.
- Updated documentation for ProBuilder 5.0.0.
- Add `GameObject/ProBuilder` menu to create primitives with default dimensions.
- Added `com.unity.modules.physics` and `com.unity.modules.imgui` modules as dependencies.

### Internal

- Remove backwards compatibility breaking API changes.
- Fix `Material.SetInt` deprecation warnings.
- Fix failing `Undo` tests on macOS.

## [4.4.0] - 2020-08-12

### Features

- Added a `Select Path` tool. The default shortcut is `Ctrl + Shift + Click` or `Cmd + Shift + Click`.
- Added iterative selection on edges.

### Bug Fixes

- Ensure "ProBuilderize" action is enabled for current selection on opening main window.
- [case: 1258589] Fixed error in runtime sample examples.
- Fixed warning in `ShapeEditor` caused by duplicate using statements.
- [case: 1258421] Fixed an issue where GI UV streams would be lost at runtime due to `ProBuilderMesh` assigning a new `Mesh` to the statically combined `MeshFilter`.
- [case: 1251289] Fixed `m_WireMaterial null reference exception` when re-importing ProBuilder.
- Fixed `TooltipEditor.Hide` affecting performance linearly with scene size.
- [case: 1259506] Fixed shortcut not being saved in 2018.4.
- Fixed vertex colors not applying gamma-correct value when color space is Linear.
- [case: 1251574] Disable export options when no probuilder meshes are selected
- [case: 1248708] Fixed physx error when welding all vertices to a singularity.
- Fixed tooltips always clamping to the left of the screen on secondary monitors.
- Fixed `EditorUtility.SetIconEnabled` not respecting the enabled parameter.
- [case: 1241105] Fixed an issue where `Select Edge Loop` could overflow when encountering certain non-manifold geometry.
- [case: 1242263] Fixed `UV Editor` `Move` and `Rotate` tools throwing null reference exceptions as of Unity 2020.2.0a9.
- [case: 1251289] Fixed exception on script reloads in the `EditorMeshHandles` class.
- Fixed exception on script reloads in the `EditorMeshHandles` class.
- Reduced the amount of repaints in ProBuilder window.
- [case: 1249071] Fixed Shift selection being inconsistent.
- [case: 1249056] Fixed Ctrl selection being inconsistent.
- [case: 1247270] Fixed `Null Reference Exception` when entering Prefab Stage after merging multiple `ProBuilderMesh`.
- [case: 1252394] Fixed dragging selection with Ctrl problem (and key pressed problem in general).
- Fixed Cylinder shape clamping segments to 48.

### Changes

- Update to Settings Manager 1.0.3.

### Known Issues

- Prefab Stage does not work properly when `ProBuilderMesh` contains overrides that append geometry.
- Changelog dates pre-2.4.11-f.0+r4081 are incorrect. These releases were made from SVN, and the history was lost when converting to Git.

## [4.3.1] - 2020-06-01

### Bug Fixes

- [case: 1251289] Fixed exception on script reloads in the EditorMeshHandles class.

## [4.3.0] - 2020-05-15

### Features

- Added UI to reset Shape Editor parameters.
- Make public and document the Poly Shape component.

### Bug Fixes

- [case: 1242879] Fixed import error caused by UPM CI log file.
- [case: 1230949] Fixed EditorMeshHandles losing material references in some cases.
- [case: 1232389] Fixed Null Reference Exception thrown on applying Subdivide Object on complex New Poly Shape
- [case: 1238115] Fixed Exception thrown on selecting UV 2 mode with "Lock the SceneView handle tools" is enabled in the UV Editor
- Fixed New Shape menu item always creating a Cube instead of the last selected shape.
- [case: 1237636] Fixed exception being thrown when stripping pb components and smooth groups editor is open
- [case: 1230069] Fixed selection commands not being remapped to ProBuilder when in edge/face/vertex mode (Select All/Invert Selection/Deselect All)
- [case: 1225223] Fixed Mesh Collider component missing a reference to the mesh in builds.
- [case: 1225427] Fixed UV Editor exporting the UV template offset by 11 pixels when the editor window was dockable.
- Fix ProBuilderMesh.sharedTextureLookup throwing a null reference exception when accessed from runtime.
- [case: 1209522] Fixed Poly Shape component allowing incompatible Preset feature.
- [case: 1213742] Fixed bug where Delete menu item would incorrectly shows as available with no selection.
- [case: 1192479] Fixed an issue where translating UV positions in the UV Editor with a handle would not update the Inspector offset values.
- [case: 1176370] Fixed entering Play Mode with the Shape Editor open creating a new shape in the scene.
- [case: 1218413] Fixed Poly Shape input incorrectly snapping vertices when input plane is not on a grid.
- [case: 1223330] Fixed a crash when undoing the creation of a Poly Shape object.
- Fixed Shape Editor preferences not respecting "Reset All Preferences".
- [case: 1132509] Fixed Bevel sometimes resulting in non-conforming face normals.
- [case: 1158748] Fixed Export and Strip Scripts actions leaving Mesh Filter component hidden on resulting GameObjects.
- [case: 1224413] Fixed Shape Editor leaving behind a preview object if window is closed during play mode.
- [case: 1201746] The Move tool is now compatible with grid snapping (Unity 2019.3 and higher).
- [case: 1217930] Fixed duplicated objects shape reverts to original's mesh when entering play mode
- [case: 1214103] Fixed ProBuilder created meshes not rendering in project builds.
- [case: 1217024] Fixed inverted picking bias when cursor is not hovering selected object.
- [case: 1195261] Fixed an issue where the ProBuilderize action could cause the toolbar to emit GUILayout Group errors.
- [case: 1211721] Fixed tooltips in the ProBuilder toolbar not rendering on macOS.
- [case: 1167627] Fixed boolean operations not retaining material information.
- [case: 1210096] Fixed UV Editor rotation field allowing values outside of 360°.
- [case: 1214103] Fixed ProBuilder created meshes not appearing in built projects.
- [case: 1211169] Fixed Generate Shadow Object example throwing a Null Reference Exception when invoked.
- [case: 1209864] Fixed New Shape tool creating new GameObjects with the same name.
- [case: 1201858] Fixed Export action not showing a warning when attempting to export an empty selection.
- [case: 1205318] Fixed OBJ export not retaining material color information when Scriptable Render Pipeline is active.
- [case: 1194858] Fixed a Null Reference Exception in some cases when using the Bevel tool.
- [case: 1204731] Fixed marquee selection of mesh elements when using the High Definition Render Pipeline.
- [case: 1211096] Fixed tooltips rendering off screen in some cases.
- [case: 1203685] Fixed Poly Shape creation tool not accepting input when Scene View gizmos are disabled.
- [case: 1161998] Fixed an issue where scene objects could be selected through modal windows.
- [case: 1198568] Fixed MeshFilter and MeshCollider always showing properties as "Overridden" on Prefab instances.
- [case: 1204088] Fixed UV Editor actions window not using the mouse event, allowing inactive window properties to appear as interactable.
- [case: 1183101] Fixed broken help link in Smoothing Editor window.
- [case: 1173650] Fixed an issue that resulted in vertices and edges becoming unselectable on macOS in some cases.
- Fixed Edge pre-selection highlight not rendering on macOS when using Metal as a graphics backend.
- [case: 1198568] Fixed issues with prefab overrides not being applied to ProBuilderMesh components correctly.
- [case: 1198568] Fixed MeshFilter and ProBuilderMesh components incorrectly showing instance overrides on un-modified prefab instances.
- [case: 1194858] Fixed Bevel Edges throwing NullReferenceException in some cases.
- [case: 1208475] Fixed Set Pivot and Center Pivot actions throwing NullReferenceException if the selected mesh contains children.
- [case: 1183100] Fixed the Shape Editor Stair slider fields appearing too small to contain the text when Android is the selected build target.
- [case: 1206302] Fixed an issue with the Merge action that could result in invalid geometry.
- [case: 1161998] Fixed an issue where ProBuilder meshes could be selected through overlaying Editor windows.
- [case: 1198588] Fixed a rare case where ProBuilderMesh could throw errors due to an invalid internal state.
- [case: 1196134] Fixed an issue where resetting component data on a ProBuilderMesh through the Inspector window would not update the MeshFilter and scene gizmos.
- Fixed an issue where an invalid selection on ProBuilderMesh could prevent the selected mesh from being edited due to errors.
- Fixed case where the default material could fail to initialize when a Scriptable Render Pipeline is in use at runtime.

### Changes

- [case: 1203585] Removed Custom Shape option from the Shape Editor window.
- The ProBuilder Mesh component now has an icon in the Inspector.
- The MeshFilter component is now hidden by default on GameObjects created by ProBuilder, and the ProBuilderMesh component is renamed in the Inspector to ProBuilder MeshFilter.

### Known Issues

- Certain properties on ProBuilderMesh, MeshFilter, and MeshCollider always show as overridden on prefab instances.
- The ProBuilderMesh icon is always the Pro Skin version.
- The ProBuilderMesh icon is toggle-able in the Gizmos window (initial value is 'Off').
- Marquee selection when the Universal Render Pipeline is enabled does not work (currently blocked due to URP missing required functionality).

## [4.2.1] - 2019-11-22

### Features

- Added the `Offset Elements` action to quickly move selected mesh elements in world, local, or element space.
- Added the ability to set a custom range in the `Subdivide Edges` options window.
- Added the ability to set wireframe and edge line width on macOS when using Metal as the graphics backend.

### Bug Fixes

- Fixed `Boolean Editor` menu item not respecting the Experimental Features Enabled preference.
- Fixed `Boolean Editor` preview images not updating with selection changes.
- Fixed potential error when pressing the `Object Mode` shortcut without a `ProBuilder Editor` instance available.
- Fixed an issue where a currently editing text field would lose focus when a ProBuilder tooltip was shown.
- Fixed a case where `Weld Vertices` could leave the mesh selection in an invalid state.
- Fixed an issue where a ProBuilder tooltip could appear when hovering on a window on top of the toolbar.
- Fixed the ProBuilder toolbar background color applying to entire button (Unity 2019.3 only).
- Fixed `Select by Material` sometimes returning incorrect results.
- Fixed `Merge Objects` not retaining active `GameObject` components and properties.
- Added a dialog when `New Poly Shape` fails to find an unlocked `Inspector` window.
- Fixed case where `New Poly Shape` could create a mis-aligned object when used with ProGrids.
- Fixed edge case where the ProBuilder selection could become desynchronized with the Unity selection.
- Fixed `UV Editor` window minimum width not being sufficient to accommodate the toolbar.
- Fixed a case where duplicate `ProBuilderMesh` components could share a reference to the same `Mesh`.
- Fixed an issue that caused mesh element picking to stop working for some users.
- Fixed the `Smooth Group` window toolbar buttons not showing active state correctly.
- Fixed a case where selecting a previously edited `ProBuilderMesh` could append to the prior selection instead of replacing it.
- Fixed some cases where `ProBuilderMesh` would incorrectly enforce a max vertex count.
- Fixed an issue that caused errors to thrown when entering play mode with the `Shape Tool` window open.
- Fixed the `Shape Tool` destroying and recreating the preview `GameObject` when adjusting settings.
- Fixed an issue where the `Poly Shape` editor would destroy the `GameObject` if initialized with an invalid path.
- Fixed a potential error when undoing a `ProBuilderize` action.
- Fixed a warning when importing ProBuilder to macOS projects using Metal as the rendering backend.

### Changes

- [Samples] Rename LWRP to Universal Render Pipeline.
- Added support for holes when creating shapes from a polygon (API only).
- [Samples] Remove obsolete references from `EventSystem` GameObject.
- [Samples] Remove `GUILayer` components from `Camera`.
- `Shape Tool` settings are now persistent as user settings.

## [4.1.0] - 2019-07-03

### Features

- Added the option to export assets either as prefabs, or just the mesh.
- Improved the naming of exported mesh assets.
- Added API examples as a sample package.
- Re-enabled FBX Exporter integration, adding support for quad export and automatic removal of ProBuilder components on export.
- Added ability to duplicate faces to a new game object or to new submesh.
- Added the option to toggle the dimensions overlay between object and element bounds.
- Added a shortcut to toggle the dimensions overlay between object, element, and off states.
- Added a `Duplicate Face` action.
- The UV Inspector is now resize-able from all sides and corners.
- Added the ability to set a `Poly Shape` height to `0` for a single face plane.
- Add support for HDRP and LWRP pipelines through Samples packages (available in the Package Manager UI for ProBuilder).
- Added the ability to copy UV transform values between manual and auto unwrapped faces.
- Added an explicit toggle to enable or disable rendering the background texture when exporting UV templates.
- Added additional methods for testing and fixing face topology (`MeshValidation.ContainsNonContiguousTriangles` and `MeshValidation.EnsureFacesAreComposedOfContiguousTriangles`).

### Bug Fixes

- Fixed new shapes not instantiating in prefab staging scenes.
- Fixed `Unlit Color` and `Face Highlight` shaders flickering in some cases.
- Fixed shortcuts not saving correctly in Unity 2018.3.
- Fixed a potential exception when an incompatible version of ProGrids is present in the project.
- Fixed an exception when maximizing and un-maximizing the ProBuilder editor window.
- Fixed the `Boolean Editor` incorrectly resizing it's contents.
- Fixed an exception when `Connect Edges` is executed on an edge with coplanar faces.
- Fixed a potential exception in the `Normals` class when vertex count exceeds `ushort.max`.
- Fixed an issue when upgrading ProBuilder from a version lower than `4.0.0` that would result in meshes with multiple materials being condensed to a single material.
- Fixed a potential exception when removing `ProBuilderMesh` components from code.
- Fixed an inconsistency in UV projection that could result in faces being unwrapped differently between Unity versions.
- Fixed obscured edges sometimes taking priority over visible edges when picking elements.
- Fixed `Select Holes` action incorrectly showing as disabled in some cases.
- Fixed compile errors when opened in Unity 2018.4.
- Fixed scene information view not showing the correct selected element counts.
- Fixed vertex dots rendering slightly offset from the vertex position with an orthographic camera.
- Fixed `Poly Shape` creation tool not recognizing terrain when adding the origin point.
- Fixed `Select Faces with Color` not selecting faces with no color.
- Fixed compile error in runtime samples on Unity 2019.3.
- Fixed `Split Vertices` not collecting coincident vertices when selected with a mouse click.
- Fixed case where `Apply Material` would not be registered for Undo.
- Fixed `Export OBJ` resulting in corrupted files when exporting multiple objects as a single model.
- Fixed `Poly Shape` tool incorrectly rendering a mesh preview before the shape is finalized.
- Fixed case where `Poly Shape` could leave the active tool in an invalid state.
- Fixed case where drag selecting a single vertex would enable "Collapse Vertices" in toolbar
- Fixed case where drag-and-dropping material onto selected faces applies material to all faces if "Edit UVs in Scene" enabled
- Fixed `Box Project UVs` not resetting UV coordinates to origin.
- Fixed naming conflict with `UnityEngine.Snapping` in Unity 2019.3.
- Fixed `Apply Material` not respecting the current face selection when editing UVs.
- Fixed the dimensions overlay graphics not updating when modifying vertices.
- Fixed bug where selecting an element with the `shift` key held would not make it the active selection.
- Fixed `Poly Shape` tool not clearing the mesh when an invalid path is created.
- Fixed `Collapse Vertices` action showing as available in some cases where the action was not applicable.
- Fixed an issue where setting the toolbar to use icon or text mode would not immediately refresh the UI.
- Fixed bug where exporting an OBJ could fail if a mesh did not have vertex colors.
- Fixed the shortcut for `Copy UVs` on macOS referencing `Control` instead of `Command`.
- Fixed an issue where the ProBuilder toolbar font size would initially be very small, then later return to the correct size (specific to Unity 2019.3).
- Fixed a bug that caused the `Material Editor` to not render the preview material on HDPI screens.
- Fixed a null reference error when attempting to subdivide a face with non-contiguous triangles.

### Changes

- Assembly definitions now have `Auto Referenced` enabled, meaning it is no longer required that a project use Assembly Definition files in order to access the ProBuilder API.
- Project layout restructured such that the git url may now be used as a version in the project manifest.
- Improved the default UV layout of Cone shape.
- Settings Manager is now a dependency instead of bundled with code.
- Changed the default value of `Apply Transforms` to false when exporting models.
- Changed the default value of `As Group` to true when exporting models.

## [4.0.4] - 2019-03-13

### Bug Fixes

- Set the pivot of merged meshes to the active selection pivot instead of origin.
- Fixed `Reset Auto UVs` not breaking element and texture group associations.
- Fixed in-scene texture tool setting faces to manual when editing in face mode.
- Fixed undo not refreshing the UV editor graph.
- Fixed UV editor tools remaining disabled after a selection change.
- Fixed `Auto Stitch UVs` action not getting correct edge for adjacent faces.
- Fixed `Boolean Tool` expecting certain mesh attributes to be present, causing errors in some cases.
- Fixed texture groups not saving correctly in some cases.
- Fixed selected object outline not rendering in specific cases.
- Fixed typo in Mirror Objects tooltip.
- Fixed `Export Asset` breaking prefab instances.
- `Vertex Color` action tooltip no longer mentions deprecated paint mode.
- Fixed changelog dates for January 2019.
- Fixed Material Editor rendering incorrectly when an SRP material is assigned to the "Quick Material" field.
- Lessen an edge case where `Math.PointInPolygon` could return the wrong value.
- Fixed an exception when keyframing mesh properties via Timeline.
- Fixed project settings not being consistently being saved in certain cases.
- Fixed assertion when importing ProBuilder to an empty project.
- Fixed warning in tests caused by a left-over meta file from a temporary directory.
- Fixed creating a new PB shape with the Shape Tool does not dirty the scene.
- Fixed MaterialEditor not filtering out unused submesh index when applying new material.
- Fixed Vertices/Faces/Edges picking in Prefab mode.
- Fixed Action window in UVEditor not scrollable.
- Fixed deep selection on faces shouldn't trigger when multiple faces are selected.
- Fixed UV action panel scroll does not work if nothing or just one vertex selected.
- Fixed `ProBuilderize` action potentially creating un-editable geometry if "Import Smoothing" is enabled.
- Fixed change to one member of a UV group not replicating to the other member of the group.
- Fixed UV group not having their settings applied at the same time.
- Fixed sometimes needing to click twice on a face in the UV editor to the right actions.
- Fixed de-selecting vertices not working properly in certain situations.
- Fixed edge selection sometimes picking edge behind the mesh.
- Fixed UV panel shows incorrect info when zero UVs are selected.
- Fixed deep Selection should not trigger if no elements are selected.
- Fixed Element Selection lost after using Weld action.
- Fixed UVs converting to manual when merging 2 objects.
- Fixed Selection being discarded when connecting edges.
- Fixed trigger and collider types not always hiding properly.
- Fixed Box Project resulting in skewed UVs.
- Fixed in-scene Texture Move tool not translating UVs at a constant speed when scaled.
- Fixed `Weld Vertices` and `Collapse Vertices` sometimes leaving invalid edges in affected faces.
- Fixed ProBuilder window not updating when toggling handle orientation from shortcut key.
- Fixed `Subdivide` action resulting in horizontally flipped UV coordinates.
- Fixed `Box Project` action resulting in horizontally flipped UV coordinates.
- Fixed UV Editor preview texture sometimes not appearing.
- Fixed lag when setting select mode via shortcuts.
- Frame bounds focuses on element selection when in Object mode.
- Apply material incorrectly applies to face selection when in Object mode.
- Dimension overlay displays element bounds when in Object mode.
- Fixed handle position not updating when the element selection mode changed.
- Fixed `Strip ProBuilder Scripts on Build` not being respected.
- Fixed `Export OBJ` not sharing vertex data, resulting in split vertices in other DCCs.

### Changes

- Name newly merged `GameObject`s after the active selected `GameObject`.
- Changed the order of UV transform operation (rotation and scale are now called before applying the anchor).
- Changed picking to now prefers the object hovered over the selected object.
- Added "Open ProBuilder" button back to `ProBuilderMesh` component Editor.
- Increased the minimum size of Auto UV Editor window to not require vertical scrollbars.
- Significantly improve the accuracy of `Convert to Auto UV` action.
- Add support for auto UV unwrapped faces in the `Auto Stitch UVS` action.
- Use `ShortcutManager` API to manage single-key shortcuts.
- Added option to show current tool delta in the Scene View ("Show Handle Info".)

## [4.0.3] - 2019-01-25

### Bug Fixes

- Fix an issue that caused version validation to run when entering playmode, resulting in large memory spikes and lag.
- Fix tooltips closing other popup dialogs.

## [4.0.2] - 2019-01-18

### Bug Fixes

- Fix OBJ export failing due to missing materials.

## [4.0.1] - 2019-01-16

### Bug Fixes

- Add missing [3.0.9] changelog entry.
- Update package metadata to meet current requirements.

## [4.0.0] - 2019-01-14

### Features

- New public API.
- Project now distributed as source code, with assembly definition files.
- Add experimental pre-selection highlight for vertices and faces (enable in Preferences / ProBuilder / Experimental).
- Improve the behaviour of vertex and edge selection with hidden faces.
- Add ability to resize the UV settings window.
- Dimensions overlay now works with mesh element selections.
- Update FBX Exporter integration to use version 2.0.0.
- Improve performance of UV calculation methods.
- Improve the default UV mapping of sphere primitives.
- Support new 2018.3 prefab system.
- Redesigned Lightmap UV workflow now exposes settings on the ProBuilderMesh component, provides a modifiable default value, and is generally smarter about keeping Lightmap UV channels in sync with changes.
- Improve performance of toolbar rendering by caching some frequently accessed selection information.
- Redesigned settings interface, now supports search and resetting individual fields.
- Add support for `Pivot` and `Center` handle position toggle.
- Handles now support operating in selection space (Position: Pivot + Orientation: Normal).
- Texture scene tool now supports vertices and edges.
- Improve performance of mesh rebuild functions.
- Improve performance of vertex, edge, and face gizmos.
- Respect Unity pivot mode and pivot orientation (note that setting `Orientation: Normal` overrides the Unity pivot orientation).
- Add a preference to disable depth testing for mesh selection highlights.
- Add manual documentation.

### Bug Fixes

- Fix regression that broke dragging and dropping GameObjects onto ProBuilder meshes.
- Fix Poly Shape and Bezier Shape incorrectly resetting materials to default.
- Fix `Export` not generating UV2 in some cases.
- Fix `Export` functions not refreshing the Project view.
- Fix edge colors not matching preferences.
- Fix oversized vertex handle pre-selection billboard.
- Fix `Collapse Vertices` breaking mesh topology.
- Fix "UV Overlap" warnings on default shapes when baking GI.
- Fix mismatched plane width, height segment fields.
- Fix ProGrids "Push to Grid" affecting un-selected vertices.
- Fix `Extrude` incorrectly applying smoothing groups to extruded face sides.
- Fix `Detach to GameObject` sometimes including children in duplicated `GameObject`.
- Fix vertex handle pre-selection gizmo drawing 2x larger on scaled screens.
- Fix "Detach to GameObject" deleting the current face selection.
- Fix deprecated GUID check running on every domain reload.
- Fix `ProBuilderize` importing quad topologies with incorrect winding.
- Fix `Extrude Edges` sometimes splitting vertices when extruding as a group.
- Fix toolbar vertex actions showing as available in some cases where not applicable.
- Fix case where drag selecting mesh elements could clear the current selection.
- Fix element preselection highlight incorrectly showing when a GUI control has focus.
- Fix `Create Poly Shape` throwing errors in some cases.
- Fix `Connect Edges` action showing incorrect results in notifications.
- Fix incorrect use of object finalizers in some classes.
- Fix vertex drag selection with "Select Hidden: Off" omitting distinct but coincident vertices.
- Fix changes to `MeshRenderer` materials being incorrectly reset by ProBuilder.
- Fix `Delete Faces` tooltip not showing "Backspace" as the shortcut key on Windows.
- Fix Auto UV settings inspector not allowing certain properties to be edited with multiple selections.
- Fix face, edge, and vertex modes requiring user to first select an object before registering element selection when clicking.
- Fix bug where adjusting shape creation parameters would move the preview mesh.
- Fix bug where the mesh element gizmos would not respect the screen DPI on startup.
- Fix `Pipe` and `Sphere` shapes not setting a consistent pivot point.
- Fix possible null reference exception when deleting the `Shape Editor` preview mesh.
- Automatically destroy invalid `Poly Shape` objects when the selection is lost.
- `Detach to GameObject` now sets the detached object as a child of the donor mesh.
- Fix vertex billboards not rendering when the backing graphics API does not support geometry shaders.
- Fix new shapes not instantiating on the grid when `Snap New Shape To Grid` is enabled.
- Fix potential error when `Poly Shape` enters an invalid path.
- Fix `Poly Shape` not instantiating on grid when ProGrids is enabled.
- Fix toggling static flags not consistently rebuilding lightmap UVs when `Auto Lightmap UVs` is enabled.
- Fix `Poly Shape` not aligning with the ProGrids plane.
- Fix FBX Exporter incompatibilities breaking compiliation.
- Fix stair shape showing dimensions field twice.
- Fix incorrect wireframe overlay when editing a `Poly Shape`.

### Changes

- Tests and documentation are no longer imported with package, significantly improving initial import times.
- Face selection highlight is now rendered with both front and back faces.
- Adding custom actions to the ProBuilder toolbar is now done by registering an attribute.
- ProBuilder Debug Editor removed.
- Rename `MenuAction::DoAlternativeAction` to `DoAlternateAction`.
- Simplify assembly definition files, merging ProBuilder.Core & ProBuilder.MeshOperations to single assembly.
- Minor performance improvements to some common mesh editing actions.
- Remove "Precise Element Selection" preference.
- Project preferences are no longer saved in the Assets directory (now located at "ProjectSettings/ProBuilderSettings.json").
- Improve performance of normal and tangent calculations.
- `MeshSelection.Top()` becomes `MeshSelection.top` property.
- Include third party dependencies as source code with assembly definitions instead of pre-compiled DLLs.
- Performance optimization for selection changes in editor.
- Make auto-resizing colliders opt-in instead of on by default.
- Use the last selected mesh element as the active selection pivot, matching object selection.
- Remove "About" window.
- Expose APIs necessary for mesh element picking at runtime.

### Changes since [4.0.0-preview.41]

- Testing new CI runner.

## [3.0.9] - 2018-05-30

- Fix exporting to OBJ and PLY not refreshing assets when the destination directory is in the project.
- Fix bug that broke drag and dropping prefabs onto ProBuilder meshes.

## [3.0.8] - 2018-05-07

### Bug Fixes

- Fix incompatibility with ProGrids 3.0.1.

## [3.0.7] - 2018-04-30

### Features

- Add a material property to the Poly Shape component.

### Bug Fixes

- Poly Shape creation now respects the user-set default material.

## [3.0.6] - 2018-04-19

### Bug Fixes

- Fix upgrade prompt failing to show on version changes.

## [3.0.5] - 2018-04-16

### Bug Fixes

- Fix About Window opening on every script reload event.

## [3.0.4] - 2018-04-10

### Bug Fixes

- Use the default material preference for shapes created through the Boolean Editor.

## [3.0.3] - 2018-04-05

### Features

- API Examples are now published on [Github](https://github.com/Unity-Technologies/ProBuilder-API-Examples).
- Expose poly shape creation methods. Add API example.
- Support drag and drop materials to ProBuilder meshes.
- Mesh handles now use Unity gizmo colors by default.
- New options to set unselected and selected edge colors.
- New option to set edge and wireframe line width (not available on Metal).

### Bug Fixes

- Fix scene info not updating with selection changes.
- Fix `Apply Material` only applying to parent gameobjects if children are also selected.
- Fix `pb_Object.SetSelectedFaces` setting duplicate vertex indices.
- Fix `Alt+Num` material shortcut throwing null if Material Editor has not been opened and no default palette is found.
- Fix bug where `Undo` on a Poly Shape would reset any mesh edits.
- Fix preferences interface not updating after resetting all preferences.
- Fix bug where the edge picker would prefer vertical lines over horizontal.
- Fix wireframe rendering with unselected edge color in certain cases.
- Fix edge selection preferring vertical edges.
- Expand preferences window contents to match size.
- Don't show "shortcuts were cleared" warning if no prior version is detected.
- Fix overexposed imgui controls in scene view with scene lighting disabled.
- Fix certain actions switching the current scene focus.
- Fix Create Material Palette failing to create asset.
- Export model files with culture invariant settings.
- Fix scene info display overlapping ProGrids toolbar in some cases.
- Fix local preferences not loading until restarting the ProBuilder editor.
- Fix Boolean Editor rendering a white texture filling the entire screen.

### Changes

- Remove update checking.
- Expose `pb_MeshImporter` class, making "ProBuilderize" action available at runtime.

## [3.0.1-f.0] - 2018-02-12

### Features

- ProBuilder now runs as package manager module.
- Custom color palettes for Vertex Color Editor.
- Added a new window to display licenses for third party software used in ProBuilder.
- Debug symbols are now included.
- Support partial and complete drag selection for edge elements.
- New `Set Trigger` and `Set Collider` replace the deprecated `pb_Entity` component.
- Add `pb_ShapeGenerator.CreateShape(pb_ShapeType shape)` function to build default primitives without requiring parameters.
- Add preference to set the static flags for new ProBuilder objects.

### Bug Fixes

- Fix "Select Hidden" preference not matching the state shown in toolbar on launch.
- Fix Standard Vertex Color shader errors in 2018.1 and up.
- Catch edge cases of bad input for Poly Shape, preventing console spam.
- Match element drag rect visual to Unity's.
- Fix Poly Shape objects not updating graphics on undo when not currently editing the contour.
- Fix "Connect Edges" notification showing 2x the amount of edges as connected.
- Fix ProBuilder Editor not refreshing after a ProBuilderize action.
- Fix UV editor throwing null if shader has no _MainTex uniform.
- Fix potential error if a menu action is called without a pb_Editor instance.
- Improve performance of pb_Editor selection caching, addressing lag when selecting high vertex count objects.
- Fix scene toolbar sometimes not matching free/pro skin.
- Fix washed colors in gui textures with linear rendering.
- Remove unused preferences from the ProBuilder/Preferences window.
- Fix torus and icosphere not remembering last used settings in a session.
- Fix Poly Shape editor repainting the scene view more than necessary.
- Corrected tool-tips for "Clear Smoothing" and "Break Smoothing" buttons.

### Changes

- Move Dimensions Overlay into ProBuilder menu.
- Move `ProBuilder2.Common` and `ProBuilder2.EditorCommon` namespaces to `ProBuilder.Core` and `ProBuilder.EditorCore`.
- Move `pb_Object` to ProBuilder.Core namespace.

### Known Issues

- "Library already loaded" warning logs when importing Package Manager over Asset Store install.
- "Cannot create menu item" warnings when importing Package Manager over Asset Store install.
- Deleting an Asset Store package before importing a Package Manager version does not automatically pop up the Convert To Package Manager editor window.
- API Examples are not accessible from Package Manager.

### Changes from 3.0.0-f.1

- Fix `pb_Fbx` errors when Unity FbxExporter is present in project.
- Update Package Manager description and display name.

## [2.9.8-f.2] - 2017-11-01

### Bug Fixes

- Fix shader compile errors when targeting mobile platforms.
- Fix possible overflow in vector hashing functions.
- Fix ProBuilderize failing to import quads in some cases.
- Fix FBX export not including manually unwrapped UVs.
- Fix toolbar using old icons with basic skin.

## [2.9.7-f.5] - 2017-10-23

### Features

- Unity 2017.3 beta support.
- New toolbar icons (Right/Context + Click in Toolbar -> Use Icon Mode).
- Significantly improved quad detection in ProBuilderize function.
- ProBuilderize now able to import smoothing groups.
- Support exporting quads to FBX format (requires Unity FbxExporter in project).
- Newly redesigned `Smooth Groups Editor`.
- New `Select Face Loop` and `Select Face Ring` actions.

### Bug Fixes

- Fix possible null reference when picking ProBuilder objects.
- Fix "Select Hole" disappearing instead of showing as disabled.
- Fix "Extrude Face" disabled icon not matching current mode.
- Fix Standard Vertex Color shader preventing builds on some platforms when fog is enabled.

### Changes

- Don't show `pb_Lightmapping` warnings by default.
- Smoothing groups may now extend beyond the 42 provided in the editor. Any smoothing group between 1 and 24, or greater than 42 is treated as a smooth face (currently only accessible in code).
- Set ProBuilder Standard Vertex Color shader fallback to "Standard."

## [2.9.5-f.3] - 2017-08-30

### Features

- Deep selection support when clicking faces.

### Bug Fixes

- Drag select with "Select Hidden: Off" now works consistently in Unity 5.6 and up.
- Fix automatic lightmapping attempting to update while ProBuilder is modifying geometry.

### Changes

- Unity 4.7 and 5.0 are no longer supported (2.9.4 will continue to be available for these version of Unity).

## [2.9.4-f.1] - 2017-08-03

### Features

- When a lightmapping finishes baking show a warning if any ProBuilder objects marked as Detail were left out due to missing UV2s.

### Bug Fixes

- Fix single key shortcuts emitting system beep on Mac.
- Fix vertex movement not respecting snap settings when ProGrids is placed in Plugins folder.
- Don't delete UnityEngine meshes when unloading a scene in play mode.
- Fix occasional crashes when using Select Hole and Fill Hole actions.
- Fix hang on opening context menu with very large projects.
- Fix Vertex Color Palette not applying colors correctly in vertex mode.

## [2.9.3-f.0] - 2017-07-13

### Features

- Support exporting quads in OBJ format.
- Add a context menu item to quickly create material palettes from the current Asset window selection.

### Bug Fixes

- Use `additionalVertexStreams` mesh attributes where possible when ProBuilderizing.
- Fix for Windows version sometimes endlessly creating folders when root folder moved from Assets/ProCore.

## [2.9.2-f.1] - 2017-06-22

### Bug Fixes

- Add file stubs for deprecated repair actions to prevent compilation errors when updating.

## [2.9.1-f.0] - 2017-06-12

### Features

- New PLY model exporter.
- OBJ export rewritten, now supports:
	- Multiple texture maps
	- Vertex colors (MeshLab format)
	- Texture map offset / scale
	- Local or world space mesh coordinates
	- PBR maps (http://exocortex.com/blog/extending_wavefront_mtl_to_support_pbr)
- Improve Mesh Asset export dialog and options.

### Bug Fixes

- Fix ProBuilder preferences sometimes not loading.
- Fix deprecated method warnings in 2017.1 beta.
- Fix occasionally flipped UV axis on merged faces.
- Don't crash if ProBuilder folder has been renamed.

## [2.9.0-f.3] - 2017-05-22

### Features

- Support for saving and loading custom Material Palettes.
- ProBuilder now able to store preferences per-project as well as globally.
- Significantly improve performance of Weld Vertices function.

### Bug Fixes

- Preferences Window now renders correctly in Unity 5.6 and up.
- Exit Bezier Shape editing when Esc is pressed.
- Quell unnecessary errors when ProGrids interface fails to load UnityEditor assembly.
- Fix potentially ambiguous reference to Axis enum in API examples.
- Fix mesh leak when exporting STL files.

### Changes

- Move pb_Constant into ProBuilder2.Common namespace.
- Move pb_Lightmapping class in EditorCommon namespace.
- Improve wording of warning when shortcut preferences are reset.

## [2.8.1-f.0] - 2017-04-17

### Features

- Improve grid snapping when placing Poly Shapes.
- Add a callback when a mesh is rebuilt (pb_EditorUtility.AddOnMeshCompiledListener).
- Remove max width limitation on Material Editor window.

### Bug Fixes

- Fix incorrect UV render scaling on retina and other scaled screens.
- Fix deprecated warnings on Handles calls in Unity 5.6.
- Fix Icosphere API example deprecated function calls in Unity 5.6.
- Fix UnityObjectToViewPos warnings Unity 5.6.
- Fix Poly Shape not generating UV2 for mesh.
- Catch an occasional Null Reference when viewing UV2 channel.
- Fix Null Reference in Poly Tool undo callbacks.
- Fix errors in adding pb_Entity script during repair mesh references action.
- Improve consistency of Vector2/3/4 hashing functions.
- Fix particularly slow function in MergeFaces action.
- Fix preferences GUI layout.

### Changes

- Start Poly Shape height at 0.
- First Poly Shape click always sets pivot.

## [2.8.0-f.1] - 2017-03-29

### Features

- New "Poly Shape" interactive shape.
- New "Bezier Shape" interactive shape.
- Unity 5.6 beta compatibility.
- Improve default UV layouts for new shapes.
- Add a shader for reference billboard planes ("ProBuilder/Reference Unlit").

### Bug Fixes

- Fix material editor applying to child transforms of selection.
- Fix instantiated objects not getting a UV2 channel when "Auto Generate UV2" is enabled.
- Material Editor now works with a relative path.
- Fix incorrect handle rotations in Element mode.
- About Window now loads even when not in *Assets/ProCore/ProBuilder*.
- Address a rare NullReferenceException when ProBuilder Editor is initialized.
- Don't spam Console with errors if update check fails to connect (only affects WebPlayer target).
- Add a more descriptive message to update check if connection fails.

### Changes

- Automatically toggle Detail Entity Type object's lightmapping static flag, preventing broken lightmap atlases.

## [2.7.0-f.7] - 2017-02-24

### Features

- New redesigned "About" window.
- New "Check for Updates" window and menu item.
- Add a repair script to apply materials when upgrading from Basic to Advanced.
- Include option to restrict "Select by Vertex Color" to current selection.
- Add "Generate Shadow Volume" API example and action.
- Add preference to enable experimental features (current feature: Bezier shape).
- Add option to smooth round sides of cylinder in Shape Tool.
- Add repair script to strip and rebuild pb_Object from Unity mesh.
- Add repair script for rebuilding shared index caches (addressed IndexOutOfRange errors in RefreshNormals function).

### Bug Fixes

- Fix inconsistent About Window behavior when importing updates.
- Make face highlight code snippet more robust in Runtime Editing example.
- Don't leave a progress bar behind if Probuilderize fails.
- Fix ProBuilder-ize adding pb_Entity multiple times.
- Apply Quick Offset adjustments to all selected ProBuilder objects.

## [2.6.9-f.3] - 2017-01-27

### Bug Fixes

- Fix vertex handles appearing offset when using an orthographic scene view.
- Mark KDTree Triangle and pb_Stl as Any Platform in build targets.
- Fix compile errors in standard vertex color shader on Unity 5.5 on iOS
- Fix compile warnings in Unity 5.6.0b4
- Use multi-Unity-version compatible shader in Vertex Colors API example.

## [2.6.8-f.1] - 2017-01-16

### Features

- Add option to restrict Select by Material to the current selection.
- Add alternate method of specifying torus radius dimensions.

### Bug Fixes

- Fix regression that broke iOS and Android build targets.
- Fix arch preview shape and built shape sides not matching.
- Fix issue where UV Editor could be out of sync with scene UVs after planar or box projection.
- Don't fail face or vertex picking when the required materials aren't found.
- Whenever a prefab change is detected rebuild the mesh.
- Fix null reference when attempting to bevel open edges.
- Remove unused and buggy Debug UVs shader.

## [2.6.7-f.4] - 2017-01-09

### Features

- New face extrusion options to extrude along face normal, vertex normal, or per-face.

### Bug Fixes

- Fix potential drag selection inconsistencies when picking vertices in Unity 5.5.
- Fix bug where Backspace key in UV Editor with a GUI control focused would incorrectly register as a shortcut.
- Fix auto uvs rotating around handle pivot when using the rotate gizmo.
- Add undo for auto uv changes made by gizmos.
- Fix uv tiling toolbar not applying with mixed selection.
- Move pb_Reflection to Editor so as not to be included in builds, fixing errors when targeting Windows Store.  Remove unused ParseEnum<T> function in pbUtil for same reason.
- Retain smoothing group information when extruding.
- When flipping a selection of face edges where faces are not quads be specific about the reason for failing.
- Fix misc. potential hangs when registering ProBuilder objects for Undo.

### Changes

- Make default height segments for cylinder 0.
- Move some common class files into the Core folder so they're included with the Core lib instead of MeshOps.
- When grow selection by angle is off force iterative to on.

## [2.6.6-f.0] - 2016-12-09

### Features

- Unity 5.5 support.
- Improve readability of Unity version -> ProBuilder package chart.
- Improve performance of mesh rebuild functions.
- Add API example showing how to highlight faces based on distance to a point.

### Bug Fixes

- Don't update mouse edge when not in geometry mode.
- Fix face and edge previews only rendering a subset of elements on meshes with high vertex counts.
- ProBuilder Editor doesn't need to be open to export meshes.
- Fix weird triangulation on Door shape.
  Fix some incorrect calls to pbUndo.RecordObjects that could cause Unity to lock up with large meshes.
- Fix bug where "Export Mesh Asset" would always create an extra folder.

### Changes

- pb_Object.Refresh now accepts a bitmask to enable/disable different component refreshes.
- Move Create ProBuilder Cube menu item to "GameObject/3D Object/ProBuilder Cube".

## [2.6.5-f.0] - 2016-11-11

### Features

- Add procedural mesh extrusion example.

### Bug Fixes

- Fix bug where face/vertex picking with hidden selection off and intersect rect would not work in deferred rendering path.
- Fix bug when "Meshes are Assets" is enabled where exiting play mode would clear the mesh cache.
- Fix bug when "Meshes are Assets" is enabled where entering play mode would invalidate the mesh cache.
- Fix bug when "Meshes are Assets" is enabled where deleting a selection of pb_Objects would leave orphaned cached meshes.
- Fix bug where UV2 channel of selected objects would be lost on scene save with "Meshes are Assets" enabled.
- Fix bug where stripping ProBuilder scripts with "Meshes are Assets" enabled would also delete the cached mesh asset.
- Fix incorrectly scaling slider control on retina display.

## [2.6.4-f.1] - 2016-10-19

### Features

- Unity 5.5 Beta compatibility.

### Bug Fixes

- Fix bug where generating UV2 would incorrectly merge incompatible vertices in the optimization function due to vertex references not being unique when returning from GeneratePerTriangleMesh.
- Add icons for `Drag Selection Mode`.
- Add `Toggle Drag Rect Mode` icons.
- Fix selected vertex billboards not rendering in 5.5 beta.
- Fix crash when setting pivot with multiple objects selected.
- Fix setting pivot of a parent object moving children.
- Don't disable the object outline when Probuilder is open, but do disable wireframe.
- Fix prism shape height resulting in half-sized shapes.
- Add `Triangulate Faces` icon.
- Ensure icons are always imported with the correct settings.
- Fix "Generate Scene UVs" toggle not being respected in `Generate UV2` action
- Fix bug where drag selecting with shift in subtraction mode with complete rect selection would always deselect the entire selection.

## [2.6.3-f.4] - 2016-09-23

### Features

- New `Vertex Positions Editor` provides fine-grained control over vertex postions.
- Add `Anchor` setting for Auto UVs to align faces before user transforms.
- When drag selecting faces add option to select by intersection.  Toggle with menu item `Drag: Complete/Intersect`.
- New icons for `Select by {Material, Color}`.
- Significantly improve editor performance when drag selecting faces and vertices with `Select Hidden: Off`.
- New alternate drag selection modes: `Add`, `Subtract`, and `Difference` affects how the `Shift` key modifies selection when drag selecting elements.

### Bug Fixes

- Fix bug where using Undo in face selection mode could potentially delete faces on prefabs.
- Fix import settings on icons.
- Fix bug where `Fill Hole` would not correctly align normals after operation, sometimes also reversing neighboring face normals.
- Fix bug where `Fill Hole` would sometimes leave the filled faces with invalid edge caches (resulting in incorrect normals on extruding from the filled face).
- Added miscellaneous missing actions to documentation.
- Fix bug where drag selecting faces would sometimes leave the picker rect visible.
- Disable Select By Material/Color when no elements are selected.

### Changes

- Use Unity's default UV2 unwrapping parameters since they seem to produce generally better results than the padded params currently in use.
- New ProBuilder objects instantiate with `ShadowCastingMode.TwoSided` (configurable in Preferences/ProBuilder).

## [2.6.2-f.0] - 2016-09-02

### Features

- Add STL file export support.
- Standard Vertex Color shader now compatible with Unity 5.5 beta changes.
- Add API Example showing how to set custom default UV2 Unwrap Parameters.

### Bug Fixes

- Fix bug where exporting OBJ would sometimes insert "AllFiles" into the file path.
- Fix drag selecting edges then bridging sometimes using the default material instead of a neighboring one.
- Fix compile error when using Fog with Standard Vertex Color shader.
- Fix console warnings in Unity 5.5.0b1
- Fix bug where `Select Edge Loop` would select too many edges.
- Don't show missing icon warnings unless in PB_DEBUG mode.

## [2.6.1-f.0] - 2016-08-26

### Features

- Add `Triangulate Face` action.
- Add ability to view UV2/3/4 channels in UV Editor.
- Add ability to edit per-object UV2 generation parameters in the Generate UV2 options menu.
- Improve performance of "Grow Selection" when flood selecting with angle restriction.
- Improve performance of some selection actions when in face mode.
- Add RenameNewObjects script to API examples folder (shows use of OnProBuilderObjectCreated delegate).
- Add "Select Faces with Material" and "Select Faces with Vertex Color" to the Selection menu.
- New options icon in toolbar: gear instead of triple lines.

### Bug Fixes

- Fix "About" window showing every changelog ever instead of just the latest.
- Fix bug in pb_Math.Normal(pb_Face) overload that would potentially return normals facing the wrong direction if fed ngons.
- Fix UV Editor incompatibilities with retina display on macOS.
- Fix bold label text color in Debug Window when Pro skin is used.
- Increase max allowed vertex handle size to 3 to accomodate macOS retina display.
- Fix import settings for `Center Elements` disabled icon.
- In ProBuilder-ize function don't bother showing 'include children' dialog if the top selection already contains all valid meshfilters
- Fix Advanced icon in PB Basic rendering blurry in toolbar.
- When freezing transforms also apply rotation in world space.  Fixes some issues when freezing hierarchies of objects.
- Fix bug where edge ring would include faces with odd number of edges.
- Added "Fill Hole" and "Subdivide Edge" to documentation.
- Fix occasionally flipped face normals when connecting edges or vertices on n-gons.
- When connecting edges weed out any edges that don't connect to anything, preventing accidental edge subdivisions.

### Changes

- Make default angle error for uv2 unwrap a little higher to avoid bad unwraps in some common cases.
- Move ProBuilder-ize function to menu actions.

## [2.6.0-f.1] - 2016-08-02

### Features

- Add `Bevel Edges` action.
- Add `Fill Hole` action to quickly insert a face in a mesh hole (with option to fully select and fill hole or just the selected parts).
- Completely rewritten documentation: http://procore3d.github.io/probuilder2/
- Add `Select Hole` menu action to quickly select the edges of any hole touching a selected vertex.
- Add a preference to disable "Precise Element Selection."  When disabled edge and vertex modes will always select an edge or vertex no matter how far from the element they are.
- Add "Break Texture Groups" button to UV editor.
- Add non-manifold edge extrusion pref to Extrude Edge settings window.
- Replace mesh optimation functions with faster and more accurate versions.
- Improve performance of topology query operations (Grow Selection, Shrink Selection, Edge Loop, Edge Ring).
- `Center Pivot` action now available in Basic.
- Add Generate UV2 toolbar entry when "Disable Auto UV2 Generation" is enabled.
- Add a delegate in `pb_EditorUtility` to notify subscribers when a new pb_Object has been initialized.
- New API example `Tools > ProBuilder > API Examples > Log Callbacks Window` demonstrates hooking into various editor delegates.
- Adds an experimental new option to store Mesh objects as Assets in the project so as not to clutter the Unity scene file. Use with a prefab for maximum scene lean-ness.  Enable this feature in Preferences/ProBuilder/Experimental/Meshes Are Assets.
- Add support for local/common toggles in Edge Debug mode.
- Add `Select Holes` action to editor toolbar (selects all connected open edge paths).
- `Connect {Edge, Vertices}` re-factored for speed and more robust edge case handling.
- New "Options" button for toolbar icons.
- Improve performance of `Delete Faces` action.
- Improve performance of `Subdivide` action.
- Add `Alt-S` shortcut for `Subdivide` action.
- Add option to `Mirror` action to either duplicate or move the selection when mirroring.

### Bug Fixes

- Fix some instances where modifying a mesh would result in NaN warnings.
- Fix icosphere audio example scene in ProBuilder Basic.
- Add `Center Pivot` action to menu.
- Bypass sRGB sampling for icons, fixing dark appearance in pro skin.
- Fix regression where switching between icon mode and text mode in toolbar would sometimes not immediately reload the toolbar.
- Fix an issue where meshes would be discarded and rebuilt on every instance id change, which Unity does a lot.  The result of constant mesh rebuilds being invalidating the lightmap, making getting a decent bake very difficult.
- Ignore API examples in any build target that hasn't been tested (which is all of them save for standalones).
- Fix edge extrusion leaving black geometry when extrusion fails.
- Add extrude settings button to edge extrude toolbar item.
- Add a single context-sensitive Extrude shortcut so that super+e works properly in both edge & face modes.
- Fix 'KeyNotFound' exception when centering pivot sometimes.
- Fix UV3/4 assignment and getter functions reading out of bounds channel index.
- Fix Delete key notification not showing.
- Fix editor toolbar "leaking" due to incorrect hideflags in Unity 4.
- Fix cases where user could provide bad input to Arch generator.
- Fix `Weld Vertices` not welding vertices in some cases.
- Set detail pb_Objects with ReflectionProbeStatic flags.
- Fix key shortcuts for hidden but enabled menu actions not working.
- Don't show hover tooltips if mouse is outside window bounds.
- Fix some edge cases in `Conform Normals` action.
- Fix `Grow Selection` itererative field incorrectly being disabled when "Grow by Angle" is off.
- Fix issue where n-gons with > 90 degree angles would not auto UV unwrap correctly.
- Fix some cases where subdivide would fail due to non-planar vertex positions on a face.
- Fix bug where extruding edges or faces would sometimes align the inserted face normals incorrectly.
- Hide geometry actions when in object mode.
- Fix edge selection when mouse is hovering an object not in the selection but a valid edge is within distance of mouse.
- Fix bug where subdividing a face with an adjacent concave n-gon would break the adjacent face.
- When generating the menu item text for shortcuts always use lower case, since Mac doesn't recognize upper case as shortcuts.  Fixes an issue with shortcuts not working on OSX.
- Support cases where texture groups on pb_Object aren't in linear order.
- Clear debug lines when a selected object is deleted.
- Fix bug where `Detach Faces` to submesh would incorrectly split all the detached selection vertices.
- Put UV Editor in namespace, preventing errors where common function names would be confused with other assets.
- In `pbUndo` use each individual object vertex count when deciding whether to diff or store object state for undo.  Fixes hang when performing actions with small selections on large objects.
- Lower UV toolbar buttons by 1px when not using the Command GUIStyle since Button style adds 1px padding.
- When building ProBuilder delete user generated folders so that upgrades don't overwrite them.

### Changes

- Menu toolbar re-arranged for consistency.
- Remove UV2 generation parameters from pb_Object.
- Add a public function for setting tangents on pb_Objects.
- Deprecate GenerateUV2 extension method since mesh optimization is now an intertwined process.
- Improve hashing function in IntVec3 and Edge.
- Suffix pb_Math.Approx functions to make implicit casting of vectors more difficult to do accidentally.
- Move "World Space" toggle up in the Auto UV editor
- In Auto UV mode rename the scale property `Tiling`.
- `Detach Selection` now behaves like toolbar option panel instead of popup.
- Remove unnecessary option to save duplicates of selected gameobjects when using `Merge Objects` action.
- In addition to changing the icon and text, also show a brief explanation of the current handle alignment mode in the tooltip.
- Move Mirror to object level, making it an action instead of panel popup.

## [2.5.0-f.0+r4241] - 2016-04-07

### Features

- Toolbar redesign now adapts to both vertical and horizontal layouts (swap between Text and Icons by context clicking in the Inspector).
- New tooltips show inline documentation and keyboard shortcuts.  Hold `Shift` to instantly view hovered tooltip, and turn off tooltips on hover in Preferences menu.
- New `Subdivide Edges` action inserts vertices along selected edges.
- GUI items are no longer stored in Resources.  Changing the location of the ProBuilder directory is still supported.
- Add option to collapse vertices to the first selected vertex instead of always averaging.
- Mark the current mode and floating state in the toolbar context menu.
- Add preference toggle to disable Dimension Overlay lines.
- New Color Mask setting in Vertex Painter Editor enables painting only to specified component.
- Vector4 UV{3, 4} channels can now be stored in pb_Object (use pb.SetUVs(index, List<Vec4>)).

### Bug Fixes

- Fix shortcut editor modifier keys not being correctly stored.
- Fix Freeze Transforms moving objects when selection contains hierarchies of meshes.
- Entity visibility toggles no longer interferes with Collisions, as well as remembers all manually changed object visibility.
- Fix Element Toolbar placement in Scene view on Retina display Macs.
- Fix UVs all being set to {0,0} when using Weld in the UV Editor.
- When extruding an edge check that the new face winding order is equivalent to the face of the donor edge and flip if necessary.
- Fix shortcut editor not recognizing all keycode values (notably Alpha0-9).

### Changes

- Remove option to display Mode Toolbar in the Inspector window.
- ProBuilder2.Math namespace removed, pb_Math now belongs to ProBuilder2.Common.

## [2.4.11-f.0+r4081] - 2016-04-07

### Bug Fixes

- Fix regression in 2.4.10f2 that broke assigning materials in ProBuilder Basic.

### API

- Add `onEditLevelChanged` delegate to pb_Editor to notify other classes of edit level changes (Polybrush compatibility).

## [2.4.10-f.2+r4027] - 2016-04-07

### Features

- Shape and Material windows are now dockable (context click in window and select Window/Set {Floating, Dockable}).
- Add "Snap to Face" shortcut when dragging the move tool (hold 'C' while dragging to snap tool to the nearest face).
- New ShaderForge compatible Standard Vertex Color shader on ProBuilder default material.

### Bug Fixes

- Unity 5.4 compatibility.
- Workaround for Unity crash "Check DisallowAllocation Error"
- Fix most cases of meshes going completely black when modifying them in any way.
- Fix NullRef error when scaling a single selected edge.

### Changes

- Remove various `Get{Vertices, UVs, Triangles}` functions from `pb_Object`.  Use `pbUtil.ValuesWithIndices` directly instead.
- Remove Instantiation API Example (there's nothing special about instantiating ProBuilder meshes anymore).

## [2.4.9-f.1+r3978] - 2016-04-07

### Features

- New "Flip Edge" tool swaps the direction of a connecting edge in a quad.

### Bug Fixes

- Fix bug where Trigger or Collider entities could remain visible in play mode when using source.
- Fix slowdowns when inserting edge loops due to undo.
- Fix missing namespace errors in Unity 5.3.
- Increase the resolution with which vertex positions are compared to avoid incorrectly merging distinct vertices (often causing trouble when modeling at very small dimensions).

## [2.4.8-f.1+r3764] - 2016-04-07

### Features

- Enable Set Pivot, Delete, and Vertex Painter in ProBuilder Basic.
- New Standard Shader with vertex color support (thanks to Unity Forum user @defaxer).

### Bug Fixes

- Add tooltips for every action in the ProBuilder toolbar.
- Fix consistent horizontal scrollbar showing in ProBuilder window.
- Smooth edges of curved  stair sides, and align step UVs to match rotation.
- Use white text color in Dimensions Overlay when Unity Personal skin is used.
- Ensure DLLs retain GUID between releases, enabling simpler upgrades.
- Fix poor UV editor precision when working with small distances.
- Fix ~10px vertical offset image when rendering UV template in Unity 5.
- Fix slightly offset image when rendering UV template from docked UV Editor window.

### Changes

- Increase minimum allowed zoom in UV editor.
- Make warning shown when connecting edges or vertices fails a bit more descriptive.
- Don't show tangents and bitangents when Show Normals is enabled in the smoothing editor.
- Prototype becomes ProBuilder Basic.

## [2.4.7-f.0+r3664] - 2016-04-07

### Changes

- New upgrade procedure skips complicated Upgrade Kit in favor of a slightly more manual but more reliable approach.  See FAQ or ProBuilder Documentation for more information.

### Bug Fixes

- Fix possible null reference error when working with prefabs that have been duplicated.
- Additional error checking when stripping pb_Objects from scene added.
- When ProBuilder-izing objects, ask user whether or not to traverse children.

## [2.4.6-f.0+r3616] - 2016-04-07

### Features

- Add preference to set default Entity type.
- Add preference to set dedicated keyboard shortcuts for entering Object, Face, Vertex, and Edge mode.
- New Curved Stair generator, and stairs now produce manifold geometry.
- Add "Batch Upgrade" menu items to Upgrade Kit, allowing users to run one action for an entire project.
- New GUI slider allows un-clamped input to the float field in Shape Tool.

### Bug Fixes

- Alt + E shortcut now works with only 2 vertices selected.
- Fix bug where colliders would be incorrect when instantiating trigger entities.
- Fix some cases where "Connect" would result in incorrect geometry.
- Fix UV editor not recognizing shortcuts sent from scene view.
- Fix occasional "Non-finite value in mesh vertices" error when extruding.
- Account for inconsistently sized vertex color arrays when ProBuilder-izing meshes.
- Fix null reference errors when Shape Creation Tool is open with preview enabled and a script reload takes place.
- Fix null reference sometimes caused by a Mirror action.
- Fix bug where merging objects would always add a MeshCollider, even if one already exists.
- Fix mesh bounds not refreshing when adjusting vertices with Quick Offset tool.
- Fix mis-calibrated drag selection wwhen first entering element mode after moving an object.
- Fix issue where duplicating GameObjects with child pb_Objects would leave references to original meshes intact, resulting in odd behavior when deleting objects.
- Fix bug where prefabs would not "Apply" changes to all children equally.
- Improve performance when editing scenes with many ProBuilder object prefabs.

## [2.4.5-p.0+r3531] - 2016-04-07

### Bug Fixes

- Fix bugs in Copy UV Settings and Quick Apply Material shortcuts.

## [2.4.5-f.1+r3519] - 2016-04-07

### Features

- New Torus shape.
- Greatly improve editor performance when working with medium to large meshes.
- New skin for scene info label, including more data about selection.
- Automatically batch vertices even if they don't belong to a smoothing group (actual mesh vertex is now shown in scene info box).
- Add mesh dimensions overlay (ProBuilder > Object Info > {Show, Hide} Dimensions Overlay).
- Make vertex colors button extend-able, with the option to set which color editor the shortcut should open (Palette or Painter).
- New option to show element/object mode toolbar in scene (default), including positioning parameters.
- Improve vertex painter performance with large meshes.
- Unity 5.2 compatibility.

### Bug Fixes

- Fix bug where reverting a prefab with non-prefab ProBuilder children would throw errors.
- Fix hangs when performing various actions.
- Merge now retains the GameObject properties of the first selected object.
- Axis snapping with ProGrids now translates correctly when object rotation is non-identity.
- Performance improvements in UV editor for large meshes.
- Fix bug where OBJ exporter wouldn't properly write submeshes.
- Fix one possible cause of "Mesh.{uv, colors} is out of bounds" errors.
- Catch null reference errors when creating wireframe overlays for meshes exceeding Unity's max vertex count.
- Fix issue where Merge objects would cause meshes to lose their graphics, requiring a refresh.
- Fix menu items showing as enabled when not applicable.
- Add pb_Entity in ProBuilderize if the RequireComponent attribute fails to do so.
- Fix bug where ProGrids wouldn't affect elements when PB is built to a DLL.
- Fix bug where the cube shortcut would ignore material preference.
- Fix vertices merging incorrectly in Optimize function when colors don't match.
- Recalculate mesh bounds after moving the pivot.
- Always refresh/rebuild meshes after making them assets, fixing issues with duplicate mesh references.
- Fix a few more causes of leaks in the mesh and line rendering systems.
- Fix bug where scaling a new object in the shape tool wouldn't take effect until after first refresh.
- Fix bug where Insert Edge Loop and ConnectEdges would sometimes select too many edges after application.
- Fix ProGrids over-zealously collapsing vertices when in axis snapping mode.
- Correctly set element toolbar position when toggling between scene and editor window placement.
- Re-enable user set vertex handle color preferences.

### Changes

- Move default textures out of resources folder.
- On pb_Object::Start, call ToMesh before Refresh since Refresh could try to set UVs or Colors to a mesh that has inconsistent vertex counts.
- Remove most functions accepting a pb_IntArray[] sharedIndex cache and replace with Dictionary versions.

## [2.4.4-p.1+r3425] - 2016-04-07

### Features

- Add scale shortcut toolbar for Auto UVs.
- Add Control+Shift+Left-Click when UV editor is open to copy auto UV settings (including texture).

### Bug Fixes

- Fix errors when building a project with geometry containing null materials.
- Fix rare null reference error when switching scenes.

## [2.4.4-f.1+r3385] - 2016-04-07

### Features

- Where possible* indices are now collapsed to share a single vertex.
- Add context menu to swap between dockable window modes in vertex painter.
- Unity 5.1 beta compatibility.
- New Icosphere shape.
- New API example shows a deformed icosphere with FFT spectrum.
- Grow Selection is now roughly one gajillion times faster.
- Grow with Angle can now optionally select all faces on a plane instead of just the ones near the perimeter.
- New vertex handle gizmos are now culled (and much faster).
- Add option to select only visible elements (Select All or Select Visible toggles this).
- New Repair/pb_RepairMeshReferences script fixes duplicate mesh references.
- Improve edge selection logic, making edge selection much easier.
- Add a preference to enable backface selection on meshes.
- Re-enable 'NoDraw' faces (now implemented as a shader replacement at compile time).
- Improve subdivide action performance.
- Improve performance when editing large numbers of vertices.
- Smooth Normals Window now displays vertex normals with culling, and much faster.
- Repair Missing Script references is now cancelable.
- Add option to extrude elements as a group or individual.

### Bug Fixes

- Improve Flip Normals shortcut selection context handling.
- Enable Subdivide shortcut in Top mode.
- Fix arch geometry that broke when subdividing caps.
- Fix bug where setting arch radius would also set the thickness to 0.01.
- Add option to toggle cap generation on/off in arch tool.
- Fix bug where extruding multiple adjacent faces with a shared center point would not correctly translate the shared center vertex.
- Fix bug where Smoothing Window would not repaint on selection change.
- Improve performance of MergeVertices function, helping to address lag after modifying large objects.
- Fix bug where selecting faces obscured by a culled face would sometimes not register.
- Remove obsolete preference entries.
- Add Undo support when a click drag changes the selection in the UV editor.
- Fix 'Quaternion Look Rotation is Zero' log spam when a face contains degenerate triangles.
- Fix most instances of mesh and material leaks in Editor.
- Fix bug where applying prefab changes to pb_Objects with the Editor closed would not propogate changes to instances.
- Hide some internal MonoBehaviours from the Scripts menu.
- Fix bug where deleting a face with 'delete' key shortcut would change static flags.
- Fix null ref when entering play mode with collider entities sporting boxcollider components.
- Fix bug where Connect Vertices would fail on thin isosceles triangles.
- Fix bug where Connect Edges would mangle adjacent long skinny faces.
- When adding colliders via pb_Entity toolbar, scan current collider components for isTrigger values and apply to new collider if found.
- Fix some instances where convexity and trigger for EntityType.Collider & EntityType.Trigger types would not be set on initialization.
- When detaching faces to a new object, make the detached object selected.
- Fix bug where exiting to Top or Plugin level would not clear the selection mesh.
- Copy userCollisions field when serializing pb_Object.
- Fix regression in Unity 5 that causes prefabs to lose instance modifications on save and entering playmode.
- When mirroring objects, make the mirrored results the new selection.
- Fix bug where setting entity type then undoing wouldn't catch changes to collider.
- Fix bug where duplicating multiple objects would leave pb_Object references pointing to same object.
- When probuilder-izing objects, perform the action in-place (and add undo support).
- Catch errors when repairing missing script references on objects with null materials.

### Changes

- Remove dependency on ProCore lib to communicate with ProGrids.
- Rename scripts to uniformly follow pb_ prefix and pascal case for runtime, underscore case for editor.

### API

- New `Optimize()` method calls CollapseSharedVertices and GenerateUV2.  Replaces GenerateUV2() in most cases.
- Move most of remaining scripts into proper namespaces.
- ProBuilder2.GUI namespace become ProBuilder2.Interface to avoid conflicts with UnityEngine.GUI.
- Move Triangulation code into pbTriangleOps.
- Significantly improved performance of RefreshNormals() function.
- New VerifyMesh() function in pb_EditorUtility guarantees good mesh reference and geometry.
- Add ability to delete unfixable components in pb_MissingScriptEditor.
- New PointIsOccluded() check in pb_Handle_Utility tests if a point is visible in editor.
- Significantly improve performance of pbUtil.RemoveAt().
- Significantly improve performance of many pbMeshOps methods.
- New pb_LineRenderer and pb_MeshRenderer provide fast gizmo drawing in the SceneView.

* Vertices must be smoothed, and have the same texture coordinate to qualify for weld.

### f1 Patch Notes

- Fix ProGrids not affecting vertices / faces / edges in Edit mode.
- Minor tweak to vertex handle color.

## [2.4.3-p.0+r3216] - 2016-04-07

### Features

- Weld distance now adjustable in UV editor.

### Bug Fixes

- Fix weird arch geometry near caps, noticeable when inserting edge loops.
- Improve 'Flip Normals' shortcut context awareness.

## [2.4.3-f.0+r3202] - 2016-04-07

### Features

- Add preference to enable back-face selection.

### Bug Fixes

- Remove 'here' console log.
- Fix regression in 2.4.0 that broke Undo when used with ProGrids.
- Fix 'Look Rotation is Zero' console logs when selecting a face with degenerate triangles.
- Fix bug where sometimes clicking a face would not register due to a culled face intercepting the raycast.

## [2.4.2-f.0+r3202] - 2016-04-07

### Features

- New debug window visualizes mesh information in the sceneview.

### Bug Fixes

- Fix regression that broke prefab editing applying to instances.
- Fix latency in SceneView when selecting elements in the UV window.
- Fix bug where selecting elements in the UV window would not Undo correctly.
- Fix regression that caused UV handle to not update its position when right-click dragging.
- Fix bug where texture rotation handle in the scene view would not snap correctly on finishing a UV adjustment.
- Fix bug where drag selecting edges or faces could select elements behind the scene camera.

## [2.4.1-f.1+r3174] - 2016-04-07

### Features

- New "Export UV Template" function saves a PNG of your UV maps.
- Add new preference to show object vertex, face, and triangle count in the scene view (Preferences/Show Scene Info).

### Bug Fixes

- Edge wireframe no longer renders the material preview wells.
- Fix performance issues when editing large objects in the UV editor with Auto UVs.
- Fix bug where 'Push to Grid' from ProGrids would not Undo correctly.
- Fix lagging wireframe when running "Freeze Transforms" action.
- Fix null ref when deleting multiple faces.


## [2.4.0-f.4+r3132] - 2016-04-07

### Features

- Unity 5 support.
- New wireframe shader overrides Unity default when ProBuilder Editor is open.
- New 'Merge Faces' geometry action combines selected faces to a single face.
- Add `Missing Script Reference` repair item.
- Show color name in vertex tools instead of RGBA info.
- When creating an mesh Asset, also create a prefab with the new mesh and materials already wired up.
- Cull hidden edges when in Edge mode.
- Fix spotty face selection highlight rendering when using Deferred Rendering.
- Add preference to disable automatic UV2 generation while modeling (improves editor performance).
- When selecting a texture-grouped face in UV editor, show an indicator of all faces in group.
- Improve performance when modifying geometry & UVs in Unity 5.
- New dark background in UV editor for Unity light skin users.
- Improve performance when selecting objects with large vertex counts.

### Bug Fixes

- Fix crash when a face material is null (defaults to Unity Default-Diffuse).
- Fix incorrect results when extruding multiple faces sharing a single center vertex (usually seen on the top of a cylinder).
- Save vertex colors when ProBuilder-izing a mesh.
- Fix occasional null ref when continuing UVs.
- Support Undo in UV Editor Auto panel.
- Support Undo for `Push to Grid` events from ProGrids.
- Fix occasional `Index out of range` errors when subdividing, triangulating, and setting pivot.
- Fix crash when running `Fix Missing Script References` in Unity 5.0.0b18 (big thanks to Michael N!)
- Improve the performance of Planar Mapping manual UVs.
- Create Material data asset path if it doesn't exist (fixes errors when saving Material preferences).
- Fix bug where dragging UVs in Unity 5 would sometimes corrupt the mesh.
- Disable Continuous Baking when dragging elements or making continuous changes to the mesh, fixing corruption issues in Unity 5.
- Fix occasional erroneous error message when subdividing faces.
- Fix null ref error when Auto UV panel is open with nothing selected.
- Allow `V` key usage when not in Element mode.
- Fix regression where instantiated objects would not respect ProGrids alignment.
- Fix leaks when deleting or duplicated pb_Objects.
- Fix occasional null ref errors when welding or collapsing vertices.
- When double-clicking a texture grouped face in UV editor, select the entire group.
- Fix regression that caused performance spikes when deleting or instantiating objects.
- When detaching faces to a new object, copy all of that objects properties.
- Add Undo support to Shape creation tool.
- Fix bug where running Flip Normals from the Menu would not immediately update the mesh graphics.
- When serializing pb_Objects, save color and material information (materials are now loaded by name).
- Fix bug where the texture handle tool would sometimes (most times) move UVs in the wrong direction.
- When entering Texture Blending mode in Vertex Painter, set the color to a solid variant of one of the available textures.
- Fix bug where projected UVs in manual mode could potentially be placed very far from the current handle.
- Fix lag when drag selecting edges on objects with large vertex counts.
- Fix bug where setting entity type would not immediately refresh the mesh.
- Fix minor edge selection bug that would break edge highlighting when not directly hovering a mesh.
- Improve appearance of Grow and Extrude foldouts in editor window.
- Fix bug where clicking on a vertex could sometimes select the object behind it.
- Fix compile errors when building to WebGL target.
- When creating a mesh asset, ensure that the source object mesh is not referenced by the new mesh asset.

### Changes

- Remove 'NoDraw' feature (necessary for Unity 5 compatiblity).

### Beta

- Rudimentary Boolean tool added - this is very early in development.

- Fix issue where "Repair Missing Script References" script could get stuck on prefab instances.
- Silence cast exception error in pb_Object_Editor.

## [2.3.3-f.1+r2970] - 2016-04-07

### Features

- Significantly improve performance of Subdivide action.

### Bug Fixes

- Fix incorrect language in Plane generator.
- Fix bug that resulted in mangled vertices when Welding.

## [2.3.2-f.2+r2947] - 2016-04-07

### Features

- Add a toggle in pb_Entity to turn off automatic collision generation.
- Improve UV editor grid logic (now follows camera and resizes at far zoom levels).
- New PostProcessor automatically strips ProBuilder scripts when building executables (toggle-able in Preferences).

### Bug Fixes

- Respect ProGrids `X` key shortcut when translating faces.
- Fix build errors with Static Batching enabled.
- When applying Smooothing Groups, if no face is selected apply group to entire object.
- Static Flags now initialized with `Occluder Static` unchecked.
- Pressing 'F' while a single vertex is selected no longer frames the entire object.
- Vertex colors are now copied when stripping ProBuilder scripts.

## [2.3.1-f.1+r2900] - 2016-04-07

### Features

- New Vertex Painter tool.
- New 'Triangulate ProBuilder Object' action for facetized poly-world look.
- Significantly improve UV editor performance when drawing many elements.
- ProBuilderize action now preserves UVs.

### Bug Fixes

- Fix error when opening Material Editor after assigning a Substance Material.
- Fix bug that caused pb_Editor to freeze when editing prefabs made from ProBuilder objects.
- Remove prefab dependency on ProBuilder.Instantiate or RebuildMeshOnWake.
- Fix mesh leak in Shape Tool.
- Enable Alt+NumKey material shortcut when in Object level.
- When shift-extruding faces in Edge mode, default to face extrusion over edge.
- Fix leaking mesh and material in Face editing mode.
- Fix install script bug that would incorrectly delete non-ProBuilder files.

### Changes

- Handle position is now calculated as the center of selection bounding box.

### API

- New 'HueCube.cs' API example script demonstrates changing single vertex colors.
- New 'pb_SerializableObject' class provides serializable storage for ProBuilder objects.  Add 'pb_Object::InitWithSerializableObject' constructor.

### Beta Notes f1

- Fix install script bug that would incorrectly delete non-ProBuilder files.

### Beta Notes f0

- New Vertex Painter tool.
- Fix error when opening Material Editor after assigning a Substance Material.
- Handle position is now calculated as the center of selection bounding box.

## [2.3.0-f.14+r2861] - 2016-04-07

### Features

- New UV Editor window.
- New 'Material Editor' window for quickly applying materials to ProBuilder objects.
- Completely redesigned ProBuilder EditorWindow.  Dynamically displays only relevant action buttons.
- Editor: New 'Select Edge Loop' command (double click on an edge, or shift+double click to ring selection).
- Editor: New 'Detach to Object' action creates a new ProBuilder object from a face selection.
- Editor: New 'Shrink Selection' command.
- Editor: 'Invert Selection' command now works for Edges and Vertices in addition to Faces.
- Editor: Performance improvements when editing large meshes.
- Editor: 'Grow' settings allow for a user set maximum angle between adjacent faces to limit selection growth.
- Editor: New extendable GUI settings for 'Extrude' allow for user-set extrusion distance.
- Editor: Add 'Distance' setting to Weld tool.
- Editor: Remove requirement that all pb_Objects be scaled to (1,1,1).
- Editor: Add context menu to swap between floating / dockable windows.
- Editor: New 'Conform Normals' geometry operation.
- UV: New 'Continue UVs' action.  With the UV Editor open, select a face then Ctrl + Click an adjacent face to seamlessly match UV coordinates.
- UV: Right click translation handle in UV editor to set a new pivot point (Ctrl snaps to grid, Shift key disables proximity snapping).
- UV: Merge Auto UV and Manual UV editors to a single ALL POWERFUL editor window.
- UV: New Box projection UV unwrapping.

### Bug Fixes

- Editor: Fix bug where toggling NoDraw would sometimes fail.
- Editor: Fix issue where Undo would sometimes cause actions immediately following to fail with 'Index Out of Range' exceptions.
- Editor: Fix Quick Apply Texture shortcut regression from last version.
- Editor: Fix bug that caused 'Undo Change Face Seletion' to delete faces.
- Editor: Fix bug where ProBuilder.Instantiate() would not properly traverse prefab hierarchy when initializing ProBuilder objects.
- Editor: Catch yet another 'Look Rotation is Zero' warning that would slow the editor to a crawl.
- Editor: Fix inconsistent Undo operations on Unity 4.3+ installs.
- Editor: Catch NullRef errors when dragging non-Material type objects into the SceneView.
- Editor: Fix NullRef errors on 'Connect Vertices' actions with multiple faces selected.
- Editor: Fix bug where handle rotation with multiple vertices and no faces selected would be incorrect, resulting in strange behavior.
- Editor: Fix bug in QuickStart script that would install Unity3.5 DLLs for Unity 4.3+ versions, breaking Undo operations.
- Editor: Fix ProBuilder SceneView toolbar positioning when Deferred Rendering is active.
- Editor: Improve Edge selection consistency.
- Editor: Fix incorrect zoom behavior with fewer than 2 vertices selected.
- Editor: Fix 'Set Pivot' moving selected pb_Objects all ova' the place.
- Editor: Fix regression that broke Lightmap channels on Prefab objects.
- Editor: Frame selection now takes all selected pb_Objects into calculations.
- Editor: Fix regression which broke instanced Prefab geometry when running "Apply".
- Editor: Implement 'Undo' when drag selecting elements.
- Editor: Window now implements a scroll bar when necessary.
- Editor: Switching to Rotate or Scale tool no longer resets the handle alignment to 'Local'.
- Editor: Fix bug where Subdivide / Connect Edges / Connect Vertices would not set the selection to match the newly created sub-objects.
- Editor: Fix Mirror Tool incorrectly placing mirrored object pivots.
- Editor: Fix bug where sub-object selection highlights would be left behind when modifying an object's transform via Inspector.
- Editor: Always refresh an object's materials when Undoing modifications to the SharedMaterial array.
- Editor: Face selection graphic now sits flush with faces.
- Editor: Fix bug where ProBuilder-ized meshes would instantiate disabled.
- Editor: Fix incorrect behavior when scaling multiple ProBuilder objects at once.
- Editor: When exiting AutoUV mode (formerly Texture Mode) remember the previous Edit Level, Selection Mode, and Handle aligment.
- Editor: Fix 'Weld Vertices' action failing to properly compare all vertices.
- Editor: Fix incorrect behavior when attempting to modify pb_Objects with children, or children of pb_Objects.
- Editor: Replace deprecated code for Unity 5.
- Editor: ProBuilder-ized objects now inherit donor mesh name.
- Editor: Catch null-ref when closing pb_Editor with Smoothing Window open.
- Editor: Register Undo when creating new objects (with Merge or Mirror actions).
- Editor: When Alt key is held, do not allow handles to capture mouse.
- Editor: Fix leaking mesh preview object when entering Playmode.
- Editor: Allow submeshes to reference Null materials.
- Editor: Fix z-fighting face highlight in Deferred Rendering path.
- Editor: Setting EntityType is now undo-able.
- Editor: Fix bug where extruding from faces would not inherit the correct winding order.
- UV: Retain UV modifications when Subdividing, Connecting, or otherwise noodling around with a face.

### Changes

- 'Top' and 'Geometry' modes become 'Object' and 'Element', respectively.
- Remove drill-down interface for pb_Object dimensions in favor of just always showing them.
- No longer show element highlights when generating shape previews.
- Smoothing editor now accessible via main Editor window and Menu items.
- Remove Lightmapping window shortcut button from Editor window.
- When setting a pb_Object to EntityType::Trigger, also toggle 'isConvex' on the collider.
- Texture Window becomes AutoUV Window, and no longer houses material placement tools (Material Editor window replaces this functionality).
- Smoothing Editor Normals is now a float field, allowing users to set the size of debugging mesh normal lines.

### API

- Move all menu and editor commands to pb_Menu_Commands class.
- Add ProBuilder::Instantiate(GameObject go) overload.
- Slightly improve pbMeshUtils::GetConnectedFaces() performance (still incredibly slow).
- New methods for caclulating point inclusion for complex polygons in pb_Math.
- pb_Handle_Utility is new and has some super cool stuff in it, and I think I forgot to mention it in the last changelogs.
- Add a Repair script to fix missing UV or Vertex caches.
- Improve performance of `GetUniversalEdges` by approximately 3x.
- Move `ProjectionAxis` to `pb_Enum`, and it's associated methods to `pb_Math`.
- Remove `ProjectionAxis.AUTO`, add entries for all other axes.
- New pb_Material_Editor window.
- New pb_Bounds2d class adds some functionality for AABB calculations.
- Improve frequency of expensive caching in pb_Editor.
- Removed most naughty words from the codebase.
- New pb_MenuCommands class synchronizes behavior between MenuItems and Editor buttons.

### Beta Notes: f14

- Editor: New 'Conform Normals' geometry operation.
- Editor: Fix bug where extruding from faces would not inherit the correct winding order.
- API: Add new 'GetWindingOrder(pb_Face face)' extension for pb_Object.

### Beta Notes: f13

- API: TranslateVertices now operates in local space.  Add TranslateVertices_World for backwards compatibility.
- API: Extrude now optionally outs the appended faces.

### Beta Notes: f12

- Change "Ledge Height" to "Door Height" in Door creation panel.
- Omit Entity information from instantiated pb_Object's name.
- Fix bug where duplicate faces could be selected when using Grow Selection without an Angle parameter set.

### Beta Notes: f11

- Fix sometimes incorrect results when selecting UV islands.
- Show UV popups in UV Editor window.

### Beta Notes: f8

- Fix bug where faces with flipped normals would extrude with incorrect winding order.
- Merge Entity and Visgroup toggles.
- 'J' key toggles UV editor open / closed.
- Visgroup status is now retained during playmode state changes.

## [2.2.5-f.5] - 2016-04-07

### Features

- Add 'Arch' tool to Shape Creation Panel.
- New parameters for Door shape generator.
- New 'Selection / Select All Faces with Material' menu item.
- Add a Selection menu item to select all faces with current material.
- New live information update show face movement information.
- Fancy new install script automatically detects previous installations and forgoes the need for user interaction in most upgrade cases.

### Bug Fixes

- Fix inconsistent Undo for face selection on ProBuilder objects.
- Clean up Shader warnings on initial import.
- Fix ProBuilder.Instantiate() ignoring position and rotation parameters.
- Don't force rename objects when changing the Entity type.
- Fix face selection highlight being incorrectly affected by Fog.
- Fix bug where handle alignment preference would be lost occasionally.
- Fix bug where Grow Selection (non-planar) would allow duplicate faces to be selected.
- Fix bug where prefab objects would throw "Shader wants normals" warnings and sometimes not initialize in scene.
- Fix issue where UV2 channel would not correctly initialize on prefab objects.
- Fix bug where rapidly clicking to add faces would frequently result in the deselection of all faces.
- Fix bug where MirrorTool would incorrectly affect source object's transform.
- Fix duplicate and mirrored objects affecting the original mesh geometry.
- Fix null-ref error when using Edge Ring tool on a non-circuital ring.
- Fix bug where mesh colliders added via Entity component menu would incorrectly have the 'Convex' flag toggled.
- Fix bug that caused mirrored objects to lose the source object's entity type.
- Remove the ability to select non-ProBuilder objects when in Geometry or Texture mode (toggle-able via ProBuilder/Preferences).
- Lower distance threshold for mouse distance to line to be considered selectable.
- Fix bug that broke scaling objects when not in PB editor.
- Fix regression that broke deep copying objects when duplicating or copy/pasting.
- Clean up Shape Creation interface to consistently show build button at bottom of screen, and provide scroll bars when necessary for parameters.
- Duplicate entire GameObject (including attached components) when running 'ProBuilder-ize' action.
- Fixed bug with cone shape generation not using radius parameter.
- Enable NavMeshStatic and OffMeshLinkGen flags by default on new pb-Objects.
- Add pb_Object component check in addition to pb_Entity check in Repair / Validate Components.
- Fix install script breakage on Unity 4.3+
- Fix Mirror Tool incorrectly modifying donor object's normals.
- Fix issue where applied changes to ProBuilder prefabs would not immediately update all other instances.
- Fix inconsistent extrusion with Edge and Vertex selections.
- Fix bug where TextureWindow would not initialize with current selection.
- Automatically clean up degenerate triangles caused by vertex merge/weld operations.
- Enable 'Push to Grid' support for ProGrids users with vertex, edge, and face selection.
- Fix bug that caused vertices behind the scene camera to be selected incorrectly in some cases.
- Fix object incorrectly instantiating off-grid with strange pivot placement.
- (Beta) Rename AboutWindow to avoid namespace conflicts.

### Changes

- Rename 6by7 root folder to ProCore.
- New ProCore.dll replaces SixBySeven.dll (shared classes between ProCore products).

### API

- pb_Object.SelectedTriangles is no longer guaranteed to contain only unique indices.
- Convert pb_Preferences_Internal::GetEnum<> to use ints instead of strings, modify pb_Editor to match.
- pb_Object.SelectedTriangles is no longer guaranteed to contain values corresponding to uniqueIndices array.
- Remove deprecated pb_Face::DeepCopy.  Implement copy constructor.
- Move many of ProBuilder's classes to namespaces (ProBuilder2.Common, ProBuilder2.MeshOperations, etc).
- New ClassesEditing segment of Classes folder contains all non-essential files.  This allows for a single ProBuilderCore.dll that can be redistributed with ProBuilder objects allowing users without ProBuilder to view and load ProBuilder objects.

## [2.2.4-f.0] - 2016-04-07

### Bug Fixes

- Fix 'Null Reference Error' when editing objects at runtime.
- Fix crash at runtime when ProBuilder object is selected.

## [2.2.3-f.0] - 2016-04-07

### Features

- New 'Grow Selection Plane' which expands the selected face to nearby faces on the same plane.

### Bug Fixes

- Fix regression where handle tool would not default to Top level editing in Geometry mode when no vertices were selected.
- Fix bug where colliders would be lost on upgrading PB install.
- Enable multi-object editing for pb_Entity inspectors.

### API

- Move and rename pb_Object::MeshWithMesh to pbMeshUtils.DeepCopyMesh.
- Fix PlaneNormal not returning a normalized vector (yikes!).

## [2.2.2-f.4] - 2016-04-07

### Features

- New 'Texture Groups' UV setting.  Select faces and group to project seamless UVs.
- New 'Make Asset' Action allows users to save ProBuilder objects as Mesh objects.
- New 'Subdivide' command.
- New 'Connect' command (edges, faces, vertices).
- New 'Insert Edge Loop' command.
- New 'Select Ring' command.
- New 'Grow Selection' command (Alt-G).
- Significant performance improvements when working with large objects.
- New preferences to set vertex handle colors and size.
- Improve performance when drag selecting edges.
- New 'Remove Degenerate Triangles' Repair menu item.
- New snap to nearest vertex feature.  When moving vertices, hold 'V' to snap handle to nearest vertex.
- New 'Quick Offset' tool in pb_Object inspector window.  Set a value and immediately move the selected vertices by that amount (thanks to Matt1988 for initially developing this feature).

### Bug Fixes

- Override Frame selection to focus on only selected vertices (thanks @nickgravelyn for this tip).
- Fix inconsistent keyboard shortcuts on Mac.
- Tool buttons are now respected by ProBuilder handle.
- Fix bug where ProBuilder GUISkin wouldn't correctly initialize when left open during a Unity restart.
- Fix bug where double clicking a pb_Object to select all would not properly select all Edges.
- Fix bug where ProBuilder would affect other EditorWindow GUI layouts.
- Fix bug where Mirror Tool would fail to correctly initialize objects with pb_Entity.
- Drag selection box now more closely matches Unity's default drag box.
- Update and improve ProBuilderize Action (now attempts to create faces instead of just triangles).
- Fix Rotation handle incorrectly updating to match selection when dragging (occasionally throwing Quaternion.LookRotation == Zero warnings).
- Fix Scale tool incorrectly using world coordinates when translating vertices.
- Fix weird Prism geometry.
- Fix bug where setting an object pivot with ProGrids enabled would sometimes move the object's vertices off grid.
- Edges may now be shift-deselected.
- Update Undo defines to check against Unity versions 4.1 -> 4.9.
- 'Use' events when shortcuts are detected.  Seems to work about 60% of the time on Mac.
- Fix bug where pivot would instantiate offset from grid when used in conjunction with ProGrids.
- Fix bug that broke OBJ export when attempting to export more than one model per session.

### Changes

- Vertex Color shortcuts are now declared in ProBuilderMenuItems, allowing users to edit them without installing Source.
- Reorganized Menu structure.

### API

- Selection management at object level is now entirely set in pb_Object, using new SetSelected[Faces, Edges, Triangles].
- New naming and placement guidelines for Menu items (see pb_Constant).
- New ShiftExtrude() method in pb_Editor removes duplicate code in Handle functions.
- New pb_Editor_Graphics class replaces calls to UnityEngine.Handles in pb_Editor.
- Move most MenuItems to ProBuilder2.Actions namespace (exceptions being Windowed items).
- New pbUndo class replaces #if > UNITY_4_3 junk.

## [2.2.0-f.4] - 2016-04-07

### Features

- Update Undo code for Unity 4.3 compatibility (Install Interface will determine the correct package for your Unity version automatically).
- Add Rotate and Scale tool when editing faces or vertices (accessed by 'E' and 'R' shortcuts, respectively).
- Add EditLevel toolbar in sceneview for quickly viewing and setting EditLevel. @Genstein suggested improvement.
- New Edge selection mode.
- New 'Bridge Edges' action.  Selected 2 edges to create a face bridging them.
- New 'Collapse Selected Vertices' action.  Select any number of vertices and merge them to a single point.
- New 'Split Selected Vertices' action.  Splits the selected vertices.
- New 'Weld Selected Vertices' action.  Checks if any selected vertices share a point, and if so, merge them.
- New 'Invert Selection' action. (ProBuilder -> Edit -> Invert Selection).
- New 'Extrude' action (ProBuilder -> Edit -> Extrude).  Works for single or multiple faces, as well as edges.  Hold shift while moving a face to automatically extrude (works for translate, rotate, and scale).
- New Install / Upgrade interface provides options to install Release and Source versions, as well as older packages.
- Source code is now included as an installation option.
- New Door primitive type in Shape Generator.
- New Pipe primitive in Shape Generator.
- New Sprite primitive in Shape Generator.
- New Cone primitive in Shape Generator.
- Improved Runtime Example scene demonstrating face highlighting.
- New "Default Material" user preference.
- New "Select Faces with Material" tool.
- New API example scene showing object and primitive instantiation
- New GUI buttons for 'Flip Normals', 'Mirror Object', 'Set Pivot', 'Vertex Color Interface' and 'Extrude Face'.
- Add ability to select vertex by clicking on it.
- Add preference for turning off sceneview notifications (Preferences/ProBuilder).
- New preference item allows you to specify the 'Force Convex' field of a 'Mesh Collider' if it is set to default collider.
- New 'Reset Projection Axis UV' repair tool.  Resets all UV settings to use the 'Auto' face projection.
- New 'Force Pivot to Vertex' and 'Force Pivot to Grid' preferences allow for easier grid snapping.
- New default material for ProBuilder objects.

### Bug Fixes

- Fix system beep on Mac OS when using keyboard shortcuts (this could be a headling feature).
- Fix bug where detaching or deleting a face wouldn't always reset the _uniqueIndices array, causing bugs in the handle selection code.
- Add undo functionality to DetachFace action.
- Fix bug where vertex color information would be lost on duplication, refresh, build, or just about any other action you can imagine.
- Fix bug where detaching a face could result in empty entries to the pb_Object->_sharedIndices member, throwing null-ref.
- Fix InvertFaceSelection not correctly updating the pb_Object->SelectedTriangles list.
- Don't show 'Nodraw Face' notification if in Top Level editing mode.
- 'G' key now exits Texture Mode.
- Texture window shortcuts now show notifications.
- Fix button sizing in pb_Editor window.
- Show notification when toggling Selection Mode from GUI button.
- Fix error in 'Detach Face' where occasionally a null shared index array would survive the rebuild.
- Fix compile errors in Editor code when exporting to Web.
- Fix bug where notification for Selection Mode handle would be incorrect.
- Fix bug where deleting a face, then undoing so would result in a NullReferenceError
- Fix bug where 'Fix GameObject Flags' would improperly exit on failing to find a pb_Entity component.
- Fix vertex selection mouse icon drawing when not in Vertex Editing mode.
- Fix vertex color interface losing user preferences across Unity launches.
- Fix issue where pb_Upgrade_Utility would break installation on failing to run.
- Fix bug where rotated UVs would not move in the proper direction when dragging with texture move tool.
- Enable z-testing for face selection graphic.
- Don't show notification post-installation of Static Flag fixes if no fixes were performed.
- Fix bug where texture handles sometimes wouldn't match the selected face's transform.
- Refactor shortcut code to differentiate between modal specific actions. Fixes bug where entity assignments would incorrectly be applied in Geometry level and not Top level.
- Fix incorrect skin colors in Unity Free on 4.3.
- Fix bug introduced in 2.1.4 that broke texture handle toggling (thanks, H. David).
- Fix bug where UV rotate tool would be incorrectly calculated on selection change.
- Change UV scale and rotation behavior to no longer operate in world coordinates.
- Fix bug where extruding would occasionally corrupt the pb_Object.uniqueIndices cache, resulting in 'NullRefError' in pb_Object::GetVertices.
- Adjust minSize of pb_Editor window to completely encompass buttons.
- Re-word toggle select mode and edit level notifications and make them consistent between the different access points.
- Fix bug where 'Axis Constraints' toggle in ProGrids would not be respected when translating faces.
- Fix bug where UV and Smoothing group changes would not immediately revert on Undo operations.
- Fix regression that broke Ctrl-Left click to copy UV settings to face.
- Fix bug where ProBuilder Editor skin settings would "leak" to other Editor windows.
- Fix bug where collisions would sometimes not respect user preference when creating new geometry.
- Fix bug where SceneView would sometimes not refresh on an Undo event.
- Fix bug where pressing 'W' key in the SceneView Fly mode would lock the camera to forward movement.

### Changes

- In pb_Entity, switch the 'Sphere Collider' option for 'Remove Collider'.
- Change verbage in Geometry shortcut description.
- Add tooltip for selection mode toggle button.
- Show HandleAlignment text when using shortcut to modify.
- Move DetachFace to Edit menu.
- StaticBatchingFlags.BatchingStatic is now set by default on Occluder and Detail entity objects, and toggled appropriately when NoDraw is detected.
- Move "Create ProBuilder Cube" to "GameObject->Create Other" menu
- Re-organize ProBuilder menu.
- New "Fix GameObject Flags" utility to address static batching issues.  Users experiencing issues with missing ProBuilder objects at compile time should run this command once (per scene).
- Remove 'Faces' menu item, merge with 'Geometry'
- Move 'Mirror Tool' and 'Vertex Color Interface' to Editor Core.
- Repair scripts now live in their own folder.
- Tool scripts (any Action with an interface) now live in their own folder.
- Remove unused beta upgrade script from Install folder.
- Drag selecting faces now (optional; defaults to true) limits face searching to selected objects.
- Remove 'Seamless' mode.

### API / Internal

- Add get/set for pb_Obect->_sharedIndices.
- Use ProBuilder.Actions namespace for all non-window requiring functions.
- When initializing a pb_Object with a pb_Object, use the vertex cache instead of accessing the mesh.
- Remove per-vertex smoothing methods in pb_Object.
- Remove _smoothIndices member from pb_Object.
- Move pb_Profiler to ClassesCore, allowing usage at runtime.
- Add 'color' property to pb_Face.  Used when setting Mesh.colors32.
- New pb_Edge class (not currently in use).
- New ProBuilder.Instantiate(GameObject go) method.  Behaves exactly like UnityEngine.GameObject.Instantiate() and may be used with ProBuilder and non-ProBuilder objects.
- Move math methods from pbUtil to pb_Math.
- Added List<> overrides to many of the more commonly used pb_Object method calls.
- Clean up face selection graphic rendering code (small editor performance improvement).
- New FixDegenerateTriangles method (handy when merging vertices or faces).
- CombineObjects method re-built for faster combine operations.
- New ProBuilder2.Common, ProBuilder2.MeshOperations, and ProBuilder2.Math namespaces.  Partially integrated.
- New pb_Editor_Enum class and namespace.

### Known issues

- With Unity 4.3 and up, undoing a Collapse Vertices operation is slow.
- Merging rotated objects does not account for UV rotation.
- OBJ export, something broken, etc.
- Unity inserts an additional Undo when selecting a new face on an already selected object.
- Can't shift-click to deselect edges.

## [2.1.4-f.0] - 2016-04-07

### Features

- Notifications are now displayed when a shortcut is recognized.
- New preview feature in Geometry Interface.  Interactively create and place shapes.
- Remove dependency on concave MeshCollider for face selection.
- New MenuItems for opening the Texture Window, and assorted editor commands.

### Changes

- Move GUI folder to Resources, allowing 6by7 root folder to be placed anywhere in Project hierarchy.
- Decouple collisions from ProBuilder API entirely.

### Bug Fixes

- Fix bug where Mesh.Colors32 property would be lost on duplication.
- Clamp values in Geometry Interface to sane values.
- Fix plane generation pivot location when segments < 0.
- Fix bug that caused Unity to no longer recognize numberical input.
- Fix regression in 2.1.3 that caused MeshColliders break on entering playmode.
- Fix bug where shortcut keys would sometimes not be recognized.
- When updating ProBuilder, the editor window is now force-reloaded.
- Editor window is now sized correctly for both dockable and non-dockable frames.
- Fix compile errors when building project in Unity 4.1.2+
- Fix bug that caused merged objects to incorrectly snap vertex points while ProGrids window is present.
- Fix NullReferenceError when clicking Merge button with nothing selected.
- Fix GUISkin issues in Unity 3.5.
- Fix GUISkin modifications affecting pb_Geometry_Editor incorrectly.
- Fix 'Delete Face' notification incorrectly displaying on OSX.
- Fix merged objects losing collisions.

### API

- ProBuilder.Shortcut is now pb_Shortcut.
- Add pb_Upgrade_Utility as a base class for all updating operations.

## [2.1.3] - 2016-04-07

### Features

- New Vertex Color Interface.
- New 'Detach Face' action.
- New 'Toggle Mover Visibility' button.

### Changes

- pb_Mesh_Extension renamed to pb_Object_Extensions.
- Transition default shader to Diffuse Vertex Color.

### Bug Fixes

- Fix pb_Object breakage when upgrading to 2.1.2+ from <= 2.1.1.
- Fix bug where switching to Geometry mode would not always correctly set Tool.current to Tools.None.
- Fix bug where calling the distinctIndices member of a pb_Face would sometimes throw an exception.
- Fix null reference errors when deleting object faces.
- Fix regression in 2.1.2 that caused non-cube type primitives to lose entity data and mesh information.
- Fix regression that caused Nodraw Visiblity Toggle to break.

### API

- Remove unnecessary calls to the mesh reference when accessing vertex information (most notably in UV mapping functions).
- Cache distinct indices in pb_Face, replacing pb_Face::DistinctIndices() with pb_Face.distinctIndices.
- Add pb_Edge class, and accompanying methods to retrieve all face edges and selectively perimeter edges.
- Add SetColors32(Color32[] colors) to pb_Object class.
- Add DetachFace(pb_Face face) to pb_Object class.

### Internal

- Update to SVN 1.7, small adjustments to build scripts.
- Add shell script to build distributable packages on OSX.

## [2.1.2] - 2016-04-07

### Features

- New interface for pb_Entity class in Inspector.
- Scale transform now supported.
- Double click pb_Object face to select all faces.
- New ProBuilder/About window provides more build information.
- Full prefab support (removes "Create Prefab" button from ProBuilder editor).

### Changes

- Rewrite context tip for Lightmapping button to reflect it's new purpose.
- Automatically freeze scale transform when applying any change to vertices.
- Always ZTest for selection graphic in face mode.
- 'G' key now toggles between Edit Levels.
- Remove face vertex handle information from scene view.
- Remove install script from package.

### Bug Fixes

- Fix bug where user would be allowed to add multiple collision components to pb_Object.
- Fix bug where geometry would shift on Undo/Redo incorrectly.
- Fix leak when deleting pb_Objects.
- Fix regression in 2.1.1 that introduced a leak on switching pb_Objects while in ModeBased vertex editing.
- Fix bug where selection graphics would occasionally not update on undo, redo, or prefab apply / revert.
- Fix bug where setting EntityType would destroy transform parent/child connections.
- Fix incorrecty window sizing in pb_Editor.
- Fix rare error log when duplicating prefab objects.

### API

- Add OnVertexMovementFinished event to pb_Editor.

### Internal

- Implement SixBySeven shared library.

## [2.1.1] - 2016-04-07

### Features

- Add MirrorTool action.
- Add Prism primitive.
- Add ProBuilderizer action (API example).
- Add Flip Winding Order action (flips face normals).
- Add dimensions parameter to Prism and Cube in Geometry Interface.
- Add ability to delete faces (select faces and press backspace)

### Changes

- "Auto NoDraw" becomes "NoDraw Tool", and features a vastly improved interface.
- Scroll bars added to ProBuilder Preferences panel, allowing for unlimited preference additions.
- Add undo support to Set Pivot action.
- No longer force rename pb_Objects post-initialization.
- Comment out menu item for Project Wide Nodraw utility, leaving action available for advanced users.

### Bug Fixes

- Fix bug where handles in Seamless editing mode would not draw.
- Fix bug where selected objects would disappear at runtime.
- Fix bug where drag selection would not be recognized in Seamless editing mode.
- Fix Unity crash when importing packages while ProBuilder window is open.
- Fix regression in 2.1 where a MeshCollider would always be assigned to pb_Object, regardless of Collider settings.
- Fix cylinder generation code to properly account for height divisions (now accepts 0 as a parameter).
- Fix bug where undoing texture modifications would not consistently refresh pb_Object to original state.
- Fix bug where pb_Objects would disappear at runtime with static batching enabled.
- Add overload to TranslateVertices that accepts bool forceDisableSnap.
- Fix bug in PivotTool that caused vertices to incorrectly be snapped when setting new pivot with snapping enabled.

### API Changes

- Add pb_Object::InitWithObject
- Add ProBuilder::CreateObjectWithObject
- Add pb_Object::GetName
- Add ProBuilder::CreatePrimitive(ProBuilder.Shape shape)

### Internal

- Add DrawAllFaceNormals to #DEBUG flagged pb_Editor.
- Update Sublime Extension to version 3.

## [2.1.0] - 2016-04-07

### Features

- Add Smoothing Group support.
- New face selection graphic system respects depth order + speed boost.
- Add drag selection support for faces.
- UV2 channel generation now totally automated.
- New Lightmap Window exposes UnwrapParam properties per-object for fine-grained UV2 generation control.
- Add smart object naming, with the convention "pb(Shape Type)([Entity Type])-(Object ID)" - ex: pb-Cube[Detail]-1701)
- Add new "Mover" entity type, which is non-static and allows complete control at runtime.
- Add support for n-gon faces.

### Changes

- 'World' is now default handle alignment.
- Update default materials with dedicated textures.
- Update QuickStart window with more explicit options.
- Default values for Cylinder are now slightly more sane.

### Bug Fixes

- Fix ProceduralMaterials throwing errors in Texture Editor.
- Fix rare bug where incorrect vertex indices would be selected in an UpdateSelection() call, throwing a NullReferenceException.
- Fix bug where toggling selected faces would not correctly remove vertices from internal selection list.
- Fix bug where pivot would center at 0,0,0 on merging objects.
- Hide ACG property in Inspector window.
- Fix bug where merged objects would lose EntityType information.
- Fix bug where prefab creation would not account for pb_Group data.
- Fix bug where merged objects would lose normal data.
- Fix bug where exiting Texture Mode would not consistently set Edit Mode to Top.
- Fix bug where generating UV2 channel would incorrectly hide NoDraw faces, breaking synchronization with pb_Editor UI.
- Fix bug where ListenForTopLevelMovement would incorrectly fire, significantly slowing scene navigation.
- Fix bug where duplicating multiple objects would result in referenced pb_Objects.
- Fix bug in pb_Group where SetActive would incorrectly be called in Unity 3.5.
- Fix bug where collision meshes would not correctly update after an Undo / Redo event.
- Fix bug where drag selection would not exit properly if a function key is pressed mid drag.
- Fix bug where vertex handles would incorrectly be drawn in Top level editing mode.
- Fix bug where deleting a pb_Object would occasionally cause a NullReferenceError in UpdateSelection().
- Fix bug where Occluder objects would not allow textures to be applied.
- Fix bug where box colliders would not properly inherit trigger boolean value.
- Fix bug where merging objects or creating groups would not snap pivot point to grid (this also introduces centered pivot points).
- Fix rare bug where get_localRotation would fail.
- Fix white flash in Texture Window preview.
- Fix bug where ProBuilder would not remember Handle Alignment setting.
- Fix bug where editor selection property would not correctly update on object deletion.
- Fix minor bug where vertex handles would sometimes not immediately draw on entering Geometry editing mode.
- Fix bug where closing Texture Window manually would not always exit EditLevel.Texture.
- Fix bug where an Undo/Redo event would sometimes cause pb_Editor to attempt to refresh every pb_Object in scene.
- Fix bug where exiting EditLevel.Texture to Geo Mode would not correctly remember the previous SelectionMode.
- Fix bug where cylinder object sometimes initialize with un-even side lengths.
- Fix bug where on deleting a pb_Object's MeshCollider, ProBuilder would not immediately re-initialize it (prevents common PEBKAC error).

### API

- Integrate Doxygen (Still a work in progress - feel free to drop by the forums with any questions).
- Add SharedTrianglesWithFacesExclusive for extracting shared triangle indices exclusive to passed faces.
- VerticesWithIndiceArray is now VerticesWithTriangleArray.
- Remove pb_Object::CreatePrimitive.  Use pb_Shape for object creation, or pb_Object::CreateCube(float scale).
- Add OnVertexMovement EventHandler to pb_Object.
- pb_Object::CreateObjectWithPointsfaces is now pb_Object::CreateObjectWithVerticesFaces.

### Actions

- Update AutoNodraw to cast from all vertices + center point when determining hidden flag.
- In PivotTool.cs, snap pivot point to grid if no vertices are selected.
- Refactor EntityType.Brush to EntityType.Detail.

### Internal

- Add pb_Profiler class
- Add UVee window + ProBuilder specific modifications
- Add internal preference to force update preference when necessary (usually means adding shortcut items).
- Significant performace improvements in handle drawing.
