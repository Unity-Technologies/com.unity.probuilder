using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(SubdivideOnVertex))]
    public class SubdivideOnVertexEditor : Editor
    {

        SubdivideOnVertex vertexOnFace
        {
            get { return target as SubdivideOnVertex; }
        }

        private int m_ControlId;

        void OnEnable()
        {
            if (vertexOnFace == null)
            {
                DestroyImmediate(this);
                return;
            }

            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            Undo.undoRedoPerformed += UndoRedoPerformed;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate += DuringSceneGUI;
#endif
        }

        void OnDisable()
        {
            // Quit Edit mode when the object gets de-selected.
            if (vertexOnFace != null && vertexOnFace.vertexEditMode == SubdivideOnVertex.VertexEditMode.Edit)
                vertexOnFace.vertexEditMode = SubdivideOnVertex.VertexEditMode.None;

            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= DuringSceneGUI;
#endif
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            //Removing the script from the object
            DestroyImmediate(vertexOnFace);
        }


        private void DuringSceneGUI(SceneView obj)
        {
            if (vertexOnFace.vertexEditMode == SubdivideOnVertex.VertexEditMode.None)
                return;

            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.KeyDown)
                HandleKeyEvent(currentEvent);

            if (EditorHandleUtility.SceneViewInUse(currentEvent))
                return;

            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
            if (currentEvent.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_ControlId);

            DoPointPlacement();
        }

        private void UndoRedoPerformed()
        {
            //throw new System.NotImplementedException();
        }

        private void OnSelectModeChanged(SelectMode obj)
        {
            //throw new System.NotImplementedException();
        }

        private void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            if (vertexOnFace.vertexEditMode == SubdivideOnVertex.VertexEditMode.Edit)
            {
                if (evtType == EventType.MouseDown && HandleUtility.nearestControl == m_ControlId)
                {
                    Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    RaycastHit pbHit;

                    if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, vertexOnFace.mesh, out pbHit))
                    {
                        UndoUtility.RecordObject(vertexOnFace, "Add Vertex On Face");

                        Face hitFace = vertexOnFace.mesh.faces[pbHit.face];
                        vertexOnFace.m_vertexToAdd = new SimpleTuple<Face, Vector3>(hitFace,pbHit.point);

                        UpdateProBuilderMesh();

                        evt.Use();
                    }
                }
            }
        }


        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Escape:
                {
                    DestroyImmediate(vertexOnFace);
                    evt.Use();
                    break;
                }
            }
        }

        public void UpdateProBuilderMesh()
        {
            UndoUtility.RecordObject(vertexOnFace.mesh, "Add Vertex to ProBuilder Mesh");

            //vertexOnFace.mesh.AppendVerticesToFace(vertexOnFace.m_vertexToAdd.item1, new Vector3[]{vertexOnFace.m_vertexToAdd.item2},false);
            AddVerticesToFace(vertexOnFace.mesh,vertexOnFace.m_vertexToAdd.item1, vertexOnFace.m_vertexToAdd.item2);

            UndoUtility.RecordObject(vertexOnFace, "Removing Script from ProBuilder Object");
            DestroyImmediate(vertexOnFace);

            Debug.Log("Insertion Done");
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
