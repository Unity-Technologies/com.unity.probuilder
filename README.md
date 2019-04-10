# Table of Contents

- [About](#about)
- [Setting Up Your Development Environment](#development)
- [API Overview](#api)
- [Distributing](#distribution)
- [Building Older Versions](#older-versions)
- [Build for Asset Store](#asset-store)
- [Building Docs](#documentation)

# About

ProBuilder is a 3D modeling plugin for [Unity](https://unity3d.com).

This readme is intended for developers interested in working with the API, or compiling packaged versions from source.

# Development

ProBuilder is a developed as a package and distributed with Package Manager.

See the [Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html) documentation for how to work with packages locally.

# API

There are 3 major namespaces.

| Namespace | Function |
|--|--|
| `ProBuilder.Core` | Mesh types and functions to compile meshes to Unity compatible assets. |
| `ProBuilder.MeshOperations` | Mesh editing. |
| `ProBuilder.EditorCore` | Unity editor integration. |

All mesh creation and editing functionality is restricted to the `Core` and
`MeshOperations` libraries, which are both available at runtime.

Mesh data is stored in a component (`ProBuilderMesh`) and compiled to a
`UnityEngine.Mesh` (referred to as `UMesh` from here on) as necessary.

`ProBuilderMesh` retains the following mesh information:

- Positions
- UVs
- Faces
	- Triangles
	- Material
	- Smoothing group
	- Auto/Manual UVs*
- Tangent (if user set)
- UV3/4 (if user set)
- Colors
- Shared indices (also called common vertices)

Normals, tangents, collisions, and UVs are calculated as necessary.

\*ProBuilder can automatically UV unwrap triangles on a per-face basis. `Face`
has a toggle to enable or disable this feature (users are free to unwrap faces
by manually as well).

Creating a simple quad with the ProBuilder API could look like this (assuming
one doesn't use `ShapeGenerator`):

```
// Create a new quad facing forward.
ProBuilderMesh quad = pb_Object.Create(new Vector3[] {
	new Vector3(0f, 0f, 0f),
	new Vector3(1f, 0f, 0f),
	new Vector3(0f, 1f, 0f),
	new Vector3(1f, 1f, 0f) },
	new Face[] { new Face(new int[] { 0, 1, 2, 1, 3, 2 } ) } );
```

Modifying a ProBuilder mesh is a bit different from a Unity mesh. Instead of
working with the `MeshFilter.sharedMesh` you'll instead be operating on the
ProBuilder representation of the mesh: `ProBuilderMesh`.

The basics are the same however. Set vertex positions, modify triangles (faces
in ProBuilder), then rebuild the mesh.

