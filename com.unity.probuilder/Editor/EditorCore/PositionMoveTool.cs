using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class PositionMoveTool : PositionTool
    {
        const float k_CardinalAxisError = .001f;
        Vector3 m_HandlePosition;
        bool m_SnapInWorldCoordinates;
        Vector3 m_WorldSnapDirection;
        Vector3 m_WorldSnapMask;

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
        Vector3 m_IndividualOriginDirection;
        bool m_DirectionOriginInitialized;
#endif

        protected override void OnToolEngaged()
        {
            m_SnapInWorldCoordinates = false;
            m_WorldSnapMask = new Vector3Mask(0x0);
#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
            m_DirectionOriginInitialized = false;
#endif
        }

        protected override void DoTool(Vector3 handlePosition, Quaternion handleRotation)
        {
            base.DoTool(handlePosition, handleRotation);

            if (!isEditing)
                m_HandlePosition = handlePosition;

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
            if (isEditing)
                DrawSelectionOriginGizmos();
#endif

            EditorGUI.BeginChangeCheck();

            m_HandlePosition = Handles.PositionHandle(m_HandlePosition, handleRotation);

            if (EditorGUI.EndChangeCheck())
            {
                if (!isEditing)
                    BeginEdit("Translate Selection");

                var delta = m_HandlePosition - handlePositionOrigin;

                if (vertexDragging)
                {
                    Vector3 nearest;

                    if (FindNearestVertex(currentEvent.mousePosition, out nearest))
                    {
                        var unrotated = handleRotationOriginInverse * delta;
                        var dir = new Vector3Mask(unrotated, k_CardinalAxisError);

                        if (dir.active == 1)
                        {
                            var rot_dir = handleRotationOrigin * dir * 10000f;

                            m_HandlePosition = HandleUtility.ProjectPointLine(nearest,
                                    handlePositionOrigin + rot_dir,
                                    handlePositionOrigin - rot_dir);

                            delta = m_HandlePosition - handlePositionOrigin;
                        }
                    }
                }
                else if (progridsSnapEnabled)
                {
                    var localDir = handleRotationOriginInverse * delta;
                    m_WorldSnapDirection = delta.normalized;

                    if (!m_SnapInWorldCoordinates && (Math.IsCardinalAxis(delta) || !Math.IsCardinalAxis(localDir)))
                        m_SnapInWorldCoordinates = true;

                    if (m_SnapInWorldCoordinates)
                    {
                        m_WorldSnapMask |= new Vector3Mask(m_WorldSnapDirection, k_CardinalAxisError);
                        m_HandlePosition = Snapping.SnapValue(m_HandlePosition, m_WorldSnapMask * progridsSnapValue);
                        delta = m_HandlePosition - handlePositionOrigin;
                    }
                    else
                    {
                        var travel = delta.magnitude;
                        delta = m_WorldSnapDirection * Snapping.SnapValue(travel, progridsSnapValue);
                        m_HandlePosition = handlePositionOrigin + delta;
                    }
                }

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
                if (pivotPoint == PivotPoint.IndividualOrigins && !m_DirectionOriginInitialized)
                {
                    var mask = new Vector3Mask(handleRotationOriginInverse * delta, k_CardinalAxisError);

                    if (mask.active > 0)
                    {
                        m_IndividualOriginDirection = mask;
                        m_DirectionOriginInitialized = true;
                    }
                }
#endif

                ApplyTranslation(handleRotationOriginInverse * delta);
            }
        }

        void ApplyTranslation(Vector3 translation)
        {
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
                        // res = Group pre-apply matrix * world vertex position
                        // res += translation
                        // res = Group post-apply matrix * res
                        // positions[i] = mesh.worldToLocal * res
                        positions[index] = worldToLocal.MultiplyPoint3x4(
                                postApplyMatrix.MultiplyPoint3x4(
                                    translation + preApplyMatrix.MultiplyPoint3x4(origins[index])));
                    }
                }

                mesh.mesh.vertices = positions;
                mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
                mesh.Refresh(RefreshMask.Normals);
            }

            ProBuilderEditor.UpdateMeshHandles(false);
        }

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
        void DrawSelectionOriginGizmos()
        {
            if (isEditing && currentEvent.type == EventType.Repaint)
            {
                foreach (var key in meshAndElementGroupPairs)
                {
                    foreach (var group in key.elementGroups)
                    {
                        EditorMeshHandles.DrawTransformOriginGizmo(group.postApplyMatrix, m_IndividualOriginDirection);
                    }
                }
            }
        }
#endif
    }
}
