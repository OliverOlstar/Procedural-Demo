using UnityEngine;

namespace ODev
{
	[RequireComponent(typeof(Camera))]
    public class MainCamera : MonoBehaviourSingleton<MainCamera>
    {
		private Camera m_Camera = null;
		private Camera m_LocalCamera = null;

		public static Camera Camera => Instance.m_Camera;
		public static Vector3 Position => Instance.transform.position;
		public static Quaternion Rotation => Instance.transform.rotation;
		public static Vector3 Forward => Instance.transform.forward;
		public static Vector3 Up => Instance.transform.up;

		protected override void Awake()
		{
			base.Awake();
			
			m_LocalCamera = GetComponent<Camera>();
			OnEnable();
		}
		

		private void OnEnable()
		{
			m_Camera = m_LocalCamera;
		}

		private void OnDisable()
		{
			m_Camera = null;
		}
	}
}
