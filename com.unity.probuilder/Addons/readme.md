# Enabling FBX Integration

By default, FBX integration is disabled. To enable it, you will need to modify the add-ons assembly definition file to reference the FBX dependencies.

- Install FBX Exporter 2.0.1 or greater using Package Manager 
- Open `Unity.ProBuilder.AddOns.Editor.asmdef` in a text editor
- Add the following dependencies to the "references" array:
    - "Autodesk.Fbx"
    - "Unity.Formats.Fbx.Editor" 
- Open the `Project Settings` window,
- Add `PROBUILDER_FBX_2018_3` in the `Scripting Define Symbols` field.

