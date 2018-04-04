using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ProBuilder.Core;
using UnityEngine.Rendering;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Utilities for creating and manipulating Handles and points in GUI space.
	/// </summary>
	static class pb_EditorHandleUtility
	{
		public static bool SceneViewInUse(Event e)
		{
			return 	e.alt
					|| Tools.current == Tool.View
					|| GUIUtility.hotControl > 0
					|| (e.isMouse ? e.button > 1 : false)
					|| Tools.viewTool == ViewTool.FPS
					|| Tools.viewTool == ViewTool.Orbit;
		}

		const int HANDLE_PADDING = 8;
		const int LEFT_MOUSE_BUTTON = 0;
		const int MIDDLE_MOUSE_BUTTON = 2;

		static readonly Quaternion QuaternionUp = Quaternion.Euler(Vector3.right*90f);
		static readonly Quaternion QuaternionRight = Quaternion.Euler(Vector3.up*90f);
		static readonly Vector3 ConeDepth = new Vector3(0f, 0f, 16f);

		static readonly Color k_HandleColorUp = new Color(0f, .7f, 0f, .8f);
		static readonly Color k_HandleColorRight = new Color(0f, 0f, .7f, .8f);
		static readonly Color k_HandleColorRotate = new Color(0f, .7f, 0f, .8f);
		static readonly Color k_HandleColorScale = new Color(.7f, .7f, .7f, .8f);

		static Material m_HandleMaterial = null;

		public static Material handleMaterial
		{
			get
			{
				if(m_HandleMaterial == null)
					m_HandleMaterial = (Material) EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");

				return m_HandleMaterial;
			}
		}

		static Material m_UnlitVertexColorMaterial = null;

		public static Material unlitVertexColorMaterial
		{
			get
			{
				if (m_UnlitVertexColorMaterial == null)
				{
					Shader unlitVertexColorShader = Shader.Find("ProBuilder/UnlitVertexColor");

					if (unlitVertexColorShader == null)
					{
						m_UnlitVertexColorMaterial = handleMaterial;
					}
					else
					{
						m_UnlitVertexColorMaterial = new Material(unlitVertexColorShader);
						m_UnlitVertexColorMaterial.hideFlags = HideFlags.HideAndDontSave;
					}
				}

				return m_UnlitVertexColorMaterial;
			}
		}


		static Material m_EdgeMaterial = null;

		public static Material edgeMaterial
		{
			get
			{
				if(m_EdgeMaterial == null)
					m_EdgeMaterial = (Material) EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
					// _edgeMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/VertexSelectionMaterial.mat");

				return m_EdgeMaterial;
			}
		}

		public static int CurrentID { get { return currentId; } }
		static int currentId = -1;

		static Vector2 handleOffset = Vector2.zero;
		static Vector2 initialMousePosition = Vector2.zero;

		static pb_HandleConstraint2D axisConstraint = new pb_HandleConstraint2D(0, 0);	// Multiply this value by input to mask axis movement.
		public static pb_HandleConstraint2D CurrentAxisConstraint { get { return axisConstraint; } }

		public static bool limitToLeftButton = true;

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
			pb_Handles.CircleCap(-1, position, Quaternion.identity, width / 2f);
			Handles.color = k_HandleColorUp;

			// Y Line
			Handles.DrawLine(position, position - Vector2.up * size);

			// Y Cone
			if(position.y - size > 0f)
				pb_Handles.ConeCap(0,
					((Vector3)((position - Vector2.up*size))) - ConeDepth,
					QuaternionUp,
					width/2);

			Handles.color = k_HandleColorRight;

			// X Line
			Handles.DrawLine(position, position + Vector2.right * size);

			// X Cap
			if(position.y > 0f)
				pb_Handles.ConeCap(0,
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
			Handles.color = k_HandleColorRotate;
			pb_Handles.CircleCap(-1, position, Quaternion.identity, radius);

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

			Handles.color = k_HandleColorUp;
			Handles.DrawLine(position, position - Vector2.up * size * scale.y);

			if(position.y - size > 0f)
				pb_Handles.CubeCap(0,
					((Vector3)((position - Vector2.up*scale.y*size))) - Vector3.forward*16,
					QuaternionUp,
					width/3);

			Handles.color = k_HandleColorRight;
			Handles.DrawLine(position, position + Vector2.right * size * scale.x);

			if(position.y > 0f)
				pb_Handles.CubeCap(0,
					((Vector3)((position + Vector2.right*scale.x*size))) - Vector3.forward*16,
					Quaternion.Euler(Vector3.up*90f),
					width/3);

			Handles.color = k_HandleColorScale;
			pb_Handles.CubeCap(0,
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

		/**
		 * Pick the GameObject nearest mousePosition (filtering out @ignore) and raycast for a face it.
		 */
		internal static bool FaceRaycast(Vector2 mousePosition, out pb_Object pb, out pb_RaycastHit hit, Dictionary<pb_Object, HashSet<pb_Face>> ignore = null)
		{
			pb = null;
			hit = null;

			GameObject go = HandleUtility.PickGameObject(mousePosition, false);

			if(go == null)
				return false;

			pb = go.GetComponent<pb_Object>();

			if(pb == null || (ignore != null && ignore.ContainsKey(pb)))
				return false;

			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

			return pb_HandleUtility.FaceRaycast(ray, pb, out hit, ignore[pb]);
		}

		/**
		 * Return all GameObjects under the mousePosition.
		 * Note - only available from Unity 5.3+. Prior versions return first GameObject always.
		 */
		internal static List<GameObject> GetAllOverlapping(Vector2 mousePosition)
		{
#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			return new List<GameObject>() { HandleUtility.PickGameObject(mousePosition, false) };
#else
			List<GameObject> intersecting = new List<GameObject>();

			GameObject nearestGameObject = null;

			do
			{
				nearestGameObject = HandleUtility.PickGameObject(mousePosition, false, intersecting.ToArray());

				if(nearestGameObject != null)
					intersecting.Add(nearestGameObject);
				else
					break;
			}
			while( nearestGameObject != null );

			return intersecting;
#endif
		}

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

		/// <summary>
		/// Pick the closest point on a world space set of line segments.
		/// Similar to UnityEditor.HandleUtility version except this also
		/// returns the index of the segment that best matched (source modified
		/// from UnityEngine.HandleUtility class).
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="index"></param>
		/// <param name="distanceToLine"></param>
		/// <param name="closeLoop"></param>
		/// <param name="trs"></param>
		/// <returns></returns>
		public static Vector3 ClosestPointToPolyLine(List<Vector3> vertices, out int index, out float distanceToLine, bool closeLoop = false, Transform trs = null)
		{
			distanceToLine = Mathf.Infinity;

			if(trs != null)
				distanceToLine = HandleUtility.DistanceToLine(trs.TransformPoint(vertices[0]), trs.TransformPoint(vertices[1]));
			else
				distanceToLine = HandleUtility.DistanceToLine(vertices[0], vertices[1]);

			index = 0;
			int count = vertices.Count;

			for (int i = 2; i < (closeLoop ? count + 1 : count); i++)
			{
				var distance = 0f;

				if(trs != null)
					distance = HandleUtility.DistanceToLine(trs.TransformPoint(vertices[i - 1]), trs.TransformPoint(vertices[i % count]));
				else
					distance = HandleUtility.DistanceToLine(vertices[i - 1], vertices[i % count]);

				if (distance < distanceToLine)
				{
					distanceToLine = distance;
					index = i - 1;
				}
			}

			Vector3 point_a = trs != null ? trs.TransformPoint(vertices[index]) : vertices[index];
			Vector3 point_b = trs != null ? trs.TransformPoint(vertices[(index + 1) % count]) : vertices[index + 1];

			index++;

			Vector2 gui_a = Event.current.mousePosition - HandleUtility.WorldToGUIPoint(point_a);
			Vector2 gui_b = HandleUtility.WorldToGUIPoint(point_b) - HandleUtility.WorldToGUIPoint(point_a);

			float magnitude = gui_b.magnitude;
			float travel = Vector3.Dot(gui_b, gui_a);

			if (magnitude > 1E-06f)
				travel /= magnitude * magnitude;

			Vector3 p = Vector3.Lerp(point_a, point_b, Mathf.Clamp01(travel));

			return trs != null ? trs.InverseTransformPoint(p) : p;
		}

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

		static Vector3[] DrawBoundsEdge(Vector3 center, float x, float y, float z, float size)
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

		static MethodInfo s_ApplyWireMaterial = null;

		static object[] s_ApplyWireMaterialArgs = new object[]
		{
			CompareFunction.Always
		};

		internal static bool BeginDrawingLines(CompareFunction zTest)
		{
			if (Event.current.type != EventType.Repaint)
				return false;

			if (!pb_MeshHandles.geometryShadersSupported ||
			    !pb_MeshHandles.lineMaterial.SetPass(0))
			{
				if (s_ApplyWireMaterial == null)
				{
					s_ApplyWireMaterial = typeof(HandleUtility).GetMethod(
						"ApplyWireMaterial",
						BindingFlags.Static | BindingFlags.NonPublic,
						null,
						new System.Type[] { typeof(CompareFunction) },
						null);

					if (s_ApplyWireMaterial == null)
					{
						pb_Log.Info("Failed to find wire material, stopping draw lines.");
						return false;
					}
				}

				s_ApplyWireMaterialArgs[0] = zTest;
				s_ApplyWireMaterial.Invoke(null, s_ApplyWireMaterialArgs);
			}

			GL.PushMatrix();
			GL.Begin(GL.LINES);

			return true;
		}

		internal static void EndDrawingLines()
		{
			GL.End();
			GL.PopMatrix();
		}
	}
}
