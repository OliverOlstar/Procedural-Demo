using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class BuildModeScreen : ScreenBase
{
	[SerializeField]
	private BuildingWidget m_Widget = null;

	[Header("Open Close")]
	[SerializeField]
	private Button m_OpenCloseButton = null;
	[SerializeField]
	private TMP_Text m_OpenCloseText = null;
	[SerializeField]
	private float m_DistanceToMoveOffScreen = 200.0f;

	private readonly List<BuildingWidget> m_WidgetPool = new();
	private Vector3 m_InitalPosition;
	private bool m_IsSlideOpen = true;
	private BuildModeScreenContext m_Context;

	private void Start()
	{
		m_WidgetPool.Add(m_Widget);
		m_InitalPosition = transform.position;
		SlideClose();

		m_OpenCloseButton.onClick.AddListener(
			() =>
			{
				if (m_IsSlideOpen)
				{
					SlideClose();
				}
				else
				{
					SlideOpen();
				}
			});
	}

	protected override void OnOpen()
	{
		SetupWidgets();
		SlideClose();
	}

	protected override void OnOpenWithData<TContext>(in TContext pContext)
	{
		if (pContext is BuildModeScreenContext context)
		{
			m_Context = context;
		}
	}

	private void SlideClose()
	{
		if (!m_IsSlideOpen)
		{
			return;
		}
		m_IsSlideOpen = false;
		transform.position = m_InitalPosition + new Vector3(-m_DistanceToMoveOffScreen, 0.0f);
		m_OpenCloseText.text = ">>";
	}

	private void SlideOpen()
	{
		if (m_IsSlideOpen)
		{
			return;
		}
		m_IsSlideOpen = true;
		transform.position = m_InitalPosition;
		m_OpenCloseText.text = "<<";
	}

	private void SetupWidgets()
	{
		PlayerBuildingInventory inventory = PlayerBuildingInventory.Instance;
		int index = 0;
		foreach (PlayerBuildingInventory.Item item in inventory.GetItems())
		{
			if (index >= m_WidgetPool.Count)
			{
				BuildingWidget newWidget = Instantiate(m_Widget);
				newWidget.transform.SetParent(m_Widget.transform.parent);
				newWidget.transform.localScale = Vector3.one;
				m_WidgetPool.Add(newWidget);
			}
			BuildingWidget widget = m_WidgetPool[index];
			index++;
			widget.Initalize(item, () => OnBuildingPicked(item.Data));
			widget.gameObject.SetActive(true);
		}

		while (index < m_WidgetPool.Count)
		{
			m_WidgetPool[index].gameObject.SetActive(false);
			index++;
		}
	}

	private void OnBuildingPicked(SOBuildingItem pData)
	{
		this.Log($"Picked building {pData.name}");
		SlideClose();
		m_Context.SelectBuilding?.Invoke(pData);
	}
}
