using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;

public class InteractableItem : InteractableBase
{
	[SerializeField]
	private Easing.EaseParams m_HoverEase = new();
	[SerializeField]
	private float m_HoverSeconds = 1.0f;
	[SerializeField]
	private float m_HoverHeight = 0.5f;
	[SerializeField]
	private Easing.EaseParams m_StartHoverEase = new();
	[SerializeField]
	private float m_StartHoverSeconds = 1.0f;
	[SerializeField]
	private float m_StartHoverHeight = 0.5f;
	[SerializeField]
	private Rigidbody m_Rigidbody = new();

	private Anim.IAnimation m_Animation;

	public void Reset()
	{
		m_Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
	}

	public override void Interact()
	{
		Destroy(gameObject);
	}

	protected override void OnHoverEnter()
	{
		m_Rigidbody.isKinematic = true;
		Vector3 startPosition = transform.localPosition;
		m_Animation = Anim.Play(m_StartHoverEase, m_StartHoverSeconds, Anim.Type.Visual,
		(float pProgress) =>
		{
			transform.localPosition = startPosition + new Vector3(0.0f, (m_StartHoverHeight + m_HoverHeight) * pProgress, 0.0f);
		},
		(float _) =>
		{
			transform.localPosition = startPosition + new Vector3(0.0f, m_StartHoverHeight + m_HoverHeight, 0.0f);
			PlayLoopedAnimation(transform.localPosition, 0, -1);
		});
	}

	private void PlayLoopedAnimation(Vector3 pStartPosition, int pFrom, int pTo)
	{
		m_Animation?.Cancel(false);
		m_Animation = Anim.Play(m_HoverEase, m_HoverSeconds, Anim.Type.Visual,
		(float pProgress) =>
		{
			pProgress = Mathf.Lerp(pFrom, pTo, pProgress);
			transform.localPosition = pStartPosition + new Vector3(0.0f, m_HoverHeight * pProgress, 0.0f);
		},
		(float _) =>
		{
			transform.localPosition = pStartPosition + new Vector3(0.0f, m_HoverHeight, 0.0f);
			PlayLoopedAnimation(pStartPosition, pTo, pFrom);
		});
	}

	protected override void OnHoverExit()
	{
		m_Rigidbody.isKinematic = false;
		if (m_Animation != null)
		{
			m_Animation.Cancel(false);
			m_Animation = null;
		}
	}
}
