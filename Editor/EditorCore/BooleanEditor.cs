#if PROBUILDER_EXPERIMENTAL_FEATURES
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Experimental.CSG;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Editor window for accessing boolean functionality.
    /// </summary>
    sealed class BooleanEditor : ConfigurableWindow
    {
        enum BooleanOp
        {
            Intersection,
            Union,
            Subtraction
        }

        const int k_Padding = 6;
        const int k_PreviewInset = 2;

        GameObject m_LeftGameObject, m_RightGameObject;
        int m_PreviewHeight, m_PreviewWidth;

        Rect m_LeftObjectField = new Rect(k_Padding + k_PreviewInset, k_Padding + k_PreviewInset, 0f, 0f);
        Rect m_RightObjectField = new Rect(0f, k_Padding + k_PreviewInset, 0f, 0f);
        Rect m_ReverseOperationOrderRect = new Rect(0f, 0f, 42f, 42f);

        static GUIStyle previewBackground;
        static GUIStyle unicodeIconStyle;

        Color backgroundColor = new Color(.15625f, .15625f, .15625f, 1f);
        Texture2D backgroundTexture;
        Editor m_LeftPreviewEditor, m_RightPreviewEditor;
        BooleanOp operation = BooleanOp.Intersection;
        bool mouseClickedSwapRect = false;
        Vector2Int screen = Vector2Int.zero;
        static readonly string k_ReverseArrowsIcon = ((char)8644).ToString();

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Experimental/Boolean (CSG) Tool", false, PreferenceKeys.menuMisc)]
        public static void MenuOpenBooleanTool()
        {
            GetWindow<BooleanEditor>(true, "Boolean (Experimental)", true).Show();
        }

        void OnEnable()
        {
            minSize = new Vector2(200f, 135f);

            var meshes = Selection.transforms.GetComponents<ProBuilderMesh>();

            if (meshes.Length == 2)
            {
                m_LeftGameObject = meshes[0].gameObject;
                m_RightGameObject = meshes[1].gameObject;
            }

            previewBackground = new GUIStyle();

            backgroundTexture = new Texture2D(2, 2);

            backgroundTexture.SetPixels(new Color[] {
                backgroundColor,
                backgroundColor,
                backgroundColor,
                backgroundColor
            });

            backgroundTexture.Apply();

            previewBackground.normal.background = backgroundTexture;

            unicodeIconStyle = new GUIStyle();
            unicodeIconStyle.fontSize = 32;
            unicodeIconStyle.normal.textColor = Color.white;
            unicodeIconStyle.alignment = TextAnchor.MiddleCenter;

            var arrowSize = unicodeIconStyle.CalcSize(UI.EditorGUIUtility.TempContent(k_ReverseArrowsIcon));
            m_ReverseOperationOrderRect.width = arrowSize.x;
            m_ReverseOperationOrderRect.width = arrowSize.y;
        }

        void OnDisable()
        {
            if (backgroundTexture != null)
            {
                DestroyImmediate(backgroundTexture);
            }
        }

        void OnGUI()
        {
            DoContextMenu();

            Event e = Event.current;
            screen.x = (int)position.width;
            screen.y = (int)position.height;

            // Since image wells eat mouse clicks, listen for a mouse up when hovering over 'reverse operation order' button
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (m_ReverseOperationOrderRect.Contains(e.mousePosition))
                    {
                        mouseClickedSwapRect = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (mouseClickedSwapRect && m_ReverseOperationOrderRect.Contains(Event.current.mousePosition))
                    {
                        ReverseOperationOrder();
                        e.Use();
                    }
                    mouseClickedSwapRect = false;
                    break;

                case EventType.Ignore:
                    mouseClickedSwapRect = false;
                    break;
            }

            DrawPreviewWells();

            if (ListenForDragAndDrop())
                return;

            GUILayout.BeginHorizontal();
            ProBuilderMesh lpb = m_LeftGameObject != null ? m_LeftGameObject.GetComponent<ProBuilderMesh>() : null;
            ProBuilderMesh rpb = m_RightGameObject != null ? m_RightGameObject.GetComponent<ProBuilderMesh>() : null;

            lpb = (ProBuilderMesh)EditorGUILayout.ObjectField(lpb, typeof(ProBuilderMesh), true);
            rpb = (ProBuilderMesh)EditorGUILayout.ObjectField(rpb, typeof(ProBuilderMesh), true);

            m_LeftGameObject = lpb != null ? lpb.gameObject : null;
            m_RightGameObject = rpb != null ? rpb.gameObject : null;
            GUILayout.EndHorizontal();

            // Boolean controls
            GUILayout.Space(4);

            GUI.backgroundColor = PreferenceKeys.proBuilderDarkGray;
            UI.EditorGUIUtility.DrawSeparator(2);
            GUI.backgroundColor = Color.white;

            operation = (BooleanOp)EditorGUILayout.EnumPopup("Operation", operation);

            if (GUILayout.Button("Apply"))
            {
                switch (operation)
                {
                    case BooleanOp.Union:
                        MenuUnion(m_LeftGameObject.GetComponent<ProBuilderMesh>(), m_RightGameObject.GetComponent<ProBuilderMesh>());
                        break;

                    case BooleanOp.Intersection:
                        MenuIntersect(m_LeftGameObject.GetComponent<ProBuilderMesh>(), m_RightGameObject.GetComponent<ProBuilderMesh>());
                        break;

                    case BooleanOp.Subtraction:
                        MenuSubtract(m_LeftGameObject.GetComponent<ProBuilderMesh>(), m_RightGameObject.GetComponent<ProBuilderMesh>());
                        break;
                }
            }
        }

        void ReverseOperationOrder()
        {
            GameObject tmp = m_LeftGameObject;
            m_LeftGameObject = m_RightGameObject;
            m_RightGameObject = tmp;
            m_LeftPreviewEditor = null;
            m_RightPreviewEditor = null;
        }

        // Draw the mesh previews
        void DrawPreviewWells()
        {
            GUILayout.BeginHorizontal();
            m_LeftObjectField = GUILayoutUtility.GetRect(GUIContent.none, UI.EditorStyles.sceneTextBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            m_RightObjectField = GUILayoutUtility.GetRect(GUIContent.none, UI.EditorStyles.sceneTextBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.EndHorizontal();

            GUI.Box(m_LeftObjectField, GUIContent.none, UI.EditorStyles.sceneTextBox);
            GUI.Box(m_RightObjectField, GUIContent.none, UI.EditorStyles.sceneTextBox);

            m_ReverseOperationOrderRect.x = (screen.x / 2f) - (m_ReverseOperationOrderRect.width / 2f);
            m_ReverseOperationOrderRect.y = m_LeftObjectField.y + m_LeftObjectField.height * .5f - m_ReverseOperationOrderRect.height * .5f;

            m_LeftObjectField = InsetRect(m_LeftObjectField, k_PreviewInset);
            m_RightObjectField = InsetRect(m_RightObjectField, k_PreviewInset);

            if (m_LeftGameObject != null)
            {
                if (m_LeftPreviewEditor == null)
                    m_LeftPreviewEditor = UnityEditor.Editor.CreateEditor(m_LeftGameObject);
                m_LeftPreviewEditor.OnPreviewGUI(m_LeftObjectField, previewBackground);
            }
            else
            {
                GUI.Label(m_LeftObjectField, "Drag GameObject Here", EditorStyles.centeredGreyMiniLabel);
            }

            if (m_RightGameObject != null)
            {
                if (m_RightPreviewEditor == null)
                    m_RightPreviewEditor = UnityEditor.Editor.CreateEditor(m_RightGameObject);

                m_RightPreviewEditor.OnPreviewGUI(m_RightObjectField, previewBackground);
            }
            else
            {
                GUI.Label(m_RightObjectField, "Drag GameObject Here", EditorStyles.centeredGreyMiniLabel);
            }

            // Show text summary
            if (m_LeftGameObject && m_RightGameObject)
            {
                switch (operation)
                {
                    case BooleanOp.Intersection:
                        GUI.Label(new Rect(k_Padding + 2, k_Padding + 2, screen.x, 128), m_LeftGameObject.name + " Intersects " + m_RightGameObject.name, EditorStyles.boldLabel);
                        break;

                    case BooleanOp.Union:
                        GUI.Label(new Rect(k_Padding + 2, k_Padding + 2, screen.x, 128), m_LeftGameObject.name + " Union " + m_RightGameObject.name, EditorStyles.boldLabel);
                        break;

                    case BooleanOp.Subtraction:
                        GUI.Label(new Rect(k_Padding + 2, k_Padding + 2, screen.x, 128), m_LeftGameObject.name + " Subtracts " + m_RightGameObject.name, EditorStyles.boldLabel);
                        break;
                }
            }

            // http://xahlee.info/comp/unicode_arrows.html
            if (GUI.Button(m_ReverseOperationOrderRect, k_ReverseArrowsIcon, unicodeIconStyle))
                ReverseOperationOrder();
        }

        static Rect InsetRect(Rect rect, int pad)
        {
            return new Rect(rect.x + pad, rect.y + pad, rect.width - pad * 2, rect.height - pad * 2);
        }

        /**
         *  Accept drags into window.
         *  MUST BE CALLED AFTER PREVIEW WELL RECTS ARE CALCULATED
         */
        bool ListenForDragAndDrop()
        {
            Vector2 mPos = Event.current.mousePosition;

            bool inLeft = m_LeftObjectField.Contains(mPos);

            if (!inLeft && !m_RightObjectField.Contains(mPos))
                return false;

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) && DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object pb in DragAndDrop.objectReferences)
                    {
                        if ((pb is GameObject && ((GameObject)pb).GetComponent<ProBuilderMesh>()) || pb is ProBuilderMesh)
                        {
                            if (pb == m_LeftGameObject || pb == m_RightGameObject) continue;

                            if (inLeft)
                            {
                                m_LeftGameObject = (GameObject)pb;

                                if (m_LeftPreviewEditor != null)
                                {
                                    DestroyImmediate(m_LeftPreviewEditor);
                                    m_LeftPreviewEditor = null;
                                }
                            }
                            else
                            {
                                m_RightGameObject = (GameObject)pb;

                                if (m_RightPreviewEditor != null)
                                {
                                    DestroyImmediate(m_RightPreviewEditor);
                                    m_RightPreviewEditor = null;
                                }
                            }

                            return true;
                        }
                    }
                }

                Repaint();
            }
            return false;
        }

        enum BooleanOperation
        {
            Union,
            Subtract,
            Intersect
        }

        static ActionResult MenuBooleanOperation(BooleanOperation operation, ProBuilderMesh lhs, ProBuilderMesh rhs)
        {
            if (lhs == null || rhs == null)
                return new ActionResult(ActionResult.Status.Failure, "Must Select 2 Objects");

            string op_string = operation == BooleanOperation.Union ? "Union" : (operation == BooleanOperation.Subtract ? "Subtract" : "Intersect");

            ProBuilderMesh[] sel = new ProBuilderMesh[] { lhs, rhs };

            UndoUtility.RecordSelection(sel, op_string);

            Mesh c;

            switch (operation)
            {
                case BooleanOperation.Union:
                    c = CSG.Union(lhs.gameObject, rhs.gameObject);
                    break;

                case BooleanOperation.Subtract:
                    c = CSG.Subtract(lhs.gameObject, rhs.gameObject);
                    break;

                default:
                    c = CSG.Intersect(lhs.gameObject, rhs.gameObject);
                    break;
            }

            GameObject go = new GameObject();

            go.AddComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            go.AddComponent<MeshFilter>().sharedMesh = c;

            ProBuilderMesh pb = InternalMeshUtility.CreateMeshWithTransform(go.transform, false);
            DestroyImmediate(go);

            Selection.objects = new Object[] { pb.gameObject };

            return new ActionResult(ActionResult.Status.Success, op_string);
        }

        /**
         * Union operation between two ProBuilder objects.
         */
        public static ActionResult MenuUnion(ProBuilderMesh lhs, ProBuilderMesh rhs)
        {
            return MenuBooleanOperation(BooleanOperation.Union, lhs, rhs);
        }

        /**
         * Subtract boolean operation between two pb_Objects.
         */
        public static ActionResult MenuSubtract(ProBuilderMesh lhs, ProBuilderMesh rhs)
        {
            return MenuBooleanOperation(BooleanOperation.Subtract, lhs, rhs);
        }

        /**
         * Intersect boolean operation between two pb_Objects.
         */
        public static ActionResult MenuIntersect(ProBuilderMesh lhs, ProBuilderMesh rhs)
        {
            return MenuBooleanOperation(BooleanOperation.Intersect, lhs, rhs);
        }
    }
}
#endif
