using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using Math = System.Math;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class FreezeTransform : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Pivot_Reset", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Freeze Transform",
                @"Set the pivot point to world coordinates (0,0,0) and clear all Transform values while keeping the mesh in place."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public bool ShouldFlipFaces(Vector3 scale)
        {
            var globalSign = Mathf.Sign(scale.x) * Mathf.Sign(scale.y) * Mathf.Sign(scale.z);
            return globalSign < 0;
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Freeze Transform", true)]
        static bool ValidateFreezeTransformAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Freeze Transform", false, 19)]
        static void FreezeTransformAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<FreezeTransform>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
#endif

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordMeshAndTransformSelection("Freeze Transforms");

            var selection = MeshSelection.topInternal;
            Vector3[][] positions = new Vector3[selection.Count][];

            for (int i = 0, c = selection.Count; i < c; i++)
                positions[i] = selection[i].VerticesInWorldSpace();

            for (int i = 0, c = selection.Count; i < c; i++)
            {
                ProBuilderMesh pb = selection[i];
                bool flipFaces = ShouldFlipFaces(pb.transform.localScale);

                pb.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                pb.transform.localScale = Vector3.one;

                foreach(Face face in pb.facesInternal)
                {
                    face.manualUV = true;
                    if(flipFaces)
                        face.Reverse();
                }

                pb.positions = positions[i];

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();

            return new ActionResult(ActionResult.Status.Success, "Freeze Transforms");
        }
    }
}
