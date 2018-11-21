using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    abstract class MeshAndElementGroupPair
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

        public MeshAndElementGroupPair(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation, bool collectCoincidentIndices)
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

        public static List<ElementGroup> GetElementGroups(ProBuilderMesh mesh, PivotPoint pivot, HandleOrientation orientation, bool collectCoincident)
        {
            var groups = new List<ElementGroup>();
            var trs = mesh.transform.localToWorldMatrix;
            var selectMode = ProBuilderEditor.selectMode;

            switch (pivot)
            {
                case PivotPoint.IndividualOrigins:
                {
                    // todo Pass SelectMode as part of args
                    if (selectMode != SelectMode.Face || mesh.selectedFaceCount < 1)
                    {
                        var bounds = Math.GetBounds(mesh.positionsInternal, mesh.selectedIndexesInternal);
                        var indices = collectCoincident
                            ? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
                            : new List<int>(mesh.selectedIndexesInternal);

                        var rot = selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge)
                            ? EditorHandleUtility.GetEdgeRotation(mesh, mesh.selectedEdgesInternal, orientation)
                            : EditorHandleUtility.GetVertexRotation(mesh, indices, orientation);

                        var post = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), rot, Vector3.one);

                        groups.Add(new ElementGroup()
                        {
                            m_Indices = indices,
                            m_PostApplyPositionsMatrix = post,
                            m_PreApplyPositionsMatrix = post.inverse
                        });
                    }
                    else
                    {
                        foreach (var list in GetFaceSelectionGroups(mesh))
                        {
                            var bounds = Math.GetBounds(mesh.positionsInternal, list);
                            var rot = EditorHandleUtility.GetFaceRotation(mesh, list, orientation);
                            var post = Matrix4x4.TRS(trs.MultiplyPoint3x4(bounds.center), rot, Vector3.one);

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

                            groups.Add(new ElementGroup()
                            {
                                m_Indices = indices,
                                m_PostApplyPositionsMatrix = post,
                                m_PreApplyPositionsMatrix = post.inverse
                            });
                        }
                    }

                    break;
                }

                case PivotPoint.ActiveElement:
                {
                    var post = Matrix4x4.TRS(MeshSelection.GetHandlePosition(), MeshSelection.GetHandleRotation(), Vector3.one);

                    groups.Add(new ElementGroup()
                    {
                        m_Indices = collectCoincident
                            ? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
                            : new List<int>(mesh.selectedIndexesInternal),
                        m_PostApplyPositionsMatrix = post,
                        m_PreApplyPositionsMatrix = post.inverse,
                    });

                    break;
                }

                default:
                {
                    var post = Matrix4x4.TRS(MeshSelection.GetHandlePosition(), MeshSelection.GetHandleRotation(), Vector3.one);

                    groups.Add(new ElementGroup()
                    {
                        m_Indices = collectCoincident
                            ? mesh.GetCoincidentVertices(mesh.selectedIndexesInternal)
                            : new List<int>(mesh.selectedIndexesInternal),
                        m_PostApplyPositionsMatrix = post,
                        m_PreApplyPositionsMatrix = post.inverse,
                    });

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
