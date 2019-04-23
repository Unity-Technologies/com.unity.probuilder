# Pipe
The pipe shape is similar to the ProBuilder [Cylinder](Cylinder.md), but hollow. It has similar properties, but you can also specify the thickness of the pipe wall.

![Pipe shapes](images/shape-tool_pipe.png)

![A](images/LetterCircle_A.png) Basic pipe shape (default values)

![B](images/LetterCircle_B.png) Pipe with increased number of height segments (faces per side)

![C](images/LetterCircle_C.png) Pipe with three sides and increased height

![D](images/LetterCircle_D.png) Pipe with increased number of sides and thickness

You can customize the shape of a pipe with these shape properties:

![Pipe shape properties](images/shape-tool_pipe-props.png)


| **Property:** | **Description:** |
|:-- |:-- |
| __Radius__ | Set the radius (width) of the pipe in meters. The default value is 1. Minimum value is 0.1. |
| __Height__ | Set the height of the pipe in meters. The default value is 2. Minimum value is 0.1. |
| __Thickness__ | Set the thickness of the walls of the pipe in meters. When this value approaches the __Radius__ value, the hole becomes smaller. The default value is 0.2. Valid values range from 0.01 to the __Radius__ value minus 0.01. |
| __Number of Sides__ | Set the number of sides for the pipe. The more sides you use (relative to the size of the __Radius__), the smoother the sides of the pipe become. The default value is 6. Valid values range from 3 to 32. |
| __Height Segments__ | Set the number of divisions to use for the height of the pipe. For example, using a value of 3 produces four faces on every side of the pipe. The default value is 1. Valid values range from 0 to 32. |
