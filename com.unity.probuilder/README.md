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

This readme is intended for developers interested in working with the API, or
compiling packaged versions from source.

# Development

ProBuilder is a developed as a package and distributed with [Package Manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html). You can either check out the ProBuilder repository directly into the `Packages` folder of a Unity project (embed), or use the `file:` protocol in the `Packages/manifest.json` to link to the project (local).

- [Package Manager: How to embed a package in your project](https://confluence.hq.unity3d.com/display/PAK/How+to+embed+a+package+in+your+project)
- [Package Manager: How to add a local package to your project](https://confluence.hq.unity3d.com/display/PAK/How+to+add+a+local+package+to+your+project)
- [Package Manager: How to add tests for your package](https://confluence.hq.unity3d.com/display/PAK/How+to+add+a+test+project+for+your+package)

# API

There are 3 major namespaces.

| Namespace | Function |
|--|--|
| `ProBuilder.Core` | Mesh types and functions to compile meshes. |
| `ProBuilder.MeshOperations` | Mesh editing. |
| `ProBuilder.EditorCore` | Unity editor integration. |

All mesh creation and editing functionality is restricted to the `Core` and
`MeshOps` libraries, which are both available at runtime.

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
- UV3/4 (if user set)
- Colors
- Shared indices (also called common vertices)

Normals, tangents, collisions, and UVs are calculated as necessary.

\*ProBuilder can automatically UV unwrap triangles on a per-face basis. `pb_Face`
has a toggle to enable or disable this feature (users are free to unwrap faces
by manually as well).

Creating a simple quad with the ProBuilder API could look like this (assuming
one doesn't use `pb_ShapeGenerator`):

```
// Create a new quad facing forward.
pb_Object quad = pb_Object.CreateInstanceWithVerticesFaces(new Vector3[] {
	new Vector3(0f, 0f, 0f),
	new Vector3(1f, 0f, 0f),
	new Vector3(0f, 1f, 0f),
	new Vector3(1f, 1f, 0f) },
	new pb_Face[] { new pb_Face(new int[] { 0, 1, 2, 1, 3, 2 } ) } );
```

Modifying a ProBuilder mesh is a bit different from a Unity mesh. Instead of
working with the `MeshFilter.sharedMesh` you'll instead be operating on the
ProBuilder representation of the mesh: `pb_Oject`.

The basics are the same however. Set vertex positions, modify triangles (faces
in ProBuilder), then rebuild the mesh. Say for example you'd like to move the
vertices up on that quad from the previous example:

```
// Move vertex positions up
for(int i = 0; i < quad.vertexCount; i++)
	quad.vertices[i] += Vector3.one;

// Rebuild the triangle and submesh arrays, and apply vertex positions & submeshes to `MeshFilter.sharedMesh`
// ToMesh handles condensing like faces to the fewest number of submeshes possible.
quad.ToMesh();

// Recalculate UVs, Normals, Tangents, Collisions, then apply to UMesh.
quad.Refresh();

// If in editor, generate UV2 and collapse duplicate vertices with
pb_EditorMeshUtility.Optimize(quad, true);
// If at runtime, collapse duplicate vertices with
pb_MeshCompiler.CollapseSharedVertices(quad);
```

Note that you should not ever directly modify the `MeshFilter.sharedMesh`.
ProBuilder controls updating the UMesh via the `pb_Object::ToMesh` and
`pb_Object::Refresh` functions.

# Distribution

Starting at version 3.0, ProBuilder is distributed as a Package Manager package. These instructions are for the current trunk (4.x and up). To update an older version of ProBuilder (2.x or 3.x), follow the instructions in **Building Older Versions**.

Packages are created through a Gitlab pipeline, triggered by tags.

In addition to a tag, the version number needs to be set in the following places:

- [ ] CHANGELOG.md
- [ ] package.json
- [ ] Runtime/Core/pb_Version.cs

[Create a Pre-Release Package](https://gitlab.internal.unity3d.com/upm-packages/upm-package-template#create-a-pre-release-package)

[Preparing your package for Staging](https://gitlab.internal.unity3d.com/upm-packages/upm-package-template#preparing-your-package-for-staging)

[Get your package published to Production](https://gitlab.internal.unity3d.com/upm-packages/upm-package-template#get-your-package-published-to-production)

Once a build is verified with a [QA Report](https://drive.google.com/drive/u/0/folders/1neI43BrzpTmyHvE5Qe5TN8YVHTOp-5Dd) and cleared for release, modify the `package.json` file to omit the pre-release information (ex, `-f.0`) and tag the commit with a `vMajor.Minor.Patch`.

- [QA Report](https://docs.google.com/document/d/1uGJV1Wkij_fqB_TeCAUDYoYoSiU1IryoKrjtzPRxN4g/edit)

## Building Older Versions

### ProBuilder 3.x

Starting with 4.x, ProBuilder is distributed as source code using assembly definition files. ProBuilder 3.x was distributed in pre-compiled DLLs. To build a DLL package for a 3.x update:

- [ ] Check out the `v3.x-release` branch
- [ ] Check out the `com.unity.probuilder-dll` repository
- [ ] Follow build instructions in the README of the `3.x` branch.

### Asset Store

If updating the Asset Store version of ProBuilder 2.x, check out a new branch from `v2.9.8f3` and follow the instructions in the readme.

`git checkout v2.9.8f3 -b v2.10-dev`

# Documentation

To build the documentation, install the Pacakge Manager DocTools package to your project.

https://gitlab.internal.unity3d.com/upm-packages/package-ux/package-manager-doctools/tree/master

Then in the Package Manager UI, select the local version of ProBuilder and select "Generate Documentation."
