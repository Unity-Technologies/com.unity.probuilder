# Stairs
You can create straight stairs, curved stairs, long stairs, stairs with a lot of steps, wide stairs, and stairs without side polygons.

![Stairs shapes](images/shape-tool_stair.png)

You can customize the shape of your stairs with these shape-specific properties:

<!--


| **Property:** | **Description:** |
|:-- |:-- |
| __Steps Generation__ | Select how you want ProBuilder to build steps:<br /><br />- Select the **Height** method if you want ProBuilder to generate a predictable height for each step in the staircase. This means that if you increase the height of the overall size of the staircase, the number of of steps increases.<br />- Select the **Count** method if you want ProBuilder to generate a specific number of steps, regardless of any changes in the size of the staircase. This means that if you increase the height of the overall size of the stairs, each step becomes higher.<br /><br />The default value is the **Count** method. |
| __Steps Height__ | Set the fixed height of each step on the stairs. The default value is 0.2. <br /><br />This property is only available when the **Steps Generation** method is set to **Height**. |
| __Homogeneous Steps__ | Enable this option to force every step to be the exactly the same height. This is enabled by default.<br /><br />This property is only available when the **Steps Generation** method is set to **Height**. |
| __Steps Count__ | Set the fixed number of steps that the stairs always has. The default value is 10. Valid values range from 1 to 256.<br /><br />This property is only available when the **Steps Generation** method is set to **Count**. |
| __Sides__ | Enable this option to draw polygons on the sides of the stairs. This is enabled by default. You can disable this option if the sides of your stairs are not visible to the camera (for example, if your stairs are built into a wall). |
| __Circumference__ | Set the degree of curvature on the stairs in degrees, where 0 makes straight stairs and 360 makes stairs in a complete circle. Keep in mind that you might need to increase the number of stairs to compensate as you increase this value. The default value is 0. Valid values range from 0 to 360. |

-->



<table>
<thead>
<tr>
<th colspan="2"><strong>Property:</strong></th>
<th><strong>Description:</strong></th>
</tr>
</thead>
<tbody>
<tr>
<td colspan="2"><strong>Steps Generation</strong></td>
<td>Select whether you want ProBuilder to build the same number of steps regardless of how the size of the stairs changes (the default) or make each step the same height and automatically adapt the number of steps to match the stairs size.</td>
</tr>
<tr>
<td></td>
<td><strong>Height</strong></td>
<td>Select this method if you want ProBuilder to generate a predictable height for each step in the staircase. This means that if you increase the height of the overall size of the staircase, the number of steps increases.</td>
</tr>
<tr>
<td></td>
<td><strong>Count</strong></td>
<td>Select this method if you want ProBuilder to generate a specific number of steps, regardless of any changes in the size of the staircase. This means that if you increase the height of the overall size of the stairs, each step becomes higher. This is the default value.</td>
</tr>
<tr>
<td colspan="2"><strong>Steps Height</strong></td>
<td>Set the fixed height of each step on the stairs. The default value is 0.2. <br /><br />This property is only available when the <strong>Steps Generation</strong> method is set to <strong>Height</strong>.</td>
</tr>
<tr>
<td colspan="2"><strong>Homogeneous Steps</strong></td>
<td>Enable this option to force every step to be the exactly the same height. This is enabled by default.<br /><br />If disabled, the height of the last step is smaller than the others depending on the remaining height.<br /><br />This property is only available when the <strong>Steps Generation</strong> method is set to <strong>Height</strong>.</td>
</tr>
<tr>
<td colspan="2"><strong>Steps Count</strong></td>
<td>Set the fixed number of steps that the stairs always has. The default value is 10. Valid values range from 1 to 256.<br /><br />This property is only available when the <strong>Steps Generation</strong> method is set to <strong>Count</strong>.</td>
</tr>
<tr>
<td colspan="2"><strong>Sides</strong></td>
<td>Enable this option to draw polygons on the sides of the stairs. This is enabled by default. You can disable this option if the sides of your stairs are not visible to the camera (for example, if your stairs are built into a wall).</td>
</tr>
<tr>
<td colspan="2"><strong>Circumference</strong></td>
<td>Set the degree of curvature on the stairs in degrees, where 0 makes straight stairs and 360 makes stairs in a complete circle. Keep in mind that you might need to increase the number of stairs to compensate as you increase this value. The default value is 0. Valid values range from -360 (full turn to the left) to 360 (full turn to the right).</td>
</tr>
</tbody>
</table>


