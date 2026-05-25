using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using Edge = UnityEngine.ProBuilder.Edge;
using Math = UnityEngine.ProBuilder.Math;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

namespace UnityEditor.ProBuilder
{
    partial class CutTool
    {
        /// <summary>
        /// Compute the position designated by the user in the current mesh/face taking into account snapping
        /// </summary>
        /// <returns>true is a valid position is computed in the mesh</returns>
        bool UpdateHitPosition()
        {
            Event evt = Event.current;

            Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
            RaycastHit pbHit;

            m_CurrentFace = null;

            if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, m_Mesh, out pbHit))
            {
                UpdateCurrentPosition(m_Mesh.faces[pbHit.face], pbHit.point ,pbHit.normal);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add the position in the face to the cut taking into account snapping
        /// </summary>
        internal void UpdateCurrentPosition(Face face, Vector3 position, Vector3 normal)
        {
            m_CurrentPosition = position;
            m_CurrentPositionNormal = normal;
            m_CurrentFace = face;
            m_CurrentVertexTypes = VertexTypes.None;

            CheckPointInCutPath();

            if (m_CurrentVertexTypes == VertexTypes.None && !m_ModifyingPoint)
                CheckPointInMesh();

            // Only apply grid snap if the point is a free-floating new vertex on the face
            // (don't pull already-snapped edge/vertex points off their targets)
            if (m_CurrentVertexTypes == VertexTypes.NewVertex || m_CurrentVertexTypes == VertexTypes.None)
                ApplyGridSnap();
        }

        /// <summary>
        /// Snap the current position to Unity's grid if grid snapping is enabled.
        /// </summary>
        void ApplyGridSnap()
        {
            if (m_SnapToGrid)
            {
                Vector3 snapped = ProBuilderSnapping.Snap(m_CurrentPosition, EditorSnapping.activeMoveSnapValue);
                Plane facePlane = new Plane(m_CurrentPositionNormal, m_CurrentPosition);
                m_CurrentPosition = facePlane.ClosestPointOnPlane(snapped);
            }
        }

        /// <summary>
        /// Updates the connections between the cut path and the mesh vertices
        /// </summary>
        internal void UpdateMeshConnections()
        {
            m_MeshConnections.Clear();
            if(m_CutPath.Count < 2)
                return;

            List<Vector3> existingVerticesInCut =
                m_CutPath.Where(v => ( v.types & VertexTypes.ExistingVertex ) != 0)
                         .Select(v => v.position).ToList();

            Vector3[] verticesPositions = m_Mesh.positionsInternal;
            if(!isALoop)
            {
                //Connects to start and the end of the path to create a loop
                float minDistToStart = Single.PositiveInfinity, minDistToStart2 = Single.PositiveInfinity;
                float minDistToEnd = Single.PositiveInfinity, minDistToEnd2 = Single.PositiveInfinity;
                int bestVertexIndexToStart = -1, bestVertexIndexToStart2 = -1, bestVertexIndexToEnd = -1,  bestVertexIndexToEnd2 = -1;
                float dist;
                foreach(var vertexIndex in m_TargetFace.distinctIndexes)
                {
                    if(existingVerticesInCut.Count > 0)
                    {
                        if(existingVerticesInCut.Exists(vert => Math.Approx3(verticesPositions[vertexIndex], vert)))
                            continue;
                    }

                    if(( m_CutPath[0].types & VertexTypes.NewVertex ) != 0)
                    {
                        dist = Vector3.Distance(verticesPositions[vertexIndex], m_CutPath[0].position);
                        if(dist < minDistToStart)
                        {
                            minDistToStart2 = minDistToStart;
                            bestVertexIndexToStart2 = bestVertexIndexToStart;
                            minDistToStart = dist;
                            bestVertexIndexToStart = vertexIndex;
                        }else if(dist < minDistToStart2)
                        {
                            minDistToStart2 = dist;
                            bestVertexIndexToStart2 = vertexIndex;
                        }
                    }
                    if(m_CutPath.Count > 1 && ( m_CutPath[m_CutPath.Count - 1].types & VertexTypes.NewVertex ) != 0)
                    {
                        dist = Vector3.Distance(verticesPositions[vertexIndex], m_CutPath[m_CutPath.Count - 1].position);
                        if(dist < minDistToEnd)
                        {
                            minDistToEnd2 = minDistToEnd;
                            bestVertexIndexToEnd2 = bestVertexIndexToEnd;
                            minDistToEnd = dist;
                            bestVertexIndexToEnd = vertexIndex;
                        }
                        else if(dist < minDistToEnd2)
                        {
                            minDistToEnd2 = dist;
                            bestVertexIndexToEnd2 = vertexIndex;
                        }
                    }
                }

                //Do not connect the 2 extremities to the same point
                if(bestVertexIndexToStart == bestVertexIndexToEnd)
                {
                    if(minDistToStart2 < minDistToEnd2)
                        bestVertexIndexToStart = bestVertexIndexToStart2;
                    else
                        bestVertexIndexToEnd = bestVertexIndexToEnd2;
                }

                if(bestVertexIndexToStart >= 0)
                    m_MeshConnections.Add(new SimpleTuple<int, int>(0,bestVertexIndexToStart));

                if(bestVertexIndexToEnd >= 0)
                    m_MeshConnections.Add(new SimpleTuple<int, int>(m_CutPath.Count - 1,bestVertexIndexToEnd));
            }
            else if(isALoop)
            {
                int requiredConnections = m_RectangleMode ? 4 : 2;
                if (connectionsToBordersCount >= requiredConnections)
                    return;

                //The path must have minimum connections with the face borders, find the closest vertices
                foreach(var vertexIndex in m_TargetFace.distinctIndexes)
                {
                    if(existingVerticesInCut.Count > 0)
                    {
                        if(existingVerticesInCut.Exists(vert => Math.Approx3(verticesPositions[vertexIndex], vert)))
                            continue;
                    }

                    int pathIndex = -1;
                    float minDistance = Single.MaxValue;
                    for(int i = 0; i < m_CutPath.Count; i++)
                    {
                        if(( m_CutPath[i].types & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex) ) == 0)
                        {
                            float dist = Vector3.Distance(verticesPositions[vertexIndex], m_CutPath[i].position);
                            if(dist < minDistance)
                            {
                                minDistance = dist;
                                pathIndex = i;
                            }
                        }
                    }

                    if(pathIndex >= 0)
                    {
                        if(m_MeshConnections.Exists(tup => tup.item1 == pathIndex))
                        {
                            var tuple = m_MeshConnections.Find(tup => tup.item1 == pathIndex);
                            if(Vector3.Distance(m_CutPath[tuple.item1].position, verticesPositions[tuple.item2])
                               > Vector3.Distance(m_CutPath[pathIndex].position, verticesPositions[vertexIndex]))
                            {
                                m_MeshConnections.Remove(tuple);
                                m_MeshConnections.Add(new SimpleTuple<int, int>(pathIndex, vertexIndex));
                            }
                        }
                        else
                            m_MeshConnections.Add(new SimpleTuple<int, int>(pathIndex, vertexIndex));
                    }
                }

                m_MeshConnections.Sort((a,b) =>
                    (int)Mathf.Sign(Vector3.Distance(m_CutPath[a.item1].position, verticesPositions[a.item2])
                                    - Vector3.Distance(m_CutPath[b.item1].position, verticesPositions[b.item2])));

                int connectionsCount = Mathf.Max(0, requiredConnections - connectionsToBordersCount);
                connectionsCount = Mathf.Min(connectionsCount, m_MeshConnections.Count);
                m_MeshConnections.RemoveRange(connectionsCount,m_MeshConnections.Count - connectionsCount);
            }
        }

