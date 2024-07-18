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
		ODev.Util.Debug.Log("", this);
		m_Rigidbody.isKinematic = true;
		// m_Animation = Anim.Play(m_HoverEase, 0.5f, Anim.Type.Visual,
		// (float pProgress) =>
		// {
		// 	transform.position = initialPosition + Vector3.up * pProgress;
		// },
		// (float _) =>
		// {
		// });
		// transform.localPosition += Vector3.up;
		PlayLoopedAnimation(transform.localPosition, 0, 1);
	}

	private void PlayLoopedAnimation(Vector3 pStartPosition, int pFrom, int pTo)
	{
		m_Animation?.Cancel(false);
		m_Animation = Anim.Play(m_HoverEase, m_HoverSeconds, Anim.Type.Physics,
		(float pProgress) =>
		{
			pProgress = Mathf.Lerp(pFrom, pTo, pProgress);
			transform.localPosition = pStartPosition + new Vector3(0.0f, m_HoverHeight * pProgress, 0.0f);
		},
		(float _) =>
		{
			// transform.localPosition = pStartPosition;
			PlayLoopedAnimation(pStartPosition, pTo, pFrom);
		});
	}

	protected override void OnHoverExit()
	{
		ODev.Util.Debug.Log("", this);
		if (m_Animation != null)
		{
			m_Animation.Cancel(false);
			m_Animation = null;
		}
		// m_Rigidbody.isKinematic = false;
	}
}
