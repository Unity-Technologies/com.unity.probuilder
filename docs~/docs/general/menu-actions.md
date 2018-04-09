
## About

Provides info about the currently installed version of ProBuilder.

## Documentation

Opens the latest Documentation online.

## Check for Updates

Check for any available ProBuilder updates.

## Dimensions Overlay

Show or hide the Dimensions Overlay, which works on all mesh objects (not just ProBuilder)

![Dimensions Overlay Example](../images/DimensionsOverlay_Example.png)

## Actions

> *Tools > ProBuilder > Actions*

<h3>Generate UV2</h3>

- **Generate UV2 - Selection:** If you have toggled off the automatic generation of UV2 channels in Preferences, you can use this item to build UV2 (lightmap) channels for the current selection.

- **Generate UV2 - Scene:** This generates UV2 (lightmap) channels for all ProBuilder objects in the scene. This is only useful if you have toggled off automatic UV2 generation in the Preferences panel.

<h3>Strip ProBuilder Scripts</h3>

- **Strip all ProBuilder Objects in Scene:** Remove all ProBuilder scripts from all objects in this scene, leaving just the models.

- **Strip all ProBuilder Objects in Selection:** Remove all ProBuilder scripts from selected objects, leaving just the model.

---

## Repair

> *Tools > ProBuilder > Repair*

<h3>Rebuild All ProBuilder Objects</h3>

Rebuild mesh representations from stored ProBuilder data for each object in the scene. If you have a lot of objects in a scene this can take a while.

<h3>Rebuild Shared Indices Cache</h3>

Discards all shared vertex position data and rebuilds based on proximity.

<h3>Remove Degenerate Triangles</h3>

Deletes triangles on a mesh that are either taking up no space, or are duplicates.

<h3> Upgrade Scene to Advanced</h3>

After upgrading from **ProBuilder Basic** to **ProBuilder Advanced** you will need to run this action in order to preserve the materials applied to objects. This is only necessary once per scene.

<h3>Upgrade Selection to Advanced</h3>

Same as `Upgrade Scene to Advanced`, except that only the current object selection is affected (as opposed to the entire scene).

<h3>Convert to Package Manager</h3>

Used when upgrading a ProBuilder 2.x project to 3.0. See [Convert to Package Manager](troubleshooting/faq/#convert-to-package-manager) for more information.

---

## Experimental

> *Tools > ProBuilder > Experimental*

These are tools or functions that aren’t quite ready for public use, but are included for users to try out early, and report issues/feedback.

See [Experimental Tools](../experimental/experimental-overview) for more information.

---

## Debug

> *Tools > ProBuilder > Debug*

Displays detailed information on the currently selected mesh.

![Debug Window Example](../images/DebugWindow_Example.png)

---

## Export

> *Tools > ProBuilder > Export*

<h3>Export Asset</h3>
Save the selection as Unity mesh `.asset` files.

<h3>Export OBJ</h3>
Export the selected object(s) as OBJ

<h3>Export Stl Ascii</h3>
Export the selected object(s) as STL in ASCII format

<h3>Export Stl Binary</h3>
Export the selected object(s) as STL in Binary format
