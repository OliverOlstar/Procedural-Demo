using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnimateTextShadow : Unimate<UnimateTextShadow, UnimateTextShadow.Player>
{
	[SerializeField]
	private bool m_EnableComponentOnPlay = false;

	private List<Shadow> m_Shadows = new();

	protected override string OnEditorValidate(GameObject gameObject)
	{
		Shadow[] shadows = gameObject.GetComponentsInChildren<Shadow>();
		if (shadows != null)
		{
			return null;
		}

		return "Shadow components not found.";
	}

	public override UnimaDurationType GetEditorDuration(out float seconds)
	{
		seconds = 0.0f;
		return UnimaDurationType.Fixed;
	}

	public class Player : UnimaPlayer<UnimateTextShadow>
	{

		protected override void OnInitialize()
		{
			Animation.m_Shadows.Clear();
			Text[] texts = GameObject.GetComponentsInChildren<Text>();
			
			for (int i = 0; i < texts.Length; ++i)
			{
				if (texts[i].TryGetComponent<Shadow>(out Shadow shadow))
				{
					Animation.m_Shadows.Add(shadow);
				}
			}
		}

		protected override bool TryPlay(IUnimaContext context)
		{
			foreach (Shadow shadow in Animation.m_Shadows)
			{
				if (shadow != null)
				{
					shadow.enabled = Animation.m_EnableComponentOnPlay;
				}
			}
			return true;
		}
	}
}

