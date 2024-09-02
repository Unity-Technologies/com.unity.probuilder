# Selection options reference

This table lists all the selection options you can use with a ProBuilder object, and indicates which [Edit modes](modes.md) support them. 

These actions are available:

* From the **main menu** > **Tools** > **ProBuilder** > **Selection**.
* From the context menu: 
    1. In the **Tools** overlay, set the working context to ProBuilder. 
    1. In the **Tool Settings** overlay, select an edit mode.
    1. In the **Scene** view, right-click on a ProBuilder object.

These options are never available in the GameObject tool context.

| **Property** | **Vertex edit mode (ProBuilder context)** | **Edge edit mode (ProBuilder context)** | **Face edit mode (ProBuilder context)** |
| ---- | ---- | ---- | ---- |
| [Grow Selection](Selection_Grow.md) | x | x | x |
| [Select Hole](Selection_SelectHole.md) | x | x | |
| [Select Face Loop](Selection_FaceLoopRing.md) | | | x |
| [Select Face Ring](Selection_FaceLoopRing.md) | | | x |
| [Select Edge Loop](Selection_EdgeLoopRing.md) | | x | |
| [Select Edge Ring](Selection_EdgeLoopRing.md) | | x | |
| [Select Material](Selection_SelectByMaterial.md) | | | x |
| [Select Vertex Color](Selection_SelectByVertexColor.md) | x | x | x |
| [Shrink Selection](Selection_Shrink.md) | | x | x | x |

The [Select Path](SelectPath.md) action is available only through its keyboard shortcut, and only in the Face edit mode.