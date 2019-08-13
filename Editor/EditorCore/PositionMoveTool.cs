using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class PositionMoveTool : PositionTool
    {
        const float k_CardinalAxisError = .001f;
        const float k_MinTranslateDeltaSqrMagnitude = .00001f;
        Vector3 m_HandlePosition;
        Vector3 m_RawHandleDelta;
        Vector3Mask m_ActiveAxesModel;
        Vector3Mask m_ActiveAxesWorld;

        bool m_SnapAsGroup;

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
        Vector3 m_IndividualOriginDirection;
        bool m_DirectionOriginInitialized;
#endif

        protected override void OnToolEngaged()
        {
            // If grids are enabled and `snapAxisConstraint` is off:
            //     - `snapAsGroup: false` handle is snapped on active axis only, and vertices are snapped to all axes
            //     - `snapAsGroup: true` handle is snapped on all axes, and vertices are not snapped
            // by snapping the handle OR vertices, we avoid double-snapping problems that can cause vertex positions to
            // be rounded to incorrect values.
            m_ActiveAxesModel = progridsSnapEnabled && !snapAxisConstraint && m_SnapAsGroup
                ? Vector3Mask.XYZ
                : new Vector3Mask(0x0);
            m_ActiveAxesModel = new Vector3Mask(0x0);

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
            m_DirectionOriginInitialized = false;
#endif

            m_SnapAsGroup = progridsSnapEnabled && ProGridsInterface.GetProGridsSnapAsGroup();
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

            m_RawHandleDelta = m_HandlePosition - handlePositionOrigin;

            var delta = m_RawHandleDelta;

            if (EditorGUI.EndChangeCheck() && delta.sqrMagnitude > k_MinTranslateDeltaSqrMagnitude)
            {
                if (!isEditing)
                    BeginEdit("Translate Selection");

                if (vertexDragging)
                {
                    Vector3 nearest;

                    if (FindNearestVertex(currentEvent.mousePosition, out nearest))
                    {
                        var unrotated = handleRotationOriginInverse * delta;
                        var dir = new Vector3Mask(unrotated, k_CardinalAxisError);

                        if (dir.active == 1)
                        {
                            var rotationDirection = handleRotationOrigin * dir * 10000f;

                            m_HandlePosition = HandleUtility.ProjectPointLine(nearest,
                                handlePositionOrigin + rotationDirection,
                                handlePositionOrigin - rotationDirection);

                            delta = m_HandlePosition - handlePositionOrigin;
                        }
                    }
                }
                else if (progridsSnapEnabled)
                {
                    // Handle is only snapped on all axes in the case where `snapAxisConstraint == false && snapAsGroup == true`
                    if (m_SnapAsGroup && snapAxisConstraint && m_ActiveAxesWorld.active == 1)// || !m_SnapAsGroup)
                    {
                        m_ActiveAxesModel |= new Vector3Mask(handleRotationOriginInverse * delta, k_CardinalAxisError);
                        m_ActiveAxesWorld = new Vector3Mask(handleRotation * m_ActiveAxesModel);

                        m_HandlePosition = ProGridsSnapping.SnapValueOnRay(
                            new Ray(handlePositionOrigin, delta),
                            delta.magnitude,
                            progridsSnapValue,
                            m_ActiveAxesWorld);
                    }
                    else if(m_SnapAsGroup)
                    {
                        m_HandlePosition = ProGridsSnapping.SnapValue(m_HandlePosition, progridsSnapValue);
                    }

                    delta = m_HandlePosition - handlePositionOrigin;
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

            // Draw at the end so we get the snapped value
            if(showHandleInfo && isEditing)
                DrawDeltaInfo(string.Format("Translate: <b>{0:F2}</b>  {1}", delta.magnitude, (handleRotationOriginInverse * delta).ToString("0.00")));
        }

        void ApplyTranslation(Vector3 translation)
        {
            var translationMagnitude = translation.magnitude;

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
                        if (progridsSnapEnabled && !m_SnapAsGroup)
                        {
                            if (snapAxisConstraint && m_ActiveAxesWorld.active == 1)
                            {
                                var wp = postApplyMatrix.MultiplyPoint3x4(preApplyMatrix.MultiplyPoint3x4(origins[index]));

                                var snap = ProGridsSnapping.SnapValueOnRay(
                                    new Ray(wp, m_RawHandleDelta),
                                    translationMagnitude,
                                    progridsSnapValue,
                                    m_ActiveAxesWorld,
                                    false);

                                positions[index] = worldToLocal.MultiplyPoint3x4(snap);
                            }
                            else
                            {
                                var origin = origins[index];
                                var preApply = preApplyMatrix.MultiplyPoint3x4(origin);
                                var wp = postApplyMatrix.MultiplyPoint3x4(translation + preApply);
                                var snap = ProGridsSnapping.SnapValue(wp, Vector3.one * progridsSnapValue);
                                positions[index] = worldToLocal.MultiplyPoint3x4(snap);
                            }
                        }
                        else
                        {
                            positions[index] = worldToLocal.MultiplyPoint3x4(
                                postApplyMatrix.MultiplyPoint3x4(
                                    translation + preApplyMatrix.MultiplyPoint3x4(origins[index])));
                        }
                    }
                }

                mesh.mesh.vertices = positions;
                mesh.RefreshUV(MeshSelection.selectedFacesInEditZone[mesh]);
                mesh.Refresh(RefreshMask.Normals);
            }

            ProBuilderEditor.Refresh(false);
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
