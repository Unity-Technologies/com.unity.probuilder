# Custom
You can define your own custom Mesh. To do this, you need to specify a set of vertices in the __Custom Geometry__ field: 

    // Vertical Plane
    0, 0, 0
    1, 0, 0
    0, 1, 0
    1, 1, 0

The order to specify vertices is to follow a backwards Z pattern for each face:

![Order of vertices per face on a custom shape](images/custom_vtx_order.png)

Repeat for each adjacent face until you have created the shape you want.
