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
ProBuilder representation of the mesh: `pb_Oject`.

The basics are the same however. Set vertex positions, modify triangles (faces 
in ProBuilder), then rebuild the mesh. Say for example you'd like to move the 
vertices up on that quad from the previous example:
	
	// Move vertex positions up
	for(int i = 0; i < quad.vertexCount; i++)
		quad.vertices[i] += Vector3.one;

	// Assign positions and triangles to the `MeshFilter.sharedMesh`
	quad.ToMesh();

	// Recalculate UVs, Normals, Tangents, Collisions
	quad.Refresh();

Note that you should not ever directly modify the `MeshFilter.sharedMesh`.
ProBuilder controls updating the UMesh via the `pb_Object::ToMesh` and 
`pb_Object::Refresh` functions.

## Building Asset Store Projects & Packages

To facilitate building the projects and packages for the various Unity versions a custom built build system is employed. The lifting is done by a mono app called `pb-build` and driven by a set of `json` files. The `json` files describe the build process for a single version of ProBuilder. They are located in `build/targets` and named to match their intended destination (ex, "ProBuilderAdvanced-5.6" builds ProBuilder Advanced for Unity 5.6).

You can either run builds individually using `pb-build` directly:

```
mono pb-build.exe build/targets/ProBuilderAdvanced-5.6.json
```

Or run a batch build with the `advanced.sh` or `basic.sh` scripts.

`pb-build` provides some switches for debugging (see `mono pb-build.exe --help`).

The result of `pb-build` is a Unity project in `bin/projects` that is ready to be uploaded to the Asset Store.

To export a project to a `.unitypackage` there is a bash script named `export-packages.sh`. This exports all ProBuilder Advanced projects to `bin/packages`.

To selectively build a package pass the project suffix to `export-packages.sh`. Ex, `sh export-packages.sh 56 SRC` will build the Unity 5.6 and Source code packages.

Valid arguments:

- `SRC`
- `47`
- `50`
- `53`
- `55`
- `56`
- `2017.2`
