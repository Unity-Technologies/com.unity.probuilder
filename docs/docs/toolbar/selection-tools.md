[![PB Getting Started Vid Link](../images/VidLink_GettingStarted_Slim.png)](https://youtu.be/Ta3HkV_qHTc)

---

<div style="text-align:center">
<img src="../../images/Toolbar_SelectionTools.png">
</div>

---

##![Select Hidden ON](../images/icons/Selection_SelectHidden-ON.png) Select Hidden

<div class="video-link">
Section Video: <a href="https://youtu.be/le7AchazndE?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Hidden</a>
</div>

Determines whether hidden elements are selected or ignored, when drag-selecting. This is a toggle button, click to change modes:

Toolbar Icon | Description
:---:|---
![Select Hidden ON](../images/icons/Selection_SelectHidden-ON.png) | **On**: all [elements](../general/fundamentals/#editing-meshes) are selectable, regardless of their visibility
![Select Hidden OFF](../images/icons/Selection_SelectHidden-OFF.png) |  **Off**: drag selection will ignore any [elements](../general/fundamentals/#editing-meshes) that cannot currently be seen

![Handle Alignment Examples](../images/SelectHidden_Example.png)

---

##![Handle Alignment Local](../images/icons/HandleAlign_Local.png) Handle Alignment

<div class="video-link">
Section Video: <a href="https://youtu.be/C9sXO4sNhKM?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Handle Alignment</a>
</div>

**Keyboard Shortcut** : `P`

Choose how the scene handles will be oriented when selecting [elements](../general/fundamentals/#editing-meshes). This is a toggle button; click to change modes:

Toolbar Icon | Description
:---:|---
![Handle Alignment Global](../images/icons/HandleAlign_World.png) | Global: Similar to a compass, the handle orientation is always the same, regardless of local rotation.
![Handle Alignment Local](../images/icons/HandleAlign_Local.png) | Local: Similar to "left vs right", handle orientation is relative the object's rotation.
![Handle Alignment Planar](../images/icons/HandleAlign_Plane.png) | Planar: This special mode aligns the handles to exact normal direction of the selected face.

![Handle Alignment Examples](../images/HandleAlign_ExamplesWithTextAndIcons.png)

---

<a id="grow"></a>
##![Grow Selection](../images/icons/Selection_Grow.png) Grow Selection

<div class="video-link">
Section Video: <a href="https://youtu.be/yX29De1bcUE?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Grow Selection</a>
</div>

**Keyboard Shortcut**: `ALT G`

Expands the selection outward to adjacent faces, edges, or vertices.

![Options Icon](../images/icons/Options.png) **Options**:

Setting | Description
--- | ---
**Restrict To Angle** | Only Grow Selection to faces within a specified angle
**Max Angle** | The angle to use when Restrict to Angle is **On**
**Iterative** | Only Grow Selection to adjacent faces, with each button press

![Handle Alignment Examples](../images/GrowSelection_Example.png)

---

##![Shrink Selection](../images/icons/Selection_Shrink.png) Shrink Selection

<div class="video-link">
Section Video: <a href="https://youtu.be/1z2sDcHF69o?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Shrink Selection</a>
</div>

**Keyboard Shortcut** : `ALT SHIFT G`

Does the opposite of Grow Selection: removes the elements on the perimeter of the current selection.

![Shrink Selection Example](../images/ShrinkSelection_Example.png)

---

##![Invert Selection](../images/icons/Selection_Invert.png) Invert Selection

<div class="video-link">
Section Video: <a href="https://youtu.be/Dj9qHeCIZwY?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Invert Selection</a>
</div>

**Keyboard Shortcut** : `CTRL SHIFT I`

Selects the inverse of the current selection. All unselected elements will become selected, the current selection will be unselected.

![Invert Selection Example](../images/InvertSelection_Example.png)

---

##![Select Edge Loop Icon](../images/icons/Selection_Loop.png "Select Edge Loop Icon") Select Edge Loop

<div class="video-link">
Section Video: <a href="https://youtu.be/gh_cV_lkI6s?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Edge Loop</a>
</div>

**Keyboard Shortcut** : `ALT L`

Selects an edge loop from each selected edge.

![Handle Alignment Examples](../images/Selection_LoopExample.png)

---

##![Select Edge Ring Icon](../images/icons/Selection_Ring.png "Select Edge Ring Icon") Select Edge Ring

<div class="video-link">
Section Video: <a href="https://youtu.be/sVZgWycaZ4M?list=PLrJfHfcFkLM-b6_N-musBp4MFaEnxpF6y">Selection Tools: Select Edge Ring</a>
</div>

**Keyboard Shortcut** : `ALT R`

Selects an edge ring from each selected edge.

![Handle Alignment Examples](../images/Selection_RingExample.png)

---

##![Select Hole Icon](../images/icons/Selection_SelectHole.png "Select Hole Icon") Select Hole

With an open vertex or edge selected, click **Select Hole** to select all elements along opening.

With no elements selected, clicking **Select Hole** will automatically select **all** holes in the selected object.

<div style="text-align:center">
<img src="../../images/Example_SelectHole.png">
</div>

---

## ![Select by Material Icon](../images/icons/Selection_SelectByMaterial.png "Select by Material Icon") Select by Material

Click to select all faces on this object, which use the same material as the selected face.

<div style="text-align:center">
<img src="../../images/Example_SelectByMaterial.png">
</div>

---

## ![Select by Vertex Color Icon](../images/icons/Selection_SelectByVertexColor.png "Select by Vertex Color Icon") Select by Vertex Color

Click to select all faces on this object, which have the same vertex color as the selected face.

<div style="text-align:center">
<img src="../../images/Example_SelectByVertexColor.png">
</div>

---

## ![Shift Modifier Icon](../images/icons/Selection_ShiftDifference.png "Shift Modifier Icon") Shift Modifier

Choose how holding the SHIFT key will affect selection, when clicking or drag-selecting:

Toolbar Icon | Description
:---:|---
![SHIFT Modifier Add](../images/icons/Selection_ShiftAdd.png) | Add: always add to the selection
![SHIFT Modifier Subtract](../images/icons/Selection_ShiftSubtract.png) | Subtract: always subtract from the selection 
![SHIFT Modifier Difference](../images/icons/Selection_ShiftDifference.png) | Difference: unselected elements are added, selected elements are subtracted 

<div style="text-align:center">
<img src="../../images/ShiftModifier_Example.png">
</div>

---

## ![Selection Rect Icon](../images/icons/Selection_Rect_Intersect.png "Selection Rect Icon") Selection Rect Mode

Choose whether drag selection should only select elements inside the drag-rect (Complete), or also elements intersected by the drag-rect.

Toolbar Icon | Description
:---:|---
![Complete](../images/icons/Selection_Rect_Complete.png) | Complete: Only select elements entirely within the drag-rect
![Intersect](../images/icons/Selection_Rect_Intersect.png) | Intersect: Select both occluded and intersected elements 

<div style="text-align:center">
<img src="../../images/DragRect_Example.png">
</div>

---