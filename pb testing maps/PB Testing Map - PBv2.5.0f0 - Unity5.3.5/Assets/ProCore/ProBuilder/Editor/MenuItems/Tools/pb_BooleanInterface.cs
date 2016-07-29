#if !PROTOTYPE

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Editor window for accessing boolean functionality.
	 */
	public class pb_BooleanInterface : EditorWindow
	{
		enum BooleanOp
		{
			Intersection,
			Union,
			Subtraction
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Experimental/Boolean (CSG) Tool", false, pb_Constant.MENU_MISC)]
		public static void MenuOpenBooleanTool()
		{
			EditorWindow.GetWindow<pb_BooleanInterface>(true, "Boolean (Experimental)", true).Show();
		}

		const int PAD = 6;
		const int PREVIEW_INSET = 2;

		GameObject lhs, rhs;
		Texture2D lhsPreview, rhsPreview;
		int previewHeight = 0, previewWidth = 0;

		Rect lhsRect = new Rect(PAD, PAD, 0f, 0f);
		Rect lhsPreviewRect = new Rect(PAD + PREVIEW_INSET, PAD + PREVIEW_INSET, 0f, 0f);

		Rect rhsRect = new Rect(0f, PAD, 0f, 0f);
		Rect rhsPreviewRect = new Rect(0f, PAD + PREVIEW_INSET, 0f, 0f);

		Rect swapOrderRect = new Rect(0f, 0f, 42f, 42f);

		static GUIStyle previewBackground;
		static GUIStyle unicodeIconStyle;

		Color previewBorderColor = new Color(.3f, .3f, .3f, 1f);
		Color backgroundColor = new Color(.15625f, .15625f, .15625f, 1f);
		Texture2D backgroundTexture;
		Editor lhsEditor, rhsEditor;

		BooleanOp operation = BooleanOp.Intersection;
		bool mouseClickedSwapRect = false;

		int lowerControlsHeight = 32;

		void OnEnable()
		{
			pb_Object[] pbs = (pb_Object[])Selection.transforms.GetComponents<pb_Object>();

			if(pbs.Length == 2)
			{
				lhs = pbs[0].gameObject;
				rhs = pbs[1].gameObject;
			}

			previewBackground = new GUIStyle();
			
			backgroundTexture = new Texture2D(2,2);

			backgroundTexture.SetPixels(new Color[] {
				backgroundColor,
				backgroundColor, 
				backgroundColor, 
				backgroundColor });

			backgroundTexture.Apply();

			previewBackground.normal.background = backgroundTexture;

			unicodeIconStyle = new GUIStyle();
			unicodeIconStyle.fontSize = 64;
			unicodeIconStyle.normal.textColor = Color.white;
			unicodeIconStyle.alignment = TextAnchor.MiddleCenter;
		}

		void OnDisable()
		{
			if(backgroundTexture != null)
			{
				DestroyImmediate(backgroundTexture);
			}
		}

		void OnGUI()
		{	
			Event e = Event.current;

			// Since image wells eat mouse clicks, listen for a mouse up when hovering over 'reverse operation order' button
			switch(e.type)
			{
				case EventType.MouseDown:
						if(swapOrderRect.Contains(e.mousePosition))
						{
							mouseClickedSwapRect = true;
							e.Use();
						}
					break;

				case EventType.MouseUp:
					if(mouseClickedSwapRect && swapOrderRect.Contains(Event.current.mousePosition))
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

			if(ListenForDragAndDrop())
			{
				return;
			}

			swapOrderRect.x = (Screen.width/2f)-(swapOrderRect.width/2f);
			swapOrderRect.y = PAD + previewHeight/2f - (swapOrderRect.width/2f);

			// http://xahlee.info/comp/unicode_arrows.html
			if(GUI.Button( swapOrderRect, ((char)8644).ToString(), unicodeIconStyle))
			{
				ReverseOperationOrder();
			}

			GUILayout.Space(previewHeight + PAD*2);

			GUILayout.BeginHorizontal();
				pb_Object lpb = lhs != null ? lhs.GetComponent<pb_Object>() : null;
				pb_Object rpb = rhs != null ? rhs.GetComponent<pb_Object>() : null;

				lpb = (pb_Object) EditorGUILayout.ObjectField(lpb, typeof(pb_Object), true);
				rpb = (pb_Object) EditorGUILayout.ObjectField(rpb, typeof(pb_Object), true);

				lhs = lpb != null ? lpb.gameObject : null;
				rhs = rpb != null ? rpb.gameObject : null;
			GUILayout.EndHorizontal();

			// Boolean controls
			GUILayout.Space(4);

			GUI.backgroundColor = pb_Constant.ProBuilderDarkGray;
			pb_GUI_Utility.DrawSeparator(2);
			GUI.backgroundColor = Color.white;

			operation = (BooleanOp) EditorGUILayout.EnumPopup("Operation", operation);

			if(GUILayout.Button("Apply", GUILayout.MinHeight(32)))
			{
				switch(operation)
				{
					case BooleanOp.Union:
						pb_Menu_Commands.MenuUnion(lhs.GetComponent<pb_Object>(), rhs.GetComponent<pb_Object>());
						break;

					case BooleanOp.Intersection:
						pb_Menu_Commands.MenuIntersect(lhs.GetComponent<pb_Object>(), rhs.GetComponent<pb_Object>());
						break;

					case BooleanOp.Subtraction:
						pb_Menu_Commands.MenuSubtract(lhs.GetComponent<pb_Object>(), rhs.GetComponent<pb_Object>());
						break;
				}
			}
		}

		void ReverseOperationOrder()
		{
			GameObject tmp = lhs;
			lhs = rhs;
			rhs = tmp;
			lhsEditor = null;
			rhsEditor = null;
		}

		/**
		 * Draw the mesh previews
		 */
		void DrawPreviewWells()
		{		
			// RECT CALCULTAIONS
			previewWidth = (int)Screen.width/2-PAD-2;
			previewHeight = (int)Mathf.Min(Screen.height - lowerControlsHeight, Screen.width/2-(PAD*2));
		
			lhsRect.width = previewWidth;
			lhsRect.height = previewHeight;

			lhsPreviewRect.width = lhsRect.width -  PREVIEW_INSET*2;
			lhsPreviewRect.height = lhsRect.height - PREVIEW_INSET*2;

			rhsRect.x = lhsRect.x + lhsRect.width + PAD;
			rhsRect.width = lhsRect.width;
			rhsRect.height = lhsRect.height;

			rhsPreviewRect.x = rhsRect.x + PREVIEW_INSET;
			rhsPreviewRect.width = lhsPreviewRect.width;
			rhsPreviewRect.height = lhsPreviewRect.height;
			// END RECT CALCULATIONS

			// DRAW PREVIEW WELLS

			GUI.color = previewBorderColor;
			EditorGUI.DrawPreviewTexture(lhsRect, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill);
			EditorGUI.DrawPreviewTexture(rhsRect, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill);
			GUI.color = Color.white;

			if (lhs != null)
			{
				if(lhsEditor == null)
					lhsEditor = Editor.CreateEditor(lhs);
				lhsEditor.OnPreviewGUI(lhsPreviewRect, previewBackground);
			}
			else
			{
				GUI.color = backgroundColor;
				EditorGUI.DrawPreviewTexture(lhsPreviewRect, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill);
				GUI.color = Color.white;
			}

			if (rhs != null)
			{
				if(rhsEditor == null)
					rhsEditor = Editor.CreateEditor(rhs);

				rhsEditor.OnPreviewGUI(rhsPreviewRect, previewBackground);
			}
			else
			{
				GUI.color = backgroundColor;
				EditorGUI.DrawPreviewTexture(rhsPreviewRect, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill);
				GUI.color = Color.white;
			}

			// Show text summary
			if(lhs && rhs)
			{
				switch(operation)
				{
				case BooleanOp.Intersection:
					GUI.Label(new Rect(PAD+2, PAD + 2, Screen.width, 128), lhs.name + " Intersects " + rhs.name, EditorStyles.boldLabel);
					break;

				case BooleanOp.Union:
					GUI.Label(new Rect(PAD+2, PAD + 2, Screen.width, 128), lhs.name + " Union " + rhs.name, EditorStyles.boldLabel);
					break;

				case BooleanOp.Subtraction:
					GUI.Label(new Rect(PAD+2, PAD + 2, Screen.width, 128), lhs.name + " Subtracts " + rhs.name, EditorStyles.boldLabel);
					break;
				}
			}
			// END PREVIEW WELLS
		}
	 
	 	/**
		 *	Accept drags into window.
	 	 *	MUST BE CALLED AFTER PREVIEW WELL RECTS ARE CALCULATED
	 	 */
	 	bool ListenForDragAndDrop()
		{
			Vector2 mPos = Event.current.mousePosition;

			bool inLeft = lhsPreviewRect.Contains(mPos);

			if(!inLeft && !rhsPreviewRect.Contains(mPos))
			   return false;

			if( (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) && DragAndDrop.objectReferences.Length > 0)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				
				if(Event.current.type == EventType.DragPerform)
				{
					DragAndDrop.AcceptDrag();
				
					foreach(Object pb in DragAndDrop.objectReferences)
					{
						if( (pb is GameObject && ((GameObject)pb).GetComponent<pb_Object>()) || pb is pb_Object)
						{
							if(pb == lhs || pb == rhs) continue;

							if(inLeft)
								lhs = (GameObject) pb;
							else
								rhs = (GameObject) pb;

							return true;
						}
					}
				}
				
				Repaint();
			}
			return false;
		}
	}
}
#endif
