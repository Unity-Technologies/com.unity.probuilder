using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class FillHole : MenuAction
    {
        Pref<bool> m_SelectEntirePath = new Pref<bool>("FillHole.selectEntirePath", true);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Edge_FillHole";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Fill Hole",
                @"Create a new face connecting all selected vertices."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Edge | SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && (MeshSelection.selectedEdgeCount > 0 || MeshSelection.selectedSharedVertexCount > 0); }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        public override VisualElement CreateSettingsContent()
        {
            var root = new VisualElement();

            var toggle = new Toggle("Fill Entire Hole");
            toggle.tooltip = "Fill Hole can optionally fill entire holes (default) or just the selected vertices on the hole edges. If no elements are selected, the entire object will be scanned for holes.";
            toggle.style.flexGrow =1;
            toggle.SetValueWithoutNotify(m_SelectEntirePath);
            toggle.RegisterCallback<ChangeEvent<bool>>(OnFillHoleToggleChanged);
            root.Add(toggle);

            return root;
        }

        void OnFillHoleToggleChanged(ChangeEvent<bool> evt)
        {
            m_SelectEntirePath.SetValue(evt.newValue);
            PreviewActionManager.UpdatePreview();
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Fill Hole Settings", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox("Fill Hole can optionally fill entire holes (default) or just the selected vertices on the hole edges.\n\nIf no elements are selected, the entire object will be scanned for holes.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            m_SelectEntirePath.value = EditorGUILayout.Toggle("Fill Entire Hole", m_SelectEntirePath);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Fill Hole"))
                EditorUtility.ShowNotification(PerformAction().notification);
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Fill Hole");

            ActionResult res = new ActionResult(ActionResult.Status.NoChange, "No Holes Found");
            int filled = 0;
            bool wholePath = m_SelectEntirePath;

            foreach (ProBuilderMesh mesh in MeshSelection.topInternal)
            {
                bool selectAll = mesh.selectedIndexesInternal == null || mesh.selectedIndexesInternal.Length < 1;
                IEnumerable<int> indexes = selectAll ? mesh.facesInternal.SelectMany(x => x.indexes) : mesh.selectedIndexesInternal;

                mesh.ToMesh();

                List<WingedEdge> wings = WingedEdge.GetWingedEdges(mesh);
                HashSet<int> common = mesh.GetSharedVertexHandles(indexes);
                List<List<WingedEdge>> holes = ElementSelection.FindHoles(wings, common);

                HashSet<Face> appendedFaces = new HashSet<Face>();

                foreach (List<WingedEdge> hole in holes)
                {
                    List<int> holeIndexes;
                    Face face;

                    if (wholePath)
                    {
                        // if selecting whole path and in edge mode, make sure the path contains
                        // at least one complete edge from the selection.
                        if (ProBuilderEditor.selectMode == SelectMode.Edge &&
                            !hole.Any(x => common.Contains(x.edge.common.a) &&
                                common.Contains(x.edge.common.b)))
                            continue;

                        holeIndexes = hole.Select(x => x.edge.local.a).ToList();
                        face = AppendElements.CreatePolygon(mesh, holeIndexes, false);
                    }
                    else
                    {
                        IEnumerable<WingedEdge> selected = hole.Where(x => common.Contains(x.edge.common.a));
                        holeIndexes = selected.Select(x => x.edge.local.a).ToList();
                        face = AppendElements.CreatePolygon(mesh, holeIndexes, true);
                    }

                    if (face != null)
                    {
                        filled++;
                        appendedFaces.Add(face);
                    }
                }

                mesh.SetSelectedFaces(appendedFaces);

                wings = WingedEdge.GetWingedEdges(mesh);

                // make sure the appended faces match the first adjacent face found
                // both in winding and face properties
                foreach (var appendedFace in appendedFaces)
                {
                    var wing = wings.FirstOrDefault(x => x.face == appendedFace);

                    if (wing == null)
                        continue;

                    using (var it = new WingedEdgeEnumerator(wing))
                    {
                        while (it.MoveNext())
                        {
                            if (it.Current == null)
                                continue;

                            var currentWing = it.Current;
                            var oppositeFace = it.Current.opposite != null ? it.Current.opposite.face : null;

                            if (oppositeFace != null && !appendedFaces.Contains(oppositeFace))
                            {
                                currentWing.face.submeshIndex = oppositeFace.submeshIndex;
                                currentWing.face.uv = new AutoUnwrapSettings(oppositeFace.uv);
                                SurfaceTopology.ConformOppositeNormal(currentWing.opposite);
                                break;
                            }
                        }
                    }
                }

                mesh.ToMesh();
                mesh.Refresh();
                mesh.Optimize();
            }

            ProBuilderEditor.Refresh();

            if (filled > 0)
                res = new ActionResult(ActionResult.Status.Success, filled > 1 ? string.Format("Filled {0} Holes", filled) : "Fill Hole");
            return res;
        }
    }
}
