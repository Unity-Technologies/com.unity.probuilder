
## General

> *Tools > ProBuilder*

### About
Provides info on the currently installed version of ProBuilder

### Documentation
Opens the latest Documentation online

### Dimensions Overlay
Show or hide the Dimensions Overlay, which works on all mesh objects  (not just ProBuilder)

![Dimensions Overlay Example](../images/DimensionsOverlay_Example.png)

---

## Editors

> *Tools > ProBuilder > Editors*

### Vertex Positions Editor

Opens a special panel for manually positioning vertex positions in world space.

![Vertex Positions Editor Example](../images/VertexPositionsEditor_Example.png)

## Actions

> *Tools > ProBuilder > Actions*

### Generate UV2
- **Generate UV2 - Selection:** If you have toggled off the automatic generation of UV2 channels in Preferences, you can use this item to build UV2 (lightmap) channels for the current selection.
- **Generate UV2 - Scene:** This generates UV2 (lightmap) channels for all ProBuilder objects in the scene. This is only useful if you have toggled off automatic UV2 generation in the Preferences panel.

### Strip ProBuilder Scripts
- **Strip all ProBuilder Objects in Scene:** Remove all ProBuilder scripts from all objects in this scene, leaving just the models.
- **Strip all ProBuilder Objects in Selection:** Remove all ProBuilder scripts from selected objects, leaving just the model.

---

## Repair

> *Tools > ProBuilder > Repair*

### Clean Leaked Meshes
If you see console logs saying anything about leaking meshes, run this command to clean up the leaks.

### Force Refresh Scene
Sometimes necessary after an upgrade. Will regenerate mesh geometry and refresh the scene view.

### Rebuild ProBuilder Objects
@todo

### Rebuild Shared Indices
@todo

### Remove Degenerate Triangles
This deletes triangles on a mesh that are either taking up no space, or are duplicates.

### Rebuild Vertex Colors
Reset all vertex colors on the selection to plain white.

### Upgrade Scene to Advanced
@todo

### Upgrade Selection to Advanced
@todo

### Repair Missing Script References
Looks through the scene for any GameObjects with missing components that were at one time `pb_Object` or `pb_Entity`.

---

## Experimental

> *Tools > ProBuilder > Experimental*

These are tools or functions that aren’t quite ready for public use, but are included for users to try out early, and report issues/feedback. WARNING: Use with caution, unwanted results may occur!

### Boolean (CSG) Tool

Union, Intersection, and Subtraction methods currently implemented.

---

## Debug

> *Tools > ProBuilder > Debug*

Displays detailed information on the currently selected mesh.

![Debug Window Example](../images/DebugWindow_Example.png)

---

## Export

> *Tools > ProBuilder > Export*

### Export Asset
Save the selection as Unity mesh `.asset` files.

### Export OBJ
Export the selected object(s) as OBJ

### Export Stl Ascii
Export the selected object(s) as STL in ASCII format

### Export Stl Binary
Export the selected object(s) as STL in Binary format
