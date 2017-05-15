# ProBuilder

ProBuilder is a 3D modeling plugin for [Unity](https://unity3d.com).

This readme is intended for developers interested in working with the API, or 
compiling packaged versions from source.

## Code Organization

There are 3 major namespaces, each corresponding to a DLL target.

- ProBuilder2.Common -> ProBuilderCore.dll
- ProBuilder2.MeshOperations -> ProBuilderMeshOps.dll
- ProBuilder2.EditorCommon -> ProBuilderEditorCore.dll

All mesh creation and editing functionality is restricted to the `Core` and 
`MeshOps` libraries, which are both available at runtime.

### ProBuilder2.Common

The Common namespace includes most of the data types and functionality required 
to compile meshes from aforementioned types. This corresponds to the 
`ProBuilder/Classes/ClassesCore/` folder.

### ProBuilder2.MeshOperations

MeshOperations is a collection of mostly static classes for manipulating mesh 
data. This corresponds to the `ProBuilder/Classes/ClassesEditing/` folder.

### ProBuilder2.EditorCommon

Unity editor integration. This corresponds to the 
`ProBuilder/Editor/EditorCore` folder.

---

## API Quick Start

Mesh data is stored in a component (`pb_Object`) and compiled to a 
`UnityEngine.Mesh` (referred to as `UMesh` from here on) as necessary.

`pb_Object` retains the following mesh information:

- Positions
- UVs
- Faces
	- Triangles
	- Material
	- Smoothing group
	- Auto/Manual UVs*
- Tangent (if user set)
- Colors
- Shared indices (also called common vertices)

Normals, tangents, collisions, and UVs are calculated as necessary.

*ProBuilder can automatically UV unwrap triangles on a per-face basis. `pb_Face`
has a toggle to enable or disable this feature (users are free to unwrap faces
by manually as well).

Creating a simple quad with the ProBuilder API could look like this (assuming 
one doesn't use `pb_ShapeGenerator`):

	// Create a new quad facing forward.
	pb_Object quad = pb_Object.CreateInstanceWithVerticesFaces(new Vector3[] {
		Vector3.zero, 
		Vector3.right,
		Vector3.up,
		Vector2.one },
		new pb_Face[] { new pb_Face(new int[] { 0, 1, 2, 1, 3, 2 } ) } );

Modifying a ProBuilder mesh is a bit different from a Unity mesh. Instead of 
working with the `MeshFilter.sharedMesh` you'll instead be operating on the 
ProBuilder representation of the mesh: `pb_Oject`. The basics are the same 
however. Set vertex positions, modify triangles (faces in ProBuilder), rebuild
the mesh. Say for example you'd like to move the vertices up on that quad from
the previous example:
	
	// Move vertex positions up
	for(int i = 0; i < quad.vertexCount; i++)
		quad.vertices[i] += Vector3.one;

	// Assign positions and triangles to the `MeshRenderer.sharedMesh`
	quad.ToMesh();

	// Recalculate UVs, Normals, Tangents, Collisions
	quad.Refresh();
