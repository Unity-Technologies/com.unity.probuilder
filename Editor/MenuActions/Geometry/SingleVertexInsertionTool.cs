using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Vertex Insertion", typeof(ProBuilderMesh))]
    public class SingleVertexInsertionTool : EditorTool
    {

        void OnEnable()
        {
            //Selection.selectionChanged += SelectionChanged;
            //MeshSelection.objectSelectionChanged += MeshSelectionChanged;
        }

        private void SelectionChanged()
        {
            Debug.Log("Selection Changed");
        }

        private void MeshSelectionChanged()
        {
            Debug.Log("Mesh Selection Changed");
        }

        void OnDisable()
        {
            //Selection.selectionChanged -= SelectionChanged;
            //MeshSelection.objectSelectionChanged -= MeshSelectionChanged;
        }

        // This is called for each window that your tool is active in. Put the functionality of your tool here.
        public override void OnToolGUI(EditorWindow window)
        {
            Event currentEvent = Event.current;

            if (EditorHandleUtility.SceneViewInUse(currentEvent))
                return;

            DoPointPlacement();

            ProBuilderEditor.Refresh();
            //ProBuilderEditor.UpdateMeshHandles(true);
        }


        private void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            if (evtType == EventType.MouseDown)
            {
                Debug.Log("Mesh selection active : " +target.name);
                float hitDistance = Mathf.Infinity;

                Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
                RaycastHit pbHit;

                ProBuilderMesh targetedMesh = MeshSelection.activeMesh;
                if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, targetedMesh, out pbHit))
                {
                    UndoUtility.RecordObject(targetedMesh.gameObject, "Add Vertex On Face");

                    Face hitFace = targetedMesh.faces[pbHit.face];

                    AddVerticesToFace(targetedMesh,hitFace, pbHit.point);

                    Debug.Log("Insertion Done");

                    evt.Use();
                }
            }
        }


        /// <summary>
        /// Add a set of points to a face and retriangulate. Points are added to the nearest edge.
        /// </summary>
        /// <param name="mesh">The source mesh.</param>
        /// <param name="face">The face to append points to.</param>
        /// <param name="points">Points to added to the face.</param>
        /// <returns>The face created by appending the points.</returns>
        public Face[] AddVerticesToFace(ProBuilderMesh mesh, Face face, Vector3 point)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (face == null)
                throw new ArgumentNullException("face");

            if (point == null)
                throw new ArgumentNullException("point");

            List<Vertex> vertices = mesh.GetVertices().ToList();
            List<Face> faces = new List<Face>(mesh.facesInternal);
            Dictionary<int, int> lookup = mesh.sharedVertexLookup;
            Dictionary<int, int> lookupUV = null;

            if (mesh.sharedTextures != null)
            {
                lookupUV = new Dictionary<int, int>();
                SharedVertex.GetSharedVertexLookup(mesh.sharedTextures, lookupUV);
            }

            List<Edge> wound = WingedEdge.SortEdgesByAdjacency(face);
            List<FaceRebuildData> newFacesData = new List<FaceRebuildData>();

            Vertex newVertex = new Vertex();
            newVertex.position = point;

            for (int i = 0; i < wound.Count; i++)
            {
                List<Vertex> n_vertices = new List<Vertex>();
                List<int> n_shared = new List<int>();
                List<int> n_sharedUV = lookupUV != null ? new List<int>() : null;

                n_vertices.Add(vertices[wound[i].a]);
                n_vertices.Add(vertices[wound[i].b]);
                n_vertices.Add(newVertex);

                n_shared.Add(lookup[wound[i].a]);
                n_shared.Add(lookup[wound[i].b]);
                n_shared.Add(vertices.Count);

                if (lookupUV != null)
                {
                    int uv;
                    lookupUV.Clear();

                    if (lookupUV.TryGetValue(wound[i].a, out uv))
                        n_sharedUV.Add(uv);
                    else
                        n_sharedUV.Add(-1);

                    if (lookupUV.TryGetValue(wound[i].b, out uv))
                        n_sharedUV.Add(uv);
                    else
                        n_sharedUV.Add(-1);

                    n_sharedUV.Add(vertices.Count);
                }

                List<int> triangles;

                try
                {
                    Triangulation.TriangulateVertices(n_vertices, out triangles, true);
                }
                catch
                {
                    Debug.Log("Failed triangulating face after appending vertices.");
                    return null;
                }

                FaceRebuildData data = new FaceRebuildData();

                data.face = new Face(triangles.ToArray(), face.submeshIndex, new AutoUnwrapSettings(face.uv), face.smoothingGroup, face.textureGroup, -1, face.manualUV);
                data.vertices           = n_vertices;
                data.sharedIndexes      = n_shared;
                data.sharedIndexesUV    = n_sharedUV;

                newFacesData.Add(data);
            }

            FaceRebuildData.Apply(newFacesData,
                vertices,
                faces,
                lookup,
                lookupUV);


            mesh.SetVertices(vertices);
            mesh.faces = faces;
            mesh.SetSharedVertices(lookup);
            mesh.SetSharedTextures(lookupUV);

            Face[] newFaces = newFacesData.Select(f => f.face).ToArray();

            foreach (FaceRebuildData data in newFacesData)
            {
                var newFace = data.face;

                // check old normal and make sure this new face is pointing the same direction
                Vector3 oldNrm = UnityEngine.ProBuilder.Math.Normal(mesh, face);
                Vector3 newNrm = UnityEngine.ProBuilder.Math.Normal(mesh, newFace);

                if (Vector3.Dot(oldNrm, newNrm) < 0)
                    newFace.Reverse();
            }

            mesh.DeleteFace(face);

            return newFaces;
        }

    }

}
