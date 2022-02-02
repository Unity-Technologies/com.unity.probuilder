# What's new in version 5.0

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

For a full list of changes and updates in this version, see the [ProBuilder package changelog](xref:pb-changelog).
