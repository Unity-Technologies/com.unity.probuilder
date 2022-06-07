using UnityEngine;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_FinalizeShape : ShapeState
    {
        protected override void EndState()
        {
            tool.RebuildShape();
            UndoUtility.RegisterCreatedObjectUndo(tool.currentShapeInOverlay.gameObject, "Draw Shape");
            tool.m_LastShapeCreated = tool.m_ProBuilderShape;
            tool.m_ProBuilderShape = null;
        }

        public override ShapeState DoState(Event evt)
        {
            return NextState();
        }
    }
}
