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
    sealed class NewBezierMesh : MenuAction
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
            get { return "New Bezier Mesh"; }
        }

        /// <summary>
        /// What to show in the hover tooltip window.
        /// TooltipContent is similar to GUIContent, with the exception that it also includes an optional params[]
        /// char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
        /// </summary>
        static readonly TooltipContent k_Tooltip = new TooltipContent
        (
            "New Bezier Mesh",
            "Create a bezier mesh using splines package."
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
            //CreateProBuilderMeshCylinder();

            CreateNewBezierMesh();

            Debug.Log("clicked on my menu action !");
            return new ActionResult(ActionResult.Status.Success, "Created Bezier Mesh using Splines");
        }

        private void CreateNewBezierMesh()
        {
            GameObject go = new GameObject();
            var bezier = go.AddComponent<BezierMesh>();
            go.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            bezier.Init();
            // bezier.Extrude2DMesh();
            // bezier.Extrude2DMeshOptimized();
            bezier.Extrude3DMesh();
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

        private void CreateProBuilderMeshCylinder()
        {
            float radius = .5f;
            int height = 1;
            int divisions = 16;
            int m_HeightCuts = 0;
            float heightStep = height / (m_HeightCuts + 1);

            Vector2[] circle = new Vector2[divisions];

            // get a circle
            for (int i = 0; i < divisions; i++)
            {
                float angle = i * 360f / divisions;
                circle[i] = Math.PointInEllipseCircumference(radius, radius, angle, Vector2.zero, out _);
            }

            // add two because end caps
            Vector3[] vertices = new Vector3[(divisions * (height + 1) * 4) + (divisions * 6)];
            Face[] faces = new Face[divisions * (height + 1) + (divisions * 2)];

            // build vertex array
            int it = 0;

            // +1 to account for 0 height cuts
            for (int i = 0; i < height; i++)
            {
                float Y = i * heightStep - height * .5f;
                float Y2 = (i + 1) * heightStep - height * .5f;

                for (int n = 0; n < divisions; n++)
                {
                    vertices[it + 0] = new Vector3(circle[n + 0].x, Y, circle[n + 0].y);
                    vertices[it + 1] = new Vector3(circle[n + 0].x, Y2, circle[n + 0].y);

                    if (n != divisions - 1)
                    {
                        vertices[it + 2] = new Vector3(circle[n + 1].x, Y, circle[n + 1].y);
                        vertices[it + 3] = new Vector3(circle[n + 1].x, Y2, circle[n + 1].y);
                    }
                    else
                    {
                        vertices[it + 2] = new Vector3(circle[0].x, Y, circle[0].y);
                        vertices[it + 3] = new Vector3(circle[0].x, Y2, circle[0].y);
                    }

                    it += 4;
                }
            }

            // wind side faces
            int f = 0;
            for (int i = 0; i < height + 1; i++)
            {
                for (int n = 0; n < divisions * 4; n += 4)
                {
                    int index = (i * (divisions * 4)) + n;
                    int zero = index;
                    int one = index + 1;
                    int two = index + 2;
                    int three = index + 3;

                    faces[f++] = new Face(
                        new int[6] { zero, one, two, one, three, two },
                        0,
                        AutoUnwrapSettings.tile,
                        1,
                        -1,
                        -1,
                        false);
                }
            }

            // construct caps separately, cause they aren't wound the same way
            int ind = (divisions * (height + 1) * 4);
            int f_ind = divisions * (height + 1);

            for (int n = 0; n < divisions; n++)
            {
                // bottom faces
                // var bottomCapHeight = -height * .5f;
                // vertices[ind + 0] = new Vector3(circle[n].x, bottomCapHeight, circle[n].y);
                //
                // vertices[ind + 1] = new Vector3(0f, bottomCapHeight, 0f);
                //
                // if (n != divisions - 1)
                //     vertices[ind + 2] = new Vector3(circle[n + 1].x, bottomCapHeight, circle[n + 1].y);
                // else
                //     vertices[ind + 2] = new Vector3(circle[000].x, bottomCapHeight, circle[000].y);

                faces[f_ind + n] = new Face(new int[3] { ind + 2, ind + 1, ind + 0 });

                ind += 3;

                // top faces
                // var topCapHeight = height * .5f;
                // vertices[ind + 0] = new Vector3(circle[n].x, topCapHeight, circle[n].y);
                // vertices[ind + 1] = new Vector3(0f, topCapHeight, 0f);
                // if (n != divisions - 1)
                //     vertices[ind + 2] = new Vector3(circle[n + 1].x, topCapHeight, circle[n + 1].y);
                // else
                //     vertices[ind + 2] = new Vector3(circle[000].x, topCapHeight, circle[000].y);

                faces[f_ind + (n + divisions)] = new Face(new int[3] { ind + 0, ind + 1, ind + 2 });

                ind += 3;
            }

            ProBuilderMesh mesh = ProBuilderMesh.Create(vertices, faces);
            mesh.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
        }
    }
}
