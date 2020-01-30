## Table of Contents

- [About](#about)
- [Setup](#development)
- [API Overview](#api)
- [License](#license)
- [Third Party Licenses](#third-party-licenses)
- [Contributing](#contributing)

## About

ProBuilder is a 3D modeling plugin for [Unity](https://unity3d.com).

This readme is intended as a brief introduction for developers interested in working with the API.

See the [Manual](https://docs.unity3d.com/Packages/com.unity.probuilder@4.0/manual/index.html) for a user guide, or the [Scripting Reference](https://docs.unity3d.com/Packages/com.unity.probuilder@4.0/api/index.html) for API documentation.

## Development

ProBuilder is a developed as a package and distributed with Package Manager.

The simplest way to get started working with source is to clone the repository into your `Packages` directory.

```
~/Desktop/MyProject$ cd Packages/
~/Desktop/MyProject/Packages$ git clone https://github.com/Unity-Technologies/com.unity.probuilder
```

See the [Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@2.0/manual/index.html#installing-removing-disabling-and-updating-packages) documentation for more information on installing packages.

## API

There are 3 major namespaces.

| Namespace | Function |
|--|--|
| `UnityEngine.ProBuilder` | Mesh types and functions to compile meshes to Unity compatible assets. |
| `UnityEngine.ProBuilder.MeshOperations` | Mesh editing. |
| `UnityEditor.ProBuilder` | Editor integration. |

Mesh data is stored in a component (`ProBuilderMesh`) and compiled to a `UnityEngine.Mesh` (referred to as `UMesh` from here on) as necessary.

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
- Shared vertices (also referred to as "common vertices")

Normals, tangents, collisions, and UVs are calculated as necessary.

\*ProBuilder can automatically UV unwrap triangles on a per-face basis. `Face`
has a toggle to enable or disable this feature (users are free to unwrap faces
by manually as well).

Modifying a ProBuilder mesh is a bit different from a Unity mesh. Instead of
working with the `MeshFilter.sharedMesh` you'll instead be operating on the
ProBuilder representation of the mesh: `ProBuilderMesh`.

A typical workflow looks like this:

```
// Create a new cube primitive
var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);

// Extrude the first available face along it's normal direction by 1 meter.
mesh.Extrude(new Face[] { mesh.faces.First() }, ExtrudeMethod.FaceNormal, 1f);

// Apply the changes back to the `MeshFilter.sharedMesh`.
// 1. ToMesh cleans the UnityEngine.Mesh and assigns vertices and sub-meshes.
// 2. Refresh rebuilds generated mesh data, ie UVs, Tangents, Normals, etc.
// 3. (Optional, Editor only) Optimize merges coincident vertices, and rebuilds lightmap UVs.
mesh.ToMesh();
mesh.Refresh();
mesh.Optimize();
```
## License

[Unity Companion License](LICENSE.md)

## Third Party Licenses<a name="third-party-licenses"></a>

[Third Party Licenses](https://github.com/Unity-Technologies/com.unity.probuilder/blob/master/com.unity.probuilder/Third%20Party%20Notices.md)

## Contributing

**All contributions are subject to the [Unity Contribution Agreement(UCA)](https://unity3d.com/legal/licenses/Unity_Contribution_Agreement).**

By making a pull request, you are confirming agreement to the terms and conditions of the UCA, including that your Contributions are your original creation and that you have complete right and authority to make your Contributions.

**Pull Requests**

Please include an entry to the changelog for any PR, along with a Fogbugz ticket number if applicable.

New logs should be placed under the `## [Unreleased]` header at the top of the changelog. See [Contributing](CONTRIBUTING.md) for more details.

