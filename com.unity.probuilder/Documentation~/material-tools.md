# ![Material Tools Icon](images/icons/Panel_Materials.png) Material Editor window

Use the **Material Editor** window to apply Materials to objects or faces.

![The Material Editor window](images/material-tools.png)

![A](images/LetterCircle_A.png) Use the [Quick Material](#quick) section to set up a Material to apply with a button or a hotkey.

![B](images/LetterCircle_B.png) Use the [Material Palette](#palette) section to designate several Materials to use with buttons or hotkeys. You can also save and load new Palettes, and add more slots to use for Materials.

To open this window, click the **Material Editor** button ( ![Material Tools icon](images/icons/Panel_Materials.png) ) from the [ProBuilder toolbar](toolbar.md).

> ***Tip:*** You can also access this tool from the ProBuilder menu (**Tools** > **ProBuilder** > **Editors** > **Open Material Editor**).

For an overview of working with Materials and texture mapping, see [Materials, Shaders, Textures, and UVs](workflow-texture-mapping.md).



<a name="quick"></a>

## Quick Material

The **Quick Material** section allows you to specify a Material to apply with either the hotkey combination or the **Apply** button. 

![Quick Material section](images/material-tools-quick.png)

![Item A](images/LetterCircle_A.png) You can [set a reference to the Material](#qset) to use as the *quick* Material. 

![Item B](images/LetterCircle_B.png) ProBuilder shows a preview when you set a quick Material.

> ***Note:*** If you are working in Windows, this preview may not appear.

![Item C](images/LetterCircle_C.png) With a quick Material defined, any time you click **Apply**, ProBuilder [applies the quick Material](#qapply) to the selected faces. Alternatively, you can hold down **Ctrl/Cmd+Shift** and click on a face to apply the quick Material to the clicked face.

![Item D](images/LetterCircle_D.png) Click **Match Selection** to change the quick Material to match the Material already applied on the selected face.



<a name="qset"></a>

### Setting a quick Material

To set a quick Material, set a reference to a Material Asset you want to use as the quick Material. You can drag in the Asset from the Project view or use the object picker beside the reference property. 

If there's a face on a GameObject in the Scene that is already using the Material you want to set as quick, select that face and click the **Match Selection** button. 

<a name="qapply"></a>

### Applying the quick Material

To apply the quick Material:

1. Select the object or face(s) you want to apply the quick Material to.
2. Click the **Apply** button or use the **Ctrl/Cmd+Shift+Click** hotkey combination.



<a name="palette"></a>

## Material Palette

A Material Palette is a convenient way to keep several Materials ready to apply, either with the Material Editor or by using hotkey combinations. 

You can set up to 10 different Materials on the Palette, or expand the slots in your Palette if you need more.

You can also save your Palette as a custom Palette and use it again, loading it in other Scenes or sessions. 

![Material Palette section](images/material-tools-palette.png)

![Item A](images/LetterCircle_A.png) Create a new Material Palette or select a saved Palette to load from the Palette drop-down menu. 

![Item B](images/LetterCircle_B.png) Set a reference to the Material Palette you want to load.

![Item C](images/LetterCircle_C.png) Use one of the buttons to apply the associated Material to the selected face(s). Alternatively, you can use the hotkey displayed on the button (**Alt/Opt+#**).

![Item D](images/LetterCircle_D.png) Set a reference to the Material you want to use in this slot or "well" of the Material Palette.

![Item E](images/LetterCircle_E.png) To delete an extra slot or "well", click the red square on the right edge of the button.

![Item F](images/LetterCircle_F.png) Click **Add** to add a new slot or "well" to the Material Palette. 



<a name="set"></a>

### Defining Materials on a Palette 

You can set Materials on your Palette by setting a reference to a Material Asset you want to use in a specific Material slot. You can drag in the Asset from the Project view or use the object picker beside the reference property. 

<a name="apply"></a>

### Applying Material from the Palette

To apply a specific Material from the Palette:

1. Select the object or face(s) you want to apply the Material to.
2. Click the button to the left of the Material you want to apply. For example, if you want to apply the Material in the third slot, click the button that displays **Alt + 3**. 

> ***Tip:*** You can find the number of the Material preset on each slot's button. Then you can use that number to apply its Material directly using either of these methods: 
>
> * Use the **Alt/Opt+#** hotkey. 
> * Select the Material preset from the ProBuilder menu (**Tools** > **ProBuilder** > **Materials** > **Apply Material Preset #**).



<a name="wells"></a>

### Defining your own Material slots in your Palette

By default, the Material Palette provides 10 slots (numbered 1 to 0) for your Materials. However, if you need to add more slots to your Palette, you can add any number of extra slots using this procedure:

1. Click the **Add** button at the bottom of the **Material Editor** window.

  A new slot appears with an **Apply** button on the left and the Material picker on the right.

2. Pick your new Material to use in the new slot.

> ***Note:*** You can't set up new hotkeys for the new slots but you can click their buttons in the Material Editor to apply them.



To remove any extra slots from your Palette:

* Click the red square on the right side of the **Apply** button.



<a name="save"></a>

### Saving a custom Palette

To save the current Palette to an Asset file:

1. Select **New Material Palette** from the **Material Palette** drop-down menu.

	A new file with the default name of `Material Palette.asset` appears under the `Assets` folder in the **Project** view.

2. (Optional). Change the filename to something that helps you identify it for future use.

> ***Warning:*** As soon as you make a change to your Material Palette, it is saved automatically to the Palette.



<a name="load"></a>

### Loading a custom Palette

All Material Palette Asset files in the current Project appear as menu items in the **Material Palette** drop-down menu in the **Material Editor** window.

To load a saved Material Palette from file:

* Select the saved palette by name from the **Material Palette** drop-down menu.


