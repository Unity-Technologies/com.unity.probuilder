# ProBuilder

ProBuilder is a 3D modeling plugin for [Unity](https://unity3d.com).

This readme is intended for developers interested in working with the API, or
compiling packaged versions from source.

## Code Organization

There are 3 major namespaces, each corresponding to a DLL target.

- ProBuilder.Core -> ProBuilderCore.dll
- ProBuilder.MeshOperations -> ProBuilderMeshOps.dll
- ProBuilder.EditorCore -> ProBuilderEditorCore.dll

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

## Building for Unity Package Manager

### Setup

First create a new Unity project in a directory adjacent the **probuilder2** directory and name it **upm-package-probuilder-project**.\*

\*See below for an example folder structure.

Next, create the folder `UnityPackageManager` (if it doesn't already exist) and clone the [upm-package-probuilder](https://github.com/procore3d/upm-package-probuilder) repository to a folder named `com.unity.probuilder`.

```
cd upm-package-probuilder-project
mkdir UnityPackageManager
cd UnityPackageManager
git clone https://github.com/procore3d/upm-package-probuilder.git com.unity.probuilder
```

At this point your directory structure should something like this:

```
C:/Users/karl/dev
|_ probuilder2
  |_ art_source
  |_ build
  |_ docs
  |_ probuilder2.0
|_ upm-package-probuilder-project
  |_ Assets
  |_ UnityPackageManager
    |_ com.unity.probuilder
    |_ manifest.json
```

### Building for Unity Package Manager

Run the **ProBuilderAdvanced-UPM.json** build target.

`mono pb-build.exe build/targets/ProBuilderAdvanced-UPM.json`

Alternatively, you can build using the latest version of Unity trunk using the `upm-trunk.json` build target. This is currently only tested on @karl-'s machine but should work on any setup with minimal modifications (ask on #devs-probuilder if you're having trouble).

`mono pb-build.exe build/targets/upm-trunk.json`

The build target takes care of copying all the necessary files and changelogs to the package manager staging project.

At this point the `upm-package-probuilder-project` is ready for testing and uploading.

### Push Package Manager to Staging

Follow the instructions in the [upm-package-template](https://github.com/UnityTech/upm-package-template) to set up **npm** credentials. **Do not commit `.npmrc` files to the [upm-package-probuilder](https://github.com/procore3d/upm-package-probuilder) repository.**

1. Verify that **packages.json** is up to date and contains the correct information.
1. Follow QA release steps.
1. Make 100% sure that this release is ready to go. Unpublishing is not possible.
1. `npm publish`

### Testing UPM Builds

There are two ways to install ProBuilder with Packman.

1. Edit the registry (pull the latest from [staging](https://bintray.com/unity/unity-staging))
1. Build locally (see above)

#### Installing the latest staging

1. Open a new Unity project
1. Open the `manifest.json` file in `My Unity Project/UnityPackageManager`
1. Add the package, version, and staging URL to your registry
```
{
	"dependencies": {
		"com.unity.probuilder" : "2.10.0"
	},
	"registry":"http://staging-packages.unity.com"
}
```

### Upgrading from Asset Store to Packman

1. Move user data out of the ProCore/ProBuilder folder (put it in `Assets` or where-ever else you want)
	- Move `Assets/ProCore/ProBuilder/Data` to `Assets/ProBuilder Data`
	- Move `Assets/ProCore/ProBuilder/ProBuilderMeshCache` to `Assets/ProBuilder Data/ProBuilderMeshCache`
1. Delete the ProCore/ProBuilder folder
	- **If meshes in your scene disappear when you delete this folder:** Don't panic. That just means **Meshes Are Assets** is enabled you and didn't move the Mesh Cache. Perform the next step (import Packman ProBuilder) then run `Tools/ProBuilder/Repair/Rebuild All ProBuilder Objects`.
1. Import Packman ProBuilder package
