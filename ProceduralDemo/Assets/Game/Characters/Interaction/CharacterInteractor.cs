using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ODev.Util;
using System.Linq;
using ODev;

[RequireComponent(typeof(Collider))]
public class CharacterInteractor : MonoBehaviour
{
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new(ODev.Util.Mono.Type.Default, ODev.Util.Mono.Priorities.Interactator);
	[SerializeField]
	private float m_ScoreDistanceScalar = 0.25f;
	[SerializeField]
	private PlayerRoot m_Player = null;

	private readonly Dictionary<int, InteractableBase> m_Interactables = new();
	private InteractableBase[] m_InteratablesArray = new InteractableBase[0];

	private InteractableBase m_SelectedInteractable = null;

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
		m_Player.Input.Interact.OnPerformed.AddListener(OnPerformed);
	}

	private void OnDisable()
	{
		m_Updateable.Deregister();
		m_Player.Input.Interact.OnPerformed.RemoveListener(OnPerformed);

		if (Func.IsApplicationQuitting)
		{
			return;
		}

		foreach (InteractableBase interactable in m_Interactables.Values)
		{
			interactable.HoverExit(this);
		}
		m_Interactables.Clear();
	}

	private void OnPerformed()
	{
		if (m_SelectedInteractable != null && m_SelectedInteractable.CanInteract())
		{
			m_SelectedInteractable.Interact();
		}
	}

	private void Tick(float pDeltaTime)
	{
		if (m_InteratablesArray.Length <= 0)
		{
			return;
		}
		
		int lowestIndex = -1;
		float lowestScore = float.MaxValue;
		for (int i = 0; i < m_InteratablesArray.Length; i++)
		{
			if (!m_InteratablesArray[i].CanHover())
			{
				RemoveInteractable(m_InteratablesArray[i].gameObject.GetInstanceID(), m_InteratablesArray[i]);
				i--;
				continue;
			}
			if (!m_InteratablesArray[i].CanSelect())
			{
				continue;
			}
			float score = CalculateScore(m_InteratablesArray[i]);
			if (score < lowestScore)
			{
				lowestIndex = i;
				lowestScore = score;
			}
		}
		SetSelected(lowestIndex >= 0 ? m_InteratablesArray[lowestIndex] : null);
	}

	private float CalculateScore(InteractableBase pInteractable)
	{
		Vector3 difference = pInteractable.Position - transform.position;
		float distanceSqr = Math.Horizontal2D(difference).sqrMagnitude;
		float angle = Vector3.Angle(difference, MainCamera.Forward);
		return (distanceSqr * m_ScoreDistanceScalar) + angle;
	}

	private void SetSelected(InteractableBase pInteractable)
	{
		if (m_SelectedInteractable == pInteractable)
		{
			return;
		}

		if (m_SelectedInteractable != null)
		{
			m_SelectedInteractable.SelectExit();
		}
		m_SelectedInteractable = pInteractable;
		if (m_SelectedInteractable != null)
		{
			m_SelectedInteractable.SelectEnter();
		}
	}

	private void RemoveInteractable(int pKey, InteractableBase pInteractable)
	{
		if (m_SelectedInteractable == pInteractable)
		{
			SetSelected(null);
		}
		pInteractable.HoverExit(this);
		m_Interactables.Remove(pKey);
		m_InteratablesArray = m_Interactables.Values.ToArray();
	}

	private void OnTriggerEnter(Collider other)
	{
		int key = other.gameObject.GetInstanceID();
		if (m_Interactables.ContainsKey(key)) // Already hovering
		{
			return;
		}
		if (!other.TryGetComponent(out InteractableBase interactable) || !interactable.CanHover())
		{
			return;
		}
		m_Interactables.Add(key, interactable);
		m_InteratablesArray = m_Interactables.Values.ToArray();
		interactable.HoverEnter(this);
	}

	private void OnTriggerExit(Collider other)
	{
		int key = other.gameObject.GetInstanceID();
		if (!m_Interactables.TryGetValue(key, out InteractableBase interactable)) // Already hovering
		{
			return;
		}
		RemoveInteractable(key, interactable);
	}

	public void OnInteractableDestroy(InteractableBase pInteractable)
	{
		RemoveInteractable(pInteractable.gameObject.GetInstanceID(), pInteractable);
	}
}
