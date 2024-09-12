# Create meshes

ProBuilder provides several different tools for creating editable meshes in Unity. 

The most common approach is to [build a predefined shape](shape-tool.md) with the [Shape tool](shape-tool.md), which includes a library of shapes. These predefined shapes include:

* Simple cubes, prisms, toruses, and other simple geometry that you can use to create buildings, vehicles, and other objects. 
* Predefined shapes that are typically found in buildings, such as stairs, arches, and doors.

![Mesh shape types](images/ShapeToolTypes.png) 

If you want to make a mesh with an original shape, you can: 

- Use the [Poly Shape tool](polyshape.md) to create a custom 2D shape and then extrude that shape into a 3D mesh. This is a good strategy for quickly building an irregular structure, like a medieval church or a star-shaped building.
- Use the [experimental Bezier tool](bezier.md) to define a bezier curve around which ProBuilder extrudes a mesh. For example, you can use this tool to create tunnels with lots of twists and turns.
- Apply an [experimental Boolean operation](boolean.md) to two or more meshes to create a new mesh. The new shape can be from the difference between the two (Intersection), or everything but the difference between the two (Subtraction), or the two original meshes plus the space between them (Union).

>**Warning:** Bezier shapes and Boolean operations are experimental, meaning that they're still under development, and might reduce ProBuilder's stability.

Whichever method you use to create your mesh, you can:

* Edit it using any of the [ProBuilder editing features](workflow-edit.md).
* [Apply vertex colors](workflow-vertexcolors.md).
* [Smooth its sharp edges](workflow-edit-smoothing.md).
* [Apply materials and textures](workflow-materials.md).

