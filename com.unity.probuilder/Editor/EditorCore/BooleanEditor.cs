using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Experimental.CSG;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Editor window for accessing boolean functionality.
    /// </summary>
    sealed class BooleanEditor : EditorWindow
    {
        enum BooleanOp
        {
            Intersection,
            Union,
            Subtraction
        }

        const int k_Padding = 6;
        const int k_PreviewInset = 2;
        const int k_LowerControlsHeight = 32;

        GameObject m_LeftGameObject, m_RightGameObject;
        int m_PreviewHeight, m_PreviewWidth;

        Rect lhsRect = new Rect(k_Padding, k_Padding, 0f, 0f);
        Rect lhsPreviewRect = new Rect(k_Padding + k_PreviewInset, k_Padding + k_PreviewInset, 0f, 0f);

        Rect rhsRect = new Rect(0f, k_Padding, 0f, 0f);
        Rect rhsPreviewRect = new Rect(0f, k_Padding + k_PreviewInset, 0f, 0f);
        Rect swapOrderRect = new Rect(0f, 0f, 42f, 42f);

        static GUIStyle previewBackground;
        static GUIStyle unicodeIconStyle;

        Color backgroundColor = new Color(.15625f, .15625f, .15625f, 1f);
        Texture2D backgroundTexture;
        Editor lhsEditor, rhsEditor;
        BooleanOp operation = BooleanOp.Intersection;
        bool mouseClickedSwapRect = false;
        Vector2Int screen = Vector2Int.zero;

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Experimental/Boolean (CSG) Tool", false, PreferenceKeys.menuMisc)]
        public static void MenuOpenBooleanTool()
        {
            GetWindow<BooleanEditor>(true, "Boolean (Experimental)", true).Show();
        }

        void OnEnable()
        {
            ProBuilderMesh[] pbs = (ProBuilderMesh[])Selection.transforms.GetComponents<ProBuilderMesh>();

            if (pbs.Length == 2)
            {
                m_LeftGameObject = pbs[0].gameObject;
                m_RightGameObject = pbs[1].gameObject;
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
            unicodeIconStyle.fontSize = 64;
            unicodeIconStyle.normal.textColor = Color.white;
            unicodeIconStyle.alignment = TextAnchor.MiddleCenter;
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
            Event e = Event.current;
            screen.x = (int)position.width;
            screen.y = (int)position.height;

            // Since image wells eat mouse clicks, listen for a mouse up when hovering over 'reverse operation order' button
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (swapOrderRect.Contains(e.mousePosition))
                    {
                        mouseClickedSwapRect = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (mouseClickedSwapRect && swapOrderRect.Contains(Event.current.mousePosition))
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

            swapOrderRect.x = (screen.x / 2f) - (swapOrderRect.width / 2f);
            swapOrderRect.y = k_Padding + m_PreviewHeight / 2f - (swapOrderRect.width / 2f);

            // http://xahlee.info/comp/unicode_arrows.html
            if (GUI.Button(swapOrderRect, ((char)8644).ToString(), unicodeIconStyle))
            {
                ReverseOperationOrder();
            }

            GUILayout.Space(m_PreviewHeight + k_Padding * 2);

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

            if (GUILayout.Button("Apply", GUILayout.MinHeight(32)))
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
            lhsEditor = null;
            rhsEditor = null;
        }

        /**
         * Draw the mesh previews
         */
        void DrawPreviewWells()
        {
            // RECT CALCULTAIONS
            m_PreviewWidth = (int)screen.x / 2 - k_Padding - 2;
            m_PreviewHeight = (int)Mathf.Min(screen.y - k_LowerControlsHeight, screen.x / 2 - (k_Padding * 2));

            lhsRect.width = m_PreviewWidth;
            lhsRect.height = m_PreviewHeight;

            lhsPreviewRect.width = lhsRect.width -  k_PreviewInset * 2;
            lhsPreviewRect.height = lhsRect.height - k_PreviewInset * 2;

            rhsRect.x = lhsRect.x + lhsRect.width + k_Padding;
            rhsRect.width = lhsRect.width;
            rhsRect.height = lhsRect.height;

            rhsPreviewRect.x = rhsRect.x + k_PreviewInset;
            rhsPreviewRect.width = lhsPreviewRect.width;
            rhsPreviewRect.height = lhsPreviewRect.height;
            // END RECT CALCULATIONS

            // DRAW PREVIEW WELLS
            GUI.Box(lhsRect, "", UI.EditorStyles.sceneTextBox);
            GUI.Box(rhsRect, "", UI.EditorStyles.sceneTextBox);

            if (m_LeftGameObject != null)
            {
                if (lhsEditor == null)
                    lhsEditor = UnityEditor.Editor.CreateEditor(m_LeftGameObject);
                lhsEditor.OnPreviewGUI(lhsPreviewRect, previewBackground);
            }
            else
            {
                GUI.Label(lhsRect, "Drag GameObject Here", EditorStyles.centeredGreyMiniLabel);
            }

            if (m_RightGameObject != null)
            {
                if (rhsEditor == null)
                    rhsEditor = UnityEditor.Editor.CreateEditor(m_RightGameObject);

                rhsEditor.OnPreviewGUI(rhsPreviewRect, previewBackground);
            }
            else
            {
                GUI.Label(rhsRect, "Drag GameObject Here", EditorStyles.centeredGreyMiniLabel);
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
            // END PREVIEW WELLS
        }

        /**
         *  Accept drags into window.
         *  MUST BE CALLED AFTER PREVIEW WELL RECTS ARE CALCULATED
         */
        bool ListenForDragAndDrop()
        {
            Vector2 mPos = Event.current.mousePosition;

            bool inLeft = lhsPreviewRect.Contains(mPos);

            if (!inLeft && !rhsPreviewRect.Contains(mPos))
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
                                m_LeftGameObject = (GameObject)pb;
                            else
                                m_RightGameObject = (GameObject)pb;

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

            go.AddComponent<MeshRenderer>().sharedMaterial = EditorUtility.GetUserMaterial();
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
