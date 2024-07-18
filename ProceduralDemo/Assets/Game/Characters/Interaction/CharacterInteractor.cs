using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ODev.Util;
using System;

[RequireComponent(typeof(Collider))]
public class CharacterInteractor : MonoBehaviour
{
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new(ODev.Util.Mono.Type.Default, ODev.Util.Mono.Priorities.Interactator);

	private readonly Dictionary<int, InteractableBase> m_HoveringInteractables = new();

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}

	private void OnDisable()
	{
		m_Updateable.Deregister();

		if (Func.IsApplicationQuitting)
		{
			return;
		}

		foreach (InteractableBase interactable in m_HoveringInteractables.Values)
		{
			interactable.HoverExit();
		}
		m_HoveringInteractables.Clear();
	}

	private void Tick(float pDeltaTime)
	{
		
	}

	private void OnTriggerEnter(Collider other)
	{
		int key = other.GetInstanceID();
		if (m_HoveringInteractables.ContainsKey(key)) // Already hovering
		{
			return;
		}
		if (!other.TryGetComponent(out InteractableBase interactable) || !interactable.CanHover())
		{
			return;
		}
		m_HoveringInteractables.Add(key, interactable);
		interactable.HoverEnter();
	}

	private void OnTriggerExit(Collider other)
	{
		int key = other.GetInstanceID();
		if (!m_HoveringInteractables.TryGetValue(key, out InteractableBase interactable)) // Already hovering
		{
			return;
		}
		interactable.HoverExit();
		m_HoveringInteractables.Remove(key);
	}
}
