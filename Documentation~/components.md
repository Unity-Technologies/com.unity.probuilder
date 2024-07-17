# ProBuilder mesh components

For a GameObject to be a ProBuilder object, it needs the following components:

* [ProBuilder MeshFilter](ProBuilderMesh.md): All ProBuilder shapes have this components. It displays the **Object Size** of the ProBuilder Mesh in **X**, **Y**, and **Z**. It also allows you to generate lightmap UVs for the Mesh, and customize how ProBuilder generates them. 
* [PolyShape (script)](polyshape.md): For custom shapes.
* [Bezier shape (script)](bezier.md) (Experimental)
* Mesh Renderer and Mesh Collider. These components are standard for 3D objects. For more information, refer to [GameObjects](https://docs.unity3d.com/6000.0/Documentation/Manual/GameObjects.html). 



When you first activate a creation tool, the Editor adds the appropriate ProBuilder components to the new GameObject. They expose specific properties defined in the corresponding scripts which help define the topology. After you create the new mesh, you can re-activate the tool for the same mesh and change these properties to modify the mesh's shape.

> **Note**: When you re-activate one of these tools, you lose any modifications you made to the mesh through an action or through the [Cut](cut-tool.md) tool. For example, imagine you create a new Poly Shape with five points, and then extrude one of the faces. Next, you decide to remove one of the points, so you enter Poly Shape editing mode again. The extrusion disappears as soon as you re-enter Poly Shape editing mode.

