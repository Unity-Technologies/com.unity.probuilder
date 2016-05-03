using UnityEngine;
using UnityEditor;
using System.Collections;

/**
 * Utilities for creating and manipulating Handles and points in GUI space, based on pb_Handle_Utility.cs
 */
public class UVee_HandleUtility
{

	/**
	* Degree symbol (duh).
	*/
	const char DEGREE_SYMBOL = (char)176;

	/**
	 * A class for storing and applying Vector2 masks.
	 */
	private class HandleConstraint2d
	{
		public int x, y;

		public HandleConstraint2d(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public HandleConstraint2d Inverse()
		{
			return new HandleConstraint2d(x == 1 ? 0 : 1, y == 1 ? 0 : 1);
		}

		public Vector2 Mask(Vector2 v)
		{
			v.x *= this.x;
			v.y *= this.y;
			return v;
		}

		public Vector2 InverseMask(Vector2 v)
		{
			v.x *= this.x == 1 ? 0f : 1f;
			v.y *= this.y == 1 ? 0f : 1f;
			return v;
		}

		public static readonly HandleConstraint2d None = new HandleConstraint2d(1, 1);

		public static bool operator ==(HandleConstraint2d a, HandleConstraint2d b)
		{
		    return a.x == b.x && a.y == b.y;
		}

		public static bool operator !=(HandleConstraint2d a, HandleConstraint2d b)
		{
		    return a.x != b.x || a.y != b.y;
		}

		public override int GetHashCode()
		{
		    return base.GetHashCode();
		}

		public override bool Equals(object o)
		{
		    return o is HandleConstraint2d && ((HandleConstraint2d)o).x == this.x && ((HandleConstraint2d)o).y == this.y;
		}
	}

#region Const

	const int HANDLE_PADDING = 8;

	private static Quaternion QuaternionUp = Quaternion.Euler(Vector3.right * 90f);
	private static Quaternion QuaternionRight = Quaternion.Euler(Vector3.up * 90f);

	private static Color HANDLE_COLOR_UP = new Color(0f, .7f, 0f, .8f);
	private static Color HANDLE_COLOR_RIGHT = new Color(0f, 0f, .7f, .8f);
	private static Color HANDLE_COLOR_ROTATE = new Color(0f, .7f, 0f, .8f);
	private static Color HANDLE_COLOR_SCALE = new Color(.7f, .7f, .7f, .8f);
#endregion

#region Internal Static

	public static int CurrentID { get { return currentId; } }
	private static int currentId = -1;

	private static Vector2 handleOffset = Vector2.zero;
	private static Vector2 initialMousePosition = Vector2.zero;

	private static HandleConstraint2d axisConstraint = new HandleConstraint2d(0, 0);	// Multiply this value by input to mask axis movement.
#endregion

#region Conversion

	/**
	 * Convert a UV point to a GUI point with relative size.
	 * @param pixelSize How many pixels make up a 0,1 distance in UV space.
	 */
	internal static Vector2 UVToGUIPoint(Vector2 uv, int pixelSize)
	{
		// flip y
		Vector2 u = new Vector2(uv.x, -uv.y);
		u *= pixelSize;
		u = new Vector2(Mathf.Round(u.x), Mathf.Round(u.y));

		return u;
	}

	/**
	 * Convert a GUI point back to a UV coordinate.
	 * @param pixelSize How many pixels make up a 0,1 distance in UV space.
	 * @sa UVToGUIPoint
	 */
	internal static Vector2 GUIToUVPoint(Vector2 gui, int pixelSize)
	{
		gui /= (float)pixelSize;
		Vector2 u = new Vector2(gui.x, -gui.y);
		return u;
	}
#endregion

#region Handles

