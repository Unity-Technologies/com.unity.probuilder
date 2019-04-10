// Partially derived from FlyThrough.js available on the Unify Wiki
// http://wiki.unity3d.com/index.php/FlyThrough

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace ProBuilder.Examples
{
	public enum ViewTool
	{
		/// <value>
		/// Camera is not in control of anything
		/// </value>
		None,
		/// <value>
		/// Camera is spherically rotating around target
		/// </value>
		Orbit,
		/// <value>
		/// Camera is moving right or left
		/// </value>
		Pan,
		/// <value>
		/// Camera is moving forward or backwards
		/// </value>
		Dolly,
		/// <value>
		/// Camera is looking and possibly flying
		/// </value>
		Look
	}

	/**
	 * Requires InputSettings to have:
	 * 	- "Horizontal", "Vertical", "CameraUp", with Gravity and Sensitivity set to 3.
	 */
	[RequireComponent(typeof(Camera))]
	sealed class CameraMotion : MonoBehaviour
	{
		public ViewTool cameraState { get; private set; }

#pragma warning disable 649
		[SerializeField]
		Texture2D panCursor;

		[SerializeField]
		Texture2D orbitCursor;

		[SerializeField]
		Texture2D dollyCursor;

		[SerializeField]
		Texture2D lookCursor;
#pragma warning restore 649

		Texture2D m_CurrentCursor;

		const int k_CursorIconSize = 64;

		const string k_InputMouseScrollwheel = "Mouse ScrollWheel";
		const string k_InputMouseHorizontal = "Mouse X";
		const string k_InputMouseVertical = "Mouse Y";

		const int k_LeftMouse = 0;
		const int k_RightMouse = 1;
		const int k_MiddleMouse = 2;

		const float k_MinCameraDistance = 1f;
		const float k_MaxCameraDistance = 100f;

#if USE_DELTA_TIME
		public float moveSpeed = 15f;
		public float lookSpeed = 200f;
		public float orbitSpeed = 200f;
		public float scrollModifier = 100f;
		public float zoomSpeed = .05f;
#else
		// How fast the camera position moves.
		public float moveSpeed = 15f;

		// How fast the camera rotation adjusts.
		public float lookSpeed = 5f;

		// How fast the camera rotation adjusts.
		public float orbitSpeed = 7f;

		// How fast the mouse scroll wheel affects distance from pivot.
		public float scrollModifier = 100f;

		public float zoomSpeed = .1f;
#endif

		bool m_UseEvent;
		Camera m_CameraComponent;
		Transform m_Transform;
		Vector3 m_ScenePivot = Vector3.zero;
		float m_DistanceToCamera = 10f;

		// Store the mouse position from the last frame. Used in calculating deltas for mouse movement.
		Vector3 m_PreviousMousePosition = Vector3.zero;

		Rect m_MouseCursorRect = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize);
		Rect m_ScreenCenterRect = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize);

		bool m_CurrentActionValid = true;

		void Awake()
		{
			m_CameraComponent = GetComponent<Camera>();
			Assert.IsNotNull(m_CameraComponent);
			m_Transform = GetComponent<Transform>();
			m_ScenePivot = m_Transform.forward * m_DistanceToCamera;
		}

		void OnGUI()
		{
			float screenHeight = Screen.height;

			m_MouseCursorRect.x = Input.mousePosition.x - 16;
			m_MouseCursorRect.y = (screenHeight - Input.mousePosition.y) - 16;

			m_ScreenCenterRect.x = Screen.width/2-32;
			m_ScreenCenterRect.y = screenHeight/2-32;

			Cursor.visible = cameraState == ViewTool.None;

			if(cameraState != ViewTool.None)
			{
				switch(cameraState)
				{
					case ViewTool.Orbit:
						GUI.Label(m_MouseCursorRect, orbitCursor);
						break;
					case ViewTool.Pan:
						GUI.Label(m_MouseCursorRect, panCursor);
						break;
					case ViewTool.Dolly:
						GUI.Label(m_MouseCursorRect, dollyCursor);
						break;
					case ViewTool.Look:
						GUI.Label(m_MouseCursorRect, lookCursor);
						break;
				}
			}
		}

		public bool active
		{
			get { return cameraState != ViewTool.None || m_UseEvent || Input.GetKey(KeyCode.LeftAlt); }
		}

		bool CheckMouseOverGUI()
		{
			return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
		}

		public void DoLateUpdate()
		{
			if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
			{
				m_CurrentActionValid = true;
				m_UseEvent = false;
			}
			else if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
			{
				m_CurrentActionValid = CheckMouseOverGUI();
			}

			cameraState = ViewTool.None;

			// Camera is flying itself to a target
			if(m_Zooming)
			{
				transform.position = Vector3.Lerp(m_PreviousPosition, m_TargetPosition, (m_ZoomProgress += Time.deltaTime)/zoomSpeed);
				if( Vector3.Distance(transform.position, m_TargetPosition) < .1f) m_Zooming = false;
			}

			if( (Input.GetAxis(k_InputMouseScrollwheel) != 0f || (Input.GetMouseButton(k_RightMouse) && Input.GetKey(KeyCode.LeftAlt))) && CheckMouseOverGUI())
			{
				float delta = Input.GetAxis(k_InputMouseScrollwheel);

				if( Mathf.Approximately(delta, 0f) )
				{
					cameraState = ViewTool.Dolly;
					delta = CalcSignedMouseDelta(Input.mousePosition, m_PreviousMousePosition);
				}

				m_DistanceToCamera -= delta * (m_DistanceToCamera/k_MaxCameraDistance) * scrollModifier;
				m_DistanceToCamera = Mathf.Clamp(m_DistanceToCamera, k_MinCameraDistance, k_MaxCameraDistance);
				m_Transform.position = m_Transform.localRotation * (Vector3.forward * -m_DistanceToCamera) + m_ScenePivot;
			}

			bool viewTool = true;

			// If the current tool isn't View, or no mouse button is pressed, record the mouse position then early exit.
			if (!m_CurrentActionValid || (viewTool
#if !CONTROLLER
			                            && !Input.GetMouseButton(k_LeftMouse)
			                            && !Input.GetMouseButton(k_RightMouse)
			                            && !Input.GetMouseButton(k_MiddleMouse)
			                            && !Input.GetKey(KeyCode.LeftAlt)
#endif
			    ))
			{
				Rect screen = new Rect(0, 0, Screen.width, Screen.height);

				if (screen.Contains(Input.mousePosition))
					m_PreviousMousePosition = Input.mousePosition;

				return;
			}

			// FPS view camera
			if (Input.GetMouseButton(k_RightMouse) && !Input.GetKey(KeyCode.LeftAlt)
			) //|| Input.GetKey(KeyCode.LeftShift) )
			{
				cameraState = ViewTool.Look;

				m_UseEvent = true;

				// Rotation
				float rotX = Input.GetAxis(k_InputMouseHorizontal);
				float rotY = Input.GetAxis(k_InputMouseVertical);

				Vector3 eulerRotation = m_Transform.localRotation.eulerAngles;

#if USE_DELTA_TIME
				eulerRotation.x -= rot_y * lookSpeed * Time.deltaTime; 	// Invert Y axis
				eulerRotation.y += rot_x * lookSpeed * Time.deltaTime;
#else
				eulerRotation.x -= rotY * lookSpeed;
				eulerRotation.y += rotX * lookSpeed;
#endif
				eulerRotation.z = 0f;
				m_Transform.localRotation = Quaternion.Euler(eulerRotation);

				// PositionHandle-- Always use delta time when flying
				float speed = moveSpeed * Time.deltaTime;

				m_Transform.position += m_Transform.forward * speed * Input.GetAxis("Vertical");
				m_Transform.position += m_Transform.right * speed * Input.GetAxis("Horizontal");

				try
				{
					m_Transform.position += m_Transform.up * speed * Input.GetAxis("CameraUp");
				}
				catch
				{
					Debug.LogWarning(
						"CameraUp input is not configured.  Open \"Edit/Project Settings/Input\" and add an input named \"CameraUp\", mapping q and e to Negative and Positive buttons.");
				}

				m_ScenePivot = transform.position + transform.forward * m_DistanceToCamera;
			}
			// Orbit
			else if(Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(k_LeftMouse))
			{
				cameraState = ViewTool.Orbit;

				m_UseEvent = true;

				float rotX = Input.GetAxis(k_InputMouseHorizontal);
				float rotY = -Input.GetAxis(k_InputMouseVertical);

				Vector3 eulerRotation = transform.localRotation.eulerAngles;

				if ((Mathf.Approximately(eulerRotation.x, 90f) && rotY > 0f) ||
				    (Mathf.Approximately(eulerRotation.x, 270f) && rotY < 0f))
					rotY = 0f;

#if USE_DELTA_TIME
				eulerRotation.x += rot_y * orbitSpeed * Time.deltaTime;
				eulerRotation.y += rot_x * orbitSpeed * Time.deltaTime;
#else
				eulerRotation.x += rotY * orbitSpeed;
				eulerRotation.y += rotX * orbitSpeed;
#endif

				eulerRotation.z = 0f;

				transform.localRotation = Quaternion.Euler(eulerRotation);
				transform.position = CalculateCameraPosition(m_ScenePivot);
			}
			// Pan
			else if(Input.GetMouseButton(k_MiddleMouse) || (Input.GetMouseButton(k_LeftMouse) && viewTool ) )
			{
				cameraState = ViewTool.Pan;

				Vector2 delta = Input.mousePosition - m_PreviousMousePosition;

				delta.x = ScreenToWorldDistance(delta.x, m_DistanceToCamera);
				delta.y = ScreenToWorldDistance(delta.y, m_DistanceToCamera);

				m_Transform.position -= m_Transform.right * delta.x;
				m_Transform.position -= m_Transform.up * delta.y;

				m_ScenePivot = m_Transform.position + m_Transform.forward * m_DistanceToCamera;
			}

			m_PreviousMousePosition = Input.mousePosition;
		}

		Vector3 CalculateCameraPosition(Vector3 target)
		{
			return transform.localRotation * (Vector3.forward * -m_DistanceToCamera) + target;
		}

		bool m_Zooming = false;
		float m_ZoomProgress = 0f;
		Vector3 m_PreviousPosition = Vector3.zero;
		Vector3 m_TargetPosition = Vector3.zero;

		/// <summary>
		/// Lerp the camera to the current selection
		/// </summary>
		/// <param name="target"></param>
		public void Focus(GameObject target)
		{
			Vector3 center = target.transform.position;
			Renderer renderer = target.GetComponent<Renderer>();
			Bounds bounds = renderer != null ? renderer.bounds : new Bounds(center, Vector3.one * 10f);

			m_DistanceToCamera = CalcMinDistanceToBounds(m_CameraComponent, bounds) + 2f;
			m_DistanceToCamera += m_DistanceToCamera;
			center = bounds.center;

			Focus(center, m_DistanceToCamera);
		}

		public void Focus(Vector3 target, float distance)
		{
			m_ScenePivot = target;
			m_DistanceToCamera = distance;
			m_PreviousPosition = transform.position;
			m_TargetPosition = CalculateCameraPosition( m_ScenePivot );
			m_ZoomProgress = 0f;
			m_Zooming = true;
		}

		float ScreenToWorldDistance(float screenDistance, float distanceFromCamera)
		{
			Vector3 start = m_CameraComponent.ScreenToWorldPoint(Vector3.forward * distanceFromCamera);
			Vector3 end = m_CameraComponent.ScreenToWorldPoint( new Vector3(screenDistance, 0f, distanceFromCamera));
			return CopySign(Vector3.Distance(start, end), screenDistance);
		}

		static float CalcSignedMouseDelta(Vector2 lhs, Vector2 rhs)
		{
			float delta = Vector2.Distance(lhs, rhs);
			float scale = 1f / Mathf.Min(Screen.width, Screen.height);

			// If horizontal movement is greater than vertical movement, use the X axis for sign.
			if( Mathf.Abs(lhs.x - rhs.x) > Mathf.Abs(lhs.y - rhs.y) )
				return delta * scale * ( (lhs.x-rhs.x) > 0f ? 1f : -1f );

			return delta * scale * ( (lhs.y-rhs.y) > 0f ? 1f : -1f );
		}

		static float CalcMinDistanceToBounds(Camera cam, Bounds bounds)
		{
			float frustumHeight = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
			float distance = frustumHeight * .5f / Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);

			return distance;
		}

		/// <summary>
		/// Return the magnitude of X with the sign of Y.
		/// </summary>
		float CopySign(float x, float y)
		{
			if(x < 0f && y < 0f || x > 0f && y > 0f || x == 0f || y == 0f)
				return x;

			return -x;
		}
	}
}
