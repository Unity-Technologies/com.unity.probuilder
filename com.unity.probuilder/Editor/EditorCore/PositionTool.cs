//#define DEBUG_HANDLES

using System.Linq;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    abstract class PositionTool : VertexManipulationTool
    {
        [UserSetting("General", "Show Handle Info", "Toggle the display of information of move, rotate, and scale deltas.")]
        static Pref<bool> s_ShowHandleInfo = new Pref<bool>("editor.showHandleDelta", false, SettingsScope.User);

        protected bool showHandleInfo
        {
            get { return s_ShowHandleInfo.value; }
        }

        protected void DrawDeltaInfo(string text)
        {
            Handles.BeginGUI();
            var view = SceneView.lastActiveSceneView;
            var gc = UI.EditorGUIUtility.TempContent(text);

            // scene view screen.height includes the tab and toolbar
            var toolbarHeight = UI.EditorStyles.sceneTextBox.CalcHeight(gc, Screen.width);
            var contentSize = UI.EditorStyles.sceneTextBox.CalcSize(gc);
            Rect handleTransformInfoRect = new Rect(
                view.position.width - (contentSize.x + 8),
                view.position.height - (contentSize.y + 8 + toolbarHeight),
                contentSize.x,
                contentSize.y);

            GUI.Label(handleTransformInfoRect, gc, UI.EditorStyles.sceneTextBox);
            Handles.EndGUI();
        }

        const bool k_CollectCoincidentVertices = true;

#if APPLY_POSITION_TO_SPACE_GIZMO
        Matrix4x4 m_CurrentDelta = Matrix4x4.identity;
#endif

        protected class MeshAndPositions : MeshAndElementSelection
        {
            Vector3[] m_Positions;

            public Vector3[] positions
            {
                get { return m_Positions; }
            }

            public MeshAndPositions(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation) : base(mesh, pivot, orientation, k_CollectCoincidentVertices)
            {
                m_Positions = mesh.positions.ToArray();

                var l2w = mesh.transform.localToWorldMatrix;

                for (int i = 0, c = m_Positions.Length; i < c; i++)
                    m_Positions[i] = l2w.MultiplyPoint3x4(m_Positions[i]);
            }
        }

        internal override MeshAndElementSelection GetElementSelection(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation)
        {
            return new MeshAndPositions(mesh, pivot, orientation);
        }

        internal Matrix4x4 GetPostApplyMatrix(ElementGroup group)
        {
            switch (pivotPoint)
            {
                case PivotPoint.Center:
                    return Matrix4x4.TRS(handlePositionOrigin, handleRotationOrigin, Vector3.one);

                case PivotPoint.ActiveElement:
                    return Matrix4x4.TRS(handlePositionOrigin, handleRotationOrigin, Vector3.one);

                case PivotPoint.IndividualOrigins:
                    return Matrix4x4.TRS(group.position, group.rotation, Vector3.one);

                default:
                    return Matrix4x4.identity;
            }
        }

        protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
        {
#if DEBUG_HANDLES
            if (isEditing && currentEvent.type == EventType.Repaint)
            {
                foreach (var key in elementSelection)
                {
                    if (!(key is MeshAndPositions))
                        break;

                    foreach (var group in key.elementGroups)
                    {
                        var positions = ((MeshAndPositions)key).positions;
                        var postApplyMatrix = GetPostApplyMatrix(group);
                        var preApplyMatrix = postApplyMatrix.inverse;

                        using (var faceDrawer = new EditorMeshHandles.TriangleDrawingScope(Color.cyan,
                                       UnityEngine.Rendering.CompareFunction.Always))
                        {
                            foreach (var face in key.mesh.GetSelectedFaces())
                            {
                                var indices = face.indexesInternal;

                                for (int i = 0, c = indices.Length; i < c; i += 3)
                                {
                                    faceDrawer.Draw(
                                        preApplyMatrix.MultiplyPoint3x4(positions[indices[i]]),
                                        preApplyMatrix.MultiplyPoint3x4(positions[indices[i + 1]]),
                                        preApplyMatrix.MultiplyPoint3x4(positions[indices[i + 2]])
                                        );
                                }
                            }
                        }
                    }
                }
            }
#endif
        }

        protected void Apply(Matrix4x4 delta)
        {
#if APPLY_POSITION_TO_SPACE_GIZMO
            m_CurrentDelta.SetColumn(3, delta.GetColumn(3));
#endif

            foreach (var key in elementSelection)
            {
                if (!(key is MeshAndPositions))
                    continue;

                var kvp = (MeshAndPositions)key;
                var mesh = kvp.mesh;
                var worldToLocal = mesh.transform.worldToLocalMatrix;
                var origins = kvp.positions;
                var positions = mesh.positionsInternal;

                foreach (var group in kvp.elementGroups)
                {
                    var postApplyMatrix = GetPostApplyMatrix(group);
                    var preApplyMatrix = postApplyMatrix.inverse;

                    foreach (var index in group.indices)
                    {
                        positions[index] = worldToLocal.MultiplyPoint3x4(
                                postApplyMatrix.MultiplyPoint3x4(
                                    delta.MultiplyPoint3x4(preApplyMatrix.MultiplyPoint3x4(origins[index]))));
                    }
                }

                mesh.mesh.vertices = positions;
                mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
                mesh.Refresh(RefreshMask.Normals);
            }

            ProBuilderEditor.Refresh(false);
        }
    }
}
