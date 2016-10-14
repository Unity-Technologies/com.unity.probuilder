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

Reduces all polygons to their base triangles, creating a "[Polyworld-like](http://qt-ent.com/PolyWorld/)" effect.

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

##![ProBuilderize Icon](../images/icons/Object_ProBuilderize.png "ProBuilderize Icon") ProBuilderize Object

Converts the selected object(s) into ProBuilder-editable objects.

![Options Icon](../images/icons/Options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**Preserve Faces** | If **On**, ProBuilder will attempt to keep ngons. Otherwise, all the mesh will be converted to hard tris.

---
