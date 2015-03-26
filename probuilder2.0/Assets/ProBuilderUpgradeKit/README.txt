# README

## Notes

Projects using ProBuilder 2.3.1 and up are supported.  If your project 
is using a version less than 2.3.1 (r2900), see steps outlined below 
to get your project updated to 2.3.1 and then follow these steps.

Upgrade kits are specific to the release - remove this folder once your
project has been updated.

1. Do not move or rename the ProBuilderUpgradeKit folder.
1. Run Tools / ProBuilder / Upgrade / Prepare Scene for Upgrade on every scene in your project.
	- This menu item will remove all ProBuilder objects from your scene, and store their data in new components called `pb_SerializedComponent`.  Do not remove or alter these components or their data.
