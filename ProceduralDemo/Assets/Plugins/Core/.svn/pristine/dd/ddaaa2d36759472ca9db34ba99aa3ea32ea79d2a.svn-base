
using System.Collections.Generic;
using UnityEngine;

public abstract class ParamsTrack<T> : ActTrack where T : ActParams
{
	protected T m_Params = null;

	protected override bool TryStart(ActParams actParams)
	{
		m_Params = actParams as T;
		if (m_Params == null)
		{
			return false;
		}
		return true;
	}
}
