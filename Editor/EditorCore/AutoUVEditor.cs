#pragma warning disable 0414

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Custom editor for pb_UV type.
    /// </summary>
    static class AutoUVEditor
    {
        #region MEMBERS

        static ProBuilderEditor editor
        {
            get { return ProBuilderEditor.instance; }
        }

        static AutoUnwrapSettings s_AutoUVSettings = AutoUnwrapSettings.tile;
        static int textureGroup = -1;
        static List<AutoUnwrapSettings> s_AutoUVSettingsInSelection = new List<AutoUnwrapSettings>();
        static Dictionary<string, bool> s_AutoUVSettingsDiff = new Dictionary<string, bool>()
        {
            { "projectionAxis", false },
            { "useWorldSpace", false },
            { "flipU", false },
            { "flipV", false },
            { "swapUV", false },
            { "fill", false },
            { "scalex", false },
            { "scaley", false },
            { "offsetx", false },
            { "offsety", false },
            { "rotation", false },
            { "anchor", false },
            { "manualUV", false },
            { "textureGroup", false }
        };

        public enum Axis2D
        {
            XY,
            X,
            Y
        }

        static Vector2 s_ScrollPosition;

        #endregion

        #region ONGUI

        public static bool OnGUI(ProBuilderMesh[] selection, float width)
        {
            UpdateDiffDictionary(selection);

            s_ScrollPosition = EditorGUILayout.BeginScrollView(s_ScrollPosition);
            float tempFloat = 0f;

            EditorGUI.BeginChangeCheck();

            /**
             * Set Tile mode
             */
            GUILayout.Label("Tiling & Alignment", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["fill"];
            GUILayout.Label("Fill Mode", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
            EditorGUI.BeginChangeCheck();
            s_AutoUVSettings.fill = (AutoUnwrapSettings.Fill)EditorGUILayout.EnumPopup(s_AutoUVSettings.fill);
            if (EditorGUI.EndChangeCheck())
                SetFill(s_AutoUVSettings.fill, selection);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            bool enabled = GUI.enabled;
            GUI.enabled = !s_AutoUVSettings.useWorldSpace;
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["anchor"];
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Anchor", GUILayout.MaxWidth(80), GUILayout.MinWidth(80));
            s_AutoUVSettings.anchor = (AutoUnwrapSettings.Anchor)EditorGUILayout.EnumPopup(s_AutoUVSettings.anchor);
            if (EditorGUI.EndChangeCheck())
                SetAnchor(s_AutoUVSettings.anchor, selection);
            GUI.enabled = enabled;
            GUILayout.EndHorizontal();

            GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
            UI.EditorGUIUtility.DrawSeparator(1);
            GUI.backgroundColor = Color.white;

            GUILayout.Label("Transform", EditorStyles.boldLabel);

            /**
             * Offset
             */
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["offsetx"] || s_AutoUVSettingsDiff["offsety"];
            var tempVec2 = s_AutoUVSettings.offset;
            UnityEngine.GUI.SetNextControlName("offset");
            s_AutoUVSettings.offset = EditorGUILayout.Vector2Field("Offset", s_AutoUVSettings.offset, GUILayout.MaxWidth(width));
            if (tempVec2.x != s_AutoUVSettings.offset.x) { SetOffset(s_AutoUVSettings.offset, Axis2D.X, selection); }
            if (tempVec2.y != s_AutoUVSettings.offset.y) { SetOffset(s_AutoUVSettings.offset, Axis2D.Y, selection); }

            /**
             * Rotation
             */
            tempFloat = s_AutoUVSettings.rotation;
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["rotation"];
            GUILayout.Label(new GUIContent("Rotation", "Rotation around the center of face UV bounds."), GUILayout.MaxWidth(width - 64));
            UnityEngine.GUI.SetNextControlName("rotation");
            EditorGUI.BeginChangeCheck();
            tempFloat = EditorGUILayout.Slider(tempFloat, 0f, 360f, GUILayout.MaxWidth(width));
            if (EditorGUI.EndChangeCheck())
                SetRotation(tempFloat, selection);

            /**
             * Scale
             */
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["scalex"] || s_AutoUVSettingsDiff["scaley"];
            tempVec2 = s_AutoUVSettings.scale;
            GUI.SetNextControlName("scale");
            EditorGUI.BeginChangeCheck();
            s_AutoUVSettings.scale = EditorGUILayout.Vector2Field("Tiling", s_AutoUVSettings.scale, GUILayout.MaxWidth(width));

            if (EditorGUI.EndChangeCheck())
            {
                if (tempVec2.x != s_AutoUVSettings.scale.x) { SetScale(s_AutoUVSettings.scale, Axis2D.X, selection); }
                if (tempVec2.y != s_AutoUVSettings.scale.y) { SetScale(s_AutoUVSettings.scale, Axis2D.Y, selection); }
            }

            // Draw tiling shortcuts
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(".5", EditorStyles.miniButtonLeft))   SetScale(Vector2.one * 2f, Axis2D.XY, selection);
            if (GUILayout.Button("1", EditorStyles.miniButtonMid))     SetScale(Vector2.one, Axis2D.XY, selection);
            if (GUILayout.Button("2", EditorStyles.miniButtonMid))     SetScale(Vector2.one * .5f, Axis2D.XY, selection);
            if (GUILayout.Button("4", EditorStyles.miniButtonMid))     SetScale(Vector2.one * .25f, Axis2D.XY, selection);
            if (GUILayout.Button("8", EditorStyles.miniButtonMid))     SetScale(Vector2.one * .125f, Axis2D.XY, selection);
            if (GUILayout.Button("16", EditorStyles.miniButtonRight))  SetScale(Vector2.one * .0625f, Axis2D.XY, selection);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            UnityEngine.GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
            UI.EditorGUIUtility.DrawSeparator(1);
            UnityEngine.GUI.backgroundColor = Color.white;

            /**
             * Special
             */
            GUILayout.Label("Special", EditorStyles.boldLabel);

            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["useWorldSpace"];
            EditorGUI.BeginChangeCheck();
            s_AutoUVSettings.useWorldSpace = EditorGUILayout.Toggle("World Space", s_AutoUVSettings.useWorldSpace);
            if (EditorGUI.EndChangeCheck())
                SetUseWorldSpace(s_AutoUVSettings.useWorldSpace, selection);

            GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
            UI.EditorGUIUtility.DrawSeparator(1);
            GUI.backgroundColor = Color.white;


            // Flip U
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["flipU"];
            EditorGUI.BeginChangeCheck();
            s_AutoUVSettings.flipU = EditorGUILayout.Toggle("Flip U", s_AutoUVSettings.flipU);
            if (EditorGUI.EndChangeCheck())
                SetFlipU(s_AutoUVSettings.flipU, selection);

            // Flip V
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["flipV"];
            EditorGUI.BeginChangeCheck();
            s_AutoUVSettings.flipV = EditorGUILayout.Toggle("Flip V", s_AutoUVSettings.flipV);
            if (EditorGUI.EndChangeCheck())
                SetFlipV(s_AutoUVSettings.flipV, selection);

            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["swapUV"];
            EditorGUI.BeginChangeCheck();
            s_AutoUVSettings.swapUV = EditorGUILayout.Toggle("Swap U/V", s_AutoUVSettings.swapUV);
            if (EditorGUI.EndChangeCheck())
                SetSwapUV(s_AutoUVSettings.swapUV, selection);

            /**
             * Texture Groups
             */
            GUILayout.Label("Texture Groups", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = s_AutoUVSettingsDiff["textureGroup"];

            GUI.SetNextControlName("textureGroup");
            textureGroup = UI.EditorGUIUtility.IntFieldConstrained(new GUIContent("Texture Group", "Faces in a texture group will be UV mapped as a group, just as though you had selected these faces and used the \"Planar Project\" action"), textureGroup, (int)width);

            if (EditorGUI.EndChangeCheck())
            {
                SetTextureGroup(selection, textureGroup);

                foreach (var kvp in MeshSelection.selectedFacesInEditZone)
                    kvp.Key.RefreshUV(kvp.Value);

                SceneView.RepaintAll();

                s_AutoUVSettingsDiff["textureGroup"] = false;
            }

            if (GUILayout.Button(new GUIContent("Group Selected Faces", "This sets all selected faces to share a texture group.  What that means is that the UVs on these faces will all be projected as though they are a single plane.  Ideal candidates for texture groups are floors with multiple faces, walls with edge loops, flat surfaces, etc.")))
            {
                for (int i = 0; i < selection.Length; i++)
                    TextureGroupSelectedFaces(selection[i]);

                ProBuilderEditor.Refresh();
            }

            if (GUILayout.Button(new GUIContent("Break Selected Groups", "This resets all the selected face Texture Groups.")))
            {
                SetTextureGroup(selection, -1);

                foreach (var kvp in MeshSelection.selectedFacesInEditZone)
                {
                    kvp.Key.ToMesh();
                    kvp.Key.Refresh();
                    kvp.Key.Optimize();
                }

                SceneView.RepaintAll();

                s_AutoUVSettingsDiff["textureGroup"] = false;

                ProBuilderEditor.Refresh();
            }

            /* Select all in current texture group */
            if (GUILayout.Button(new GUIContent("Select Texture Group", "Selects all faces contained in this texture group.")))
            {
                for (int i = 0; i < selection.Length; i++)
                    selection[i].SetSelectedFaces(System.Array.FindAll(selection[i].facesInternal, x => x.textureGroup == textureGroup));

                ProBuilderEditor.Refresh();
            }

            if (GUILayout.Button(new GUIContent("Reset UVs", "Reset UV projection parameters.")))
            {
                UndoUtility.RecordSelection(selection, "Reset UVs");

                for (int i = 0; i < selection.Length; i++)
                {
                    foreach (Face face in selection[i].GetSelectedFaces())
                    {
                        face.uv = AutoUnwrapSettings.tile;
                        face.textureGroup = -1;
                        face.elementGroup = -1;
                    }

                    UVEditing.SplitUVs(selection[i], selection[i].GetSelectedFaces());
                }

                ProBuilderEditor.Refresh();
            }

            GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
            UI.EditorGUIUtility.DrawSeparator(1);
            GUI.backgroundColor = Color.white;

            /**
             * Clean up
             */
            GUILayout.EndScrollView();
            EditorGUI.showMixedValue = false;

            return EditorGUI.EndChangeCheck();
        }

        static void UpdateDiffDictionary(ProBuilderMesh[] selection)
        {
            s_AutoUVSettingsInSelection.Clear();

            if (selection == null || selection.Length < 1)
                return;

            s_AutoUVSettingsInSelection = selection.SelectMany(x => x.GetSelectedFaces()).Where(x => !x.manualUV).Select(x => x.uv).ToList();

            // Clear values for each iteration
            foreach (string key in s_AutoUVSettingsDiff.Keys.ToList())
                s_AutoUVSettingsDiff[key] = false;

            if (s_AutoUVSettingsInSelection.Count < 1) return;

            s_AutoUVSettings = new AutoUnwrapSettings(s_AutoUVSettingsInSelection[0]);

            foreach (AutoUnwrapSettings u in s_AutoUVSettingsInSelection)
            {
                // if(u.projectionAxis != m_AutoUVSettings.projectionAxis)
                //      m_AutoUVSettingsDiff["projectionAxis"] = true;
                if (u.useWorldSpace != s_AutoUVSettings.useWorldSpace)
                    s_AutoUVSettingsDiff["useWorldSpace"] = true;
                if (u.flipU != s_AutoUVSettings.flipU)
                    s_AutoUVSettingsDiff["flipU"] = true;
                if (u.flipV != s_AutoUVSettings.flipV)
                    s_AutoUVSettingsDiff["flipV"] = true;
                if (u.swapUV != s_AutoUVSettings.swapUV)
                    s_AutoUVSettingsDiff["swapUV"] = true;
                if (u.fill != s_AutoUVSettings.fill)
                    s_AutoUVSettingsDiff["fill"] = true;
                if (!Math.Approx(u.scale.x, s_AutoUVSettings.scale.x))
                    s_AutoUVSettingsDiff["scalex"] = true;
                if (!Math.Approx(u.scale.y, s_AutoUVSettings.scale.y))
                    s_AutoUVSettingsDiff["scaley"] = true;
                if (!Math.Approx(u.offset.x, s_AutoUVSettings.offset.x))
                    s_AutoUVSettingsDiff["offsetx"] = true;
                if (!Math.Approx(u.offset.y, s_AutoUVSettings.offset.y))
                    s_AutoUVSettingsDiff["offsety"] = true;
                if (!Math.Approx(u.rotation, s_AutoUVSettings.rotation))
                    s_AutoUVSettingsDiff["rotation"] = true;
                if (u.anchor != s_AutoUVSettings.anchor)
                    s_AutoUVSettingsDiff["anchor"] = true;
            }

            foreach (ProBuilderMesh pb in selection)
            {
                if (s_AutoUVSettingsDiff["manualUV"] && s_AutoUVSettingsDiff["textureGroup"])
                    break;

                Face[] selFaces = pb.GetSelectedFaces();

                if (!s_AutoUVSettingsDiff["manualUV"])
                    s_AutoUVSettingsDiff["manualUV"] = System.Array.Exists(selFaces, x => x.manualUV);

                List<int> texGroups = selFaces.Select(x => x.textureGroup).Distinct().ToList();
                textureGroup = texGroups.FirstOrDefault(x => x > -1);

                if (!s_AutoUVSettingsDiff["textureGroup"])
                    s_AutoUVSettingsDiff["textureGroup"] = texGroups.Count() > 1;
            }
        }

        #endregion

        #region MODIFY SINGLE PROPERTIES

        private static void SetFlipU(bool flipU, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Flip U");
            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.flipU = flipU;
                    q.uv = uv;
                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetFlipV(bool flipV, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Flip V");
            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.flipV = flipV;
                    q.uv = uv;
                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetSwapUV(bool swapUV, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Swap U, V");
            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.swapUV = swapUV;
                    q.uv = uv;
                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetUseWorldSpace(bool useWorldSpace, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Use World Space UVs");
            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.useWorldSpace = useWorldSpace;
                    q.uv = uv;
                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetFill(AutoUnwrapSettings.Fill fill, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Fill UVs");
            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.fill = fill;
                    q.uv = uv;
                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetAnchor(AutoUnwrapSettings.Anchor anchor, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Set UV Anchor");

            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.anchor = anchor;
                    q.uv = uv;
                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetOffset(Vector2 offset, Axis2D axis, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Offset UVs");

            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    switch (axis)
                    {
                        case Axis2D.XY:
                        {
                            var uv = q.uv;
                            uv.offset = offset;
                            q.uv = uv;
                            break;
                        }
                        case Axis2D.X:
                        {
                            var uv = q.uv;
                            uv.offset = new Vector2(offset.x, q.uv.offset.y);
                            q.uv = uv;
                            break;
                        }
                        case Axis2D.Y:
                        {
                            var uv = q.uv;
                            uv.offset = new Vector2(q.uv.offset.x, offset.y);
                            q.uv = uv;
                            break;
                        }
                    }

                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        private static void SetRotation(float rot, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Rotate UVs");

            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    var uv = q.uv;
                    uv.rotation = rot;
                    if (uv.rotation > 360f)
                        uv.rotation = uv.rotation % 360f;
                    else if (uv.rotation < 0f)
                        uv.rotation = 360f + (uv.rotation % 360f);
                    q.uv = uv;
                    sel[i].SetGroupUV(uv, q.textureGroup);
                }
            }
        }

        private static void SetScale(Vector2 scale, Axis2D axis, ProBuilderMesh[] sel)
        {
            UndoUtility.RecordSelection(sel, "Scale UVs");

            for (int i = 0; i < sel.Length; i++)
            {
                foreach (Face q in sel[i].GetSelectedFaces())
                {
                    switch (axis)
                    {
                        case Axis2D.XY:
                        {
                            var uv = q.uv;
                            uv.scale = scale;
                            q.uv = uv;
                            break;
                        }
                        case Axis2D.X:
                        {
                            var uv = q.uv;
                            uv.scale = new Vector2(scale.x, q.uv.scale.y);
                            q.uv = uv;
                            break;
                        }
                        case Axis2D.Y:
                        {
                            var uv = q.uv;
                            uv.scale = new Vector2(q.uv.scale.x, scale.y);
                            q.uv = uv;
                            break;
                        }
                    }

                    sel[i].SetGroupUV(q.uv, q.textureGroup);
                }
            }
        }

        #endregion

        #region TEXTURE GROUPS

        private static void SetTextureGroup(ProBuilderMesh[] selection, int tex)
        {
            UndoUtility.RecordSelection(selection, "Set Texture Group " + textureGroup);

            foreach (ProBuilderMesh pb in selection)
            {
                if (pb.selectedFaceCount < 1)
                    continue;

                Face[] faces = pb.GetSelectedFaces();
                AutoUnwrapSettings cuv = faces[0].uv;

                foreach (Face f in faces)
                {
                    f.textureGroup = tex;
                    f.uv = new AutoUnwrapSettings(cuv);
                }
            }
        }

        private static void TextureGroupSelectedFaces(ProBuilderMesh pb)//, pb_Face face)
        {
            if (pb.selectedFaceCount < 1) return;

            Face[] faces = pb.GetSelectedFaces();

            AutoUnwrapSettings cont_uv = faces[0].uv;

            int texGroup = pb.GetUnusedTextureGroup();

            UndoUtility.RecordSelection(pb, "Create Texture Group" + textureGroup);

            foreach (Face f in faces)
            {
                f.uv = new AutoUnwrapSettings(cont_uv);
                f.textureGroup = texGroup;
            }
        }

        #endregion
    }
}
