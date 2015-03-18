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

	/**
	 * Find a triangle intersected by InRay on InMesh.  InRay is in world space.
	 * Returns the index in pb.faces of the hit face, or -1.  Optionally can ignore
	 * backfaces.
	 */
	public static bool MeshRaycast(Ray InWorldRay, pb_Object pb, out pb_RaycastHit hit)
	{
		return MeshRaycast(InWorldRay, pb, out hit, true);
	}

	public static bool MeshRaycast(Ray InWorldRay, pb_Object pb, out pb_RaycastHit hit, bool ignoreBackfaces)
	{
		/**
		 * Transform ray into model space
		 */
		InWorldRay.origin -= pb.transform.position;  // Why doesn't worldToLocalMatrix apply translation?
		
		InWorldRay.origin 		= pb.transform.worldToLocalMatrix * InWorldRay.origin;
		InWorldRay.direction 	= pb.transform.worldToLocalMatrix * InWorldRay.direction;

		Vector3[] vertices = pb.vertices;

		float dist = 0f;
		float OutHitPoint = Mathf.Infinity;
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

				if(pb_Math.RayIntersectsTriangle(InWorldRay, a, b, c, out dist))
				{
					if(dist > OutHitPoint)
						continue;

					// Don't allow culled faces to trigger

					if(ignoreBackfaces)
					{
						Vector3 nrm = Vector3.Cross(b-a, c-a);
						float dot = Vector3.Dot(InWorldRay.direction, -nrm);

						if(dot > 0f)
						{
							OutNrm = nrm;
							OutHitFace = CurFace;
							OutHitPoint = dist;
						}
					}
					else
					{
						OutNrm = Vector3.Cross(b-a, c-a);
						OutHitFace = CurFace;
						OutHitPoint = dist;
					}

					continue;
				}
			}
		}

		hit = new pb_RaycastHit();
		hit.Distance = OutHitPoint;
		hit.Point = InWorldRay.GetPoint(OutHitPoint);
		hit.Normal = OutNrm;
		hit.FaceIndex = OutHitFace;

		return OutHitFace > -1;
	}
#endregion

