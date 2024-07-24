using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingWidget : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_Header = null;
	[SerializeField]
	private TMP_Text m_Body = null;
	[SerializeField]
	private Image m_Icon = null;
	[SerializeField]
	private Button m_Button = null;

	private Action m_OnClicked = null;

	private void Start()
	{
		m_Button.onClick.AddListener(OnButtonClicked);
	}

	public void Initalize(PlayerBuildingInventory.Item pItem, Action pOnClicked)
	{
		m_Header.text = pItem.Data.Header;
		m_Body.text = pItem.Data.Body;
		m_Icon.sprite = pItem.Data.Icon;

		m_OnClicked = pOnClicked;
	}

	private void OnButtonClicked() => m_OnClicked?.Invoke();
}
