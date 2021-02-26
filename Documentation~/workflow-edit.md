# Editing Meshes

ProBuilder provides these ways to edit ProBuilder Meshes:

- You can [modify the elements](#edit) of any ProBuilder Mesh to change its shape. As you move or extrude faces, edges, or vertices, you are distorting and deforming the Mesh itself. 

  Other than basic transformations, ProBuilder also lets you fill holes, split vertices, collapse edges, and many more actions. You can use these actions to build up an existing shape or combine it with other shapes, including merging or detaching Meshes.

  ProBuilder provides a modal [Cut tool](cut-tool.md) that lets you draw a custom sub-face onto an existing Mesh face.

  You can also modify any regular Unity GameObject with ProBuilder tools, if you [Probuilderize](Object_ProBuilderize.md) it first.

- In addition to modifying Mesh elements, there are special editing modes for (predefined) Shapes, Poly Shapes, and Bezier Shapes that allow you to return to the shape you created or last defined:

  - For Shapes based on shape primitives, you can change the size of the bounding box and even switch the shape primitive, after you finish [creating it](workflow-create-predefined.md).

  - For Poly Shapes, you can [modify the base shape, the extrusion, or the normals](polyshape.md) after you finish [creating it](workflow-create-polyshape.md). For example, you can move the points that define the base or add new points to refine the base shape. You can also change the height and flip the normals.

  - For Bezier Shapes, you can [edit the underlying bezier curve](bezier.md); you can delete and move existing points, add new ones, close the loop, and smooth it.
  
  	> **Warning:** Bezier shapes are experimental, meaning that they are still under development, and might reduce ProBuilder's stability. Please use with caution.




<a name="edit"></a>

## Modifying objects and elements

To edit objects and elements, you need to:

1. Decide which actions can help you achieve the end results. There might be multiple solutions that can all produce the effect you want. This can be a very challenging stage, particularly for new users who don't know what kind of tools and actions ProBuilder provides.
2. Select the element(s) that you want to modify. Often, the editing tool or action impacts which elements you need to select and how you need to select them.
3. Depending on which action you are using, set any options to help customize the outcome or change the default settings. If an action offers options, an indicator appears on the button in the [ProBuilder toolbar](toolbar.md):
	* **Alt/Opt+Click** the gear ![Options Icon](images/icons/Options.png) indicator that appears in the top right of the button in **Icon** mode.
	* **Click** the `+` icon that appears on the right side of the button in **Text** mode.
4. Perform the action or activate the tool. Depending on what you are doing, this may be a simple matter of clicking a button. In some cases, you may be carrying out some intricate procedures. For example, you can click to [extrude edges](Edge_Extrude.md) with the default settings, or you can use the **Shift+Drag** method to control exactly how and where to locate the extruded edge.

ProBuilder tools and actions create, destroy, join, split, and transform objects and elements. Some actions modify the geometry of the Mesh without changing the overall shape, whereas some actions change both.

