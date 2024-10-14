# Select Material

To select all the faces that use the same material as your current selection, use the **Select Material** action. For example, you might want to select all faces using a material so that you can replace that material. 

This action is only available in [face mode](modes.md).

![On the left, a single face with a brick material is selected. On the right, all the faces on the same object that use the brick material are selected.](images/Example_SelectByMaterial.png)

To select faces by material:

1. In the **Tools** overlay, select the **ProBuilder** context.
1. In the **Tool Settings** overlay, select the **Face** edit mode.
1. Select a face. To select more than one material, hold **Shift** and select more faces.
1. Do one of the following:
    * Right-click (macOS: **Ctrl**+click) and click **Select** > **ProBuilder Select** > **Select Material**.
    * From the main menu, select **Tools** > **ProBuilder** > **Selection** > **Select Material**.
1. The **Select Material** overlay opens and the selection is expanded to match the default settings.
    By default, ProBuilder selects matching faces from all GameObjects in the scene. To limit your selection to the current GameObject, in the **Select Material Options** overlay, select **Current Selection**.

## Select Material Options

To limit the face selection to the current GameObject, select **Current Selection**.

When disabled, ProBuilder selects every face that has a matching material on any GameObject in the scene. 

