using UnityEngine;

namespace ODev.Camera
{
	public class SpectatorTarget : MonoBehaviour
	{
		public Transform ThirdPersonTarget = null;
		public Transform FirstPersonTarget = null;

		[SerializeField]
		private GameObject[] m_DisableOnSpectate = new GameObject[0];
		[SerializeField]
		private GameObject[] m_EnableOnSpectate = new GameObject[0];

		private void Start()
		{
			OnEnable(); // Calls after SpectatorCamera Singleton Awake();
		}

		private void OnEnable()
		{
			SpectatorCamera camera = SpectatorCamera.s_Instance;
			if (camera != null)
			{
				camera.Targets.Add(this);
			}
		}

		private void OnDisable()
		{
			SpectatorCamera camera = SpectatorCamera.s_Instance;
			if (camera == null)
			{
				return;
			}
			if (camera.isActiveAndEnabled)
			{
				camera.OnTargetLost(this);
			}
			camera.Targets.Remove(this);
		}

		public void Toggle(bool pFirstPerson)
		{
			foreach (GameObject o in m_DisableOnSpectate)
			{
				o.SetActive(!pFirstPerson);
			}
			foreach (GameObject o in m_EnableOnSpectate)
			{
				o.SetActive(pFirstPerson);
			}
		}
	}
}