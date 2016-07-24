#Video: Object Actions

[![ProBuilder Fundamentals Video](../images/VideoLink_YouTube_768.png)](@todo)

---

<div style="text-align:center">
<img src="../../images/Toolbar_ObjectActions.png">
</div>

---

## ![Merge Objects Icon](../images/icons/Object_Merge.png "Merge Objects Icon") Merge Objects

<div class="info-box warning">
Section Video: <a href="@todo">Object Actions: Merge Objects</a>
</div> 

Merges the selected objects into one, single object. When clicked, you will be prompted to choose between two options:

* Merge Delete: Merge to a new object, and delete the source objects
* Merge Save: Merge to a new object, but only disable (not delete) the source objects

---

## ![Flip Object Normals Icon](../images/icons/Object_FlipNormals.png "Flip Object Normals Icon") Flip Object Normals
<div class="info-box warning">

Section Video: <a href="@todo">Object Actions: Flip Object Normals</a>
</div> 

Flips the normals of **all** faces on the selected object(s). 

> Especially useful for converting an exterior modeled shape into an interior space.

![Flip Object Normals Example](../images/FlipObjectNormals_Example.png "Flip Object Normals Example")

---

## ![Subdivide Objects Icon](../images/icons/Object_Subdivide.png "Subdivide Objects Icon") Subdivide Object

<div class="info-box warning">
Section Video: <a href="@todo">Object Actions: Subdivide Object</a>
</div> 

**Keyboard Shortcut** : `ALT S`

Divides every face on selected objects, allowing for greater levels of detail when modeling.

![Subdivide Object Example](../images/SubdivideObject_Example.png "Subdivide Object Example")

---

## ![Reset Transform Icon](../images/icons/Pivot_Reset.png "Reset Transform Icon") Reset / Freeze Transform

<div class="info-box warning">
Section Video: <a href="@todo">Object Actions: Reset / Freeze Transform</a>
</div> 

Sets the selected objects position, rotation, and scale to world-relative {0,0,0} without changing any vertex positions.

---

## ![Center Pivot Icon](../images/icons/Pivot_CenterOnObject.png "Center Pivot Icon") Move Pivot to Center of Object

<div class="info-box warning">
Section Video: <a href="@todo">Object Actions: Center Pivot</a>
</div> 

**Keyboard Shortcut** : `CTRL J`

Moves the mesh pivot to the center of the object’s bounds.

![Center Pivot Example](../images/CenterPivot_Example.png "Center Pivot Example")

---

## ![Conform Normals Icon](../images/icons/Object_ConformNormals.png "Conform Normals Icon") Conform Normals

<div class="info-box warning">
Section Video: <a href="@todo">Object Actions: Conform Normals</a>
</div> 

Sets all face normals on the selected object to the same relative direction, in case you suspect a "leak".

---

## ![Triangulate Icon](../images/icons/Object_Triangulate.png "Triangulate Icon") Triangulate

<div class="info-box warning">
Section Video: <a href="@todo">Object Actions: Triangulate</a>
</div> 

Reduces all polygons to their base triangles, creating a "[Polyworld-like](http://qt-ent.com/PolyWorld/)" effect.

![Triangulate Object Example](../images/TriangulateObject_Example.png "Triangulate Object Example")
