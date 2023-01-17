# About the ProBuilder Scripting API

ProBuilder provides a Scripting API for C#, which you can use to extend the ProBuilder tools and windows. It includes these namespaces:

- [UnityEditor.ProBuilder](xref:UnityEditor.ProBuilder) provides classes and enums for Unity editor integration. Use them to extend ProBuilder menus, windows, toolbars, and Mesh operations that are only available through the ProBuilder windows and tools.
- [UnityEngine.ProBuilder](xref:UnityEngine.ProBuilder) provides classes, structs, and enums for compiling Meshes. Use them to access a lot of core ProBuilder functionality, such as creating Meshes, dealing with events, and some math functions.
- [UnityEngine.ProBuilder.MeshOperations](xref:UnityEngine.ProBuilder.MeshOperations) provides classes for Mesh editing. Use them to manipulate ProBuilder Meshes, including topology and I/0 operations.

All Mesh creation and editing functionality is restricted to the `UnityEngine.ProBuilder` and
`UnityEngine.ProBuilder.MeshOperations` libraries, which are both available at run time.

ProBuilder stores Mesh data in a component ([ProBuilderMesh](xref:UnityEngine.ProBuilder.ProBuilderMesh)) and compiles it to a
[UnityEngine.Mesh](https://docs.unity3d.com/ScriptReference/Mesh.html) object as necessary.

`ProBuilderMesh` stores the following Mesh information:

- Positions
- UVs
- Faces
  - Triangles
  - Material
  - Smoothing group
  - Auto/Manual UVs

    **Note:** ProBuilder can automatically UV unwrap triangles on a per-face basis. You can toggle this feature with the [Face](xref:UnityEngine.ProBuilder.Face) class. In addition, users can unwrap faces manually.

- Tangent (if user set)
- UV3/4 (if user set)
- Colors
- Shared indices (also called common vertices)

Normals, tangents, collisions, and UVs are calculated as necessary.

## Create a Mesh

This example demonstrates how to build a simple quad with the ProBuilder API (not with the [ShapeGenerator](xref:UnityEngine.ProBuilder.ShapeGenerator) class):

```c#
// Create a new quad facing forward.
ProBuilderMesh quad = ProBuilderMesh.Create(
    new Vector3[] {
        new Vector3(0f, 0f, 0f),
        new Vector3(1f, 0f, 0f),
        new Vector3(0f, 1f, 0f),
        new Vector3(1f, 1f, 0f)
    },
	new Face[] { new Face(new int[] { 0, 1, 2, 1, 3, 2 } )
} );
```



## Modify a Mesh

Modifying a ProBuilder Mesh is different from modifying a Unity Mesh: instead of working with [MeshFilter.sharedMesh](https://docs.unity3d.com/ScriptReference/MeshFilter-sharedMesh.html) you work with the ProBuilder representation of the Mesh: [ProBuilderMesh](xref:UnityEngine.ProBuilder.ProBuilderMesh).

The basics are the same: set vertex positions, modify triangles (faces in ProBuilder), then rebuild the mesh. For example, to move the vertices up on that quad from the previous example:

```c#
// Move vertex positions up
Vertex[] vertices = quad.GetVertices();
for(int i = 0; i < quad.vertexCount; i++)
	vertices[i] += Vector3.one;

// Rebuild the triangle and submesh arrays, and apply vertex positions & submeshes to `MeshFilter.sharedMesh`
quad.SetVertices(vertices);
quad.Rebuild();

// Recalculate UVs, Normals, Tangents, Collisions, then apply to Unity Mesh.
quad.Refresh();

// If in editor, generate UV2 and collapse duplicate vertices with
EditorMeshUtility.Optimize(quad, true);

// At runtime, `EditorMeshUtility` is not available. To collapse duplicate
// vertices in runtime, modify the MeshFilter.sharedMesh directly.
// Note that any subsequent changes to `quad` will overwrite the sharedMesh.
var umesh = quad.GetComponent<MeshFilter>().sharedMesh;
MeshUtility.CollapseSharedVertices(umesh);
```

Note that you should never directly modify the `MeshFilter.sharedMesh`. ProBuilder controls updating the Unity Mesh with [ProBuilderMesh::ToMesh](xref:UnityEngine.ProBuilder.ProBuilderMesh.ToMesh(UnityEngine.MeshTopology)) and [ProBuilderMesh::Refresh](xref:UnityEngine.ProBuilder.ProBuilderMesh.Refresh(UnityEngine.ProBuilder.RefreshMask)) functions.