	/**
	 * A 2D GUI view position handle.
	 * @param id The Handle id.
	 * @param position The position in GUI coordinates.
	 * @param size How large in pixels to draw this handle.
	 */
	public static Vector2 PositionHandle2d(int id, Vector2 position, int size)
	{
		int width = size / 4;

		Rect handleRectUp = new Rect(position.x - width / 2, position.y - size - HANDLE_PADDING, width, size + HANDLE_PADDING);
		Rect handleRectRight = new Rect(position.x, position.y - width / 2, size, width + HANDLE_PADDING);

		Handles.color = Color.yellow;
		Handles.CircleCap(-1, position, Quaternion.identity, width / 2);
		Handles.color = HANDLE_COLOR_UP;
		Handles.DrawLine(position, position - Vector2.up * size);
		Handles.ConeCap(0,
			((Vector3)((position - Vector2.up * size))) - Vector3.forward * 16,
			Quaternion.Euler(Vector3.right * 90f),
			width / 2);
		Handles.color = HANDLE_COLOR_RIGHT;
		Handles.DrawLine(position, position + Vector2.right * size);
		Handles.ConeCap(0,
			((Vector3)((position + Vector2.right * size))) - Vector3.forward * 16,
			QuaternionRight,
			width / 2);

		// If a Tool already is engaged and it's not this one, bail.
		if (currentId >= 0 && currentId != id)
		  return position;

		Event e = Event.current;
		Vector2 mousePosition = e.mousePosition;
		Vector2 newPosition = position;

		if (currentId == id)
		{
			switch (e.type)
			{
				case EventType.MouseDrag:
				  newPosition = axisConstraint.Mask(mousePosition + handleOffset) + axisConstraint.InverseMask(position);
				  break;

				case EventType.MouseUp:
				case EventType.Ignore:
				  currentId = -1;
				  break;
			}
		}
	    else
		{
			if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers != EventModifiers.Alt)
			{
				if (Vector2.Distance(mousePosition, position) < width / 2)
				{
	
					currentId = id;
					handleOffset = position - mousePosition;
					axisConstraint = new HandleConstraint2d(1, 1);
				}
				else if (handleRectRight.Contains(mousePosition))
				{
					
					currentId = id;
					handleOffset = position - mousePosition;
					axisConstraint = new HandleConstraint2d(1, 0);
				}
				else if (handleRectUp.Contains(mousePosition))
				{
					
					currentId = id;
					handleOffset = position - mousePosition;
					axisConstraint = new HandleConstraint2d(0, 1);
				}

			}
		}

		return newPosition;
	}

	static Vector2 initialDirection;
	public static float RotationHandle2d(int id, Vector2 position, float rotation, int radius)
	{
		Event e = Event.current;
		Vector2 mousePosition = e.mousePosition;
		float newRotation = rotation;

		Vector2 currentDirection = (mousePosition - position).normalized;

		// Draw gizmos
		Handles.color = HANDLE_COLOR_ROTATE;
		Handles.CircleCap(-1, position, Quaternion.identity, radius);

		if (currentId == id)
		{
			Handles.color = Color.gray;
			Handles.DrawLine(position, position + (mousePosition - position).normalized * radius);
			GUI.Box(new Rect(position.x, position.y, 90f, 30f), newRotation.ToString("F2") + DEGREE_SYMBOL);
		}

		// If a Tool already is engaged and it's not this one, bail.
		if (currentId >= 0 && currentId != id)
		  return rotation;

		if (currentId == id)
		{
			switch (e.type)
			{
				case EventType.MouseDrag:

				  newRotation = Vector2.Angle(initialDirection, currentDirection);

				  if (Vector2.Dot(new Vector2(-initialDirection.y, initialDirection.x), currentDirection) < 0)
					  newRotation = 360f - newRotation;
				  break;

				case EventType.MouseUp:
				case EventType.Ignore:
					currentId = -1;
					break;
			}
		}
		else
		{
			if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers != EventModifiers.Alt)
			{
				if (Mathf.Abs(Vector2.Distance(mousePosition, position) - radius) < 8)
				{
					currentId = id;
					initialMousePosition = mousePosition;
					initialDirection = (initialMousePosition - position).normalized;
					handleOffset = position - mousePosition;
				}
			}
		}

