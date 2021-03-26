# ![Entity icon](images/icons/Entity_Trigger.png) Entity type actions

ProBuilder provides some default "entity" behaviors. These are MonoBehaviours that provide some commonly used functionality.

> **Note:** These actions are not available from the [The ProBuilder toolbar](toolbar.md) in [icon mode](toolbar.md#buttonmode).



## Set Trigger

The __Set Trigger__ action assigns the **Trigger Behaviour** script to selected objects, which does the following:

- If no collider is present, the **Trigger Behaviour** script adds a [MeshCollider](https://docs.unity3d.com/Manual/class-MeshCollider.md) component.
- If the collider is a Mesh Collider, the **Trigger Behaviour** script enables its **Convex** property.
- The **Trigger Behaviour** script enables the collider's **isTrigger** property.
- The **Trigger Behaviour** script sets the [Mesh Renderer](https://docs.unity3d.com/Manual/class-MeshRenderer.md) Material to ProBuilder's **Trigger** Material.
- The **Trigger Behaviour** script automatically disables the Mesh Renderer when you enter **Play Mode** or build your project.

> **Tip:** You can also use the **T** shortcut to set the selected object(s) as a trigger, or from the ProBuilder menu (**Tools** > **ProBuilder** > **Object** > **Set Trigger**). 



<a name="Collider"></a>

## Set Collider

The __Set Collider__ action assigns the **Collider Behaviour** script to selected objects, which does the following:

- If no collider is present, the **Collider Behaviour** script adds a [MeshCollider](https://docs.unity3d.com/Manual/class-MeshCollider.md) component.
- The **Collider Behaviour** script sets the [MeshRenderer](https://docs.unity3d.com/Manual/class-MeshRenderer.md) Material to ProBuilder's **Collider** Material.
- The **Collider Behaviour** script automatically disables the MeshRenderer when you enter **Play Mode** or build your project.

> **Tip:** You can also launch this action from the ProBuilder menu (**Tools** > **ProBuilder** > **Object** > **Set Collider**).
