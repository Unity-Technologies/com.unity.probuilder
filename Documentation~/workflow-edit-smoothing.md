# Smooth hard edges on meshes

To create a smooth and rounded look on part or all of your mesh, define a Smoothing Group. If you include only a part of your mesh, the rest of the mesh has more of a sharp and hard-cornered look.

Smoothing doesn't subdivide the mesh; it controls whether vertices are split for hard edges. This often works well for simpler meshes, like cylinders or more organic shapes, curved walls, or meshes for Terrain.

![One quarter of the torus - shown in yellow - is smoothed](images/Smoothing_Editor.png)

> **Note:** This feature produces a subtle smoothing. If you want to turn sharp edges into smooth curves, you need to either [bevel those edges](Edge_Bevel.md) or [subdivide the faces](Face_Subdivide.md) around them for greater control.

You can do the following:

* [Create a smoothing group](#)
* [Remove faces from a group](#clear)
* [Select all faces in a group](#select)

<a name="define"></a>

## Create a smoothing group

To control the degree of smoothness of complex meshes, you can define up to 30 smoothing groups for each mesh. 

To smooth a part of your mesh:

1. From the main menu, select **Tools** > **ProBuilder** > **Editors** > **Open Smoothing Editor** to open the [Smooth Group Editor](smoothing-groups.md).
1. In the **Scene** view, in the **Tools** overlay, enable the **ProBuilder** tool context.
1. In the **Tool Settings** overlay, select the **Face** editing mode.
1. Select the faces that you want to have smooth adjoining edges. Use **Shift** to select multiple faces.
1. Click an unused smoothing group number on the [Smooth Group Editor](smoothing-groups.md) window. If a group is already in use, its button [highlights in blue when you hover over it](smoothing-groups.md#preview-colors).

    > **Tip**: If you enable the **Preview** option in the **Smooth Group Editor** window, smoothing groups that are in use have a color below their respective button. This color corresponds to the color of the group in the **Scene** view.  

You can repeat these steps using different number buttons to create more groups.

<a name="clear"></a>

## Remove faces from a group

To clear selected smoothing groups:

1. Select the faces you want to clear.
1. In the Smooth Group Editor window, select ![break smooth groups](images/icons/Face_BreakSmoothing.png) **Clear Smoothing Group**.

<a name="select"></a>

## Select all faces in a group

To select all faces matching the current smoothing group index, in the Smooth Group Editor window, select ![select by smooth group](images/icons/Selection_SelectBySmoothingGroup.png) **Select Faces**.