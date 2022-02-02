## Table of Contents

- [About](#about)
- [Setup](#development)
- [API Overview](#api)
- [License](#license)
- [Third Party Licenses](#third-party-licenses)
- [Contributing](#contributing)

## About

ProBuilder is a 3D modeling plugin for [Unity](https://unity3d.com).

This readme provides a brief introduction for developers interested in working with the API. For more information, the following guides are available:

* See the [Manual](https://docs.unity3d.com/Packages/com.unity.probuilder@latest/index.html?subfolder=/manual/index.html) for information about working with ProBuilder in the Unity Editor. 
* See the [Scripting Reference](https://docs.unity3d.com/Packages/com.unity.probuilder@latest/index.html?subfolder=/api/index.html) for API documentation.

Working code samples are also available from the package repository under the `Samples~` subfolder, or from the Package Manager you can import them directly into your Unity project.

## Development

Unity provides ProBuilder as a package, distributed with the Package Manager.

To start working with ProBuilder source, clone the repository into your `Packages` directory.

```
~/Desktop/MyProject$ cd Packages/
~/Desktop/MyProject/Packages$ git clone https://github.com/Unity-Technologies/com.unity.probuilder
```

Then you can install the cloned package directly in the Package Manager using the "local" method. For more information on installing local packages in the Package Manager, see the [Package Manager](https://docs.unity3d.com/Manual/upm-ui-local.html) documentation.

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

<a name="third-party-licenses"></a>

## Third Party Notices

[Third Party Licenses](https://github.com/Unity-Technologies/com.unity.probuilder/blob/master/Third%20Party%20Notices.md)

## Contributing

**All contributions are subject to the [Unity Contribution Agreement(UCA)](https://unity3d.com/legal/licenses/Unity_Contribution_Agreement).**

By making a pull request, you are confirming agreement to the terms and conditions of the UCA, including that your Contributions are your original creation and that you have complete right and authority to make your Contributions.

**Pull Requests**

Please include an entry to the changelog for any PR, along with a Fogbugz ticket number if applicable.

New logs should be placed under the `## [Unreleased]` header at the top of the changelog. See [Contributing](CONTRIBUTING.md) for more details.

