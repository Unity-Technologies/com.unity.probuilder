<!-- #Video: Face Actions

[![Face Actions Video](../images/VideoLink_YouTube_768.png)](@todo) -->

<div style="text-align:center">
<img src="../../images/Toolbar_FaceActions.png">
</div>

---

## ![Delete Face Icon](../images/icons/Face_Delete.png "Delete Faces Icon") Delete Face

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Delete Faces</a>
</div>

**Keyboard Shortcut** : `BACKSPACE`

Deletes the selected face(s).

<div style="text-align:center">
<img src="../../images/DeleteFace_Example.png">
</div>

---

## ![Detach Face Icon](../images/icons/Face_Detach.png "Detach Faces Icon") Detach Face

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Detach Faces</a>
</div>

Detach the selected face(s) to a new sub-mesh, or seperate object.

<div style="text-align:center">
<img src="../../images/DetachFace_Example.png">
</div>

![Options Icon](../images/icons/options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**Detach To New Object** | If **On**, the face(s) will be detached to a new, seperate object. Otherwise, they will be detached to a sub-mesh within the original object.

---

<a id="extrude">
## ![Extrude Face Icon](../images/icons/Face_Extrude.png "Detach Faces Icon") Extrude Face

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Extrude Faces</a>
</div>

**Keyboard Shortcut** : `SHIFT DRAG`

Creates a new face by pulling out the currently selected face and attaching sides to each edge.

You can also extrude by holding `SHIFT` while moving, rotating, or scaling the faces.

<div style="text-align:center">
<img src="../../images/ExtrudeFace_Example.png">
</div>

![Options Icon](../images/icons/options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**As Group** | If **On**, adjacent faces in the selection will be extruded together, along an averaged normal direction. Otherwise, all faces will be extruded individually, along their own normal direction.
**Distance** | Distance to extrude the selected faces(s).

---

## ![Flip Normals Icon](../images/icons/Face_FlipNormals.png "Flip Normals Icon") Flip Normals

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Flip Normals</a>
</div>

Flips the normals on the selected face(s).

<div style="text-align:center">
<img src="../../images/FlipFaceNormals_Example.png">
</div>

---

## ![Flip Triangles Icon](../images/icons/Face_FlipTri.png "Flip Face Edge Icon") Flip Face Edge

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Flip Face Edge</a>
</div>

Swap the triangle orientation on the selected face(s). This will only work on quads (faces with 4 sides).

<div style="text-align:center">
<img src="../../images/FlipTri_Example.png">
</div>

---

## ![Conform Normals Icon](../images/icons/Face_ConformNormals.png "Conform Normals Icon") Conform Normals

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Conform Normals</a>
</div>

Sets all selected face normals to the same relative direction.

---

## ![Merge Faces Icon](../images/icons/Face_Merge.png "Merge Faces Icon") Merge Faces

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Merge Faces</a>
</div>

Merges selected faces into a single face, and removes any dividing edges.

<div style="text-align:center">
<img src="../../images/MergeFaces_Example.png">
</div>

---

## ![Subdivide Face Icon](../images/icons/Face_Subdivide.png "Subdivide Face Icon") Subdivide Face

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Subdivide Face</a>
</div>

Split each selected face by adding a vertex at the center of each edge and connecting them in the center.

<div style="text-align:center">
<img src="../../images/SubdivideFace_Example.png">
</div>

---

## ![Bevel Icon](../images/icons/Edge_Bevel.png "Bevel Icon") Bevel

<div class="video-link">
Section Video: <a href="@todo">Face Actions: Bevel Face</a>
</div>

Performs the [Bevel Edge](edge/#bevel) action on all the edges of the selected face(s).

<div style="text-align:center">
<img src="../../images/BevelFace_Example.png">
</div>

![Options Icon](../images/icons/options.png) **Custom Settings Available** :

Setting | Description
--- | ---
**Distance** | Sets the distance each new edge is moved, from the position of the original.

---


