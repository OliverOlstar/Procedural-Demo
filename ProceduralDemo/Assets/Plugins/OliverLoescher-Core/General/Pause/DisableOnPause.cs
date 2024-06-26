using UnityEngine;

namespace OCore
{
	public class DisableOnPause : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] m_DisableOnPause = new GameObject[1];
		[SerializeField]
		private GameObject[] m_EnableOnPause = new GameObject[1];

		private void Start()
		{
			PauseSystem.s_OnPause += OnPause;
			PauseSystem.s_OnUnpause += OnUnpause;
		}

		private void OnDestroy()
		{
			PauseSystem.s_OnPause -= OnPause;
			PauseSystem.s_OnUnpause -= OnUnpause;
		}

		private void OnPause() => Toggle(true);
		private void OnUnpause() => Toggle(false);

		private void Toggle(bool pPause)
		{
			foreach (GameObject o in m_DisableOnPause)
			{
				o.SetActive(!pPause);
			}

			foreach (GameObject o in m_EnableOnPause)
			{
				o.SetActive(pPause);
			}
		}
	}
}