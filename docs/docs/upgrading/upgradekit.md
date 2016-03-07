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