		return newRotation;
	}

	/**
	 * Draw a scale handle in 2d space.
	 */
	public static Vector2 ScaleHandle2d(int id, Vector2 position, Vector2 scale, int size)
	{
		Event e = Event.current;
		Vector2 mousePosition = e.mousePosition;
		int width = size / 4;

		Handles.color = HANDLE_COLOR_UP;
		Handles.DrawLine(position, position - Vector2.up * size * scale.y);
		Handles.CubeCap(0,
			((Vector3)((position - Vector2.up * scale.y * size))) - Vector3.forward * 16,
			QuaternionUp,
			width / 3);

		Handles.color = HANDLE_COLOR_RIGHT;
		Handles.DrawLine(position, position + Vector2.right * size * scale.x);
		Handles.CubeCap(0,
			((Vector3)((position + Vector2.right * scale.x * size))) - Vector3.forward * 16,
			Quaternion.Euler(Vector3.up * 90f),
			width / 3);

		Handles.color = HANDLE_COLOR_SCALE;
		Handles.CubeCap(0,
			((Vector3)position) - Vector3.forward * 16,
			QuaternionUp,
			width / 2);

		// If a Tool already is engaged and it's not this one, bail.
		if (currentId >= 0 && currentId != id)
			return scale;

		Rect handleRectUp = new Rect(position.x - width / 2, position.y - size - HANDLE_PADDING, width, size + HANDLE_PADDING);
		Rect handleRectRight = new Rect(position.x, position.y - width / 2, size + 8, width);
		Rect handleRectCenter = new Rect(position.x - width / 2, position.y - width / 2, width, width);

		if (currentId == id)
		{
			switch (e.type)
			{
				case EventType.MouseDrag:
					Vector2 diff = axisConstraint.Mask(mousePosition - initialMousePosition);
					diff.x += size;
					diff.y = -diff.y;	// gui space Y is opposite-world
					diff.y += size;
					scale = diff / size;
					if (axisConstraint == HandleConstraint2d.None)
					{
						scale.x = Mathf.Min(scale.x, scale.y);
						scale.y = Mathf.Min(scale.x, scale.y);
					}
					break;

				case EventType.MouseUp:
				case EventType.Ignore:
					currentId = -1;
					break;
			}
		}
		else
		{
			if (e.type == EventType.MouseDown && e.button == 0 && e.modifiers != EventModifiers.Alt)
			{
				if (handleRectCenter.Contains(mousePosition))
				{
					currentId = id;
					handleOffset = position - mousePosition;
					initialMousePosition = mousePosition;
					axisConstraint = new HandleConstraint2d(1, 1);
				}
				else if (handleRectRight.Contains(mousePosition))
				{
					currentId = id;
					handleOffset = position - mousePosition;
					initialMousePosition = mousePosition;
					axisConstraint = new HandleConstraint2d(1, 0);
				}
				else if (handleRectUp.Contains(mousePosition))
				{
					currentId = id;
					handleOffset = position - mousePosition;
					initialMousePosition = mousePosition;
					axisConstraint = new HandleConstraint2d(0, 1);
				}

			}
		}

		return scale;
	}
#endregion

#region Point Methods

	/**
	 * Given two Vector2[] arrays, find the nearest two points within maxDelta and return the difference in offset. 
	 * @param points First Vector2[] array.
	 * @param compare The Vector2[] array to compare @c points againts.
	 * @param maxDelta The maximum distance for two points to be apart to be considered for nearness.
	 * @notes This should probably use a divide and conquer algorithm instead of the O(n^2) approach (http://www.geeksforgeeks.org/closest-pair-of-points/)
	 */
	public static bool NearestPointDelta(Vector2[] points, Vector2[] compare, float maxDelta, out Vector2 offset)
	{
		float dist = 0f;
		float minDist = maxDelta;
		bool foundMatch = false;
		offset = Vector2.zero;

		for (int i = 0; i < points.Length; i++)
		{
			for (int n = 0; n < compare.Length; n++)
			{
				dist = Vector2.Distance(points[i], compare[n]);
				if (dist < minDist)
				{
					minDist = dist;
					offset = compare[n] - points[i];
					foundMatch = true;
				}
			}
		}

		return foundMatch;
	}
#endregion
}
