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
