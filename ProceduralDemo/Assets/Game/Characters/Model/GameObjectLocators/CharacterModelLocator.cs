using ODev.Picker;
using ODev.Util;
using ODev.VariableSOs;
using UnityEngine;

public class CharacterModelLocator : MonoBehaviour
{
	[SerializeField] private CharacterModelLocatorsManager m_Manager;
	[SerializeField, AssetNonNull] private EmptyVariableSO m_LocatorIdVariable;

	private bool m_Claimed = false;

	public EmptyVariableSO LocatorIdVariable => m_LocatorIdVariable;
	public bool IsClaimed => m_Claimed;

	private void OnValidate()
	{
		m_Manager = GetComponentInParent<CharacterModelLocatorsManager>();
		if (m_Manager == null)
		{
			this.LogError($"No {nameof(CharacterModelLocatorsManager)} was found, please add one or delete this");
		}
	}

	private void Start()
	{
		if (m_Manager == null)
		{
			OnValidate();
		}
		m_Manager.AddLocator(this);
	}

	public void SetClaimed(bool pClaimed)
	{
		m_Claimed = pClaimed;
	}

	public void Attach(Transform pTransform, bool pSetScale)
	{
		pTransform.SetParent(transform);
		pTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		if (pSetScale)
		{
			pTransform.localScale = Vector3.one;
		}
	}
}
