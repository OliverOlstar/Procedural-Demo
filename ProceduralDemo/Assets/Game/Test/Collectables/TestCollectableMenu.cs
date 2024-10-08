using UnityEngine;

public class TestCollectableMenu : MonoBehaviour
{
	private static TestCollectableMenu _Instance;

	private static int collectedCount = 0;
	private static int collectableTotal = 0;

	[SerializeField]
	private TMPro.TMP_Text text = null;
	[SerializeField]
	private float scale = 50.0f;

	private void Start()
	{
		_Instance = this;
		OnValuesChanged();
	}

	private void OnDestroy()
	{
		collectedCount = 0;
		collectableTotal = 0;
	}

	private static void OnValuesChanged()
	{
		if (_Instance == null)
		{
			return;
		}

		if (collectedCount == collectableTotal)
		{
			_Instance.text.text = "Success!";
			_Instance.text.fontSize = _Instance.scale;
			return;
		}
		_Instance.text.text = $"{collectedCount} / {collectableTotal}";
	}

	public static void AddToTotal()
	{
		collectableTotal++;
		OnValuesChanged();
	}

	public static void OnCollected()
	{
		collectedCount++;
		OnValuesChanged();
	}
}
