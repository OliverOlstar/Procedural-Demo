
using System.Collections.Generic;
using UnityEngine;

public abstract class SOTrackController<TParams> : ScriptableObject where TParams : ActParams
{
	public abstract ActTrack.EndEventType GetEndEventType();

	public abstract float EditorDisplayDuration();

	protected TParams m_Params = null;

	public void StartController(TParams param)
	{
		m_Params = param;
		OnStart();
	}

	public abstract bool IsValid();

	public abstract void OnStart();

	public abstract void OnCancelled();

	public abstract void OnEnd();

	public abstract void OnInterrupted();

	public abstract bool OnUpdate(float deltaTime);
}
