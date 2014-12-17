using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using ProBuilder2.GUI;

public class pb_BooleanInterface : EditorWindow
{
	enum BooleanOp
	{
		Intersection,
		Union,
		Subtraction
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Boolean (CSG) Tool")]
	public static void MenuOpenBooleanTool()
	{
		EditorWindow.GetWindow<pb_BooleanInterface>(true, "Boolean", true).Show();
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

	static GUIStyle previewBackground;
	static GUIStyle unicodeIconStyle;

	Color previewBorderColor = new Color(.3f, .3f, .3f, 1f);
	Color backgroundColor = new Color(.15625f, .15625f, .15625f, 1f);
	Texture2D backgroundTexture;
	Editor lhsEditor, rhsEditor;

	BooleanOp operation = BooleanOp.Intersection;

	int lowerControlsHeight = 32;

	void OnEnable()
	{
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
		DrawPreviewWells();

		if(lhs && rhs)
			GUI.Label(new Rect(PAD, PAD + 2, Screen.width, 128), lhs.name + " Intersects " + rhs.name, EditorStyles.boldLabel);

		if(ListenForDragAndDrop())
		{
			return;
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

		// http://xahlee.info/comp/unicode_arrows.html
		if(GUI.Button( new Rect( Screen.width - 64 - PAD, PAD, 64, 64), ((char)8644).ToString(), unicodeIconStyle ))
		{
			GameObject tmp = lhs;
			lhs = rhs;
			rhs = tmp;
			lhsEditor = null;
			rhsEditor = null;
		}
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

		// END PREVIEW WELLS
	}
 
 	/**
 	 * MUST BE CALLED AFTER PREVIEW WELL RECTS ARE CALCULATED
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

				Object[] pbs = DragAndDrop.objectReferences.ToArray();
				
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
