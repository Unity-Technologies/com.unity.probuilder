<div class="site"><a href="https://youtu.be/Ta3HkV_qHTc"><img src="images/VidLink_GettingStarted_Slim.png"></a></div>

---

## ![Select Hidden ON](images/icons/Selection_SelectHidden-ON.png) Select Hidden

<div class="video-link">
Section Video: <a href="https://youtu.be/le7AchazndE?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Hidden</a>
</div>

Determines whether hidden elements are selected or ignored, when drag-selecting. This is a toggle button, click to change modes:

|**Toolbar Icon:** |**Description:** |
|:---|:---|
| ![Select Hidden ON](images/icons/Selection_SelectHidden-ON.png) | **On**: all [elements](fundamentals#editing-meshes) are selectable, regardless of their visibility |
| ![Select Hidden OFF](images/icons/Selection_SelectHidden-OFF.png) |  **Off**: drag selection will ignore any [elements](fundamentals#editing-meshes) that cannot currently be seen |

![Handle Alignment Examples](images/SelectHidden_Example.png)


## ![Handle Alignment Local](images/icons/HandleAlign_Local.png) Handle Alignment

<div class="video-link">
Section Video: <a href="https://youtu.be/C9sXO4sNhKM?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Handle Alignment</a>
</div>

**Keyboard Shortcut** : `P`

Choose how the scene handles will be oriented when selecting [elements](fundamentals#editing-meshes). This is a toggle button; click to change modes:

|**Toolbar Icon:** |**Description:** |
|:---|:---|
| ![Handle Alignment Global](images/icons/HandleAlign_World.png) | Global: Similar to a compass, the handle orientation is always the same, regardless of local rotation. |
| ![Handle Alignment Local](images/icons/HandleAlign_Local.png) | Local: Similar to "left vs right", handle orientation is relative the object's rotation. |
| ![Handle Alignment Normal](images/icons/HandleAlign_Plane.png) | Normal: This special mode aligns the handles to exact normal direction of the selected face. |

> **TIP:** If you set Unity's "Pivot <> Center" toggle to "Pivot" while also using ProBuilder's "Normal" alignment, you can manipulate elements from the active item, for example hinging a selection of vertices from a specific point! 

![Handle Alignment Examples](images/HandleAlign_ExamplesWithTextAndIcons.png)


<a id="grow"></a>
## ![Grow Selection](images/icons/Selection_Grow.png) Grow Selection

<div class="video-link">
Section Video: <a href="https://youtu.be/yX29De1bcUE?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Grow Selection</a>
</div>

**Keyboard Shortcut**: `ALT G`

Expands the selection outward to adjacent faces, edges, or vertices.

![Options Icon](images/icons/Options.png) **Options**:

Setting | Description
--- | ---
**Restrict To Angle** | Only Grow Selection to faces within a specified angle
**Max Angle** | The angle to use when Restrict to Angle is **On**
**Iterative** | Only Grow Selection to adjacent faces, with each button press

![Handle Alignment Examples](images/GrowSelection_Example.png)


## ![Shrink Selection](images/icons/Selection_Shrink.png) Shrink Selection

<div class="video-link">
Section Video: <a href="https://youtu.be/1z2sDcHF69o?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Shrink Selection</a>
</div>

**Keyboard Shortcut** : `ALT SHIFT G`

Does the opposite of Grow Selection: removes the elements on the perimeter of the current selection.

![Shrink Selection Example](images/ShrinkSelection_Example.png)


## ![Invert Selection](images/icons/Selection_Invert.png) Invert Selection

<div class="video-link">
Section Video: <a href="https://youtu.be/Dj9qHeCIZwY?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Invert Selection</a>
</div>

**Keyboard Shortcut** : `CTRL SHIFT I`

Selects the inverse of the current selection. All unselected elements will become selected, the current selection will be unselected.

![Invert Selection Example](images/InvertSelection_Example.png)


## ![Select Edge Loop Icon](images/icons/Selection_Loop.png) Select Edge Loop

<div class="video-link">
Section Video: <a href="https://youtu.be/gh_cV_lkI6s?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Edge Loop</a>
</div>

**Keyboard Shortcut** : `ALT L`

Selects an edge loop from each selected edge.

![Handle Alignment Examples](images/Selection_LoopExample.png)


## ![Select Edge Ring Icon](images/icons/Selection_Ring.png) Select Edge Ring

<div class="video-link">
Section Video: <a href="https://youtu.be/sVZgWycaZ4M?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Edge Ring</a>
</div>

**Keyboard Shortcut** : `ALT R`

Selects a ring from each selected edge.

![Handle Alignment Examples](images/Selection_RingExample.png)


## ![Select Face Loop Icon](images/icons/Selection_Loop_Face.png) Select Face Loop

<!-- <div class="video-link">
Section Video: <a href="https://youtu.be/gh_cV_lkI6s?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Face Loop</a>
</div>
 -->
**Keyboard Shortcut** : `ALT L` `Shift + Double Click`

Selects a face loop from each selected face. Faces are only considered to be part of a loop if they contain exactly 4 sides.

![](images/SelectFaceLoop_Example.png)


## ![Select Face Ring Icon](images/icons/Selection_Ring_Face.png) Select Face Ring

<!-- <div class="video-link">
Section Video: <a href="https://youtu.be/sVZgWycaZ4M?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Face Ring</a>
</div> -->

**Keyboard Shortcut** : `ALT R` `Control + Double Click`

Selects a face ring from each selected face. Faces are only considered to be part of a ring if they contain exactly 4 sides.

![](images/SelectFaceRing_Example.png)


## ![Select Hole Icon](images/icons/Selection_SelectHole.png) Select Hole

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=kqfcaxmRT-8&index=6&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">Selection Tools: Select Hole</a>
</div>

With an open vertex or edge selected, click **Select Hole** to select all elements along opening.

With no elements selected, clicking **Select Hole** will automatically select **all** holes in the selected object.

![](images/Example_SelectHole.png)


## ![Select by Material Icon](images/icons/Selection_SelectByMaterial.png) Select by Material

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=OAyQ-hf45NA&index=2&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">Selection Tools: Select byMaterial</a>
</div>

Click to select all faces on this object, which use the same material as the selected face.

![](images/Example_SelectByMaterial.png)


## ![Select by Vertex Color Icon](images/icons/Selection_SelectByVertexColor.png) Select by Vertex Color

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=_LxVLz8AbMg&index=1&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">Selection Tools: Select by Vertex Color</a>
</div>

Click to select all faces on this object, which have the same vertex color as the selected face.

![](images/Example_SelectByVertexColor.png)


## ![Shift Modifier Icon](images/icons/Selection_ShiftDifference.png) Shift Modifier

<div class="video-link">
Section Video: <a href="https://www.youtube.com/watch?v=2bOdeAZ3EJU&index=5&list=PL1GU9r7hfosDHqJBqsBzkrLRDOH2EXCMa">Selection Tools: Shift Modifier</a>
</div>

Choose how holding the SHIFT key will affect selection, when clicking or drag-selecting:

|**Toolbar Icon**: |**Description:** |
|:---|:---|
| ![SHIFT Modifier Add](images/icons/Selection_ShiftAdd.png) |Add: always add to the selection |
| ![SHIFT Modifier Subtract](images/icons/Selection_ShiftSubtract.png) |Subtract: always subtract from the selection |
| ![SHIFT Modifier Difference](images/icons/Selection_ShiftDifference.png) |Difference: unselected elements are added, selected elements are subtracted |

![](images/ShiftModifier_Example.png)


## ![Selection Rect Icon](images/icons/Selection_Rect_Intersect.png) Selection Rect Mode

Choose whether drag selection should only select elements inside the drag-rect (Complete), or also elements intersected by the drag-rect.

|**Toolbar Icon**: |**Description:** |
|:---|:---|
| ![Complete](images/icons/Selection_Rect_Complete.png) |Complete: Only select elements entirely within the drag-rect |
| ![Intersect](images/icons/Selection_Rect_Intersect.png) |Intersect: Select both occluded and intersected elements |

![](images/DragRect_Example.png)


