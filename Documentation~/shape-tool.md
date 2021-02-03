# ![Shape Tool icon](images/icons/Panel_Shapes.png) New Shape tool

Use the **New Shape** tool to create new ProBuilder Mesh shapes, such as cylinders, arches, and stairs.

<span style="color:blue">**@DEVQ**: How do I use the Edit Shape tool on a ProBuilderized Unity primitive without explicitly adding a ShapeComponent (and would that even work)?</span>

<span style="color:blue">**@DEVQ**: Do the XYZ properties on the shape components represent a delta from some fixed point? If so, what is that origin?</span>

![Shape Tool properties](images/shape-tool.png) 

Each shape has specific properties. You can customize these before you add the shape to your Scene. For example, the **Stairs** shape lets you choose items like ~~step height, arc, and which parts of the stairway to build~~. 

<span style="color:blue">**@DEVQ**: How do you create curved stairs without the Curvature property?</span>

![Shape Tool Example](images/Example_ShapeToolsWithCurvedStair.png)

ProBuilder Mesh shapes are similar to other GameObjects in Unity in terms of how they interact with other GameObjects and respond to Physics in the Scene. However, you can use [ProBuilder actions](workflow-edit) to customize and deform ProBuilder Meshes after you create them. 

## Creating specific shapes

To create a new ProBuilder Mesh based on one of the predefined shapes:

1. In the ProBuilder toolbar, click the **New Shape** tool. 

	> **Tip:** You can also use the **Ctrl/Cmd+Shift+K** hotkey to open the **Shape Tool** window, or use the menu (**Tools** > **ProBuilder** > **Editors** > **Open Shape Editor Menu Item**).

1. Choose the shape you'd like to create (such as *Cube*, *Cylinder* or *Torus*) from the __Shape Selector__ drop-down menu. The properties specific to that shape type appear in the Shape Tool window.

1. Set the options (width, height, radius, number of stairs) according to the type of shape.

1. Move or rotate the preview object within the Scene view until it's in a position you're happy with.

1. Click **Build** to create the new Mesh. 

	Now that you have a ProBuilder Mesh, you can use [ProBuilder's editing tools](workflow-edit) to further define the Mesh.

## Shape-specific properties

The following sections describe the shape-specific properties available for these shape types:

* [Cube](Cube.md)
* [Sprite](Sprite.md)
* [Prism](Prism.md)
* [Stairs](Stair.md)
* [Cylinder](Cylinder.md)
* [Door](Door.md)
* [Plane](Plane.md)
* [Pipe](Pipe.md)
* [Cone](Cone.md)
* [Arch](Arch.md)
* [Sphere](Sphere.md)
* [Torus](Torus.md)
* [Custom](Custom.md)



