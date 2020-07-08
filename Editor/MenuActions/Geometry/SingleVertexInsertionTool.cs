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
                float hitDistance = Mathf.Infinity;

                Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
                RaycastHit pbHit;

                ProBuilderMesh targetedMesh = MeshSelection.activeMesh;
                if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, targetedMesh, out pbHit))
                {
                    UndoUtility.RecordObject(targetedMesh.gameObject, "Add Vertex On Face");

                    Face hitFace = targetedMesh.faces[pbHit.face];

                    targetedMesh.InsertVertexInFace(hitFace, pbHit.point);

                    Debug.Log("Insertion Done");

                    evt.Use();
                }
            }
        }

    }

}
