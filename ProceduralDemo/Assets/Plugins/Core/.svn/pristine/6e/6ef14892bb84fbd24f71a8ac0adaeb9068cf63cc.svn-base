
using System.Collections.Generic;
using UnityEngine;

public class SOTrack<TController, TParams> : ParamsTrack<TParams>
	where TController : SOTrackController<TParams>
	where TParams : ActParams
{
	[SerializeField]
	private TController m_Controller = null;

	private TController m_Instance = null;

	public override EndEventType GetEndEventType()
	{
		return m_Controller == null ? EndEventType.NoEndEvent : m_Controller.GetEndEventType();
	}

	protected override float EditorDisplayDuration()
	{
		return m_Controller == null ? -1.0f : m_Controller.EditorDisplayDuration();
	}

	protected override bool OnInitialize(ActParams actParams)
	{
		if (m_Controller == null || !m_Controller.IsValid())
		{
			return false;
		}
		m_Instance = Instantiate(m_Controller);
		return true;
	}

	protected override void OnStart()
	{
		m_Instance.StartController(m_Params);
	}

	protected override void OnCancelled()
	{
		m_Instance.OnCancelled();
	}

	protected override void OnEnd()
	{
		m_Instance.OnEnd();
	}

	protected override void OnInterrupted()
	{
		m_Instance.OnInterrupted();
	}

	protected override bool OnUpdate(float time)
	{
		return m_Instance.OnUpdate(DeltaTime);
	}

	private void OnDestroy()
	{
		Destroy(m_Instance);
	}
}
