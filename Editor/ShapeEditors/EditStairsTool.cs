using System;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Stairs", typeof(Stairs))]
    public class EditStairsTool : EditShapeTool<Stairs>
    {
        protected override void DoShapeGUI(Stairs shape, Matrix4x4 l2w, Bounds bounds)
        {
            base.DoShapeGUI(shape, l2w, bounds);
            
            var center = l2w.inverse.MultiplyPoint3x4(bounds.center);
            var extents = bounds.extents;
            var stepCountSliderOffset = .1f;

            var start = center + new Vector3(extents.x, -extents.y + stepCountSliderOffset, -extents.z);
            var end = center + new Vector3(extents.x, extents.y + stepCountSliderOffset, extents.z);

            using (new Handles.DrawingScope(l2w))
            {
                EditorGUI.BeginChangeCheck();
                shape.steps = (int) WorldSpaceSlider(start, end, new Vector2(2, 32), shape.steps);
                if (EditorGUI.EndChangeCheck())
                    RebuildShape(shape, bounds, shape.transform.rotation);
            }
        }

        public static float WorldSpaceSlider(Vector3 start, Vector3 end, Vector2 range, float value)
        {
            var direction = (end - start).normalized;

            var x = Mathf.Abs(range.x);
            var y = Mathf.Abs(range.y);

            var sliderMagnitude = Vector3.Distance(start, end);
            var valueMagnitude = x > y ? x - y : y - x;
            var valuePercentage = (value - x) / valueMagnitude;
            var sliderPosition = start + direction * sliderMagnitude * valuePercentage;

            Handles.DrawLine(start, end);

            sliderPosition = Handles.Slider(sliderPosition, direction, HandleUtility.GetHandleSize(sliderPosition) * .15f, Handles.CubeHandleCap, 0f);
            valuePercentage = Vector3.Distance(sliderPosition, start) / sliderMagnitude;

            return Mathf.Clamp(x + (valuePercentage * valueMagnitude), x, y);
        }
    }
}
