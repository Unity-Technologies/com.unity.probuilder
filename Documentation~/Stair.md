# Stairs

You can create straight stairs, curved stairs, long stairs, stairs with a lot of steps, wide stairs, and stairs without side polygons.

![Stairs shapes](images/shape-tool_stair.png)

There are two ways to generate stairs:

* **Height**: ProBuilder generates a predictable height for each step in the staircase. This means that if you increase the height of the overall size of the staircase, the number of steps increases.
* **Count**: ProBuilder generates a specific number of steps, regardless of any changes in the size of the staircase. This means that if you increase the height of the overall size of the stairs, each step becomes higher. This is the default value.

### Stair properties

| Property | Description |
| --- | --- |
| **Steps Height** | Set the fixed height of each step on the stairs. The default value is `0.2`. <br /><br />This property is only available when the **Steps Generation** method is set to **Height**. |
| **Homogeneous Steps** | Force every step to be exactly the same height. This is enabled by default.<br /><br />If disabled, the height of the last step is smaller than the others depending on the remaining height.<br /><br />This property is only available when the **Steps Generation** method is set to **Height**. |
| **Steps Count** | Set the fixed number of steps. The default value is `10`. Valid values range from `1` to `256`.<br /><br />This property is only available when the **Steps Generation** method is set to **Count**. |
| **Sides** | Draw polygons on the sides of the stairs. This is enabled by default. You can disable this option if the sides of your stairs are not visible to the camera (for example, if your stairs are built into a wall). |
| **Circumference** | Set the degree of curvature on the stairs in degrees, where `0` makes straight stairs and `360` makes stairs in a complete circle. Remember that you might need to increase the number of stairs to compensate as you increase this value. The default value is `0`. Valid values range from `-360` (full turn to the left) to `360` (full turn to the right). |


