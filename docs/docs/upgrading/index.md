<div class="alert-box warning">
<h2>Important!</h2>
The upgrade process is <b>not</b> reversible. If an error occurs, you will lose all your work. Always make a backup of your entire project before upgrading.
</div>

If you are using **ProBuilder 2.4.7** or later (you can check in the **Tools > ProBuilder > About** window) you can skip this entirely and just import the new version.

If you're running an older version of **ProBuilder** you'll need to follow a slightly more complex path.  **Make a backup of your entire project before proceeding!**

Depending on the version of ProBuilder you currently have installed, the process differs.  Use this chart to decide which guide you should follow.

| Currently Using | Upgrading To | Process |
| - | - | - |
| None (no ProBuilder install) | ProBuilder Basic | [Standard](#standard) |
| None (no ProBuilder install) | ProBuilder Advanced | [Standard](#standard) |
| ProBuilder Basic 2.4.8+ | ProBuilder Basic 2.4.8+ | [Standard](#standard) |
| ProBuilder Advanced 2.4.8+ | ProBuilder Advanced 2.4.8+ | [Standard](#standard) |
| ProBuilder 2.1.0 - 2.3.x | ProBuilder Advanced | [Upgrade Kit](#upgradekit) |
| ProBuilder 2.4.0 - 2.4.7 | ProBuilder Advanced | [DLL Rename Upgrade](#dll_rename) |
| Prototype 2.4 - 2.6 | ProBuilder Basic/Advanced | [Prototype Upgrade](#prototype) |
| ProBuilder Source (any version) | ProBuilder (any version) | [Upgrade Kit](#upgradekit) |
| ProBuilder (any version) | ProBuilder Source (any version) | [Upgrade Kit](#upgradekit) |


<a name="standard"></a>
# Standard

If you're upgrading from version 2.4.7 or higher (either ProBuilder Basic or ProBuilder Advanced), updating a ProBuilder project is as simple as importing the new package.  

There are some circumstances where this will fail however, so always be sure to make a backup prior to upgrading your project.  In the event that simply importing the new package fails, follow this the [DLL Rename Upgrade](#dll_rename) guide.

<a name="prototype"></a>
# Prototype

If you're using **Prototype** (the precursor to **ProBuilder Basic) you'll need to perform one additional step prior to using the [DLL Rename](#dll_rename) guide.

- Rename `ProCore/Prototype` folder to `ProCore/ProBuilder`
- Follow [DLL Rename Upgrade](#dll_rename)

<a name="dll_rename"></a>
# DLL Rename

<div class="alert-box warning">
<h2>Important!</h2>
As of Unity 5.3 this guide no longer applies.  Use the <a href="standard.html">Standard</a> upgrade procedure.
</div>

[Video tutorial](https://www.youtube.com/watch?v=mpluzo9Zrxs&feature=youtu.be)

- Import the new ProBuilder unity package.  Make sure all items are toggled in the Importing Package window.
- After import, close the ProBuilder About Window with this version's changelog.
- There are now errors in the Console.  This is expected.
- Navigate to the `ProCore > ProBuilder > Classes` folder.
- Right-Click (Context-Click Mac) the `ProBuilderCore-Unity5` file and select `Show In Explorer`.
- In the File Explorer (or Finder, on Mac), delete the `ProBuilderCore-Unity5` and `ProBuilderMeshOps-Unity5`
- Next (still in the File Explorer) rename `ProBuilderCore-Unity6` and `ProBuilderMeshOps-Unity6` to `ProBuilderCore-Unity5` and `ProBuilderMeshOps-Unity5`.  If visible meta files are enabled, don't worry about changing their file names.  Unity will take care of that for you.
- Staying in the File Explorer, navigate one folder up and into the `Editor` folder.
- Follow the same procedure with the `ProBuilderEditor-Unity5` files.  Delete `ProBuilderEditor-Unity5` then rename `ProBuilderEditor-Unity6` to `ProBuilderEditor-Unity5`.
- Open Unity again.  The project will recompile.
- Depending on what version of ProBuilder you are upgrading from, you may see some errors in the Console from deprecated scripts.  Just click the error to find the file, then delete it (making sure that it is in the ProBuilder folder, don't delete any of your own scripts!).
	- Common deprecated files to delete:
		- `ProBuilder > Editor > MenuItems > File > pb_SaveLoad`
		- `ProBuilder > Editor > MenuItems > Tools > pb_VertexPainter`
		- `ProBuilder > Editor > MenuItems > Tools > pb_MaterialSelectionTool`
		- `ProCore > Shared` (entire folder is deprecated)
- Done!

<a name="upgradekit"></a>
# Upgrade Kit

Follow these steps if you are upgrading a ProBuilder project from a version less than 2.4 (Tools / ProBuilder / About to check your current version).  If you are switching from the Source version of ProBuilder to the DLL version this guide also applies.

[Youtube Tutorial](https://www.youtube.com/watch?v=O-Dz0Q3KgCs)

[Upgrade Kit Download](http://parabox.co/probuilder/upgrade.html)

- **Back up your project**
- Import the ProBuilderUpgradeKit package before importing the new version of ProBuilder (downloadable in the User Toolbox, or bundled in the ProBuilder package).
- Run `Tools > ProBuilder > Upgrade > Batch Prepare Scenes for Upgrade`.
- Delete the ProBuilder folder, and optionally the ProCore > Shared folder (if you delete this, make sure to also update ProGrids).
- Import the new ProBuilder package.
- Run `Tools > ProBuilder > Upgrade > Batch Re-attach ProBuilder Scripts`.
- Delete ProBuilderUpgradeKit folder.
