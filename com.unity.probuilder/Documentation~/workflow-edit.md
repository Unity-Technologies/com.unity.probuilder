# Editing Meshes

ProBuilder provides these ways to edit ProBuilder Meshes:

- You can [modify the elements](#edit) of any ProBuilder Mesh to change its shape. As you move or extrude faces, edges, or vertices, you are distorting and deforming the Mesh itself. 

  Besides basic transformations, ProBuilder also provides tools to fill holes, split vertices, collapse edges, and many more actions. You can use these tools to build up an existing shape or combine it with other shapes, including merging or detaching Meshes.

  You can also modify any regular Unity GameObject with ProBuilder tools, provided that you [Probuilderize](Object_ProBuilderize.md) it first.

- In addition to modifying Mesh elements, there are special editing modes for Poly Shapes and Bezier Shapes that allow you to return to the shape you created or last edited:

  - For Poly Shapes, you can [edit the extrusion and the normals](polyshape.md), after you are finished [creating it](workflow-create-polyshape.md).

  - For Bezier Shapes, you can [edit the underlying bezier curve](bezier.md) by deleting and moving existing points, adding new ones, closing the loop, and smoothing it.

  	> ***Warning:*** Bezier shapes are experimental, meaning that they are still under development, and may reduce ProBuilder's stability. Please use with caution.




<a name="edit"></a>

## Modifying objects and elements

To edit objects and elements, you need to:

1. Decide which tools can help you achieve the end results. There may be multiple solutions that can all produce the effect you want. This can be a very challenging stage, particularly for new users who don't know what ProBuilder's tools can do yet.
2. Select the element(s) that you want to modify. Often, the editing tool impacts which elements you need to select and how you need to select them.
3. Depending on which tool you are using, set any options to help customize the outcome or change the default settings. If a tool offers options, an indicator appears on the button in the [ProBuilder toolbar](toolbar.md):
	* **Alt+Click** (Windows) or **Opt+Click** (Mac) the gear ![Options Icon](images/icons/Options.png) indicator that appears in the top right of the button in **Icon** mode.
	* **Click** the `+` icon that appears on the right side of the button in **Text** mode.
4. Perform the action. Depending on what you are doing, this may be a simple matter of clicking a button. In some cases, you may be carrying out some intricate procedures. For example, you can [extrude edges](Edge_Extrude.md) using the default settings with a simple click of the button; or you can use the **Shift+Drag** method to control exactly how and where to locate the extruded edge.

ProBuilder actions create, destroy, join, split, and transform objects and elements. Some actions modify the geometry of the Mesh without changing the overall shape, whereas some actions change both.

