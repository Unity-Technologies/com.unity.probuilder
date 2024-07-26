# Create and edit Bezier shapes

> **Note:** Bezier shapes are an experimental feature. To use them, you must [enable experimental features](preferences.md#experimental).

To create curved shapes, use the Bezier Shape tool to create a spline and extrude it as a 3D mesh.

## Create a Bezier shape

To create a Bezier shape:

1. [Enable experimental features](preferences.md#experimental).
1. In the main menu, go to **Tools** > **ProBuilder** > **Editors** > **New Bezier Shape**.
1. The Unity Editor create an initial curve wtih two control points. The points have tangent handles you can use to control the curve's bend.

## Shape the curve

The Bezier shape editing isn't the same as the ProBuilder shape editing:

* The Bezier shape editing mode allows you to move, add, and remove tangent points and so change the curve itself. It's unique to the spline-based Bezier shape.
* The ProBuilder shape editing works on the regular control points. You can use them to reshape the existing shape's face, but not the curve itself.

To edit the shape of the curve:

1. In the **Scene** view, select the Bezier shape.
1. In the **Tools** overlay, set the active context to **ProBuilder**.
1. In the **Inspector** window, select **Edit Bezier Shape** to enter edit mode.
1. To shape the curve:
    * To move the tangent points, click and drag.
	* To add a tangent point in the middle of the curve, click along an existing tangent.
	* To add a tangent point at the end of the curve, in the **Inspector** window, click **Add Point**.
    * To remove a selected tangent point, press **Backspace** (macOS: **Delete**).
	* To remove all tangent points, in the **Inspector** window, click **Clear Points**.

## Component reference

You can change the Bezier shape's properties in the **Inspector** window.

### Tangent point transforms

Transform the tangent points to shape the curve.

| **Property** | **Description** |
| --- | --- |
| **Position** | Move the tangent point in the space. |
| **Tan. In**  | Move the tangent's internal handle. Note: If the **Tangent Mode** is **Mirrored**, the **Tan. In** and **Tan. Out** points give the same result, no matter which one you move. |
| **Tan. Out** | Move the tangent's outer handle. Note: If the **Tangent Mode** is **Mirrored**, the **Tan. In** and **Tan. Out** points give the same result, no matter which one you move. |
| **Rotation** | Rotate the spline around the selected tangent point. |

<a name="tangent"></a>

### Tangent modes

Tangent modes change how a tangent handle transforms when you transform its opposite handle (**Tan. In** and **Tan. Out**).

| **Mode** | **Description** |
| --- | --- |
| **Free** | Transforming one tangent handle doesn't transform the other. |
| **Aligned** | Transforming one tangent handle forces the other handle's magnitude to match. |
| **Mirrored** | Locks tangent handles in a straight line; moving one moves the other in an equal and opposite motion. |

<a name="shape"></a>

### Shape property values

Change the appearance of the 3D shape extruded from the spline.

| **Property** | **Description:** |
| --- | --- |
| **Close Loop** | Connect the start and end of the curve to create a closed shape. |
| **Smooth** | Use soft normals for the extruded pipe faces. |
| **Radius** | How far from the spline the 3D shape builds. Minimum value: `0.001`. |
| **Rows** | The number of segments wrapped around the spline. More rows create a smoother pipe shape. |
| **Columns** | The number of break points between two tangent points. More columns create a smoother curve. |





