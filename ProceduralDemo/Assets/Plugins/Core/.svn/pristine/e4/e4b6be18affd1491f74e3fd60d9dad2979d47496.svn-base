

using System.Collections.Generic;
using UnityEngine;

public class GOParams : ActParams
{
	GameObject m_GameObject = null;
	public GameObject GetGameObject() { return m_GameObject; }

	public GOParams(GameObject gameObject)
	{
		m_GameObject = gameObject;
	}

	public override string ToString() { return "GOParams(" + (m_GameObject == null ? Core.Str.EMPTY : m_GameObject.name) + ")"; }
}
