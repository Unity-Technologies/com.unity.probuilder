# ![Entity icon](images/icons/Entity_Trigger.png) Entity type tools

ProBuilder provides some default *entity* behaviors. These are simply MonoBehaviours that provide some commonly used functionality.

> ***Note:*** These tools are only available from the [The ProBuilder toolbar](toolbar.md) in [text mode](toolbar.md#buttonmode).



## Set Trigger

Assigns the **Trigger Behaviour** script to selected objects, which does the following:

- If no collider is present, adds a [MeshCollider](https://docs.unity3d.com/Manual/class-MeshCollider.md) component.
- If the collider is a MeshCollider, enables its **Convex** property.
- Enables the collider's **isTrigger** property.
- Sets the [MeshRenderer](https://docs.unity3d.com/Manual/class-MeshRenderer.md) Material to ProBuilder's **Trigger** Material.
- Automatically disables the MeshRenderer when entering **Play Mode** or building. 

> ***Tip:*** You can also use the **T** hotkey to set the selected object(s) as a trigger. If you want to change this hotkey assignment, you can modify it in the [Shortcut Settings](preferences.md#shortcuts) section of the ProBuilder Preferences window.



<a name="Collider"></a>

## Set Collider

Assigns the **Collider Behaviour** script to selected objects, which does the following:

- If no collider is present, adds a [MeshCollider](https://docs.unity3d.com/Manual/class-MeshCollider.md) component.
- Sets the [MeshRenderer](https://docs.unity3d.com/Manual/class-MeshRenderer.md) Material to ProBuilder's **Collider** Material.
- Automatically disables the MeshRenderer when entering **Play Mode** or building.

> ***Tip:*** You can also use the **C** hotkey to set the selected object(s) as a collider. If you want to change this hotkey assignment, you can modify it in the [Shortcut Settings](preferences.md#shortcuts) section of the ProBuilder Preferences window.

