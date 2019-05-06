// A simple orbiting camera

using UnityEngine;

namespace ProBuilder.Examples
{
	public class CameraControls : MonoBehaviour
	{
		const string k_InputMouseScroll = "Mouse ScrollWheel";
		const string k_InputMouseHorizontal = "Mouse X";
		const string k_InputMouseVertical = "Mouse Y";
		const float k_MinCameraDistance = 10f;
		const float k_MaxCameraDistance = 40f;

		[Tooltip("How fast the camera should rotate around the sphere.")]
		[Range(2f, 15f)]
		public float orbitSpeed = 6f;

		[Tooltip("The speed at which the camera zooms in and out.")]
		[Range(.3f, 2f)]
		public float zoomSpeed = .8f;

		[Tooltip("How fast the camera should rotate around the sphere when idle.")]
		public float idleRotation = 1f;

		float m_Distance = 0f;
		Vector2 m_LookDirection = new Vector2(.8f, .2f);

		void Start()
		{
			m_Distance = Vector3.Distance(transform.position, Vector3.zero);
		}

		void LateUpdate()
		{
			Vector3 eulerRotation = transform.localRotation.eulerAngles;
			eulerRotation.z = 0f;

			// orbits
			if( Input.GetMouseButton(0) )
			{
				float rot_x = Input.GetAxis(k_InputMouseHorizontal);
				float rot_y = -Input.GetAxis(k_InputMouseVertical);

				eulerRotation.x += rot_y * orbitSpeed;
				eulerRotation.y += rot_x * orbitSpeed;

				// idle direction is derived from last user input.
				m_LookDirection.x = rot_x;
				m_LookDirection.y = rot_y;
				m_LookDirection.Normalize();
			}
			else
			{
				eulerRotation.y += Time.deltaTime * idleRotation * m_LookDirection.x;
				eulerRotation.x += Time.deltaTime * Mathf.PerlinNoise(Time.time, 0f) * idleRotation * m_LookDirection.y;
			}

			transform.localRotation = Quaternion.Euler( eulerRotation );
			transform.position = transform.localRotation * (Vector3.forward * -m_Distance);

			if( Input.GetAxis(k_InputMouseScroll) != 0f )
			{
				float delta = Input.GetAxis(k_InputMouseScroll);

				m_Distance -= delta * (m_Distance/k_MaxCameraDistance) * (zoomSpeed * 1000) * Time.deltaTime;
				m_Distance = Mathf.Clamp(m_Distance, k_MinCameraDistance, k_MaxCameraDistance);
				transform.position = transform.localRotation * (Vector3.forward * -m_Distance);
			}
		}
	}
}
