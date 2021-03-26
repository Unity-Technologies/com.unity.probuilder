# ![Select by Material icon](images/icons/Selection_SelectByMaterial.png) Select by Material

The __Select by Material__ action selects all faces on this object that have the same Material as the selected face(s). You can also extend the selection to other GameObjects if you disable the **Current Selection** option.

![Select all faces with brick Material on the Mesh](images/Example_SelectByMaterial.png)

This action is useful if you want to replace all Materials on a complex object. It is only available in [face mode](modes.md).

> **Tip:** You can also access this action from the ProBuilder menu (**Tools** > **ProBuilder** > **Selection** > **Select Material**).



## Select by Material Options

Enable the **Current Selection** option to extend the selection to other faces on the currently selected GameObject(s) only. By default, this option is disabled. 

When disabled, ProBuilder selects every face that has a matching Material on any GameObject in the scene. This is particularly useful if you want to replace this Material with another on every GameObject in the scene at once.

![Grow Selection Options](images/Selection_SelectByMaterial_props.png)
