using System.Collections.Generic;
using ODev.Util;
using ODev.VariableSOs;
using UnityEngine;

public class CharacterModelLocatorsManager : MonoBehaviour
{
	private readonly Dictionary<EmptyVariableSO, List<CharacterModelLocator>> m_LocatorsByIdVariable = new();

	public void AddLocator(CharacterModelLocator pLocator)
	{
		if (!m_LocatorsByIdVariable.TryGetValue(pLocator.LocatorIdVariable, out var locators))
		{
			locators = new();
			m_LocatorsByIdVariable.Add(pLocator.LocatorIdVariable, locators);
		}
		locators.Add(pLocator);
	}

	public bool TryGetFreeLocator(EmptyVariableSO pLocatorIdVariable, out CharacterModelLocator oLocator)
	{
		if (!m_LocatorsByIdVariable.TryGetValue(pLocatorIdVariable, out var locators))
		{
			this.LogWarning($"Locator of id {pLocatorIdVariable.name} was not found.");
			oLocator = null;
			return false;
		}

		foreach (var locator in locators)
		{
			if (!locator.IsClaimed)
			{
				oLocator = locator;
				return true;
			}
		}

		this.LogWarning($"Locator(s) of id {pLocatorIdVariable.name} were all claimed already. Consider adding more.");
		oLocator = null;
		return false;
	}
}
