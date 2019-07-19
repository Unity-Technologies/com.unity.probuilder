using System;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class WeldVertices : MenuAction
    {
        Pref<float> m_WeldDistance = new Pref<float>("WeldVertices.weldDistance", .01f);
        static readonly GUIContent gc_weldDistance = new GUIContent("Weld Distance", "The maximum distance between two vertices in order to be welded together.");
        const float k_MinWeldDistance = .00001f;

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Vert_Weld", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Weld Vertices",
                @"Searches the current selection for vertices that are within the specified distance of on another and merges them into a single vertex.",
                keyCommandAlt, 'V'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedSharedVertexCountObjectMax > 1; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Weld Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_WeldDistance.value = EditorGUILayout.FloatField(gc_weldDistance, m_WeldDistance);

            if (EditorGUI.EndChangeCheck())
            {
                if (m_WeldDistance < k_MinWeldDistance)
                    m_WeldDistance.value = k_MinWeldDistance;
                ProBuilderSettings.Save();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Weld Vertices"))
                DoAction();
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            ActionResult res = ActionResult.NoSelection;

            UndoUtility.RecordSelection("Weld Vertices");

            int weldCount = 0;

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                weldCount += mesh.sharedVerticesInternal.Length;

                if (mesh.selectedIndexesInternal.Length > 1)
                {
                    mesh.ToMesh();

                    var selectedVertices  = mesh.GetCoincidentVertices(mesh.selectedVertices);
                    int[] welds = mesh.WeldVertices(mesh.selectedIndexesInternal, m_WeldDistance);
                    res = welds != null ? new ActionResult(ActionResult.Status.Success, "Weld Vertices") : new ActionResult(ActionResult.Status.Failure, "Failed Weld Vertices");

                    if (res)
                    {
                        var newSelection = welds ?? new int[0] { };

                        if (MeshValidation.ContainsDegenerateTriangles(mesh))
                        {
                            List<int> removedIndices = new List<int>();
                            var vertexCount = mesh.vertexCount;

                            if(MeshValidation.RemoveDegenerateTriangles(mesh, removedIndices))
                            {
                                if (removedIndices.Count < vertexCount)
                                {
                                    var newlySelectedVertices = new List<int>();
                                    selectedVertices.Sort();
                                    removedIndices.Sort();

                                    int count = 0;

                                    for (int i = 0; i < selectedVertices.Count; i++)
                                    {
                                        if (count >= removedIndices.Count || selectedVertices[i] != removedIndices[count])
                                        {
                                            newlySelectedVertices.Add(selectedVertices[i] - UnityEngine.ProBuilder.ArrayUtility.NearestIndexPriorToValue(removedIndices, selectedVertices[i]) - 1);
                                        }
                                        else
                                        {
                                            ++count;
                                        }
                                    }

                                    newSelection = newlySelectedVertices.ToArray();
                                }
                                else
                                {
                                    newSelection = new int[0];
                                }
                            }
                            mesh.ToMesh();
                        }
                        mesh.SetSelectedVertices(newSelection);
                    }

                    mesh.Refresh();
                    mesh.Optimize();
                }

                weldCount -= mesh.sharedVerticesInternal.Length;
            }

            ProBuilderEditor.Refresh();

            if (res && weldCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Weld " + weldCount + (weldCount > 1 ? " Vertices" : " Vertex"));

            return new ActionResult(ActionResult.Status.Failure, "Nothing to Weld");
        }
    }
}
