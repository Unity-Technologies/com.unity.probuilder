# ![Bezier Shape Icon](images/icons/NewBezierSpline.png) Bezier Shape component

> ***Warning:*** Bezier shapes are experimental, meaning that they are still under development, and may reduce ProBuilder's stability. Please use with caution.

After you initially [create a Bezier Shape](workflow-create-bezier.md), you can still modify the shape as long as you are in Bezier Shape editing mode. You can toggle the Bezier Shape editing mode on and off with the **Edit(ing) Bezier Shape** button.

![Bezier Shape component](images/Experimental_BezierInspector.png)

![A](images/LetterCircle_A.png) [Curve property values](#curve)

![B](images/LetterCircle_B.png) [Tangent modes](#tangent)

![C](images/LetterCircle_C.png) [Shape property values](#shape)



> ***Note:*** You can always modify Mesh elements on the Bezier Shape Mesh using the standard ProBuilder editing tools. However, each time you re-enter Bezier Shape editing mode, any element changes are lost.



## Editing a Bezier Shape

When you are in Bezier Shape editing mode, the **Bezier Shape** component in the Inspector displays the **Editing Bezier Shape** button. If you are not in Bezier Shape editing mode, the **Edit Bezier Shape** button appears instead.

To enter the Bezier Shape editing mode, click the **Edit Bezier Shape** button.  To exit out of the Bezier Shape editing mode, click the button (**Editing Bezier Shape**) again.



In Bezier Shape editing mode, you can modify the shape by performing the following tasks in the Scene view:

- Click and drag existing control points to move them around. When you select control points, translation and rotation handles appear for more precise control.
- Click existing control points to select them, then use **Backspace** (Windows) or **Delete** (Mac) to remove the points from the shape.

- Click along the bezier path line to add new control points.



You can also use the controls in the **Bezier Shape** component in the Inspector to:

- Enter [Curve property values](#curve) (position, rotation, tangent) directly.
- Click the __Clear Points__ button to clear all control points on the Mesh. 
- Click the __Add Point__ button to add a new control point at the end of the bezier path.
- Change the [Tangent mode](#tangent) to use for the handles.
- Customize the overall look of the shape by setting the [Shape property values](#shape).



<a name="curve"></a>

## Curve property values

Use these property values to set precise values for the curve's position, rotation, and tangent handles.

![Curve property values in the Bezier Shape component](images/bezier_curveprops.png)

| ***Property:*** | ***Description:***                                           |
| :-------------- | :----------------------------------------------------------- |
| __Position__    | Enter the local position of the selected control point.      |
| __Tan. In__     | Enter the local position of the selected control tangent *in* handle. |
| __Tan. Out__    | Enter the local position of the selected control tangent *out* handle. |
| __Rotation__    | Enter the additional rotation to be applied to the vertices of the extruded tube. |



<a name="tangent"></a>

## Tangent modes

Tangent modes change how much control you have over the tangent handles.

| ***Icon***                              | ***Tangent Mode*** | ***Description***                                            |
| :-------------------------------------- | :----------------- | :----------------------------------------------------------- |
| ![Free](images/Bezier_Free.png)         | **Free**           | Adjusting one tangent does not affect the other.             |
| ![Aligned](images/Bezier_Aligned.png)   | **Aligned**        | Adjusting one tangent forces the other tangent's magnitude to match. |
| ![Mirrored](images/Bezier_Mirrored.png) | **Mirrored**       | Locks tangent handles in a straight line.                    |



<a name="shape"></a>

## Shape property values

Use these property values to set some basic characteristics of the Bezier Shape.

![Curve property values in the Bezier Shape component](images/bezier_shapeprops.png)

| ***Property:*** | ***Description:***                                           |
| :-------------- | :----------------------------------------------------------- |
| __CloseLoop__   | Enable this option to loop the extruded path back around to the start point. |
| __Smooth__      | Enable this option to use soft normals for the extruded pipe faces.<br />Disable it for hard normals. |
| __Radius__      | Enter the radius of the extruded pipe.                       |
| __Rows__        | Enter the number of segments to insert between control points when extruding the pipe. |
| __Columns__     | Enter the number of vertices that make up the ring around the radius of the pipe. |




