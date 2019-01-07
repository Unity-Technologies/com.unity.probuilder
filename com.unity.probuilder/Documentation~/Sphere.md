# Sphere
A sphere is actually a polygon with 42 vertices, and each vertex is shared by five triangles (faces).

![Sphere shapes](images/shape-tool_sphere.png)

![A](images/LetterCircle_A.png) Sphere shape with no subdivisions showing five triangles colored. Superimposed on that shape is a wireframe of a default sphere (the same radius but with one subdivision.

![B](images/LetterCircle_B.png) Default sphere shape (one subdivision).

![C](images/LetterCircle_C.png) Sphere with two subdivisions.

![D](images/LetterCircle_D.png) Sphere with three subdivisions.

![E](images/LetterCircle_E.png) Sphere with four subdivisions.

You can customize the shape of a sphere with these shape properties:

![Sphere shape properties](images/shape-tool_sphere-props.png)


| Property: | Description: |
|: |: |
| __Radius__ | Set the radius (size) of the sphere. Default value is 1. Valid values range from 0.01 to 10. |
| __Subdivisions__ | Set the number of times to subdivide each triangle. Default value is 1. Valid values range from 0 to 4.<br /><br />The more subdivisions you create, the smoother the sphere appears. However, remember that each subdivision increases the number of triangles exponentially, which means that it uses a lot more resources to render. |
