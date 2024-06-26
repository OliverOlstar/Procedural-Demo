using UnityEngine;

public class TestCollectable : MonoBehaviour
{
	[SerializeField]
	private int PlayerLayer = 0;

	private void Start()
	{
		TestCollectableMenu.AddToTotal();
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer != PlayerLayer)
		{
			return;
		}
		TestCollectableMenu.OnCollected();
		Destroy(gameObject);
	}
}
