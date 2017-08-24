<div style="text-align:center">
<img src="../../images/Toolbar_ObjectActions.png">
</div>

---

## ![Merge Objects Icon](../images/icons/Object_Merge.png "Merge Objects Icon") Merge Objects

<div class="video-link">
Section Video: <a href="https://youtu.be/luxCckVIu8k?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Merge Objects</a>
</div>

Merges the selected objects into a single object.

---

## ![Flip Object Normals Icon](../images/icons/Object_FlipNormals.png "Flip Object Normals Icon") Flip Object Normals
<div class="video-link">

Section Video: <a href="https://youtu.be/Rwu4pr5EeIc?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Flip Object Normals</a>
</div>

Flips the normals of **all** faces on the selected object(s).

> Especially useful for converting an exterior modeled shape into an interior space.

![Flip Object Normals Example](../images/FlipObjectNormals_Example.png "Flip Object Normals Example")

---

## ![Subdivide Objects Icon](../images/icons/Object_Subdivide.png "Subdivide Objects Icon") Subdivide Object

<div class="video-link">
Section Video: <a href="https://youtu.be/pIEvtGyvbOs?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Subdivide Object</a>
</div>

Divides every face on selected objects, allowing for greater levels of detail when modeling.

![Subdivide Object Example](../images/SubdivideObject_Example.png "Subdivide Object Example")

---

## ![Reset Transform Icon](../images/icons/Pivot_Reset.png "Reset Transform Icon") Reset / Freeze Transform

<div class="video-link">
Section Video: <a href="https://youtu.be/IjXYkQ8PfAY?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Reset / Freeze Transform</a>
</div>

Sets the selected objects position, rotation, and scale to world-relative `{0,0,0}` without changing any vertex positions.

---

## ![Center Pivot Icon](../images/icons/Pivot_CenterOnObject.png "Center Pivot Icon") Move Pivot to Center of Object

<div class="video-link">
Section Video: <a href="https://youtu.be/C5hCXTItzfE?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Center Pivot</a>
</div>

**Keyboard Shortcut** : `CTRL J`

Moves the mesh pivot to the center of the object’s bounds.

![Center Pivot Example](../images/CenterPivot_Example.png "Center Pivot Example")

---

## ![Conform Normals Icon](../images/icons/Object_ConformNormals.png "Conform Normals Icon") Conform Normals

<div class="video-link">
Section Video: <a href="https://youtu.be/Dc6G1TDvBj4?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Conform Normals</a>
</div>

Sets all face normals on the selected object to the same relative direction.

---

## ![Triangulate Icon](../images/icons/Object_Triangulate.png "Triangulate Icon") Triangulate

<div class="video-link">
Section Video: <a href="https://youtu.be/OQvY8j20MpY?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Object Actions: Triangulate</a>
</div>

Reduces all polygons to their base triangles, creating a sharp, faceted appearance.

![Triangulate Object Example](../images/TriangulateObject_Example.png "Triangulate Object Example")

---

<a id="mirror"></a>
##![Mirror Tool Icon](../images/icons/Object_Mirror.png "Mirror Tool Icon") Mirror Tool

<div class="video-link">
Section Video: <a href="https://youtu.be/OvzNJ7z0OTs?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">ProBuilder Toolbar: Mirror Tool</a>
</div>

Use the Mirror action to create mirrored copies of objects.

Mirror is especially useful when creating symmetrical items- build one half, mirror it, then weld the two together
for a perfectly symmetrical result.

![Mirror Tool Example](../images/Mirror_Example.png "Mirror Tool Example")