        /// <summary>
        /// Check whether the current position (m_CurrentPosition) can be associated/snapped to an existing position of the path
        /// </summary>
        void CheckPointInCutPath()
        {
            //Check if trying to reach the start point
            if(!m_ModifyingPoint && m_CutPath.Count > 1)
            {
                float snapDistance = 0.1f;
                var vertexData = m_CutPath[0];
                if(Math.Approx3(vertexData.position, m_CurrentPosition, snapDistance))
                {
                    m_CurrentPosition = vertexData.position;
                    m_CurrentVertexTypes = vertexData.types | VertexTypes.VertexInShape;
                    m_SelectedIndex = 0;
                }
            }
            else if (m_SnappingPoint || m_ModifyingPoint)
            {
                float snapDistance = m_SnappingDistance;
                for(int i = 0; i < m_CutPath.Count; i++)
                {
                    var vertexData = m_CutPath[i];
                    if(Math.Approx3(vertexData.position,
                        m_CurrentPosition,
                        snapDistance))
                    {
                        snapDistance = Vector3.Distance(vertexData.position, m_CurrentPosition);
                        if(!m_ModifyingPoint)
                            m_CurrentPosition = vertexData.position;
                        m_CurrentVertexTypes = vertexData.types | VertexTypes.VertexInShape;
                        m_SelectedIndex = i;
                    }
                }
            }
        }