#region Point Methods

	/**
	 * Given two Vector2[] arrays, find the nearest two points within maxDelta and return the difference in offset. 
	 * @param points First Vector2[] array.
	 * @param compare The Vector2[] array to compare @c points againts.
	 * @mask If mask is not null, any index in mask will not be used in the compare array.
	 * @param maxDelta The maximum distance for two points to be apart to be considered for nearness.
	 * @notes This should probably use a divide and conquer algorithm instead of the O(n^2) approach (http://www.geeksforgeeks.org/closest-pair-of-points/)
	 */
	public static bool NearestPointDelta(Vector2[] points, Vector2[] compare, int[] mask, float maxDelta, out Vector2 offset)
	{
		float dist = 0f;
		float minDist = maxDelta;
		bool foundMatch = false;
		offset = Vector2.zero;

		for(int i = 0; i < points.Length; i++)
		{
			for(int n = 0; n < compare.Length; n++)
			{
				if(points[i] == compare[n]) continue;

				dist = Vector2.Distance(points[i], compare[n]);
				
				if(dist < minDist)
				{
					if( mask != null && System.Array.IndexOf(mask, n) > -1 )
						continue;

					minDist = dist;
					offset = compare[n]-points[i];
					foundMatch = true;
				}
			}
		}

		return foundMatch;
	}

	/**
	 * Returns the index of the nearest point in the points array, or -1 if no point is within maxDelta range.
	 */
	public static int NearestPoint(Vector2 point, Vector2[] points, float maxDelta)
	{
		float dist = 0f;
		float minDist = maxDelta;
		int index = -1;

		for(int i = 0; i < points.Length; i++)
		{
			if(point == points[i]) continue;

			dist = Vector2.Distance(point, points[i]);

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
		public static Mesh BoundsWireframe(Bounds bounds)
		{
			Vector3 cen = bounds.center;
			Vector3 ext = bounds.extents;

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
				c[i] = Color.white;
				c[i].a = .5f;
			}

			Mesh m = new Mesh();
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

#region Fill

	#if PB_DEBUG
	public static bool GeneratePolygonCrosshatch(pb_Profiler profiler, Vector2[] polygon, float scale, Color color, int lineSpacing, ref Texture2D texture)
	#else
	public static bool GeneratePolygonCrosshatch(Vector2[] polygon, float scale, Color color, int lineSpacing, ref Texture2D texture)
	#endif
	{
		#if PB_DEBUG
		profiler.BeginSample("GeneratePolygonCrosshatch");
		#endif

		pb_Bounds2D bounds = new pb_Bounds2D(polygon);

		Vector2 offset = bounds.center - bounds.extents;

		/// shift polygon to origin 0,0
		for(int i = 0; i < polygon.Length; i++)
		{
			polygon[i] -= offset;
			polygon[i] *= scale;
		}

		bounds.center -= offset;
		bounds.size *= scale;

		int width = (int)(bounds.size.x);
		int height = (int)(bounds.size.y);

		if(width <= 0 || height <= 0)
			return false;

		#if PB_DEBUG
		profiler.BeginSample("Allocate Texture");
		#endif

		if(texture == null)
		{
			texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
		}
		else
		{
			if(texture.width != width || texture.height != height)
				texture.Resize(width, height, TextureFormat.ARGB32, false);
		}

		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("Fill Clear");
		#endif

		Color[] colors = new Color[width*height];
		List<int> intersects = new List<int>();

		for(int i = 0; i < width*height; i++)
			colors[i] = Color.clear;

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		/**
		 *	Horizontal lines
		 */
		for(int h = 0; h < height/lineSpacing; h++)
		{	
			int y = (h*lineSpacing);
			intersects.Clear();

			#if PB_DEBUG
			profiler.BeginSample("Find Intersections");
			#endif

			Vector2 start = new Vector2(bounds.center.x - bounds.size.x, y);
			Vector2 end = new Vector2(bounds.center.x + bounds.size.x, y);

			for(int i = 0; i < polygon.Length; i+=2)
			{
				Vector2 intersect = Vector2.zero;

				if( pb_Math.GetLineSegmentIntersect(polygon[i], polygon[i+1], start, end, ref intersect) )
					intersects.Add((int)intersect.x);
			}

			intersects = intersects.Distinct().ToList();
			intersects.Sort();

			#if PB_DEBUG
			profiler.EndSample();
			profiler.BeginSample("Fill Color");
			#endif

			for(int i = 0; i < intersects.Count-1; i++)
			{
				// can't just use Dot product because we the winding order isn't consistent
				if( pb_Math.PointInPolygon(polygon, new Vector2(intersects[i]+2, y)) )
				{
					for(int n = intersects[i]; n < intersects[i+1]; n++)
					{
						colors[ ((height-1)-y) * width + n] = color;
					}
				}
			}

			#if PB_DEBUG
			profiler.EndSample();
			#endif
		}

		/**
		 *	Vertical lines
		 */
		if(lineSpacing > 1)
		{
			for(int w = 0; w < width/lineSpacing; w++)
			{	
				int x = (w*lineSpacing);

				intersects.Clear();

				Vector2 start = new Vector2(x, bounds.center.y - bounds.size.y);
				Vector2 end = new Vector2(x, bounds.center.y + bounds.size.y);

				for(int i = 0; i < polygon.Length; i+=2)
				{
					Vector2 intersect = Vector2.zero;
					if( pb_Math.GetLineSegmentIntersect(polygon[i], polygon[i+1], start, end, ref intersect) )
						intersects.Add((int)intersect.y);
				}

				intersects = intersects.Distinct().ToList();
				intersects.Sort();

				for(int i = 0; i < intersects.Count-1; i++)
				{
					if(pb_Math.PointInPolygon(polygon, new Vector2(x, intersects[i]+2)))
					{
						for(int y = intersects[i]; y < intersects[i+1]; y++)
						{	
							colors[ ((height-1)-y) * width + x] = color;
						}
					}
				}
			}
		}

		#if PB_DEBUG
		profiler.BeginSample("SetPixels");
		#endif
	
		texture.SetPixels(colors);
		texture.Apply(false);
		
		#if PB_DEBUG
		profiler.EndSample();
		profiler.EndSample();
		#endif

		return true;
	}
#endregion	
}
