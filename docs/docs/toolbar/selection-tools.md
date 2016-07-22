#Video: Selection Tools

[![ProBuilder Fundamentals Video](../images/VideoLink_YouTube_768.png)](@todo "Selection Tools Video")

---

<div style="text-align:center">
<img src="../../images/Toolbar_SelectionTools.png">
</div>

---

##![Select Hidden ON](../images/icons/Selection_SelectHidden-ON.png) Select Hidden

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Select Hidden</a>
</div>

Determines whether hidden elements are selected or ignored, when drag-selecting. This is a toggle button, click to change modes:

Toolbar Icon | Description
:---:|---
![Select Hidden ON](../images/icons/Selection_SelectHidden-ON.png) | **On** : all [Elements](@todo) are selectable, regardlesss of their visibility
![Select Hidden OFF](../images/icons/Selection_SelectHidden-OFF.png) |  **Off** : drag selection will ignore any [Elements](@todo) that cannot currently be seen

![Handle Alignment Examples](../images/SelectHidden_Example.png)

---

##![Handle Alignment Local](../images/icons/HandleAlign_Local.png) Handle Alignment

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Handle Alignment</a>
</div>

**Keyboard Shortcut** : `P`

Choose how the scene handles will be oriented when selecting [Elements](@todo). This is a toggle button, click to change modes:

Toolbar Icon | Description
:---:|---
![Handle Alignment Global](../images/icons/HandleAlign_World.png) | Global: Similar to a compass, the handle orientation is always the same, regardlesss of local rotation.
![Handle Alignment Local](../images/icons/HandleAlign_Local.png) | Local: Similar to "left vs right", handle orientation is relative the object's rotation.
![Handle Alignment Planar](../images/icons/HandleAlign_Plane.png) | Planar: This special mode aligns the handles to exact normal direction of the selected face.

![Handle Alignment Examples](../images/HandleAlign_ExamplesWithTextAndIcons.png)

---

##![Grow Selection](../images/icons/Selection_Grow.png) Grow Selection

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Grow Selection</a>
</div> 

**Keyboard Shortcut** : `ALT G`

Expands the selection outward, to adjacent faces. 

![Options Icon](../images/icons/options.png) **[Custom Settings](@todo) Available** :

Setting | Description
--- | ---
**Restrict To Angle** | Only Grow Selection to faces within a specified angle
**Max Angle** | The angle to use when Restrict to Angle is **On**
**Iterative** | Only Grow Selection to adjacent faces, with each button press

![Handle Alignment Examples](../images/GrowSelection_Example.png)

---
 
##![Shrink Selection](../images/icons/Selection_Shrink.png) Shrink Selection

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Shrink Selection</a>
</div> 

**Keyboard Shortcut** : `ALT SHIFT G`

Does the opposite of Grow Selection- removes the elements on the perimiter of the current selection.

---

##![Invert Selection](../images/icons/Selection_Invert.png) Invert Selection

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Invert Selection</a>
</div> 

**Keyboard Shortcut** : `ALT SHIFT G`

Selects the inverse of the current selection. Eg, all unselected elements will become selected, the current selection will be unselected.

---

##![Select Edge Loop Icon](../images/icons/Selection_Loop.png "Select Edge Loop Icon") Select Edge Loop

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Select Edge Loop</a>
</div> 

**Keyboard Shortcut** : `ALT L`

Selects an edge loop from each selected edge.

![Handle Alignment Examples](../images/Selection_LoopExample.png)

---

##![Select Edge Ring Icon](../images/icons/Selection_Ring.png "Select Edge Ring Icon") Select Edge Ring

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Select Edge Ring</a>
</div> 

**Keyboard Shortcut** : `ALT R`

Selects an edge ring from each selected edge.

![Handle Alignment Examples](../images/Selection_RingExample.png)

