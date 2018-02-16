# Table of Contents

- [Summary](#summary)
- [API Overview](#code-organization)
- [API Quick Start](#api-quick-start)
- [Build for Package Manager](#building-for-unity-package-manager)
- [Testing UPM Builds](#testing-upm-builds)
- [Build for Asset Store](#building-asset-store-projects-&-packages)

# Summary

ProBuilder is a 3D modeling plugin for [Unity](https://unity3d.com).

This readme is intended for developers interested in working with the API, or
compiling packaged versions from source.

## Code Organization

There are 3 major namespaces, each corresponding to a DLL target.

- `ProBuilder.Core` -> `ProBuilderCore.dll`
- `ProBuilder.MeshOperations` -> `ProBuilderMeshOps.dll`
- `ProBuilder.EditorCore` -> `ProBuilderEditorCore.dll`

All mesh creation and editing functionality is restricted to the `Core` and
`MeshOps` libraries, which are both available at runtime.

### ProBuilder.Core

The Core namespace includes most of the data types and functionality required
to compile meshes from aforementioned types. This corresponds to the
`ProBuilder/Classes/ClassesCore/` folder.

### ProBuilder.MeshOperations

MeshOperations is a collection of mostly static classes for manipulating mesh
data. This corresponds to the `ProBuilder/Classes/ClassesEditing/` folder.

### ProBuilder.EditorCore

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

// Assign positions and triangles to the `MeshFilter.sharedMesh`
quad.ToMesh();

// Recalculate UVs, Normals, Tangents, Collisions
quad.Refresh();
```

Note that you should not ever directly modify the `MeshFilter.sharedMesh`.
ProBuilder controls updating the UMesh via the `pb_Object::ToMesh` and
`pb_Object::Refresh` functions.

## Building for Unity Package Manager

### Setup

Check out the [com.unity.probuilder](https://gitlab.internal.unity3d.com/upm-packages/world-building/com.unity.probuilder) repository to a directory adjacent to the **probuilder2** repository.

If you do not need to publish builds to Package Manager you can simply create an empty folder named **com.unity.probuilder** in the same directory as the **probuilder2** repository.

*See below for an example folder structure.*

```
# check out package manager repository
git clone git@gitlab.internal.unity3d.com:upm-packages/world-building/com.unity.probuilder.git com.unity.probuilder
```

At this point your directory structure should something like this:

```
C:/Users/karl/dev
|_ probuilder2
  |_ art_source
  |_ build
  |_ docs
  |_ probuilder2.0
|_ com.unity.probuilder
  |_ Documentation
  |_ ProBuilder
  |_ CHANGELOG.md
  |_ etc...
```

### Compile ProBuilder from Source Project

In the **probuilder2** directory, run **upm.json** build target:

`mono pb-build.exe upm.json`

If you check out the `unity/trunk` repository to somewhere other than `$HOME` (`~` on unix, `C:/Users/%USER%` on Windows) you will need to either modify the **upm.json** file "UnityPath" to append your Unity build directory, or pass `-unity-path=<path_to_unity>` to `pb-build`. See `mono pb-build.exe --help` for more information on build arguments.

The build target takes care of copying all the necessary files and changelogs to the package manager staging project, as well as updating the version information in both the source project and upm project. It does *not* set the Unity version in the "package.json" file, so if this is a new Unity version you'll need to change that manually.

> The version info is scraped from the source changelog.txt file.

Pass `-d` for a debug build. See `pb-build --help` for additional args.

**upm.json** builds the project to `../com.unity.probuilder`. Copy this directory into a Unity project's "UnityPackageManager" directory and edit the manifest to test locally.

### Push Package Manager to Staging

See [Semantic Versioning](#Semantic-Versioning) for information about build versions.

Follow the instructions in the [upm-package-template](https://gitlab.internal.unity3d.com/upm-packages/upm-package-template) to set up **npm** credentials.

**Do not commit `.npmrc` files to the [com.unity.probuilder](https://github.com/procore3d/upm-package-probuilder) repository.**

1. Verify that **package.json** is up to date and contains the correct information (the version info in this file is automatically populated by pb-build).
1. Perform QA testing as outlined in QAReport.md.
1. To push a live version, run `npm publish`.

**Important** Once a package is pushed it is not undo-able. Un-publishing or overwriting a version is not possible.

## Testing UPM Builds

#### Locally

1. Grab the latest release from [Gitlab](https://gitlab.internal.unity3d.com/upm-packages/world-building/com.unity.probuilder/tags).
1. Create a new Unity project
1. Copy the **com.unity.probuilder** directory into the new project's **UnityPackageManager** directory.
1. Edit the **manifest.json** file to include "com.unity.probuilder" as a dependency (see below for example).
1. Follow the QA instructions outlined in the QAReport.md file.

Example **manifest.json** file (replace the version number with the tagged version).

```
{
	"dependencies":{
		"com.unity.probuilder":"3.0.0-f.0"
	}
}
```

#### From Bintray

Check for the latest version on Bintray: https://bintray.com/unity/unity-staging/com.unity.probuilder

1. Open a new Unity project
1. Open the `manifest.json` file in `My Unity Project/UnityPackageManager`
1. Add the package, version, and staging URL to your registry\*

\* The version must match the Bintray version **exactly**. That is, `3.0.0` is not the same as `3.0.0-f.0`.

```
{
	"dependencies": {
		"com.unity.probuilder" : "3.0.0-f.0"
	},
	"registry":"http://staging-packages.unity.com"
}
```

## Building Asset Store Projects & Packages

**Important:** Asset Store build paths are no longer maintained. 2018.1 and later is required to use Package Manager.

Updates to Asset Store ProBuilder version should made and pushed from the v2.9.8f3-dev branch.

To facilitate building the projects and packages for the various Unity versions a custom built build system is employed. The lifting is done by a mono app called `pb-build` and driven by a set of `json` files. The `json` files describe the build process for a single version of ProBuilder. They are located in `build/targets` and named to match their intended destination (ex, "ProBuilderAdvanced-5.6" builds ProBuilder Advanced for Unity 5.6).

You can either run builds individually using `pb-build` directly:

```
mono pb-build.exe build/targets/ProBuilderAdvanced-5.6.json
```

Build scripts search for an install of Unity matching the version set with the `$UNITY_VERSION` macro. Unity folders are expected to be installed with the version appended (ex, **Program Files/Unity 2017.1/**). The following paths are scanned for appropriate Unity installations, or you can pass the `-unity=<path to unity>` argument to `pb-build.exe`.

- */Applications/Unity $UNITY_VERSION/Unity.app*
- *D:/Applications/Unity $UNITY_VERSION*
- *C:/Program Files/Unity $UNITY_VERSION*

Or run a batch build with the `advanced.sh` or `basic.sh` scripts.

`pb-build` provides some switches for debugging (see `mono pb-build.exe --help`).

The result of `pb-build` is a Unity project in `bin/projects` that is ready to be uploaded to the Asset Store.

### Pushing an Update to the Asset Store

1. Create a new package version in the [Asset Publisher Portal](https://publisher.assetstore.unity3d.com)
2. For each Unity version in `bin/projects` open Unity and upload the ProCore folder.
3. Update the changelog and version in the Publisher Portal and submit.

### Building ProBuilder to a UnityPackage

To export a project to a `.unitypackage` there is a bash script named `export-packages.sh`. This exports all ProBuilder Advanced projects to `bin/packages`.

Packages are used for distribution via the [ProCore User Toolbox](http://www.procore3d.com/usertoolbox) (deprecated) and Github releases (internal).

To selectively build a package pass the project suffix to `export-packages.sh`. Ex, `sh export-packages.sh 56 SRC` will build the Unity 5.6 and Source code packages.

Valid arguments:

- `SRC`
- `5.3`
- `5.5`
- `5.6`
- `2017.1`
- `2017.2`
- `2017.3`

## Semantic Versioning

ProBuilder follows [semantic versioning](https://semver.org/) rules.

Non-public testing builds should include a build identifier and patch pre-release version. Ex,

`3.0.0-f.1`

Once a build is verified with a [QA Report](https://drive.google.com/drive/u/0/folders/1neI43BrzpTmyHvE5Qe5TN8YVHTOp-5Dd) and cleared for release, modify the `package.json` file to omit the pre-release information (ex, `-f.0`) and tag the commit with a `vMajor.Minor.Patch`.

