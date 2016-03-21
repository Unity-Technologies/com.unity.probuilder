// #undef PB_DEBUG

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;
using System.Reflection;

#if PB_DEBUG
using Parabox.Debug;
#endif

/**
 * Utilities for creating and manipulating Handles and points in GUI space.  Also coordinate translations.
 */
public class pb_Handle_Utility
{	
	const int LEFT_MOUSE_BUTTON = 0;
	const int MIDDLE_MOUSE_BUTTON = 2;

#region SceneView

	public static bool SceneViewInUse(Event e)
	{
		return 	e.alt
				|| Tools.current == Tool.View  
				|| GUIUtility.hotControl > 0  
				|| (e.isMouse ? e.button > 1 : false)
				|| Tools.viewTool == ViewTool.FPS 
				|| Tools.viewTool == ViewTool.Orbit;
	}
#endregion	

#region Const

	const int HANDLE_PADDING = 8;

	private static Quaternion QuaternionUp = Quaternion.Euler(Vector3.right*90f);
	private static Quaternion QuaternionRight = Quaternion.Euler(Vector3.up*90f);
	private static Vector3 ConeDepth = new Vector3(0f, 0f, 16f);

	private static Color HANDLE_COLOR_UP = new Color(0f, .7f, 0f, .8f);
	private static Color HANDLE_COLOR_RIGHT = new Color(0f, 0f, .7f, .8f);
	private static Color HANDLE_COLOR_ROTATE = new Color(0f, .7f, 0f, .8f);
	private static Color HANDLE_COLOR_SCALE = new Color(.7f, .7f, .7f, .8f);

	static Material _handleMaterial = null;
	public static Material handleMaterial
	{
		get 
		{
			if(_handleMaterial == null)
				_handleMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");

			return _handleMaterial;
		}
	}

	static Material _edgeMaterial = null;
	public static Material edgeMaterial
	{
		get 
		{
			if(_edgeMaterial == null)
				_edgeMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
				// _edgeMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/VertexSelectionMaterial.mat");

			return _edgeMaterial;
		}
	}
#endregion

#region Internal Static

	public static int CurrentID { get { return currentId; } }
	private static int currentId = -1;

	private static Vector2 handleOffset = Vector2.zero;
	private static Vector2 initialMousePosition = Vector2.zero;

	private static pb_HandleConstraint2D axisConstraint = new pb_HandleConstraint2D(0, 0);	// Multiply this value by input to mask axis movement.
	public static pb_HandleConstraint2D CurrentAxisConstraint { get { return axisConstraint; } }

	public static bool limitToLeftButton = true;
#endregion

#region Conversion

	/**
	 * Convert a UV point to a GUI point with relative size.
	 * @param pixelSize How many pixels make up a 0,1 distance in UV space.
	 */
	internal static Vector4 UVToGUIPoint(Vector4 uv, int pixelSize)
	{
		// flip y
		Vector4 u = new Vector4(
			uv.x * pixelSize,
			-uv.y * pixelSize,
			uv.z,
			uv.w);

		return u;
	}
		
	/**
	 * Convert a GUI point back to a UV coordinate.
	 * @param pixelSize How many pixels make up a 0,1 distance in UV space.
	 * @sa UVToGUIPoint
	 */
	internal static Vector4 GUIToUVPoint(Vector4 gui, int pixelSize)
	{
		gui /= (float)pixelSize;
		Vector4 u = new Vector4(gui.x, -gui.y, gui.z, gui.w);
		return u;
	}

	/**
	 * Convert a point on the UV canvas (0,1 scaled to guisize) to a GUI coordinate.
	 */
	public static Vector4 UVToGUIPoint(Vector4 uvPoint, Vector2 uvGraphCenter, Vector2 uvGraphOffset, float uvGraphScale, int uvGridSize)
	{
		Vector4 p = new Vector4(uvPoint.x, -uvPoint.y, uvPoint.z, uvPoint.w);
		p.x = uvGraphCenter.x + (p.x * uvGraphScale * uvGridSize) + uvGraphOffset.x;
		p.y = uvGraphCenter.y + (p.y * uvGraphScale * uvGridSize) + uvGraphOffset.y;
		return p;
	}

