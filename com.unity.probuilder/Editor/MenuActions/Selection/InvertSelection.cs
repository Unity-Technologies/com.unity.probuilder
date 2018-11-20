using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class InvertSelection : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_Invert", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Invert Selection",
                @"Selects the opposite of the current selection. Eg, all unselected elements will become selected, the current selection will be unselected.",
                keyCommandSuper, keyCommandShift, 'I'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Invert Selection");

            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Vertex:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        SharedVertex[] sharedIndexes = mesh.sharedVerticesInternal;
                        List<int> selectedSharedIndexes = new List<int>();

                        foreach (int i in mesh.selectedIndexesInternal)
                            selectedSharedIndexes.Add(mesh.GetSharedVertexHandle(i));

                        List<int> inverse = new List<int>();

                        for (int i = 0; i < sharedIndexes.Length; i++)
                        {
                            if (!selectedSharedIndexes.Contains(i))
                                inverse.Add(sharedIndexes[i][0]);
                        }

                        mesh.SetSelectedVertices(inverse.ToArray());
                    }
                    break;

                case SelectMode.Face:
                case SelectMode.TextureFace:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        IEnumerable<Face> inverse = mesh.facesInternal.Where(x => !mesh.selectedFacesInternal.Contains(x));
                        mesh.SetSelectedFaces(inverse.ToArray());
                    }
                    break;

                case SelectMode.Edge:

                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        var universalEdges = mesh.GetSharedVertexHandleEdges(mesh.facesInternal.SelectMany(x => x.edges)).ToArray();
                        var universal_selected_edges = EdgeUtility.GetSharedVertexHandleEdges(mesh, mesh.selectedEdges).Distinct();
                        Edge[] inverse_universal = System.Array.FindAll(universalEdges, x => !universal_selected_edges.Contains(x));
                        Edge[] inverse = new Edge[inverse_universal.Length];

                        for (int n = 0; n < inverse_universal.Length; n++)
                            inverse[n] = new Edge(mesh.sharedVerticesInternal[inverse_universal[n].a][0], mesh.sharedVerticesInternal[inverse_universal[n].b][0]);

                        mesh.SetSelectedEdges(inverse);
                    }
                    break;
            }

            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();

            return new ActionResult(ActionResult.Status.Success, "Invert Selection");
        }
    }
}
