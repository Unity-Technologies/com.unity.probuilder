# Grow Selection

The __Grow Selection__ action expands your selection outward to adjacent faces, edges, or vertices.

To expand the selection:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select an edit mode. All three modes support this action.
1. Select a face, edge, or vertex.
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) and click **Select** > **Grow Selection**.
    * From the main menu, select **Tools** > **ProBuilder** > **Selection** > **Grow Selection**.
1. The **Grow Selection** overlay opens and the selection is expanded to match the default settings.

## Grow Selection Options

| **Property** | **Description** |
| --- | --- |
| **Restrict To Angle** | Limit selection to faces at or under a certain angle from the original face. This option is only available for face selection. |
| **Max Angle** | Set the maximum angle allowed when growing the selection. This option is only available when you use **Restrict to Angle**. |
| **Iterative** | Adds faces that are next to faces it just added, and not just the faces next to the original selection. This option is only available when you use **Restrict to Angle**. |
| **Live Preview** | Add faces as you change the angle with the mouse. If this option isn't active, the faces are added only when you release the mouse. |

This image shows how **Restrict To Angle** changes the selection. In the top example, with no angle restriction, the selection includes all adjacent faces. In the bottom example, the selection includes only faces within 15 degrees of the original face

![Grow Selection. In the top example, with no angle restriction, the selection includes all adjacent faces. In the bottom example, the selection includes only faces within 15 degrees of the original face.](images/GrowSelection_Example.png)
