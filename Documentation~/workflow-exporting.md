# Exporting and re-importing

If you want to use a ProBuilder mesh in another program, such as a 3D modeling application, you can use the **Export** action to save it to one of the supported formats. For example, you might be using a ProBuilder mesh as a placeholder while greyboxing, but eventually want to create or enhance a complex mesh.

You can also export your ProBuilder mesh to the Unity `.asset` format and re-import it to use as a Prefab or spawn it in the scene. 

> **Caution:** When you export a ProBuilder mesh to file, remember that if you want to re-import and use ProBuilder tools and actions on it, you need to [ProBuilderize](Object_ProBuilderize.md) first.

This section provides information on which formats are available, how to export ProBuilder objects to file, and also some tips for re-importing back into Unity.
  
<a name="formats"></a>

## Supported formats

ProBuilder allows you to export GameObjects to the following formats:

| **Format** | **Description** |
| --- | --- |
| **OBJ** | Wavefront OBJ. This is a widely supported model format. It supports multiple Textures and mesh groups. |
| **STL** | A widely supported format, generally used in CAD software or 3D printing. It only supports Triangle geometry. |
| **PLY** | Stanford PLY. Generally supported and very extensible. It supports quads and vertex colors, but not multiple Materials. |
| **Asset** | Unity Asset format, only readable in Unity. |


 
 <a name="export"></a>

## Export a ProBuilder mesh

To export one or more objects to one of the [supported formats](#formats):

1. Select the GameObject you want to export.
1. Activate the GameObject tool context.
1. Right-click in the **Scene** view to open the **Scene** view context menu. 
1. Select **ProBuilderMesh > Export** to open the **Editor Settings** window.
1. Select the format you want to export to from the **Export Format** dropdown menu.
1. If you're exporting an OBJ to use in Unity, disable the [Copy Textures](Object_Export.md) option. 
	> **Tip:** When you re-import the mesh, follow the instructions under [Re-importing an exported mesh](#reimport).
1. When you are finished setting export options, select **Export**.
1. Use the file browser to save the exported 3D Model.
 
<a name="reimport"></a>

## Re-importing an exported mesh

When you import the OBJ format, select the 3D Model file from the Project view, and on the [Material tab](https://docs.unity3d.com/Manual/FBXImporter-Materials.html) of the **Model Import Settings** window, set the following options:

- In **Location**, select **Use Embedded Materials**.
- Inside the **Remapped Materials** section, expand the **On Demand Remap** option group.
- In **Naming**, select **From Model's Material**.
- In **Search**, select **Project-Wide**.
