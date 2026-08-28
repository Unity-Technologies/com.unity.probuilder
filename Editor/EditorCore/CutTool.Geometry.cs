using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Vertex = UnityEngine.ProBuilder.Vertex;
using Edge = UnityEngine.ProBuilder.Edge;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    partial class CutTool
    {
        /// <summary>
        /// Compute the cut result and display a notification
        /// </summary>
        void ExecuteCut(bool restorePrevious = true)
        {
            ActionResult result = DoCut();
            EditorUtility.ShowNotification(result.notification);

            if(restorePrevious)
                ExitTool();
        }

        /// <summary>
        /// Compute the faces resulting from the cut:
        /// - First inserts points defining the cut as vertices in the face
        /// - Compute the central polygon is the cut is creating a closed polygon in the face
        /// - Update the rest of the face accordingly to the cut and the central polygon
        /// </summary>
        /// <returns>ActionResult success if it was possible to create the cut</returns>
        internal ActionResult DoCut()
        {
            if (m_TargetFace == null || m_CutPath.Count < 2)
            {
                return new ActionResult(ActionResult.Status.Canceled, L10n.Tr("Not enough elements selected for a cut"));
            }

            if(!m_IsCutValid)
            {
                return new ActionResult(ActionResult.Status.Failure, L10n.Tr("The current cut overlaps itself"));
            }

            UndoUtility.RecordObject(m_Mesh, "Execute Cut");

            List<Vertex> meshVertices = new List<Vertex>();
            m_Mesh.GetVerticesInList(meshVertices);
            Vertex[] formerVertices = new Vertex[m_MeshConnections.Count];
            for(int i = 0; i < m_MeshConnections.Count; i++)
            {
                formerVertices[i] = meshVertices[m_MeshConnections[i].item2];
            }

            //Insert cut vertices in the mesh
            List<Vertex> cutVertices = InsertVertices();
            m_Mesh.GetVerticesInList(meshVertices);

            // Build vertex→index dictionary for O(1) lookups (preserve first-index behavior like IndexOf)
            Dictionary<Vertex, int> vertexIndexMap = new Dictionary<Vertex, int>();
            for (int i = 0; i < meshVertices.Count; i++)
            {
                if (!vertexIndexMap.ContainsKey(meshVertices[i]))
                    vertexIndexMap[meshVertices[i]] = i;
            }

            //Retrieve indexes of the cut points in the mesh vertices
            int[] cutIndexes = cutVertices.Select(vert => vertexIndexMap[vert]).ToArray();

            //Update mesh connections with new indexes
            for(int i = 0; i<m_MeshConnections.Count; i++)
            {
                SimpleTuple<int, int> connection = m_MeshConnections[i];
                connection.item1 = vertexIndexMap[cutVertices[connection.item1]];
                connection.item2 = vertexIndexMap[formerVertices[i]];
                m_MeshConnections[i] = connection;
            }

            List<Face> newFaces = new List<Face>();
            // If the cut defines a loop in the face, create the polygon corresponding to that loop
            if (isALoop)
            {
                Face f = m_Mesh.CreatePolygon(cutIndexes, false);

                if(f == null)
                    return new ActionResult(ActionResult.Status.Failure, L10n.Tr("Cut Shape is not valid"));

                ApplySourceFaceSettings(f, m_TargetFace);

                Vector3 nrm = Math.Normal(m_Mesh, f);
                Vector3 targetNrm = Math.Normal(m_Mesh, m_TargetFace);
                // If the shape is define in the wrong orientation compared to the former face, reverse it
                if(Vector3.Dot(nrm,targetNrm) < 0f)
                    f.Reverse();

                newFaces.Add(f);
            }

            //Compute the rest of the new faces (faces outside of the loop or division of the original face)
            List<Face> faces = ComputeNewFaces(m_TargetFace, cutIndexes);
            newFaces.AddRange(faces);

            //Remove inserted vertices only if they were inserted for the process
            List<int> verticesIndexesToDelete = new List<int>();
            for(int i = 0; i < m_CutPath.Count; i++)
            {
                if(( m_CutPath[i].types & VertexTypes.NewVertex ) != 0
                && ( m_CutPath[i].types & VertexTypes.VertexInShape ) == 0)
                    verticesIndexesToDelete.Add(cutIndexes[i]);
            }
            m_Mesh.DeleteVertices(verticesIndexesToDelete);

            //Delete former face
            m_Mesh.DeleteFace(m_TargetFace);

            m_Mesh.ToMesh();
            m_Mesh.Refresh();
            m_Mesh.Optimize();

            //Update mesh selection after the cut has been performed
            // For loop cuts, select only the central cutout so user can immediately manipulate it
            MeshSelection.ClearElementSelection();
            if (isALoop)
            {
                // Select only the central loop face (first in the list)
                m_Mesh.SetSelectedFaces(new Face[] { newFaces[0] });
            }
            else
            {
                m_Mesh.SetSelectedFaces(newFaces);
            }
            ProBuilderEditor.Refresh();

            ResetToolState(true);

            return new ActionResult(ActionResult.Status.Success, L10n.Tr("Cut executed"));
        }

        /// <summary>
        /// Based on the new vertices inserted in the face, this method computes the different faces
        /// created between the cut and the original face (external to the cut if it makes a loop)
        ///
        /// The faces are created by parsing the edges that defines the border of the original face. Is an edge ends on a
        /// vertex that is part of the cut, or belongs to a connection between the cut and the face,
        /// we close the defined polygon using the cut (though ComputeFaceClosure method) and create a face out of this polygon
        /// </summary>
        /// <param name="face">Original face to modify</param>
        /// <param name="cutVertexIndexes">Indexes of the new vertices inserted in the face</param>
        /// <returns>The list of polygons to create (defined by their vertices indexes)</returns>
        List<Face> ComputeNewFaces(Face face, IList<int> cutVertexIndexes)
        {
            List<Face> newFaces = new List<Face>();

            //Get Vertices from the mesh
            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;
            var cutVertexSharedIndexes = cutVertexIndexes.Select(ind => sharedToUnique[ind]).ToList();

            //Parse peripheral edges to unique id and find a common point between the peripheral edges and the cut
            var peripheralEdges = WingedEdge.SortEdgesByAdjacency(face);
            var peripheralEdgesUnique = new List<Edge>();
            int startIndex = -1;
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                Edge eShared = peripheralEdges[i];
                Edge eUnique = new Edge(sharedToUnique[eShared.a], sharedToUnique[eShared.b]);
                peripheralEdgesUnique.Add(eUnique);

                if (startIndex == -1 && ( cutVertexSharedIndexes.Contains(eUnique.a)
                                          || m_MeshConnections.Exists(tup => sharedToUnique[tup.item2] == eUnique.a)))
                    startIndex = i;
            }

            //Create a polygon for each cut reaching the mesh edges
            List<Face> facesToDelete = new List<Face>();
            List<int> polygon = new List<int>();
            for (int i = startIndex; i <= peripheralEdgesUnique.Count + startIndex; i++)
            {
                 polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                 Edge e = peripheralEdgesUnique[i % peripheralEdgesUnique.Count];

                 if(polygon.Count > 1)
                 {
                     int index = -1;
                     if(cutVertexSharedIndexes.Contains(e.a)) // get next vertex
                     {
                         index = e.a;
                     }
                     else if(m_MeshConnections.Exists(tup => sharedToUnique[tup.item2] == e.a))
                     {
                         SimpleTuple<int, int> connection = m_MeshConnections.Find(tup => sharedToUnique[tup.item2] == e.a);
                         polygon.Add(connection.item1);
                         index = sharedToUnique[connection.item1];
                     }

                     if(index >= 0)
                     {
                         // In the case of only 2 distinct, a face should not be added.
                         if (polygon.Count != 2)
                         {
                             List<Face> toDelete;
                             Face newFace = ComputeFaceClosure(polygon, index, cutVertexSharedIndexes, out toDelete);
                             if (newFace != null && newFace.indexesInternal != null)
                             {
                                ApplySourceFaceSettings(newFace, m_TargetFace);
                                 newFaces.Add(newFace);
                                 facesToDelete.AddRange(toDelete);
                             }
                         }

                         //Start a new polygon
                         polygon = new List<int>();
                         polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                     }
                 }
            }
            polygon.Clear();

            m_Mesh.DeleteFaces(facesToDelete);
            return newFaces;
        }

        /// <summary>
        ///    The method computes all the possible faces that can be made starting by the vertices in polygonStart and ending with the cut
        /// This method creates faces that are not the final one and that must be deleted at the end. These invalid faces are returned in facesToDelete
        /// The only valid face is returned from this method. From all defined faces, the valid face is the one with the smaller area
        /// (otherwise it means it covers another face of the mesh).
        /// </summary>
        /// <param name="polygonStart">Indexes of the first vertices of the new Face to define, these vertices are coming from the original face only</param>
        /// <param name="currentIndex">Current vertex index in the cut</param>
        /// <param name="cutIndexes">Indexes of the vertices defining the cut</param>
        /// <param name="cutIndexes">out : extra faces created by this method that will need to be deleted after
        /// (these faces cannot be deleted directly as it will break the m_MeshConnections by deleting some indexes before the end of the algorithm)
        /// <returns>the valid face that need to be kept in the resulting mesh</returns>
        Face ComputeFaceClosure( List<int> polygonStart, int currentIndex, List<int> cutIndexes, out List<Face> facesToDelete)
        {
            IList<SharedVertex> uniqueIdToVertexIndex = m_Mesh.sharedVertices;
            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;

            facesToDelete = new List<Face>();

            if (polygonStart == null || polygonStart.Count == 0 || cutIndexes == null || cutIndexes.Count == 0)
                return null;

            int polygonFirstVertex = polygonStart[0];
            int startIndex = cutIndexes.IndexOf(currentIndex);

            if (startIndex < 0 || !sharedToUnique.ContainsKey(polygonFirstVertex))
                return null;

            int polygonFirstSharedIndex = sharedToUnique[polygonFirstVertex];

            int connectionIndex = m_MeshConnections.FindIndex(tup =>
                sharedToUnique.ContainsKey(tup.item2) && sharedToUnique[tup.item2] == polygonFirstSharedIndex);
            bool hasConnection = connectionIndex >= 0;
            SimpleTuple<int,int> connection = hasConnection ? m_MeshConnections[connectionIndex] : default;

            // Hoist loop-invariant dictionary lookups for connection.item1
            int connectionSharedItem1 = -1;
            bool hasConnectionSharedItem1 = hasConnection && sharedToUnique.TryGetValue(connection.item1, out connectionSharedItem1);

            List<List<int>> closureCandidates = new List<List<int>>();

            //Go through the cut in reverse direction
            int index;
            int finalIndex = isALoop ?(startIndex - cutIndexes.Count) : 0;
            bool connected = false;
            List<int> candidate = new List<int>();
            for(index = startIndex - 1; index >= finalIndex; index--)
            {
                int vertexIndex = uniqueIdToVertexIndex[cutIndexes[(index + cutIndexes.Count) % cutIndexes.Count]][0];
                candidate.Add(vertexIndex);
                if(sharedToUnique[vertexIndex] == polygonFirstSharedIndex ||
                   (hasConnectionSharedItem1 && sharedToUnique[vertexIndex] == connectionSharedItem1))
                {
                    connected = true;
                    break;
                }
            }

            //If we find a valid candidate for the connection, add it to the list
            if(connected)
                closureCandidates.Add(candidate);

            //Go through the cut in forward direction
            finalIndex = isALoop ? (startIndex + cutIndexes.Count) : cutIndexes.Count;
            connected = false;
            candidate = new List<int>();
            for(index = startIndex + 1; index < finalIndex; index++)
            {
                int vertexIndex = uniqueIdToVertexIndex[cutIndexes[index % cutIndexes.Count]][0];
                candidate.Add(vertexIndex);
                if(sharedToUnique[vertexIndex] == polygonFirstSharedIndex ||
                   (hasConnectionSharedItem1 && sharedToUnique[vertexIndex] == connectionSharedItem1))
                {
                    connected = true;
                    break;
                }
            }

            //If we find a valid candidate for the connection, add it to the list
            if(connected)
                closureCandidates.Add(candidate);

            //Go through the different candidate and keep the best one.
            Face bestFace = null;
            float bestArea = 0f;
            foreach(var closure in closureCandidates)
            {
                closure.AddRange(polygonStart);

                Face face = m_Mesh.CreatePolygon(closure, false);
                uniqueIdToVertexIndex = m_Mesh.sharedVertices;
                sharedToUnique = m_Mesh.sharedVertexLookup;

                float area;
                if (!TryGetFaceArea(face, uniqueIdToVertexIndex, sharedToUnique, out area))
                {
                    if (face != null)
                        facesToDelete.Add(face);

                    continue;
                }

                if(bestFace != null)
                {
                    if(area < bestArea)
                    {
                        facesToDelete.Add(bestFace);
                        bestArea = area;
                        bestFace = face;
                    }
                    else
                        facesToDelete.Add(face);
                }
                else
                {
                    bestFace = face;
                    bestArea = area;
                }
            }

            return bestFace;
        }

        void ApplySourceFaceSettings(Face destination, Face source)
        {
            if (destination == null || source == null)
                return;

            destination.submeshIndex = source.submeshIndex;
            destination.manualUV = source.manualUV;
            destination.uv = new AutoUnwrapSettings(source.uv);
            destination.textureGroup = source.textureGroup;
            destination.smoothingGroup = source.smoothingGroup;
            destination.elementGroup = source.elementGroup;
        }

        bool TryGetFaceArea(Face face, IList<SharedVertex> uniqueIdToVertexIndex,
            Dictionary<int, int> sharedToUnique, out float area)
        {
            area = 0f;

            if (face == null || face.indexesInternal == null)
                return false;

            Vector3[] vertices = m_Mesh.positionsInternal;
            int[] indexes = new int[face.indexesInternal.Length];

            for (int i = 0; i < face.indexesInternal.Length; i++)
            {
                int uniqueIndex;
                if (!sharedToUnique.TryGetValue(face.indexesInternal[i], out uniqueIndex))
                    return false;

                if (uniqueIndex < 0 || uniqueIndex >= uniqueIdToVertexIndex.Count)
                    return false;

                SharedVertex sharedVertex = uniqueIdToVertexIndex[uniqueIndex];
                if (sharedVertex == null || sharedVertex.Count < 1)
                    return false;

                indexes[i] = sharedVertex[0];
            }

            area = Math.PolygonArea(vertices, indexes);
            return true;
        }

        /// <summary>
        /// Insert all position from the cut path to the current faces as new vertices
        /// </summary>
        /// <returns>The list of Vertex inserted in the face</returns>
        List<Vertex> InsertVertices()
        {
            List<Vertex> newVertices = new List<Vertex>();

            foreach (var vertexData in m_CutPath)
            {
                switch (vertexData.types)
                {
                    case VertexTypes.ExistingVertex:
                    case VertexTypes.VertexInShape:
                        newVertices.Add(InsertVertexOnExistingVertex(vertexData.position));
                        break;
                    case VertexTypes.AddedOnEdge:
                        newVertices.Add(InsertVertexOnExistingEdge(vertexData.position));
                        break;
                    case VertexTypes.NewVertex:
                        newVertices.Add(m_Mesh.InsertVertexInMesh(vertexData.position,vertexData.normal));
                        break;
                    default:
                        break;
                }
            }

            return newVertices;
        }

        /// <summary>
        /// Method to retrieve a vertex already existing in the face to avoid duplicated
        /// </summary>
        /// <param name="vertexPosition">The vertex position</param>
        /// <returns>The retrieved vertex</returns>
        Vertex InsertVertexOnExistingVertex(Vector3 vertexPosition)
        {
            Vertex vertex = null;

            List<Vertex> vertices = m_Mesh.GetVertices().ToList();
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                if (Math.Approx3(vertices[vertIndex].position, vertexPosition)
                    && !float.IsNaN(vertices[vertIndex].normal.x) )
                {
                    vertex = vertices[vertIndex];
                    break;
                }
            }

            return vertex;
        }

        /// <summary>
        /// Insert the vertex in an exiting edge
        /// </summary>
        /// <param name="vertexPosition">The position of the vertex to insert</param>
        /// <returns>The inew vertex inserted</returns>
        Vertex InsertVertexOnExistingEdge(Vector3 vertexPosition)
        {
            Vector3[] vertexPositions = m_Mesh.positionsInternal;
            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_TargetFace);

            int bestIndex = -1;
            float bestDistance = Mathf.Infinity;
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                float dist = UnityEngine.ProBuilder.Math.DistancePointLineSegment(vertexPosition,
                        vertexPositions[peripheralEdges[i].a],
                        vertexPositions[peripheralEdges[i].b]);

                if (dist < bestDistance)
                {
                    bestIndex = i;
                    bestDistance = dist;
                }
            }

            Vertex v = m_Mesh.InsertVertexOnEdge(peripheralEdges[bestIndex], vertexPosition);
            return v;
        }
    }
}
