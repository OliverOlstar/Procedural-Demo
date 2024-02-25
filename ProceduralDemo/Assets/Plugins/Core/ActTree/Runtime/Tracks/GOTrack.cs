using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GOTrack : ParamsTrack<GOParams>
{
	protected GameObject m_GameObject = null;
	protected Transform m_Transform = null;

	protected override bool OnInitialize(ActParams actParams)
	{
		if (!base.OnInitialize(actParams))
		{
			return false;
		}
		if (actParams is GOParams goParams)
		{
			m_GameObject = goParams.GetGameObject();
			m_Transform = m_GameObject.transform;
			return true;
		}
		return false;
	}
}
