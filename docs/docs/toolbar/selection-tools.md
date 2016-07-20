# Video: Selection Tools

[![ProBuilder Fundamentals Video](../images/VideoLink_YouTube_768.png)](@todo "Selection Tools Video")

---

![ProBuilder Fundamentals Video](../images/Toolbar_SelectionTools_WithLetters.png)

### **A** : Select Hidden

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Select Hidden</a>
</div> 

* Select Hidden **Off**: drag selection will ignore any [Elements](@todo) that cannot currently be seen 
* Select Hidden **On**: all [Elements](@todo) are selectable, regardlesss of their visibility

![Handle Alignment Examples](../images/SelectHidden_Example.png)

### **B** : Handle Alignment

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Handle Alignment</a>
</div>

**Keyboard Shortcut** : `P`

Choose how the scene handles will be oriented when selecting [Elements](@todo). There are three options:

* Global: Similar to a compass, the handle orientation is always the same, regardlesss of local rotation.
* Local: Similar to "left vs right", handle orientation is relative the object's rotation.
* Planar: This special mode aligns the handles to exact normal direction of the selected face.

![Handle Alignment Examples](../images/HandleAlign_ExamplesWithTextAndIcons.png)

### **C** : Grow Selection

<div class="info-box warning">
Section Video: <a href="@todo">Selection Tools: Grow Selection</a>
</div> 

Expands the selection outward, to adjacent faces. Also includes options to select by Angle and Iterative.

**Keyboard Shortcut** : `ALT G`

**Options:**

* **Restrict To Angle** : Only Grow Section to faces within a specified angle
* **Max Angle** : The angle to use when Restrict to Angle is **On**
* **Iterative** : Only Grow Selection to adjacent faces, with each button press

![Handle Alignment Examples](../images/GrowSelection_Example.png)
 
### **D** : Shrink Selection

**Keyboard Shortcut** : `ALT SHIFT G`

Does the opposite of Grow Selection- removes the elements on the perimiter of the current selection.

### **E** : Invert Selection 

**Keyboard Shortcut** : `ALT SHIFT G`

Selects the inverse of the current selection. Eg, all unselected elements will become selected, the current selection will be unselected.

### **F** : Select Edge Loop

**Keyboard Shortcut** : `ALT L`

Selects an edge loop from each selected edge.

![Handle Alignment Examples](../images/Selection_LoopExample.png)

### **G** : Select Edge Ring

**Keyboard Shortcut** : `ALT R`

Selects an edge ring from each selected edge.

![Handle Alignment Examples](../images/Selection_RingExample.png)

