# ProBuilderize a standard GameObject

You can turn a 3D GameObject that has the Mesh Filter and Mesh Renderer components to a ProBuilder object. 

To ProBuilderize a GameObject:

1. In the **Scene** view, select the GameObject.
2. Do one of the following:
    * From the main menu, select **Tools** > **ProBuilder** > **Object** > **Pro Builderize**.
    * Right-click the GameObject and select **MeshFilter** > **ProBuilderize**.
3. The **ProBuilderize** overlay opens, and you can customize the ProBuilder object.

## ProBuilderize options

Use the options in the **ProBuilderize** overlay to customize the ProBuilder object.

| **Property** | **Description** |
| :--- | :--- |
| **Import Quads** | The faces of a 3D GameObject are built of quads, and by default the ProBuilderize action maintains this build. Disable **Import Quads** to build the faces of triangles where possible.<!--can we give an example of where it won't be possible?--> |
| **Import Smoothing** | Smooth the transition between faces to make a softer object. For more information, refer to [Smoothing groups](smoothing-groups.md). |
| **Smoothing Threshold** | Decide which adjacent faces to add to each smoothing group. Use a value larger than the angle between the faces you want to add. The range is from 0.0001 to 45. This setting is only available when you use **Import Smoothing**. |

<!--Right click > MeshFilter > ProBuilderize - the overlay doesn't have the Live Preview option. This sounds wrong.-->
