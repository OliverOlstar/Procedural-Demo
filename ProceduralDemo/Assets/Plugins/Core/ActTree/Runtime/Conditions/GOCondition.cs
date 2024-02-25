
using System.Collections.Generic;
using UnityEngine;

public class GOCondition : ActCondition
{
	protected GameObject m_GameObject = null;
	protected Transform m_Transform = null;

	protected override void OnInitialize(ActParams actParams)
	{
		if (actParams is GOParams goParams)
		{
			m_GameObject = goParams.GetGameObject();
			m_Transform = m_GameObject.transform;
		}
	}
}
