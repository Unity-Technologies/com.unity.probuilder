# ![Entity icon](images/icons/Entity_Trigger.png) Entity type tools

ProBuilder provides some default *entity* behaviors. These are MonoBehaviours that provide some commonly used functionality.

> **Note:** These tools are only available from the [The ProBuilder toolbar](toolbar.md) in [text mode](toolbar.md#buttonmode).



## Set Trigger

The __Set Trigger__ tool assigns the **Trigger Behaviour** script to selected objects, which does the following:

- If no collider is present, the **Trigger Behaviour** script adds a [MeshCollider](https://docs.unity3d.com/Manual/class-MeshCollider.md) component.
- If the collider is a Mesh Collider, the **Trigger Behaviour** script enables its **Convex** property.
- The **Trigger Behaviour** script enables the collider's **isTrigger** property.
- The **Trigger Behaviour** script sets the [Mesh Renderer](https://docs.unity3d.com/Manual/class-MeshRenderer.md) Material to ProBuilder's **Trigger** Material.
- The **Trigger Behaviour** script automatically disables the Mesh Renderer when you enter **Play Mode** or build your project. 

> **Tip:** You can also use the **T** hotkey to set the selected object(s) as a trigger. If you want to change this hotkey assignment, you can modify it in the ProBuilder Preferences [Shortcut Settings](preferences.md#shortcuts).



<a name="Collider"></a>

## Set Collider

The __Set Collider__ tool assigns the **Collider Behaviour** script to selected objects, which does the following:

- If no collider is present, the **Collider Behaviour** script adds a [MeshCollider](https://docs.unity3d.com/Manual/class-MeshCollider.md) component.
- The **Collider Behaviour** script sets the [MeshRenderer](https://docs.unity3d.com/Manual/class-MeshRenderer.md) Material to ProBuilder's **Collider** Material.
- The **Collider Behaviour** script automatically disables the MeshRenderer when you enter **Play Mode** or build your project.

> **Tip:** You can also use the **C** hotkey to set the selected object(s) as a collider. If you want to change this hotkey assignment, you can modify it in the ProBuilder Preferences [Shortcut Settings](preferences.md#shortcuts).

