using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SplitVertices : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Vert_Split", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Split Vertices",
                @"Disconnects vertices that share the same position in space so that they may be moved independently of one another.",
                keyCommandAlt, 'X'
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedVertexCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            int splitCount = 0;
            UndoUtility.RecordSelection("Split Vertices");

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                // loose verts to split
                List<int> tris = new List<int>(mesh.selectedIndexesInternal);

                if (mesh.selectedFacesInternal.Length > 0)
                {
                    int[] sharedVertexHandles = new int[mesh.selectedIndexesInternal.Length];

                    // Get shared index index for each vert in selection
                    for (int i = 0; i < mesh.selectedIndexesInternal.Length; i++)
                        sharedVertexHandles[i] = mesh.GetSharedVertexHandle(mesh.selectedIndexesInternal[i]);

                    // cycle through selected faces and remove the tris that compose full faces.
                    foreach (Face face in mesh.selectedFacesInternal)
                    {
                        List<int> faceSharedIndexes = new List<int>();

                        for (int j = 0; j < face.distinctIndexesInternal.Length; j++)
                            faceSharedIndexes.Add(mesh.GetSharedVertexHandle(face.distinctIndexesInternal[j]));

                        List<int> usedTris = new List<int>();
                        for (int i = 0; i < sharedVertexHandles.Length; i++)
                            if (faceSharedIndexes.Contains(sharedVertexHandles[i]))
                                usedTris.Add(mesh.selectedIndexesInternal[i]);

                        // This face *is* composed of selected tris.  Remove these tris from the loose index list
                        foreach (int i in usedTris)
                            if (tris.Contains(i))
                                tris.Remove(i);
                    }
                }

                // Now split the faces, and any loose vertices
                mesh.DetachFaces(mesh.selectedFacesInternal);

                splitCount += mesh.selectedIndexesInternal.Length;
                mesh.SplitVertices(mesh.selectedIndexesInternal);

                // Reattach detached face vertices (if any are to be had)
                if (mesh.selectedFacesInternal.Length > 0)
                    mesh.WeldVertices(mesh.selectedFacesInternal.SelectMany(x => x.indexes), Mathf.Epsilon);

                // And set the selected triangles to the newly split
                List<int> newTriSelection = new List<int>(mesh.selectedFacesInternal.SelectMany(x => x.indexes));
                newTriSelection.AddRange(tris);
                mesh.SetSelectedVertices(newTriSelection.ToArray());

                mesh.ToMesh();
                mesh.Refresh();
                mesh.Optimize();
            }

            ProBuilderEditor.Refresh();

            if (splitCount > 0)
                return new ActionResult(ActionResult.Status.Success, "Split " + splitCount + (splitCount > 1 ? " Vertices" : " Vertex"));
            else
                return new ActionResult(ActionResult.Status.Failure, "Split Vertices\nInsuffient Vertices Selected");
        }
    }
}
