using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    abstract class MeshAndElementSelection
    {
        ProBuilderMesh m_Mesh;

        List<ElementGroup> m_ElementGroups;

        public ProBuilderMesh mesh
        {
            get { return m_Mesh; }
        }

        public List<ElementGroup> elementGroups
        {
            get { return m_ElementGroups; }
        }

        public MeshAndElementSelection(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation, bool collectCoincidentIndices)
        {
            m_Mesh = mesh;
            m_ElementGroups = ElementGroup.GetElementGroups(mesh, pivot, orientation, collectCoincidentIndices);
        }
    }

    class ElementGroup
    {
        List<int> m_Indices;
        Matrix4x4 m_PreApplyPositionsMatrix;
        Matrix4x4 m_PostApplyPositionsMatrix;
        Vector3 m_Position;
        Quaternion m_Rotation;

        /// <value>
        /// Center of this selection in world space.
        /// </value>
        public Vector3 position
        {
            get { return m_Position; }
        }

        /// <value>
        /// Rotation of this selection in world space.
        /// </value>
        public Quaternion rotation
        {
            get { return m_Rotation; }
        }

        public List<int> indices
        {
            get { return m_Indices; }
        }

        public Matrix4x4 preApplyMatrix
        {
            get { return m_PreApplyPositionsMatrix; }
            set
            {
                m_PreApplyPositionsMatrix = value;
                m_PostApplyPositionsMatrix = m_PreApplyPositionsMatrix.inverse;
            }
        }

        public Matrix4x4 postApplyMatrix
        {
            get { return m_PostApplyPositionsMatrix; }
        }

        public ElementGroup(List<int> indices, Vector3 position, Quaternion rotation)
        {
            var post = Matrix4x4.TRS(position, rotation, Vector3.one);
            m_Indices = indices;
            m_Position = position;
            m_Rotation = rotation;
            m_PostApplyPositionsMatrix = post;
            m_PreApplyPositionsMatrix = post.inverse;
        }

        public static List<ElementGroup> GetElementGroups(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation, bool collectCoincident)
        {
            var groups = new List<ElementGroup>();
            var trs = mesh.transform.localToWorldMatrix;
            var selectMode = ProBuilderEditor.selectMode;

            switch (pivot)
            {
                case PivotPoint.IndividualOrigins:
                {
                    // todo - Support individual origins for vertices and edges
                    if (selectMode == SelectMode.Vertex)
                    {
                        foreach (var list in GetVertexSelectionGroups(mesh, collectCoincident))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = EditorHandleUtility.GetVertexRotation(mesh, orientation, list);
                            groups.Add(new ElementGroup(list, trs.MultiplyPoint3x4(bounds.center), rot));
                        }
                    }
                    else if (selectMode == SelectMode.Edge)
                    {
                        foreach (var list in GetEdgeSelectionGroups(mesh))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = EditorHandleUtility.GetEdgeRotation(mesh, orientation, list);

                            List<int> indices;

                            if (collectCoincident)
                            {
                                indices = new List<int>();
                                mesh.GetCoincidentVertices(list, indices);
                            }
                            else
                            {
                                indices = list.SelectMany(x => new int[] { x.a, x.b }).ToList();
                            }

                            groups.Add(new ElementGroup(indices, trs.MultiplyPoint3x4(bounds.center), rot));
                        }
                    }
                    else if (selectMode == SelectMode.Face)
                    {
                        foreach (var list in GetFaceSelectionGroups(mesh))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = EditorHandleUtility.GetFaceRotation(mesh, orientation, list);
                            List<int> indices;

                            if (collectCoincident)
                            {
                                indices = new List<int>();
                                mesh.GetCoincidentVertices(list, indices);
                            }
                            else
                            {
                                indices = list.SelectMany(x => x.distinctIndexesInternal).ToList();
                            }

                            groups.Add(new ElementGroup(indices, trs.MultiplyPoint3x4(bounds.center), rot));
                        }
                    }
                    break;
                }

                case PivotPoint.ActiveElement:
                {
                    var indices = collectCoincident
                        ? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
                        : new List<int>(mesh.selectedIndexesInternal);

                    var position = mesh.transform.position;
                    var rotation = mesh.transform.rotation;

                    if (selectMode == SelectMode.Face)
                    {
                        var face = mesh.GetActiveFace();

                        if (face != null)
                        {
                            position = trs.MultiplyPoint3x4(Math.GetBounds(mesh.positionsInternal, face.distinctIndexesInternal).center);
                            rotation = EditorHandleUtility.GetFaceRotation(mesh, orientation, new Face[] { face });
                        }
                    }
                    else if (selectMode == SelectMode.Edge)
                    {
                        var edge = mesh.GetActiveEdge();

                        if (edge != Edge.Empty)
                        {
                            position = trs.MultiplyPoint3x4(Math.GetBounds(mesh.positionsInternal, new int [] { edge.a, edge.b }).center);
                            rotation = EditorHandleUtility.GetEdgeRotation(mesh, orientation, new Edge[] { edge });
                        }
                    }
                    else if (selectMode == SelectMode.Vertex)
                    {
                        var vertex = mesh.GetActiveVertex();

                        if (vertex < 0)
                        {
                            position = trs.MultiplyPoint3x4(mesh.positionsInternal[vertex]);
                            rotation = EditorHandleUtility.GetVertexRotation(mesh, orientation, new int[] { vertex });
                        }
                    }

                    groups.Add(new ElementGroup(indices, position, rotation));
                    break;
                }

                default:
                {
                    var indices = collectCoincident
                        ? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
                        : new List<int>(mesh.selectedIndexesInternal);

                    var bounds = Math.GetBounds(mesh.positionsInternal, indices);

                    groups.Add(new ElementGroup(indices, trs.MultiplyPoint3x4(bounds.center), trs.rotation));

                    break;
                }
            }

            return groups;
        }

        static List<List<Face>> GetFaceSelectionGroups(ProBuilderMesh mesh)
        {
            var wings = WingedEdge.GetWingedEdges(mesh, mesh.selectedFacesInternal, true);
            var filter = new HashSet<Face>();
            var groups = new List<List<Face>>();

            foreach (var wing in wings)
            {
                var group = new List<Face>() {};
                CollectAdjacentFaces(wing, filter, group);
                if (group.Count > 0)
                    groups.Add(group);
            }

            return groups;
        }

        static List<List<int>> GetVertexSelectionGroups(ProBuilderMesh mesh, bool collectCoincident)
        {
            if (!collectCoincident)
                return mesh.selectedIndexesInternal.Select(x => new List<int>() { x }).ToList();

            var shared = mesh.selectedSharedVertices;

            var groups = new List<List<int>>();

            foreach (var index in shared)
            {
                var coincident = new List<int>();
                mesh.GetCoincidentVertices(index, coincident);
                groups.Add(coincident);
            }

            return groups;
        }

        static List<List<Edge>> GetEdgeSelectionGroups(ProBuilderMesh mesh)
        {
            // todo Generating a WingedEdge collection just to get selection groups is wasteful
            var wings = WingedEdge.GetWingedEdges(mesh);
            var edges = mesh.selectedEdgesInternal;
            var filter = new HashSet<Edge>();
            var groups = new List<List<Edge>>();

            foreach (var edge in edges)
            {
                var group = new List<Edge>();
                group.Add(edge);
                groups.Add(group);

                var wing = wings.FirstOrDefault(x => x.edge.local.Equals(edge));

                if (wing == null)
                    continue;

                CollectedAdjacentEdges(wing, filter, group);
            }

            return groups;
        }

        static void CollectedAdjacentEdges(WingedEdge wing, HashSet<Edge> filter, List<Edge> group)
        {
            var edge = wing.edge.local;

            if (!filter.Add(edge))
                return;

            group.Add(edge);

            CollectedAdjacentEdges(wing.next, filter, group);
            CollectedAdjacentEdges(wing.previous, filter, group);
            CollectedAdjacentEdges(wing.opposite, filter, group);
        }

        static void CollectAdjacentFaces(WingedEdge wing, HashSet<Face> filter, List<Face> group)
        {
            if (!filter.Add(wing.face))
                return;

            group.Add(wing.face);

            var enumerator = new WingedEdgeEnumerator(wing);

            while (enumerator.MoveNext())
            {
                var opposite = enumerator.Current.opposite;
                if (opposite == null)
                    continue;
                CollectAdjacentFaces(opposite, filter, group);
            }
        }
    }
}
