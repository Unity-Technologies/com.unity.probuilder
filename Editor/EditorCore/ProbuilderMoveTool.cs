using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class ProbuilderMoveTool : PositionTool
    {
        const float k_CardinalAxisError = .001f;
        const float k_MinTranslateDeltaSqrMagnitude = .00001f;
        Vector3 m_Position;
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
            // If grids are enabled and "snap on all axes" is on, initialize active axes to all.
            m_ActiveAxesModel = EditorSnapping.snapMode == SnapMode.World && !snapAxisConstraint
                ? Vector3Mask.XYZ
                : new Vector3Mask(0x0);

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
            m_DirectionOriginInitialized = false;
#endif

            // Snap as group preference is only respected when snap mode is "World"
            m_SnapAsGroup = EditorSnapping.snapMode == SnapMode.World && EditorSnapping.snapAsGroup;
        }

        protected override void DoToolGUI()
        {
            if (!isEditing)
                m_Position = m_HandlePosition;

#if PROBUILDER_ENABLE_TRANSFORM_ORIGIN_GIZMO
            if (isEditing)
                DrawSelectionOriginGizmos();
#endif

            EditorGUI.BeginChangeCheck();

            m_Position = Handles.PositionHandle(m_Position, m_HandleRotation);

            m_RawHandleDelta = m_Position - handlePositionOrigin;

            var delta = m_RawHandleDelta;

            if (EditorGUI.EndChangeCheck() && delta.sqrMagnitude > k_MinTranslateDeltaSqrMagnitude)
            {
                if (!isEditing)
                    BeginEdit("Translate Selection");

                if (Tools.vertexDragging)
                {
                    Vector3 nearest;

                    if (FindNearestVertex(currentEvent.mousePosition, out nearest))
                    {
                        var unrotated = handleRotationOriginInverse * delta;
                        var dir = new Vector3Mask(unrotated, k_CardinalAxisError);

                        if (dir.active == 1)
                        {
                            var rotationDirection = handleRotationOrigin * dir * 10000f;

                            m_Position = HandleUtility.ProjectPointLine(nearest,
                                handlePositionOrigin + rotationDirection,
                                handlePositionOrigin - rotationDirection);

                            delta = m_Position - handlePositionOrigin;
                        }
                    }
                }
                else if (EditorSnapping.snapMode == SnapMode.World)
                {
                    if (snapAxisConstraint)
                    {
                        m_ActiveAxesModel |= new Vector3Mask(handleRotationOriginInverse * delta, k_CardinalAxisError);
                        m_ActiveAxesWorld = new Vector3Mask(m_HandleRotation * m_ActiveAxesModel);

                        if (m_ActiveAxesWorld.active == 1)
                        {
                            m_Position = ProBuilderSnapping.SnapValueOnRay(
                                new Ray(handlePositionOrigin, delta),
                                delta.magnitude,
                                GetSnapValueForAxis(m_ActiveAxesModel),
                                m_ActiveAxesWorld);
                        }
                        else
                        {
                            m_Position = ProBuilderSnapping.Snap(m_Position, snapValue);
                        }
                    }
                    else
                    {
                        m_Position = ProBuilderSnapping.Snap(m_Position, snapValue);
                    }

                    delta = m_Position - handlePositionOrigin;
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
                        if (EditorSnapping.snapMode == SnapMode.World && !m_SnapAsGroup)
                        {
                            if (snapAxisConstraint && m_ActiveAxesWorld.active == 1)
                            {
                                var wp = postApplyMatrix.MultiplyPoint3x4(preApplyMatrix.MultiplyPoint3x4(origins[index]));

                                var snap = ProBuilderSnapping.SnapValueOnRay(
                                    new Ray(wp, m_RawHandleDelta),
                                    translationMagnitude,
                                    GetSnapValueForAxis(m_ActiveAxesWorld),
                                    m_ActiveAxesWorld);

                                positions[index] = worldToLocal.MultiplyPoint3x4(snap);
                            }
                            else
                            {
                                var wp = postApplyMatrix.MultiplyPoint3x4(translation + preApplyMatrix.MultiplyPoint3x4(origins[index]));
                                var snap = ProBuilderSnapping.Snap(wp, snapValue);
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
