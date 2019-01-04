# Boolean (Experimental) window

> ***Warning:*** The Boolean Tool is experimental, meaning that it is still under development, and may reduce ProBuilder's stability. Please use with caution.

The experimental Boolean Tool provides an interface for creating new Meshes from boolean operations.

![Boolean (Experimental) window](images/Experimental_BooleanWindow.png)

Each type of operation takes two ProBuilder Mesh objects as the *left* and *right* inputs. ProBuilder combines the two input Meshes to create a new Mesh shape based on the operation you selected.

You can only access this tool from the menu, as it is experimental:

1. Open the Boolean (Experimental) window by choosing **Tools** > **ProBuilder** > **Experimental** > **Boolean (CSG) Tool** from Unity's top menu.
2. Set references to the ProBuilder meshes just under the preview windows on the left and the right side.
3. Select one of the boolean operation types from the **Operation** drop-down menu: [Intersection](#intersect), [Union](#union), or [Subtraction](#subtract).
4. Click the **Apply** button. ProBuilder performs the selected operation.



<a name="intersect"></a>

### Intersection

The new Mesh matches the shape where the two original Meshes intersected in space.

![Boolean Intersection of a Cube and a Cylinder](images/boolean_intersection.png)



<a name="union"></a>

### Union

The new Mesh matches both of the original Meshes but as a single Mesh.

![Boolean Union of a Cube and a Cylinder](images/boolean_union.png)



<a name="subtract"></a>

### Subtraction

The new Mesh is like a Union of the two original Meshes minus the shape where the two shapes occupy the same space.

![Boolean Subtraction of a Cube and a Cylinder](images/boolean_subtraction.png)