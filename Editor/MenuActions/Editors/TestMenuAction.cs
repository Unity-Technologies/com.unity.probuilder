using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder.Actions
{
    [ProBuilderMenuAction]
    sealed class TestMenuAction : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Tool; }
        }

        public override Texture2D icon
        {
            get { return null; }
        }

        public override TooltipContent tooltip
        {
            get { return k_Tooltip; }
        }

        public override string menuTitle
        {
            get { return "Test Menu Action"; }
        }

        /// <summary>
        /// What to show in the hover tooltip window.
        /// TooltipContent is similar to GUIContent, with the exception that it also includes an optional params[]
        /// char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
        /// </summary>
        static readonly TooltipContent k_Tooltip = new TooltipContent
        (
            "Test Menu Action Tooltip",
            "My test menu action tooltip."
        );

        /// <summary>
        /// Determines if the action should be enabled or grayed out.
        /// </summary>
        /// <returns></returns>
        public override bool enabled
        {
            get { return true; }
        }

        /// <summary>
        /// This action is applicable in any selection modes.
        /// </summary>
        public override SelectMode validSelectModes
        {
            get { return SelectMode.Any; }
        }

        /// <summary>
        /// Return a pb_ActionResult indicating the success/failure of action.
        /// </summary>
        /// <returns></returns>
        protected override ActionResult PerformActionImplementation()
        {
            // CreateProBuilderMeshCube();
            CreateProBuilderMeshCylinder();

            Debug.Log("clicked on my menu action !");
            return new ActionResult(ActionResult.Status.Success, "Success !");
        }

        private void CreateProBuilderMeshCylinder()
        {
            float radius = .5f;
            int height = 1;
            int divisions = 16;

            Vector2[] circle = new Vector2[divisions];

            // get a circle
            for (int i = 0; i < divisions; i++)
            {
                float angle = i * 360f / divisions;
                circle[i] = Math.PointInEllipseCircumference(radius, radius, angle, Vector2.zero, out _);
            }
        }

        private void CreateProBuilderMeshCube()
        {
            Vector3[] vertices =
            {
                new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0),
                new Vector3(1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 0),
                new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(0, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 1),
                new Vector3(1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 1),
                new Vector3(1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, 1),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1)
            };

            int[][] triangles =
            {
                new int[]
                {
                    0, 6, 3, //face front
                    0, 9, 6
                },
                new int[]
                {
                    7, 10, 12, //face top
                    7, 12, 15
                },
                new int[]
                {
                    4, 8, 16, //face right
                    4, 16, 18
                },
                new int[]
                {
                    1, 21, 13, //face left
                    1, 13, 11
                },
                new int[]
                {
                    17, 14, 22, //face back
                    17, 22, 19
                },
                new int[]
                {
                    2, 20, 23, //face bottom
                    2, 5, 20
                }
            };

            List<Face> faces = new List<Face>()
            {
                new Face(triangles[0]),
                new Face(triangles[1]),
                new Face(triangles[2]),
                new Face(triangles[3]),
                new Face(triangles[4]),
                new Face(triangles[5])
            };

            ProBuilderMesh mesh = ProBuilderMesh.Create(vertices, faces);
            mesh.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
        }
    }
}
