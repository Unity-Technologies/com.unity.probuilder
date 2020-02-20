using UnityEditor;
using UnityEngine;

namespace SlideyDebuggey
{
    static class Slider1D
    {
        private static Vector2 s_StartMousePosition, s_CurrentMousePosition;
        private static Vector3 s_StartPosition, s_HandleOffset;
        private static float s_StartHandleSize;

        internal static Vector3 Do(int id, Vector3 position, Vector3 offset, Vector3 handleDirection, Vector3 slideDirection, float size, Handles.CapFunction capFunction, float snap)
        {
            Event evt = Event.current;
            var eventType = evt.GetTypeForControl(id);
            switch (eventType)
            {
                case EventType.Layout:
                case EventType.MouseMove:
                    if (capFunction != null)
                        capFunction(id, position + offset, Quaternion.LookRotation(handleDirection), size, eventType);
                    else
                        HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(position + offset, size * .2f));
                    break;

                case EventType.MouseDown:
                    // am I closest to the thingy?
                    if (HandleUtility.nearestControl == id && evt.button == 0 && GUIUtility.hotControl == 0 && !evt.alt)
                    {
                        GUIUtility.hotControl = id; // Grab mouse focus
                        s_CurrentMousePosition = s_StartMousePosition = evt.mousePosition;
                        s_StartPosition = position;
                        s_StartHandleSize = HandleUtility.GetHandleSize(position);
                        s_HandleOffset = position - HandleUtility2.CalcPositionWithConstraint(Camera.current, evt.mousePosition, position, slideDirection);
                        evt.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }

                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        switch (HandleUtility2.projectionMethod)
                        {
                            case ProjectionMethod.Legacy:
                                s_CurrentMousePosition += evt.delta;
                                float dist = HandleUtility.CalcLineTranslation(s_StartMousePosition, s_CurrentMousePosition, s_StartPosition, slideDirection);
                                dist = Handles.SnapValue(dist, snap);
                                Vector3 worldDirection = Handles.matrix.MultiplyVector(slideDirection);
                                Vector3 worldPosition = Handles.matrix.MultiplyPoint(s_StartPosition) + worldDirection * dist;
                                position = Handles.inverseMatrix.MultiplyPoint(worldPosition);
                                break;

                            default:
                                position = HandleUtility2.CalcPositionWithConstraint(Camera.current, evt.mousePosition, s_StartPosition, slideDirection);
                                var handleOffset = s_HandleOffset * (HandleUtility.GetHandleSize(position) / s_StartHandleSize);
                                position = handleOffset + position;
                                break;
                        }

                        GUI.changed = true;
                        evt.Use();
                    }

                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                    }

                    break;

                case EventType.Repaint:
                    Color temp = Color.white;

                    if (id == GUIUtility.hotControl)
                    {
                        temp = Handles.color;
                        Handles.color = Handles.selectedColor;
                    }
                    else if (id == HandleUtility.nearestControl && GUIUtility.hotControl == 0 && !evt.alt)
                    {
                        temp = Handles.color;
                        Handles.color = Handles.preselectionColor;
                    }

                    capFunction(id, position + offset, Quaternion.LookRotation(handleDirection), size, EventType.Repaint);

                    if (id == GUIUtility.hotControl || id == HandleUtility.nearestControl && GUIUtility.hotControl == 0)
                        Handles.color = temp;
                    break;
            }

            return position;
        }
    }
}
