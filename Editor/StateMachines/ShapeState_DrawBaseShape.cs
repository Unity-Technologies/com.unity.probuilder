using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_DrawBaseShape : ShapeState
    {
        bool m_IsDragging = false;

        protected override void InitState()
        {
            m_IsDragging = false;
        }

        public override ShapeState DoState(Event evt)
        {
            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Delete:
                    case KeyCode.Escape:
                        return ResetState();
                }
            }

            if(evt.type == EventType.Repaint && m_IsDragging)
                tool.DrawBoundingBox();

            if(evt.isMouse)
            {
                switch(evt.type)
                {
                    case EventType.MouseDrag:
                        if(evt.button == 0)
                        {
                            m_IsDragging = true;

                            if(tool.m_DuplicateGO != null)
                                Object.DestroyImmediate(tool.m_DuplicateGO);

                            Drag(evt.mousePosition);
                        }
                        break;

                    case EventType.MouseMove:
                        if(evt.button == 0 && m_IsDragging)
                            Drag(evt.mousePosition);
                        break;

                    case EventType.MouseUp:
                        if(evt.button == 0)
                        {
                            if(!m_IsDragging && evt.shift)
                            {
                                CreateLastShape();
                                return ResetState();
                            }

                            if(Vector3.Distance(tool.m_BB_OppositeCorner, tool.m_BB_Origin) < .1f)
                                return ResetState();

                            return NextState();
                        }
                        break;
                }
            }

            return this;
        }

        void Drag(Vector2 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            float distance;

            if(tool.m_Plane.Raycast(ray, out distance))
                UpdateShapeBase(ray, distance);
        }

        void UpdateShapeBase(Ray ray, float distance)
        {
            var deltaPoint = ray.GetPoint(distance) - tool.m_BB_Origin;
            deltaPoint = Quaternion.Inverse(tool.m_PlaneRotation) * deltaPoint;
            deltaPoint = tool.GetPoint(deltaPoint, Event.current.control);
            tool.m_BB_OppositeCorner = tool.m_PlaneRotation * deltaPoint + tool.m_BB_Origin;
            tool.m_BB_HeightCorner = tool.m_BB_OppositeCorner;
            tool.RebuildShape();
        }

        public void CreateLastShape()
        {
            tool.handleSelectionChange = false;
            var shape = ShapeFactory.Instantiate(DrawShapeTool.activeShapeType, (PivotLocation)DrawShapeTool.s_LastPivotLocation.value).GetComponent<ProBuilderShape>();
            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, $"Create Shape");
            EditorUtility.InitObject(shape.mesh);
            DrawShapeTool.ApplyPrefsSettings(shape);

            EditorShapeUtility.CopyLastParams(shape.shape, shape.shape.GetType());
            shape.Rebuild(tool.m_Bounds, tool.m_PlaneRotation, tool.m_BB_Origin);

            //Finish initializing object and collider once it's completed
            ProBuilderEditor.Refresh(false);

            tool.m_ProBuilderShape = null;
            tool.m_LastShapeCreated = shape;
            Object.DestroyImmediate(tool.m_DuplicateGO);
            Selection.activeGameObject = shape.gameObject;
        }
    }
}
