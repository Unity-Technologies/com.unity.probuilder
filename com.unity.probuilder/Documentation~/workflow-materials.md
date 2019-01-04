# Creating and Applying Materials

The [Material Editor](material-tools.md) allows you to easily manage Materials you are using in the Scene, including: 

* [Setting a quick Material](material-tools.md#qset) and then using a hotkey to [apply it](material-tools.md#qapply) to the selected face(s).
* [Assigning up to 10 Materials](material-tools.md#set) to hotkeys and buttons in the editor so you can easily [apply multiple Materials to faces](material-tools.md#apply) without having to use the Project view to find them. 
* [Defining extra Material slots](material-tools.md#wells) in your Palette so that you can manage extra Materials at once.
* [Saving your Material arrangement](material-tools.md#save) in a new Material Palette so that you can find it easily and re-use it in other Scenes and Projects.
* [Loading](material-tools.md#load) a previously saved Material Palette into the Material Editor.

![Material Tools](images/MaterialTools_WithExample.png)

The Materials you use in the Material Editor are standard Unity Materials. You can create your own standard Unity Material by following the instructions in [Creating and Using Materials](https://docs.unity3d.com/Manual/Materials.html) in the Unity manual. For Texture Materials, follow the instructions in [Creating a Texture Material](#texmat).



<a name="texmat"></a>

## Creating a Texture Material

Before creating the Material, make sure you have a 2-dimensional image file (BMP, JPG, PNG, TIF etc.). Then follow these instructions:

1. Right-click in the Project view where you want to create the Material file and select **Create** > **Material** from the context menu.

  Alternatively, you could use the main menu (**Assets** > **Create** > **Material**).

2. Name the Material file. For example, you could describe the look or use of the Texture, such as "BrickWall" or "CheckeredTile".

  The Shader properties appear in the Inspector.

3. [Set a reference](https://docs.unity3d.com/Manual/EditingValueProperties.html) to the Texture file you want to use in the **Albedo** property.

	![Shader properties for Texture Materials](images/texmat-create.png)


Now that you have a Texture Material, you can [set it as the quick Material](material-tools.md#qset), [add it to your Material Palette](material-tools.md#palette), and [apply it to a whole object or only specific faces](material-tools.md#apply).

