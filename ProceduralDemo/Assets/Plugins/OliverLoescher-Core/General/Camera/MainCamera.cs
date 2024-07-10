using UnityEngine;

namespace OCore
{
	[RequireComponent(typeof(Camera))]
    public class MainCamera : MonoBehaviour
    {
		public static MainCamera Instance = null;
		public static Camera Camera = null;
		private Camera localCamera = null;

		public static Vector3 Position => Instance.transform.position;
		public static Quaternion Rotation => Instance.transform.rotation;
		public static Vector3 Forward => Instance.transform.forward;
		public static Vector3 Up => Instance.transform.up;

		private void Awake()
		{
			if (Instance != null)
			{
				Util.Debug.DevException($"Second MainCamera, this should never happen. Please ensure cleaning up the old one first if a new one is intended. Destroying self ({gameObject.name}).", this);
				Destroy(gameObject);
				return;
			}
			Instance = this;
			localCamera = GetComponent<Camera>();
			OnEnable();
		}

		private void OnEnable()
		{
			Camera = localCamera;
		}

		private void OnDisable()
		{
			Camera = null;
		}
	}
}
