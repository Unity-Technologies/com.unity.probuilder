# Enabling FBX Integration

- Install *FBX Exporter* 2.0.1 or newer via Package Manager.
- Add the following dependencies to `Unity.ProBuilder.AddOns.Editor.asmdef` in the "references" array:
	- "Autodesk.Fbx"
    - "Unity.Formats.Fbx.Editor"
- Open *Project Settings* and add `PROBUILDER_FBX_2_0_1_OR_NEWER` to the *Scripting Define Symbols* field.
