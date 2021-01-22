using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SelectMaterial : MenuAction
    {
        GUIContent gc_restrictToSelection = new GUIContent("Current Selection", "Optionally restrict the matches to only those faces on currently selected objects.");
        internal Pref<bool> m_RestrictToSelectedObjects = new Pref<bool>("SelectMaterial.restrictToSelectedObjects", false, SettingsScope.Project);

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Selection_SelectByMaterial", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Select by Material",
                "Selects all faces matching the selected materials."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face | SelectMode.TextureFace; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Select Material Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_RestrictToSelectedObjects.value = EditorGUILayout.Toggle(gc_restrictToSelection, m_RestrictToSelectedObjects);

            if (EditorGUI.EndChangeCheck())
                ProBuilderSettings.Save();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select Faces with Material"))
            {
                PerformAction();
                SceneView.RepaintAll();
            }
        }

        protected override ActionResult PerformActionImplementation()
        {
            IEnumerable<ProBuilderMesh> selection;

            if (m_RestrictToSelectedObjects)
                selection = MeshSelection.topInternal;
            else
                selection = Object.FindObjectsOfType<ProBuilderMesh>();

            UndoUtility.RecordSelection("Select Faces with Material");

            //Need to go from submesh index to material
            HashSet<Material> selectedMaterials = new HashSet<Material>();
            foreach(var pb in MeshSelection.topInternal)
            {
                HashSet<int> submeshIndex = new HashSet<int>(pb.selectedFacesInternal.Select(y => y.submeshIndex));
                foreach (var index in submeshIndex)
                {
                    selectedMaterials.Add(pb.renderer.sharedMaterials[index]);
                }
            }

            List<GameObject> newSelection = new List<GameObject>();
            foreach (var pb in selection)
            {
                List<int> subMeshIndices = new List<int>();
                for (int matIndex = 0; matIndex < pb.renderer.sharedMaterials.Length; ++matIndex)
                {
                    if(selectedMaterials.Contains(pb.renderer.sharedMaterials[matIndex]))
                    {
                        subMeshIndices.Add(matIndex);
                    }
                }

                if(subMeshIndices.Count > 0)
                {
                    IEnumerable<Face> matches = pb.facesInternal.Where(x => subMeshIndices.Contains(x.submeshIndex));
                    if (matches.Any())
                    {
                        newSelection.Add(pb.gameObject);
                        pb.SetSelectedFaces(matches);
                    }
                }
            }

            Selection.objects = newSelection.ToArray();

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Select Faces with Material");
        }
    }
}
