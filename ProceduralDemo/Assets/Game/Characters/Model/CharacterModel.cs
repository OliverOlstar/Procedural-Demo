using System;
using System.Collections;
using System.Collections.Generic;
using ODev;
using ODev.Util;
using UnityEngine;

public class CharacterModel : UpdateableMonoBehaviour
{
	[SerializeField]
	private PlayerRoot m_Root;
	[SerializeField]
	private Transform m_Transform = null;

	[Space, SerializeField]
	private CharacterModelRotation m_Rotation = new();
	[SerializeField]
	private CharacterModelLeaning m_Leaning = new();

	private IEnumerable<CharacterModelControllerBase> GetControllers()
	{
		yield return m_Rotation;
		yield return m_Leaning;
	}

	private void Start()
	{
		foreach (CharacterModelControllerBase controller in GetControllers())
		{
			controller.Initalize(m_Root);
		}
	}

	protected override void OnDisable()
	{
		foreach (CharacterModelControllerBase controller in GetControllers())
		{
			controller.Reset();
		}
		base.OnDisable();
	}

	protected override void Tick(float pDeltaTime)
	{
		foreach (CharacterModelControllerBase controller in GetControllers())
		{
			controller.Tick(pDeltaTime);
		}
		m_Transform.rotation = m_Leaning.CalculatedRotation * m_Rotation.CalculatedRotation;
	}

	private void OnDrawGizmos()
	{
		foreach (CharacterModelControllerBase controller in GetControllers())
		{
			controller.DrawGizmos();
		}
	}
}