	public static Vector4 GUIToUVPoint(Vector4 guiPoint, Vector2 uvGraphCenter, Vector2 uvGraphOffset, float uvGraphScale, int uvGridSize)
	{
		Vector4 p = new Vector4(
			(guiPoint.x - (uvGraphCenter.x + uvGraphOffset.x)) / (uvGraphScale * uvGridSize),
			-(guiPoint.y - (uvGraphCenter.y + uvGraphOffset.y)) / (uvGraphScale * uvGridSize),
			guiPoint.z,
			guiPoint.w );
		return p;
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
		int width = size/4;

		Rect handleRectUp = new Rect(position.x-width/2, position.y-size-HANDLE_PADDING, width, size+HANDLE_PADDING);
		Rect handleRectRight = new Rect(position.x, position.y-width/2, size, width+HANDLE_PADDING);
		
		Handles.color = Color.yellow;
		Handles.CircleCap(-1, position, Quaternion.identity, width/2);
		Handles.color = HANDLE_COLOR_UP;

		// Y Line
		Handles.DrawLine(position, position - Vector2.up * size);

		// Y Cone
		if(position.y - size > 0f)
			Handles.ConeCap(0, 
				((Vector3)((position - Vector2.up*size))) - ConeDepth,
				QuaternionUp,
				width/2);

		Handles.color = HANDLE_COLOR_RIGHT;

		// X Line
		Handles.DrawLine(position, position + Vector2.right * size);

		// X Cap
		if(position.y > 0f)
			Handles.ConeCap(0, 
				((Vector3)((position + Vector2.right*size))) - ConeDepth,
				QuaternionRight,
				width/2);

		// If a Tool already is engaged and it's not this one, bail.
		if(currentId >= 0 && currentId != id)
			return position;

		Event e = Event.current;
		Vector2 mousePosition = e.mousePosition;
		Vector2 newPosition = position;

		if(currentId == id)
		{
			switch(e.type)
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
			if(e.type == EventType.MouseDown && ((!limitToLeftButton && e.button != MIDDLE_MOUSE_BUTTON) || e.button == LEFT_MOUSE_BUTTON))
			{
				if(Vector2.Distance(mousePosition, position) < width/2)
				{
					currentId = id;
					handleOffset = position - mousePosition;
					axisConstraint = new pb_HandleConstraint2D(1, 1);
				}
				else
				if(handleRectRight.Contains(mousePosition))
				{
					currentId = id;	
					handleOffset = position - mousePosition;
					axisConstraint = new pb_HandleConstraint2D(1, 0);
				}
				else if(handleRectUp.Contains(mousePosition))
				{
					currentId = id;	
					handleOffset = position - mousePosition;
					axisConstraint = new pb_HandleConstraint2D(0, 1);
				}

			}		
		}

		return newPosition;
	}

	static Vector2 initialDirection;

	/**
	 * A 2D rotation handle.  Behaves like HandleUtility.RotationHandle
	 */
	public static float RotationHandle2d(int id, Vector2 position, float rotation, int radius)
	{
		Event e = Event.current;
		Vector2 mousePosition = e.mousePosition;
		float newRotation = rotation;

		Vector2 currentDirection = (mousePosition-position).normalized;

		// Draw gizmos
		Handles.color = HANDLE_COLOR_ROTATE;
		Handles.CircleCap(-1, position, Quaternion.identity, radius);
		
		if(currentId == id)
		{
			Handles.color = Color.gray;
			Handles.DrawLine(position, position + (mousePosition-position).normalized * radius );
			GUI.Label(new Rect(position.x, position.y, 90f, 30f), newRotation.ToString("F2") + pb_Constant.DEGREE_SYMBOL);
		}

		// If a Tool already is engaged and it's not this one, bail.
		if(currentId >= 0 && currentId != id)
			return rotation;

		if(currentId == id)
		{
			switch(e.type)
			{
				case EventType.MouseDrag:

					newRotation = Vector2.Angle(initialDirection, currentDirection);					
					
					if(Vector2.Dot(new Vector2(-initialDirection.y, initialDirection.x), currentDirection) < 0)
						newRotation = 360f-newRotation;
					break;

				case EventType.MouseUp:
				case EventType.Ignore:
					currentId = -1;
					break;
			}
		}
		else
		{
			if(e.type == EventType.MouseDown && ((!limitToLeftButton && e.button != MIDDLE_MOUSE_BUTTON) || e.button == LEFT_MOUSE_BUTTON))
			{
				if( Mathf.Abs(Vector2.Distance(mousePosition, position)-radius) < 8)
				{
					currentId = id;
					initialMousePosition = mousePosition;
					initialDirection = (initialMousePosition-position).normalized;
					handleOffset = position-mousePosition;
				}
			}
		}

		return newRotation;
	}

