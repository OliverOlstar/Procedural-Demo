using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class UnityEventOnSceneLoaded : MonoBehaviour
{
	[System.Serializable]
	public struct SceneEvent
	{
		[Min(0)]
		public int BuildIndex;
		public UnityEvent OnEvent;
	}

	[SerializeField]
	private SceneEvent[] m_Events = new SceneEvent[0];

	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		foreach (SceneEvent e in m_Events)
		{
			if (scene.buildIndex == e.BuildIndex)
			{
				e.OnEvent.Invoke();
			}
		}
	}
}
