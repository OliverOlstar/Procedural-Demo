using ODev.Cue;
using UnityEngine;

public class InteractableItem : InteractableBase
{
	[SerializeField]
	private Renderer m_Renderer = null;
	[SerializeField, ODev.Picker.Asset]
	private SOCue m_InteractCue = null;
	
	private Color m_InitialColor;

	private void Start()
	{
		m_InitialColor = m_Renderer.material.GetColor("_BaseColor");
	}

	public override void Interact(PlayerRoot pPlayer)
	{
		SOCue.Play(m_InteractCue, new CueContext(transform.position));
		Destroy(gameObject);
	}

	protected override void OnSelectEnter()
	{
		m_Renderer.material.SetColor("_BaseColor", Color.green);
	}

	protected override void OnSelectExit()
	{
		m_Renderer.material.SetColor("_BaseColor", m_InitialColor);
	}
}