![Options Icon](../images/icons/Options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**X, Y, Z** | Choose which axis (single or multiple) to mirror on
**Duplicate** | When **On**, a duplicate object will be created and mirrored, leaving the original unchanged.

---

<a id="generateuv2"></a>
##![Generate UV2 Icon](../images/icons/Object_GenerateUV2.png "Generate UV2 Icon") Generate UV2

Builds the UV2 channel for each selected mesh, or all meshes in the scene if the "Generate Scene UVs" option is toggled.

![Options Icon](../images/icons/Options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**Generate Scene UV2s** | If **On**, will generate UV2s for all meshes in the scene. Otherwise, only UV2s on the selected object(s) will be generated.
**Enable Auto UV2** | If **On**, UV2s will be auto-generated as needed. 

---

## Export

Export the selected ProBuilder objects to a 3D model.

Format | Description
--- | ---
OBJ | Wavefront OBJ. Widely supported model format supports multiple textures and mesh groups.
STL | A widely supported format generally used in CAD software or 3D printing. Only supports triangle geometry.
PLY | Stanform PLY. Widely supported and very extensible. Supports quads and vertex colors, but not multiple materials.
Asset | Unity's asset format, only readable in Unity.

Export options:

| Option | Formats | Description |
|--|--|--|
| Include Children | All | If enabled ProBuilder will include not only selected meshes, but also the children of selected objects in the exported model. |
| Export as Group | OBJ, PLY | If enabled all selected objects will be combined and exported as a single model file. Otherwise each mesh will be exported separately. |
| Apply Transforms | OBJ, PLY | Should the GameObject transform be applied to the mesh attributes before export? With this option and **Export as Group** enabled you can export your whole scene, edit, then re-import with everything exactly where you left it. |
| Right Handed | OBJ, PLY | Unity's coordinate system is "Left Handed", where most other major 3D modeling packages operate in "Right Handed" coordinates. |
| Copy Textures | OBJ | If enabled the exporter will copy texture maps to the OBJ file destination and reference them from local paths in the material library. If unchecked the material library will reference an absolute path to the textures and not copy them. If you're exporting an OBJ to use in Unity leave this unchecked and set the Mesh Importer "Material Naming" to "From Model's Material" and "Material Search" to "Project-Wide." |
| Vertex Colors | OBJ | Some 3D modeling applications can import vertex colors from an unofficial extension to the OBJ format. Toggling this will write vertex colors using the MeshLab format. This can break import in some applications, please use with caution! |
| Texture Scale, Offset | OBJ | Some 3D modeling applications import texture scale and offset paramters (Blender, for example). Toggling this will write these values to the exported mtlib file. This can break import in some applications, please use with caution! |
| STL Format | STL | The STL file specification supports both ASCII and Binary representations. This toggles between the two types. |
| Quads | PLY | Where possible, ProBuilder will preserve quads when exporting to PLY. |

---

##![ProBuilderize Icon](../images/icons/Object_ProBuilderize.png "ProBuilderize Icon") ProBuilderize Object

Converts the selected object(s) into ProBuilder-editable objects.

![Options Icon](../images/icons/Options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**Preserve Faces** | If **On**, ProBuilder will attempt to keep ngons. Otherwise, all the mesh will be converted to hard tris.

---

##![Entity Icon](../images/icons/Eye_On.png "Entity Icon") Entity Type Tools

In ProBuilder, "Entity Types" can be very helpful in projects that make heavy use of Trigger Volumes and custom Collision Volumes. There are 4 Entity Types:

### Trigger
 - Configures a mesh for use as a Trigger Volume 
 - This object will only be visible in the editor, never in game 
 - On the object's Collider component, "Force Convex" and "Is Trigger are enabled
 - Static Flags: none

### Collider 
 - Configures the mesh for use as a Collision Volume
 - This object will only be visible in the editor, never in-game
 - Static Flags: Navigation Static and Off-Link Mesh Navigation

### Mover
 - For meshes that will need to move and change in-game
 - Static Flags: NONE

### Detail
 - For objects that will NOT move or change in-game
 - Static Flags: ALL

**Using the Entity Type Controls:** To set an object's type, select it (or multiple) and click the "Set (type name)" button at the bottom of the ProBuilder GUI. Click the "eye" icon to instantly toggle the visibility of each Entity Type (for example, to quickly hide all Mover objects)

>Tip: You can choose the default Entity Type in the ProBuilder Preferences


---
