# What's New in ProBuilder

For a full list of changes and updates in this version, refer to the [ProBuilder package changelog](https://docs.unity3d.com/Packages/com.unity.probuilder@latest/index.html?subfolder=/changelog/CHANGELOG.html)


## What's new in version 6.0

Summary of changes in ProBuilder package version 6.0:

* Moved ProBuilder actions that apply to the entire ProBuilder mesh component, such as Export, Subdivide, and Center Pivot, to the Scene view context menu. 
* Moved ProBuilder actions that rely on element selection, such as Bridge, Weld Vertices, and Grow Selection, to the Scene view context menu when the ProBuilder tool context is active. Enable the ProBuilder tool context in the Tool overlay.  
* Moved the Vertex, Edge, and Face edit modes to the Tool Settings overlay in the Scene view. To display ProBuilder edit modes in the Tool Settings overlay, activate the ProBuilder tool context in the Tools overlay.  
* Moved creation and editing tools to the Tools overlay. 
* Added a preview option for most ProBuilder actions.
* Moved these tool options from the ProBuilder toolbar to the Tool Settings overlay:
	* Select Hidden
	* Orientation: Normal
	* Rect: intersect

## What's new in version 5.0

Summary of changes in ProBuilder package version 5.0.

The main updates in this release include:

### Added

* ProBuilder now supports a special modal "tool" mode for some features (the new [Cut tool](cut-tool.md), and the refactored [Shape](shape-tool.md) and [Poly Shape](polyshape.md) tools). Because of these changes, the other features that have a more immediate effect haven been rebranded "actions". For more information, see [Tools vs. actions](tools.md).
* Added a new [Cut](cut-tool.md) tool that allows you to add points on an existing face to define a new sub-face.
* Added a new [Selection X-Ray](preferences.md#sel-xray) option to highlight hidden element selections with a muted color. The default shortcut is **Ctrl/Alt+Shift+X** (modifiable in the Shortcuts Manager), and you can also access it through the ProBuilder menu (**Tools** > **ProBuilder** > **Interaction** > **Toggle X Ray**).
* Added a new preview selection for the [Select Path](SelectPath.md) action.

### Updated

* The new [Shape](shape-tool.md) tool has been completely rebuilt as an [EditorTool](https://docs.unity3d.com/ScriptReference/EditorTools.EditorTool.html).
* The [Poly Shape](polyshape.md) tool is now an [EditorTool](https://docs.unity3d.com/ScriptReference/EditorTools.EditorTool.html).

### Fixed

* Fixed Undo so that it only reverts the last action, instead of all actions performed.
* Selection, picking and highlight shaders have been updated to be compatible with SRPs as well as with orthographic cameras (there are several bug fixes that directly support these adjustments).

