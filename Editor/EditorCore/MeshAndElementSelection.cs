using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Represents the state of a ProBuilderMesh and it's selected elements.
    /// </summary>
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

        public IEnumerable<int> indices
        {
            get { return m_Indices; }
        }

        public ElementGroup(List<int> indices, Vector3 position, Quaternion rotation)
        {
            m_Indices = indices;

            m_Position = position;
            m_Rotation = rotation;
        }

        internal static List<int> GetSelectedIndicesForSelectMode(ProBuilderMesh mesh, SelectMode mode, bool collectCoincident)
        {
            if (mode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
            {
                List<int> indices = new List<int>();

                if (collectCoincident)
                    mesh.GetCoincidentVertices(mesh.selectedFacesInternal, indices);
                else
                    Face.GetDistinctIndices(mesh.selectedFacesInternal, indices);

                return indices;
            }
            else if(mode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
            {
                List<int> indices = new List<int>();

                if (collectCoincident)
                    mesh.GetCoincidentVertices(mesh.selectedEdgesInternal, indices);
                else
                    Edge.GetIndices(mesh.selectedEdgesInternal, indices);

                return indices;
            }

            return collectCoincident
                ? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
                : new List<int>(mesh.selectedIndexesInternal);
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
                    if (selectMode.ContainsFlag(SelectMode.Vertex | SelectMode.TextureVertex))
                    {
                        foreach (var list in GetVertexSelectionGroups(mesh, collectCoincident))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = UnityEngine.ProBuilder.HandleUtility.GetVertexRotation(mesh, orientation, list);
                            groups.Add(new ElementGroup(list, trs.MultiplyPoint3x4(bounds.center), rot));
                        }
                    }
                    else if (selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                    {
                        foreach (var list in GetEdgeSelectionGroups(mesh))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = UnityEngine.ProBuilder.HandleUtility.GetEdgeRotation(mesh, orientation, list);

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
                    else if (selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                    {
                        foreach (var list in GetFaceSelectionGroups(mesh))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = UnityEngine.ProBuilder.HandleUtility.GetFaceRotation(mesh, orientation, list);
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
                    var indices = GetSelectedIndicesForSelectMode(mesh, selectMode, collectCoincident);
                    var position = mesh.transform.position;
                    var rotation = mesh.transform.rotation;

                    if (selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                    {
                        var face = mesh.GetActiveFace();

                        if (face != null)
                        {
                            position = trs.MultiplyPoint3x4(Math.GetBounds(mesh.positionsInternal, face.distinctIndexesInternal).center);
                            rotation = UnityEngine.ProBuilder.HandleUtility.GetFaceRotation(mesh, orientation, new Face[] { face });
                        }
                    }
                    else if (selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                    {
                        var edge = mesh.GetActiveEdge();

                        if (edge != Edge.Empty)
                        {
                            position = trs.MultiplyPoint3x4(Math.GetBounds(mesh.positionsInternal, new int [] { edge.a, edge.b }).center);
                            rotation = UnityEngine.ProBuilder.HandleUtility.GetEdgeRotation(mesh, orientation, new Edge[] { edge });
                        }
                    }
                    else if (selectMode.ContainsFlag(SelectMode.Vertex | SelectMode.TextureVertex))
                    {
                        var vertex = mesh.GetActiveVertex();

                        if (vertex > -1)
                        {
                            position = trs.MultiplyPoint3x4(mesh.positionsInternal[vertex]);
                            rotation = UnityEngine.ProBuilder.HandleUtility.GetVertexRotation(mesh, orientation, new int[] { vertex });
                        }
                    }

                    groups.Add(new ElementGroup(indices, position, rotation));
                    break;
                }

                default:
                {
                    var indices = GetSelectedIndicesForSelectMode(mesh, selectMode, collectCoincident);
                    var position = MeshSelection.bounds.center;
                    var rotation = Quaternion.identity;

                    if (selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                    {
                        var face = mesh.GetActiveFace();

                        if (face != null)
                            rotation = UnityEngine.ProBuilder.HandleUtility.GetFaceRotation(mesh, orientation, new Face[] { face });
                    }
                    else if (selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                    {
                        var edge = mesh.GetActiveEdge();

                        if (edge != Edge.Empty)
                            rotation = UnityEngine.ProBuilder.HandleUtility.GetEdgeRotation(mesh, orientation, new Edge[] { edge });
                    }
                    else if (selectMode.ContainsFlag(SelectMode.Vertex | SelectMode.TextureVertex))
                    {
                        var vertex = mesh.GetActiveVertex();

                        if (vertex > -1)
                            rotation = UnityEngine.ProBuilder.HandleUtility.GetVertexRotation(mesh, orientation, new int[] { vertex });
                    }

                    groups.Add(new ElementGroup( indices, position, rotation));
                    break;
                }
            }

            return groups;
        }

        static IEnumerable<List<Face>> GetFaceSelectionGroups(ProBuilderMesh mesh)
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

        static IEnumerable<List<int>> GetVertexSelectionGroups(ProBuilderMesh mesh, bool collectCoincident)
        {
            if (!collectCoincident)
                return mesh.selectedIndexesInternal.Select(x => new List<int>() { x }).ToList();

            var shared = mesh.selectedSharedVertices;

            var groups = new List<List<int>>();

            foreach (var index in shared)
            {
                var coincident = new List<int>();
                mesh.GetCoincidentVertices(mesh.sharedVerticesInternal[index][0], coincident);
                groups.Add(coincident);
            }

            return groups;
        }

        static IEnumerable<List<Edge>> GetEdgeSelectionGroups(ProBuilderMesh mesh)
        {
            var edges = EdgeLookup.GetEdgeLookup(mesh.selectedEdgesInternal, mesh.sharedVertexLookup);
            var groups = new List<SimpleTuple<HashSet<int>, List<Edge>>>();

            foreach (var edge in edges)
            {
                var foundMatch = false;

                foreach (var kvp in groups)
                {
                    if (kvp.item1.Contains(edge.common.a) || kvp.item1.Contains(edge.common.b))
                    {
                        kvp.item1.Add(edge.common.a);
                        kvp.item1.Add(edge.common.b);
                        kvp.item2.Add(edge.local);
                        foundMatch = true;
                        break;
                    }
                }

                if (!foundMatch)
                    groups.Add(new SimpleTuple<HashSet<int>, List<Edge>>(
                        new HashSet<int>() { edge.common.a, edge.common.b },
                        new List<Edge>() { edge.local }));
            }

            // collect overlapping groups (happens in cases where selection order begins as two separate groups but
            // becomes one)
            var res = new List<List<Edge>>();
            var overlap = new HashSet<int>();

            for(int i = 0, c = groups.Count; i < c; i++)
            {
                if (overlap.Contains(i))
                    continue;

                List<Edge> grp = groups[i].item2;

                for (int n = i + 1; n < c; n++)
                {
                    if (groups[i].item1.Overlaps(groups[n].item1))
                    {
                        overlap.Add(n);
                        grp.AddRange(groups[n].item2);
                    }
                }

                res.Add(grp);
            }

            return res;
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
