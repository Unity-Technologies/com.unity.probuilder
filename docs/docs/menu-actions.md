
## General

> *Tools > ProBuilder*

### About
Provides info on the currently installed version of ProBuilder

### Documentation
Opens the latest Documentation online

### Dimensions Overlay
Show or hide the Dimensions Overlay, which works on all mesh objects  (not just ProBuilder)

---

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

### Force Refresh Scene
Sometimes necessary after an upgrade. Will regenerate mesh geometry and refresh the scene view.

### Clean Leaked Meshes
If you see console logs saying anything about leaking meshes, run this command to clean up the leaks.

### Repair Entity Materials
@todo

### Repair Mesh References
@todo

### Rebuild Vertex Colors
@todo

### Rebuild Vertex Colors
@todo

### Remove Degenerate Triangles
This deletes triangles on a mesh that are either taking up no space, or are duplicates.

### Invert UV Scale
- **Invert UV Scale (Selected Objects):** UV scale is inverted for selected objects.
- **Invert UV Scale (Selected Faces):** UV scale is inverted for selected faces.

---

## Upgrade

> *Tools > ProBuilder > Upgrade*

### Prepare Scene for Upgrade
@todo

### Batch Prepare Scenes for Upgrade
@todo

### Re-attach ProBuilder Scripts
@todo

### Batch Re-attach ProBuilder Scripts
@todo

---

## Experimental

> *Tools > ProBuilder > Experimental*

These are tools or functions that aren’t quite ready for public use, but are included for users to try out early, and report issues/feedback. WARNING: Use with caution, unwanted results may occur!

### Boolean (CSG) Tool
Union, Intersection, and Subtraction methods currently implemented.

---

## Export

> *Tools > ProBuilder > Export*

### Export Asset
@todo

### Export OBJ
Export the selected object(s) as OBJ

### Export Stl Ascii
Export the selected object(s) as STL in Ascii format

### Export Stl Binary
Export the selected object(s) as STL in Binary format
