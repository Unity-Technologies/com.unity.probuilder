using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    partial class CutTool
    {
        /// <summary>
        /// Rebuild the line mesh when updated
        /// </summary>
        void RebuildCutShape(bool optimize = true)
        {
            // If Undo is called immediately after creation this situation can occur
            if (m_Mesh == null)
                return;

            UpdateMeshConnections();
            ValidateCutShape();

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();

            if(optimize && isALoop && m_IsCutValid)
                ExecuteCut();

            m_Dirty = false;
        }

        void ValidateCutShape()
        {
            Vector3[] verticesPositions = m_Mesh.positionsInternal;

            m_IsCutValid = true;

            //For all segments of the current cut
            for(int i = 0; i < m_CutPath.Count-1 && m_IsCutValid; i++)
            {
                Vector2 segment1Start2D = HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[i].position));
                Vector2 segment1End2D = HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[i+1].position));

                int lastVertexIndex = (isALoop && i == 0) ? m_CutPath.Count-2 : m_CutPath.Count-1;
                //Test intersections with the rest of the cut path
                for(int j = i + 2; j < lastVertexIndex && m_IsCutValid; j++)
                {
                    if(((m_CutPath[j].types | m_CutPath[j+1].types) & VertexTypes.VertexInShape) == 0)
                    {
                        Vector2 segment2Start2D =
                            HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[j].position));
                        Vector2 segment2End2D =
                            HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[j + 1].position));

                        m_IsCutValid = !Math.GetLineSegmentIntersect(segment1Start2D, segment1End2D, segment2Start2D,
                            segment2End2D);
                    }
                }

                if(( (m_CutPath[i].types| m_CutPath[i+1].types) & VertexTypes.VertexInShape ) == 0)
                {
                    //Test intersections with the connections to the face vertices
                    for(int j = 0; j < m_MeshConnections.Count && m_IsCutValid; j++)
                    {
                        SimpleTuple<int, int> connection = m_MeshConnections[j];

                        if(connection.item1 != i && connection.item1 != i + 1)
                        {
                            Vector2 segment2Start2D =
                                HandleUtility.WorldToGUIPoint(
                                    m_Mesh.transform.TransformPoint(m_CutPath[connection.item1].position));
                            Vector2 segment2End2D =
                                HandleUtility.WorldToGUIPoint(
                                    m_Mesh.transform.TransformPoint(verticesPositions[connection.item2]));

                            m_IsCutValid = !Math.GetLineSegmentIntersect(segment1Start2D, segment1End2D,
                                segment2Start2D,
                                segment2End2D);
                        }
                    }
                }
            }

            //For all connections to the face vertices
            for(int i = 0; i <  m_MeshConnections.Count-1 && m_IsCutValid; i++)
            {
                SimpleTuple<int,int> connection1 = m_MeshConnections[i];
                Vector2 segment1Start2D =
                    HandleUtility.WorldToGUIPoint(
                        m_Mesh.transform.TransformPoint(m_CutPath[connection1.item1].position) );
                Vector2 segment1End2D =
                    HandleUtility.WorldToGUIPoint(
                        m_Mesh.transform.TransformPoint(verticesPositions[connection1.item2]));

                //Test intersection with the other connections to the face vertices
                for(int j = i+1; j < m_MeshConnections.Count && m_IsCutValid; j++)
                {
                    SimpleTuple<int,int> connection2 = m_MeshConnections[j];

                    Vector2 segment2Start2D =
                        HandleUtility.WorldToGUIPoint(
                            m_Mesh.transform.TransformPoint(m_CutPath[connection2.item1].position));
                    Vector2 segment2End2D =
                        HandleUtility.WorldToGUIPoint(
                            m_Mesh.transform.TransformPoint(verticesPositions[connection2.item2]));

                    m_IsCutValid = !Math.GetLineSegmentIntersect(segment1Start2D, segment1End2D, segment2Start2D, segment2End2D);
                }
            }
        }

        bool CanAppendCurrentPointToPath()
        {
            int polyCount = m_CutPath.Count;

            if (!Math.IsNumber(m_CurrentPosition))
                return false;

            if (!(polyCount == 0 || m_SelectedIndex != polyCount - 1))
                return false;

            // duplicate points are not permitted, except the special case where placing a final point on the starting
            // point finishes the cut operation.
            for(int i = 1; i < polyCount; i++)
                if (Math.Approx3(m_CutPath[i].position, m_CurrentPosition))
                    return false;

            // when the existing vertex count is less than 3, don't allow the special duplicate first vertex position
            return polyCount < 2 || !(polyCount < 3 && Math.Approx3(m_CutPath[0].position, m_CurrentPosition));
        }
    }
}
