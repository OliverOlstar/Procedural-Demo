using System.Linq;
using BandoWare.GameplayTags;
using TMPro;
using UnityEngine;

public class GameObjectGameplayTagDebugText : MonoBehaviour
{
	[SerializeField] private GameObjectGameplayTagContainer m_GameObjectGameplayTagContainer;
	[SerializeField] private TMP_Text m_Text;

	private void Start()
    {
		m_GameObjectGameplayTagContainer.GameplayTagContainer.OnAnyTagNewOrRemove += OnTagAddOrRemove;
		m_GameObjectGameplayTagContainer.GameplayTagContainer.OnAnyTagCountChange += OnTagCountChanged;
		UpdateText();
	}

	private void OnDestroy()
	{
		m_GameObjectGameplayTagContainer.GameplayTagContainer.OnAnyTagNewOrRemove -= OnTagAddOrRemove;
		m_GameObjectGameplayTagContainer.GameplayTagContainer.OnAnyTagCountChange -= OnTagCountChanged;
	}

	private void OnTagAddOrRemove(GameplayTag gameplayTag, int newCount)
	{
		UpdateText();
	}

	private void OnTagCountChanged(GameplayTag gameplayTag, int newCount)
	{
		UpdateText();
	}

	private void UpdateText()
	{
		var tags = m_GameObjectGameplayTagContainer.GameplayTagContainer.GetExplicitTags();
		m_Text.SetText(ODev.Util.Debug.BuildWithBetweens("\n", tags.Select(t => t.Name).ToArray()));
	}
}
