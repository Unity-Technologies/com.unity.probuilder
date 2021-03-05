using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectVertexColor : MenuAction
    {
        Pref<bool> m_SearchSelectedObjectsOnly = new Pref<bool>("SelectVertexColor.restrictToSelectedObjects", false);
        GUIContent gc_restrictToSelection = new GUIContent("Current Selection", "Optionally restrict the matches to only those faces on currently selected objects.");
        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select by Colors",
                "Selects all faces matching the selected vertex colors."
            );

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_SelectByVertexColor", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedVertexCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get
            {
                if (enabled)
                    return MenuActionState.VisibleAndEnabled;

                return MenuActionState.Visible;
            }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Select by Vertex Color Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            m_SearchSelectedObjectsOnly.value = EditorGUILayout.Toggle(gc_restrictToSelection, m_SearchSelectedObjectsOnly);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select Vertex Color"))
            {
                PerformAction();
                SceneView.RepaintAll();
            }
        }

        protected override ActionResult PerformActionImplementation()
        {
            UndoUtility.RecordSelection("Select Faces with Vertex Colors");

            HashSet<Color32> colors = new HashSet<Color32>();

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                Color[] mesh_colors = pb.colorsInternal;

                if (mesh_colors == null || mesh_colors.Length != pb.vertexCount)
                    continue;

                foreach (int i in pb.selectedIndexesInternal)
                    colors.Add(mesh_colors[i]);
            }

            List<GameObject> newSelection = new List<GameObject>();
            bool selectionOnly = m_SearchSelectedObjectsOnly;

            IEnumerable<ProBuilderMesh> pool;

            if (selectionOnly)
                pool = MeshSelection.topInternal;
            else
                pool = Object.FindObjectsOfType<ProBuilderMesh>();

            //If the original selection does not have colors assigned we will select faces without colors
            if (colors.Count == 0)
            {
                foreach (ProBuilderMesh pb in pool)
                {
                    if (pb.colorsInternal == null)
                    {
                        List<Face> matches = new List<Face>();
                        Face[] faces = pb.facesInternal;

                        foreach(var face in faces)
                        {
                            matches.Add(face);
                        }

                        if (matches.Count > 0)
                        {
                            newSelection.Add(pb.gameObject);
                            pb.SetSelectedFaces(matches);
                        }
                    }
                }
            }
            else
            {
                foreach (ProBuilderMesh pb in pool)
                {
                    Color[] mesh_colors = pb.colorsInternal;

                    if (mesh_colors == null || mesh_colors.Length != pb.vertexCount)
                        continue;

                    List<Face> matches = new List<Face>();
                    Face[] faces = pb.facesInternal;

                    for (int i = 0; i < faces.Length; i++)
                    {
                        int[] tris = faces[i].distinctIndexesInternal;

                        for (int n = 0; n < tris.Length; n++)
                        {
                            if (colors.Contains((Color32)mesh_colors[tris[n]]))
                            {
                                matches.Add(faces[i]);
                                break;
                            }
                        }
                    }

                    if (matches.Count > 0)
                    {
                        newSelection.Add(pb.gameObject);
                        pb.SetSelectedFaces(matches);
                    }
                }
            }

            Selection.objects = newSelection.ToArray();

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Select Faces with Vertex Colors");
        }
    }
}