	/**
	 * Draw a working scale handle in 2d space.
	 */
	public static Vector2 ScaleHandle2d(int id, Vector2 position, Vector2 scale, int size)
	{
		Event e = Event.current;
		Vector2 mousePosition = e.mousePosition;
		int width = size/4;

		Handles.color = HANDLE_COLOR_UP;
		Handles.DrawLine(position, position - Vector2.up * size * scale.y);

		if(position.y - size > 0f)
			Handles.CubeCap(0, 
				((Vector3)((position - Vector2.up*scale.y*size))) - Vector3.forward*16,
				QuaternionUp,
				width/3);

		Handles.color = HANDLE_COLOR_RIGHT;
		Handles.DrawLine(position, position + Vector2.right * size * scale.x);

		if(position.y > 0f)
			Handles.CubeCap(0, 
				((Vector3)((position + Vector2.right*scale.x*size))) - Vector3.forward*16,
				Quaternion.Euler(Vector3.up*90f),
				width/3);

		Handles.color = HANDLE_COLOR_SCALE;
		Handles.CubeCap(0, 
			((Vector3)position) - Vector3.forward*16,
			QuaternionUp,
			width/2);

		// If a Tool already is engaged and it's not this one, bail.
		if(currentId >= 0 && currentId != id)
			return scale;

		Rect handleRectUp = new Rect(position.x-width/2, position.y-size-HANDLE_PADDING, width, size + HANDLE_PADDING);
		Rect handleRectRight = new Rect(position.x, position.y-width/2, size + 8, width);
		Rect handleRectCenter = new Rect(position.x-width/2, position.y-width/2, width, width);

		if(currentId == id)
		{
			switch(e.type)
			{
				case EventType.MouseDrag:
					Vector2 diff = axisConstraint.Mask(mousePosition - initialMousePosition);
					diff.x+=size;
					diff.y = -diff.y;	// gui space Y is opposite-world
					diff.y+=size;
					scale = diff/size;
					if(axisConstraint == pb_HandleConstraint2D.None)
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
			if(e.type == EventType.MouseDown && ((!limitToLeftButton && e.button != MIDDLE_MOUSE_BUTTON) || e.button == LEFT_MOUSE_BUTTON))
			{
				if(handleRectCenter.Contains(mousePosition))
				{
					currentId = id;
					handleOffset = position - mousePosition;
					initialMousePosition = mousePosition;
					axisConstraint = new pb_HandleConstraint2D(1, 1);
				}
				else
				if(handleRectRight.Contains(mousePosition))
				{
					currentId = id;	
					handleOffset = position - mousePosition;
					initialMousePosition = mousePosition;
					axisConstraint = new pb_HandleConstraint2D(1, 0);
				}
				else if(handleRectUp.Contains(mousePosition))
				{
					currentId = id;	
					handleOffset = position - mousePosition;
					initialMousePosition = mousePosition;
					axisConstraint = new pb_HandleConstraint2D(0, 1);
				}

			}		
		}

		return scale;
	}
#endregion

#region Raycast

	public static bool FaceRaycast(Vector2 mousePosition, out pb_Object pb, out pb_RaycastHit hit)
	{
		pb = null;
		hit = null;

		GameObject go = HandleUtility.PickGameObject(mousePosition, false);

		if(go == null)
			return false;

		pb = go.GetComponent<pb_Object>();

		if(pb == null)
			return false;

		Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

		return MeshRaycast(ray, pb, out hit);
	}

	/**
	 * Find a triangle intersected by InRay on InMesh.  InRay is in world space.
	 */
	public static bool MeshRaycast(Ray InWorldRay, pb_Object pb, out pb_RaycastHit hit)
	{
		return MeshRaycast(InWorldRay, pb, out hit, Mathf.Infinity, Culling.Front);
	}

	/**
	 * Find the nearest triangle intersected by InWorldRay on this pb_Object.  InWorldRay is in world space.
	 * @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
	 * point may be.  @cullingMode determines what face orientations are tested (Culling.Front only tests front 
	 * faces, Culling.Back only tests back faces, and Culling.FrontBack tests both).
	 */
	public static bool MeshRaycast(Ray InWorldRay, pb_Object pb, out pb_RaycastHit hit, float distance, Culling cullingMode)
	{
		/**
		 * Transform ray into model space
		 */

		InWorldRay.origin 		-= pb.transform.position;  // Why doesn't worldToLocalMatrix apply translation?
		InWorldRay.origin 		= pb.transform.worldToLocalMatrix * InWorldRay.origin;
		InWorldRay.direction 	= pb.transform.worldToLocalMatrix * InWorldRay.direction;

		Vector3[] vertices = pb.vertices;

		float dist = 0f;
		Vector3 point = Vector3.zero;

		float OutHitPoint = Mathf.Infinity;
		float dot; // vars used in loop
		Vector3 nrm;	// vars used in loop
		int OutHitFace = -1;
		Vector3 OutNrm = Vector3.zero;

		/**
		 * Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
		 */
		for(int CurFace = 0; CurFace < pb.faces.Length; ++CurFace)
		{
			int[] Indices = pb.faces[CurFace].indices;

			for(int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
			{
				Vector3 a = vertices[Indices[CurTriangle+0]];
				Vector3 b = vertices[Indices[CurTriangle+1]];
				Vector3 c = vertices[Indices[CurTriangle+2]];

				nrm = Vector3.Cross(b-a, c-a);
				dot = Vector3.Dot(InWorldRay.direction, nrm);

				bool ignore = false;

				switch(cullingMode)
				{
					case Culling.Front:
						if(dot > 0f) ignore = true;
						break;

					case Culling.Back:
						if(dot < 0f) ignore = true;
						break;
				}

				if(!ignore && pb_Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
				{
					if(dist > OutHitPoint || dist > distance)
						continue;

					OutNrm = nrm;
					OutHitFace = CurFace;
					OutHitPoint = dist;

					continue;
				}
			}
		}

		hit = new pb_RaycastHit(OutHitPoint,
								InWorldRay.GetPoint(OutHitPoint),
								OutNrm,
								OutHitFace);

		return OutHitFace > -1;
	}

	/**
	 * Find the all triangles intersected by InWorldRay on this pb_Object.  InWorldRay is in world space.
	 * @hit contains information about the hit point.  @distance limits how far from @InWorldRay.origin the hit
	 * point may be.  @cullingMode determines what face orientations are tested (Culling.Front only tests front 
	 * faces, Culling.Back only tests back faces, and Culling.FrontBack tests both).
	 */
	public static bool MeshRaycast(Ray InWorldRay, pb_Object pb, out List<pb_RaycastHit> hits, float distance, Culling cullingMode)
	{
		/**
		 * Transform ray into model space
		 */
		InWorldRay.origin -= pb.transform.position;  // Why doesn't worldToLocalMatrix apply translation?
		
		InWorldRay.origin 		= pb.transform.worldToLocalMatrix * InWorldRay.origin;
		InWorldRay.direction 	= pb.transform.worldToLocalMatrix * InWorldRay.direction;

		Vector3[] vertices = pb.vertices;

		float dist = 0f;
		Vector3 point = Vector3.zero;

		float dot; // vars used in loop
		Vector3 nrm;	// vars used in loop
		hits = new List<pb_RaycastHit>();

		/**
		 * Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
		 */
		for(int CurFace = 0; CurFace < pb.faces.Length; ++CurFace)
		{
			int[] Indices = pb.faces[CurFace].indices;

			for(int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
			{
				Vector3 a = vertices[Indices[CurTriangle+0]];
				Vector3 b = vertices[Indices[CurTriangle+1]];
				Vector3 c = vertices[Indices[CurTriangle+2]];

				if(pb_Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
				{
					nrm = Vector3.Cross(b-a, c-a);

					switch(cullingMode)
					{
						case Culling.Front:
							dot = Vector3.Dot(InWorldRay.direction, -nrm);

							if(dot > 0f)
								goto case Culling.FrontBack;
							break;

						case Culling.Back:
							dot = Vector3.Dot(InWorldRay.direction, nrm);

							if(dot > 0f)
								goto case Culling.FrontBack;
							break;

						case Culling.FrontBack:
							hits.Add( new pb_RaycastHit(dist,
														InWorldRay.GetPoint(dist),
														nrm,
														CurFace));
							break;
					}

					continue;
				}
			}
		}

		return hits.Count > 0;
	}
#endregion

#region Point Methods

	/**
	 * Given two Vector4[] arrays, find the nearest two points (x,y) within maxDelta and return the difference in offset. 
	 * @param points First Vector4[] array.
	 * @param compare The Vector4[] array to compare @c points againts.
	 * @mask If mask is not null, any index in mask will not be used in the compare array.
	 * @param maxDelta The maximum distance for two points to be apart to be considered for nearness.
	 * @notes This should probably use a divide and conquer algorithm instead of the O(n^2) approach (http://www.geeksforgeeks.org/closest-pair-of-points/)
	 */
	public static bool NearestPointDelta(IList<Vector4> points, IList<Vector4> compare, int[] mask, float maxDelta, out Vector4 offset)
	{
		float dist = 0f;
		float minDist = maxDelta * maxDelta;
		bool foundMatch = false;
		offset = Vector4.zero;

		for(int i = 0; i < points.Count; i++)
		{
			for(int n = 0; n < compare.Count; n++)
			{
				if(points[i] == compare[n]) continue;

				dist = pb_VectorUtility.SqrDistance2D(points[i], compare[n]);
				
				if(dist < minDist)
				{
					if( mask != null && System.Array.IndexOf(mask, n) > -1 )
						continue;

					minDist = dist;
					offset = compare[n] - points[i];
					foundMatch = true;
				}
			}
		}

		return foundMatch;
	}

	/**
	 * Returns the index of the nearest point in the points array, or -1 if no point is within maxDelta range.
	 */
	public static int NearestPoint(Vector4 point, IList<Vector4> points, float maxDelta)
	{
		float dist = 0f;
		float minDist = maxDelta * maxDelta;
		int index = -1;

		for(int i = 0; i < points.Count; i++)
		{
			if(point == points[i]) continue;

			dist = pb_VectorUtility.SqrDistance2D(point, points[i]);

			if(dist < minDist)
			{
				minDist = dist;
				index = i;
			}
		}	

		return index;
	}
#endregion

#region Wireframe

		/**
		 * Generate a line segment bounds representation.
		 */
		public static Mesh BoundsWireframe(Bounds bounds, Color color, ref Mesh m)
		{
			Vector3 cen = bounds.center;
			Vector3 ext = bounds.extents + (bounds.extents.normalized * .02f);

			// Draw Wireframe
			List<Vector3> v = new List<Vector3>();

			v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y,  ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y,  ext.z, .2f) );

			v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y,  ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y,  ext.z, .2f) );

			Vector2[] u = new Vector2[48];
			int[] t = new int[48];
			Color[] c = new Color[48];

			for(int i = 0; i < 48; i++)
			{
				t[i] = i;
				u[i] = Vector2.zero;
				c[i] = color;
				c[i].a = .5f;
			}

			m.Clear();
			m.vertices = v.ToArray();
			m.subMeshCount = 1;
			m.SetIndices(t, MeshTopology.Lines, 0);

			m.uv = u;
			m.normals = v.ToArray();
			m.colors = c; 

			return m;
		}

		private static Vector3[] DrawBoundsEdge(Vector3 center, float x, float y, float z, float size)
		{
			Vector3 p = center;
			Vector3[] v = new Vector3[6];

			p.x += x;
			p.y += y;
			p.z += z;

			v[0] = p;
			v[1] = (p + ( -(x/Mathf.Abs(x)) * Vector3.right 	* Mathf.Min(size, Mathf.Abs(x))));

			v[2] = p;
			v[3] = (p + ( -(y/Mathf.Abs(y)) * Vector3.up 		* Mathf.Min(size, Mathf.Abs(y))));

			v[4] = p;
			v[5] = (p + ( -(z/Mathf.Abs(z)) * Vector3.forward 	* Mathf.Min(size, Mathf.Abs(z))));

			return v;
		}
#endregion	
}
