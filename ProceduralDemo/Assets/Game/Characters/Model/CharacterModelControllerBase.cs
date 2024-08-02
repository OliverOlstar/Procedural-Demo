using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CharacterModelControllerBase
{
	private PlayerRoot m_Root;
	protected PlayerRoot Root => m_Root;

	public void Initalize(PlayerRoot pRoot)
	{
		m_Root = pRoot;
		Reset();
	}

	public void DrawGizmos()
	{
		if (m_Root != null)
		{
			OnDrawGizmos();
		}
	}

	public abstract void Tick(float pDeltaTime);
	public abstract void Reset();
	protected virtual void OnDrawGizmos() { }
}