        /// <summary>
        /// Check whether the current position (m_CurrentPosition) can be associated/snapped to an existing
        /// edge or vertex of the current face
        /// </summary>
        void CheckPointInMesh()
        {
            m_CurrentVertexTypes = VertexTypes.NewVertex;
            bool snapedOnVertex = false;
            float snapDistance = m_SnappingDistance;
            int bestIndex = -1;
            float bestDistance = Mathf.Infinity;

            m_SnapedVertexId = -1;
            m_SnapedEdge = Edge.Empty;

            Vector3[] vertexPositions = m_Mesh.positionsInternal;
            IList<Edge> peripheralEdges = m_CurrentFace.edges;
            if (m_TargetFace != null && m_CurrentFace != m_TargetFace)
                peripheralEdges = m_TargetFace.edges;
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                if ((m_TargetFace == null || m_TargetFace == m_CurrentFace) && m_SnappingPoint)
                {
                    if (Math.Approx3(vertexPositions[peripheralEdges[i].a],
                        m_CurrentPosition,
                        snapDistance))
                    {
                        bestIndex = i;
                        snapedOnVertex = true;
                        break;
                    }
                    else
                    {
                        float dist = Math.DistancePointLineSegment(
                            m_CurrentPosition,
                            vertexPositions[peripheralEdges[i].a],
                            vertexPositions[peripheralEdges[i].b]);

                        if (dist < Mathf.Min(snapDistance, bestDistance))
                        {
                            bestIndex = i;
                            bestDistance = dist;
                        }
                    }
                }
                //Even with no snapping, try to detect if the first point is on a existing geometry
                else if(m_TargetFace == null && !m_SnappingPoint)
                {
                    if (Math.Approx3(vertexPositions[peripheralEdges[i].a],
                        m_CurrentPosition,
                        0.01f))
                    {
                        bestIndex = i;
                        snapedOnVertex = true;
                        break;
                    }
                    else
                    {
                        float dist = Math.DistancePointLineSegment(
                            m_CurrentPosition,
                            vertexPositions[peripheralEdges[i].a],
                            vertexPositions[peripheralEdges[i].b]);

                        if (dist < Mathf.Min(0.01f, bestDistance))
                        {
                            bestIndex = i;
                            bestDistance = dist;
                        }
                    }
                }
                else if(m_CurrentFace != m_TargetFace && m_TargetFace != null )
                {
                    float edgeDist = Math.DistancePointLineSegment(m_CurrentPosition,
                        vertexPositions[peripheralEdges[i].a],
                        vertexPositions[peripheralEdges[i].b]);

                    float vertexDist = Vector3.Distance(m_CurrentPosition,
                        vertexPositions[peripheralEdges[i].a]);

                    if (edgeDist < vertexDist && edgeDist < bestDistance)
                    {
                        bestIndex = i;
                        bestDistance = edgeDist;
                        snapedOnVertex = false;
                    }
                    //always prioritize vertex snap on edge snap
                    else if (vertexDist <= bestDistance)
                    {
                        bestIndex = i;
                        bestDistance = vertexDist;
                        snapedOnVertex = true;
                    }
                }
            }

            //We found a close vertex
            if (snapedOnVertex)
            {
                m_CurrentPosition = vertexPositions[peripheralEdges[bestIndex].a];
                m_CurrentVertexTypes = VertexTypes.ExistingVertex;
                m_SelectedIndex = -1;

                m_SnapedVertexId = peripheralEdges[bestIndex].a;
                CheckPointInCutPath();
            }
            //If not, did we found a close edge?
            else if (bestIndex >= 0)
            {
                if (m_TargetFace == null || m_TargetFace == m_CurrentFace)
                {
                    Vector3 left = vertexPositions[peripheralEdges[bestIndex].a],
                        right = vertexPositions[peripheralEdges[bestIndex].b];

                    float x = (m_CurrentPosition - left).magnitude;
                    float y = (m_CurrentPosition - right).magnitude;

                    m_CurrentPosition = left + (x / (x + y)) * (right - left);
                }
                else //if(m_CurrentFace != m_TargetFace)
                {
                    Vector3 a = m_CurrentPosition -
                                vertexPositions[peripheralEdges[bestIndex].a];
                    Vector3 b = vertexPositions[peripheralEdges[bestIndex].b] -
                                vertexPositions[peripheralEdges[bestIndex].a];

                    float angle = Vector3.Angle(b, a);
                    m_CurrentPosition = Vector3.Magnitude(a) * Mathf.Cos(angle * Mathf.Deg2Rad) * b / Vector3.Magnitude(b);
                    m_CurrentPosition += vertexPositions[peripheralEdges[bestIndex].a];
                }

                m_SnapedEdge = peripheralEdges[bestIndex];

                m_CurrentVertexTypes = VertexTypes.AddedOnEdge;
                m_SelectedIndex = -1;
            }
        }
    }
}
