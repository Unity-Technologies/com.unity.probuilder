# Repair meshes and strip scripts

<!--intro-->

##  Repair

Repair problems with ProBuilder meshes in the scene.

| **Menu item** | **Description** |
| --- | --- |
| **Fix Meshes in Selection** | Checks for degenerate triangles and removes them. A degenerate triangle is a triangle that has collinear vertices or one where two or more vertices are occupy the same point in space. |
| **Rebuild All ProBuilder Objects** | Rebuilds mesh representations from stored ProBuilder data for each GameObject in the scene. If you have a lot of GameObjects in a scene, this can take a while. |
| **Rebuild Shared Indexes Cache** | Discards all shared vertex position data and rebuilds based on proximity. |
| **Check for Broken ProBuilder References** | Checks for and repairs any missing or broken ProBuilder references in the scene. |


## Actions

Strip out ProBuilder scripts and leave only the models.

| **Menu item** | **Description** |
| --- | --- |
| **Strip All ProBuilder Scripts in Scene** | Remove all ProBuilder scripts from all GameObjects in the scene. |
| **Strip ProBuilder Scripts in Selection** | Remove all ProBuilder scripts from selected GameObjects. |
