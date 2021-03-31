# ![Cut Tool icon](images/icons/Cut_Tool.png) Cut tool

Use the Cut tool to subdivide Mesh faces with precision. To cut out a shape on a Mesh, you activate the Cut tool, which lets you define the cutout shape with points. The cutout shape becomes a new face on the Mesh.

To create a new face on an existing Mesh face:

1. Open the ProBuilder window (in Unity's top menu: **Tools** > **ProBuilder window**).

	The [Edit Mode toolbar](edit-mode-toolbar.md) and the [ProBuilder toolbar](toolbar.md) appear.

1. Switch to one of the element [editing modes](modes.md).

2. In the ProBuilder toolbar, click the **Cut Tool** button. The **Cut Settings** panel appears at the bottom of the Scene view with a **Complete** button and a **Cancel** button. If the ProBuilder toolbar is using [Text mode](customizing.md#text-vs-icon-mode), the button text background becomes red.

3. To control snapping to nearby edges and vertices, use the options on the [Cut Settings panel](#cut-tool_panel).

5. Click on the Mesh face where you want the vertices for the new face to be. ProBuilder creates the cutout shape based on the edges you draw with these points. For example, you can specify three points on the Mesh to define a triangular shape and the fourth to close it:

	![Example of a triangular cutout on a face](images/cut-tool-example.png)

  > **Tip**: ProBuilder displays red edges as a visual warning if your points make an invalid edge or face. When this happens, undo adding each point until all the edges become blue again.

	As soon as you return to the first point and click it again (such as the fourth point in the triangle example), the cut is complete and the new face appears selected in the Scene view. Now you can either exit the tool or start another cut.
	
	**Note**: If nothing happened when you returned to the first point and clicked it again or clicked the **Complete** button, it is probably because the points you defined do not create a valid edge or face. For example, if the tool detected only one point, that does not make a valid edge. You can define more points, undo the previous points, or click the **Cancel** button to cancel the operation and start again.

6. To start another cut, click the **Start** button. The **Cut Settings** panel displays a **Complete** button and a **Cancel** button again.

7. To exit the Cut tool, you can:

	* Click the **Quit** button on the **Cut Settings** panel.
	* Select the Esc key.
	* Click the **Cut Tool** button on the ProBuilder toolbar again.

	**Tip**: When you create a new face, the Cut tool creates extra edges in order to strengthen the geometry. Avoid merging the surrounding faces to remove the extra edges, because this could result in degenerated faces and broken geometry.

	![After you cut a face, extra edges secure the new face's integrity. Removing those images makes the new face unstable](images/cut-tool-nomerge.png)



<a name="cut-tool_panel"></a>

## Cut Settings panel

When ProBuilder enters Cut mode, the following panel appears at the bottom of the Scene view:

![Extra options for the Cut Tool](images/Cut_Tool-panel.png)

Enable the **Snap to existing edges and vertices** option to snap the points you draw on the target face to any nearby edges and vertices. This makes it easier to place points on the edges or vertices of the face.

When snapping is enabled, the **Snapping distance** defines what happens as you approach a face border. By default, if you click within 0.1 units of an edge or vertex, the Cut tool adds your point on that border instead of adding a floating point directly on the face.

When you activate the Cut tool, the **Cut Settings** panel displays a **Complete** button and a **Cancel** button. As soon as you complete the cut successfully, the **Start** button appears because the Cut tool is modal and you have the choice to either define another cutout or explicitly exit the tool:

![You can choose to start another cut or exit the Cut Tool](images/Cut_Tool-panel2.png)

When you are ready to define points for a new cutout, click the **Start** button and draw more points on the Mesh.

When you want to exit the tool, click the **Quit** button, select the Esc key, or click the **Cut Tool** button on the ProBuilder toolbar again.
