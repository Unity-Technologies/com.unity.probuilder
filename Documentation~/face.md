<div class="site"><a href="https://youtu.be/Ta3HkV_qHTc"><img src="images/VidLink_GettingStarted_Slim.png"></a></div>


## ![Delete Face Icon](images/icons/Face_Delete.png) Delete Face

<div class="video-link">
Section Video: <a href="https://youtu.be/Iy6RBaKB9jU?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Delete Faces</a>
</div>

**Keyboard Shortcut** : `BACKSPACE`

Deletes the selected face(s).

![](images/DeleteFace_Example.png)


## ![Detach Face Icon](images/icons/Face_Detach.png) Detach Face

<div class="video-link">
Section Video: <a href="https://youtu.be/rqD8tQ3GOpA?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Detach Faces</a>
</div>

Detach the selected face(s) to a new sub-mesh, or separate object.

![](images/DetachFace_Example.png)

![Options Icon](images/icons/Options.png) **Custom Settings Available** :

|**Setting:** |**Description:** |
|:---|:---|
|__Detach To New Object__ |If **On**, the face(s) will be detached to a new, separate object. Otherwise, they will be detached to a sub-mesh within the original object. |


<a id="extrude">
## ![Extrude Face Icon](images/icons/Face_Extrude.png) Extrude Face

<div class="video-link">
Section Video: <a href="https://youtu.be/5IcZd8aIS68?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Extrude Faces</a>
</div>

**Keyboard Shortcut** : `SHIFT DRAG`

Creates a new face by pulling out the currently selected face and attaching sides to each edge.

You can also extrude by holding `SHIFT` while moving, rotating, or scaling the faces.

![](images/ExtrudeFace_Example.png)

![Options Icon](images/icons/Options.png) **Custom Settings Available** :

|**Setting:** |**Description:** |
|:---|:---|
| ![FaceNormalsIcon](images/icons/ExtrudeFace_FaceNormals.png) **Face Normals** |Extrudes each selected Face according to it's own surface direction, and adjacent faces remain connected. |
| ![FaceNormalsIcon](images/icons/ExtrudeFace_VertexNormals.png) **Vertex Normals** |Extrudes selected Faces by Vertex normals. Adjacent Faces remain connected. |
| ![FaceNormalsIcon](images/icons/ExtrudeFace_Individual.png) **Individual Faces** |Extrudes each selected Face according to it's own surface direction, however adjacent faces do **not** remain connected. |
|__Distance__ |Distance to extrude the selected faces(s). |


## ![Flip Normals Icon](images/icons/Face_FlipNormals.png) Flip Normals

<div class="video-link">
Section Video: <a href="https://youtu.be/RngRqt3L8H8?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Flip Normals</a>
</div>

Flips the normals on the selected face(s).

![](images/FlipFaceNormals_Example.png)


## ![Flip Triangles Icon](images/icons/Face_FlipTri.png) Flip Face Edge

<div class="video-link">
Section Video: <a href="https://youtu.be/ftIjv3tsTGc?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Flip Face Edge</a>
</div>

Swap the triangle orientation on the selected face(s). This will only work on quads (faces with 4 sides).

![](images/FlipTri_Example.png)


## ![Conform Normals Icon](images/icons/Face_ConformNormals.png) Conform Normals

<div class="video-link">
Section Video: <a href="https://youtu.be/a9T_xe4x2pU?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Conform Normals</a>
</div>

Sets all selected face normals to the same relative direction.


## ![Merge Faces Icon](images/icons/Face_Merge.png) Merge Faces

<div class="video-link">
Section Video: <a href="https://youtu.be/fMUHuWUXnP8?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Merge Faces</a>
</div>

Merges selected faces into a single face, and removes any dividing edges.

![](images/MergeFaces_Example.png)


## ![Subdivide Face Icon](images/icons/Face_Subdivide.png) Subdivide Faces

<div class="video-link">
Section Video: <a href="https://youtu.be/jgH1MHB6p3w?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Subdivide Face</a>
</div>

Split each selected face by adding a vertex at the center of each edge and connecting them in the center.

![](images/SubdivideFace_Example.png)


## ![Bevel Icon](images/icons/Edge_Bevel.png) Bevel Faces

<div class="video-link">
Section Video: <a href="https://youtu.be/2UbvKLQyDDc?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Face Actions: Bevel Face</a>
</div>

Performs the [Bevel Edge](edge#bevel) action on all the edges of the selected face(s).

![](images/BevelFace_Example.png)

![Options Icon](images/icons/Options.png) **Custom Settings Available** :

|**Setting:** |**Description:** |
|:---|:---|
|__Distance__ | Sets the distance each new edge is moved, from the position of the original. |


## ![Triangulate Faces Icon](images/icons/Face_Triangulate.png) Triangulate Faces

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=tkbMt-XDj1I&index=3&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">Face Actions: Triangulate Faces</a>
</div>

Reduces selected faces to their base triangles, creating a faceted, non-smooth appearance. 

![Triangulate Object Example](images/TriangulateObject_Example.png)


