
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public enum UnimaControllerMode
{
	Solo,
	Alpha,
	Beta
}

public interface IUnimaContext
{

}

public interface IUnimaControllerSource
{
	void AddControllers(List<UnimaController> controllers);
}

[System.Serializable]
public class UnimaAdvanced
{
	[SerializeField]
	private int m_StartFrameDelay = 0;
	public int StartFrameDelay => m_StartFrameDelay;
	[SerializeField]
	private bool m_UnscaledDeltaTime = true;
	public bool UseUnScaledTime => m_UnscaledDeltaTime;
	[SerializeField, FormerlySerializedAs("m_IncrementChildStartTime")]
	private float m_IncrementBetaStartTime = 0.0f;
	public float IncrementBetaStartTime => m_IncrementBetaStartTime;
}

[System.Serializable]
public class UnimaController : IUpdatable
{
	[SerializeField, HideInInspector, FormerlySerializedAs("m_FamilyType")]
	private UnimaControllerMode m_Mode = UnimaControllerMode.Solo;
	public UnimaControllerMode Mode => m_Mode;
	[SerializeField, HideInInspector, FormerlySerializedAs("m_FamilyID")]
	private string m_AlphaID = string.Empty;
	public string AlphaID => m_AlphaID;

	[SerializeField]
	private UnimaSet m_Animations = new();
	[SerializeField]
	private List<UnimaEvent> m_AnimationEvents = new();
	[SerializeField]
	private UnimaAdvanced m_AdvancedSettings = new();

	private List<UnimaAction> m_EventsRegisteredPreinitialize = new();
	private List<IUnimaPlayer> m_Players = null;

	private bool m_Playing = false;
	public bool IsPlaying() { return m_Playing; }
	private int m_FrameCount = 0;
	private IUnimaContext m_Context = null;
	private float m_OffsetStartTime = 0.0f;

	private GameObject m_GameObject = null;

	private List<IUnimaControllerSource> m_BetaSources = new();
	[System.NonSerialized] // prevents a cyclical reference problem in Unity 
	private List<UnimaController> m_BetaControllers = new();

	private bool m_EditorDirty = false;
	public void EditorSetDirty() { m_EditorDirty = true; }

	double IBaseUpdatable.DeltaTime => 0.0d;

	public bool IsValid() { return m_GameObject != null; }

	public UnimaController(UnimaControllerMode mode = UnimaControllerMode.Solo, string alphaID = "")
	{
		m_Mode = mode;
		m_AlphaID = alphaID;
	}

	public UnimaController()
	{
		m_EventsRegisteredPreinitialize = new List<UnimaAction>();
		m_BetaSources = new List<IUnimaControllerSource>();
		m_BetaControllers = new List<UnimaController>();
	}

	public void Initialize(GameObject gameObject)
	{
		if (IsValid())
		{
			return;
		}
		InitializeInternal(gameObject);
	}

	private void InitializeInternal(GameObject gameObject)
	{
		if (gameObject == null)
		{
			throw new System.NullReferenceException("UnimaController.Initialize() GameObject is null");
		}
		m_GameObject = gameObject;
		m_Players = m_Animations.InstantiatePlayers(gameObject);
		foreach (UnimaEvent eve in m_AnimationEvents)
		{
			m_Players.Add(eve);
		}
		foreach (UnimaAction action in m_EventsRegisteredPreinitialize)
		{
			m_Players.Add(action);
		}
		m_EventsRegisteredPreinitialize.Clear();
	}

	public bool CanPlay() { return m_Mode != UnimaControllerMode.Beta; }

	public void Play(IUnimaContext context = null, float offsetStartTime = 0.0f)
	{
		if (!CanPlay())
		{
			Debug.LogWarning("UnimaController.StartAnimation() Only an alpha can start animation on a beta");
			return;
		}
		StartInternal(context, offsetStartTime);
	}

	// This function is only for UnimaBehaviour to register a beta with an alpha controller that's already playing
	internal void _InternalAddBeta(UnimaController beta)
	{
		if (beta == null || !IsPlaying())
		{
			return;
		}
		m_BetaControllers.Add(beta);
		beta.StartInternal(m_Context, m_OffsetStartTime);
		m_OffsetStartTime += m_AdvancedSettings.IncrementBetaStartTime;
	}

	private void StartInternal(IUnimaContext context, float offsetStartTime = 0.0f)
	{
		if (!IsValid())
		{
			Debug.LogWarning("UnimaController.StartInternal() Cannot start uninitialized controller");
			return;
		}
		Stop();

		if (m_EditorDirty) // Something has changed, re initialize our players
		{
			Debug.LogWarning("Dirty!");
			m_EditorDirty = false;
			InitializeInternal(m_GameObject);
		}

		m_Playing = true;
		Core.Chrono.Register(this);
		m_FrameCount = 0;
		m_Context = context;
		m_OffsetStartTime = offsetStartTime;
		for (int i = m_Players.Count - 1; i >= 0; --i)
		{
			IUnimaPlayer player = m_Players[i];
			if (player.IsValid())
			{
				player.Play(m_Context, m_OffsetStartTime);
			}
			else
			{
				m_Players.RemoveAt(i); // Remove stale players so they don't take up space in the list ie. UnimaEvent with unregistered actions
			}
		}

		if (m_Mode == UnimaControllerMode.Alpha)
		{
			UnimaUtil.GetBetas(m_GameObject, m_AlphaID, m_BetaSources, m_BetaControllers);
			// TODO: UnimaUtil.GetBetas() adds controllers in the reverse order of the heirarchy, instead of looping backwards bellow
			// I think it would be better to modify the GetBetas() function, but we're close to a release and I am too afraid
			for (int i = m_BetaControllers.Count - 1; i >= 0; i--)
			{
				m_BetaControllers[i].StartInternal(m_Context, m_OffsetStartTime);
				m_OffsetStartTime += m_AdvancedSettings.IncrementBetaStartTime;
			}
		}

		// If we didn't find any content to play we want to stop immediately
		// This way systems can check IsPlaying() immediately after calling Play() annd
		// if we don't play anything any end events will be called inline
		TryStop();
	}

	private void TryStop()
	{
		foreach (IUnimaPlayer player in m_Players)
		{
			if (player.IsPlaying())
			{
				return;
			}
		}
		foreach (UnimaController controller in m_BetaControllers)
		{
			if (controller.IsValid() && controller.IsPlaying())
			{
				return;
			}
		}
		StopInternal(false);
	}

	void IBaseUpdatable.OnRegistered() { }
	void IBaseUpdatable.OnDeregistered() { }

	void IUpdatable.OnUpdate(double doubleDeltaTime)
	{
		// This shouldn't really happen? If m_Playing is false we should be de-registered out OnUpdate with Chrono
		// Doesn't hurt to guard against it just in case of a weird thing
		if (!m_Playing)
		{
			//Core.Chrono.Deregister(this);
			return;
		}
		// This can happen if an object doesn't stop it's controllers before getting destroyed
		if (!IsValid())
		{
			StopInternal(true);
			return;
		}
		if (m_FrameCount < m_AdvancedSettings.StartFrameDelay)
		{
			m_FrameCount++;
			if (m_FrameCount < m_AdvancedSettings.StartFrameDelay)
			{
				return;
			}
		}
		float deltaTime = (float)doubleDeltaTime;
		deltaTime = m_AdvancedSettings.UseUnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
		deltaTime = Mathf.Min(deltaTime, Core.Util.SPF15); // Make sure if the frame rate spikes we don't skip the whole animation
		foreach (IUnimaPlayer player in m_Players)
		{
			player.UpdatePlaying(deltaTime);
		}
		TryStop();
	}

	public void Stop()
	{
		StopInternal(true);
	}

	public void RegisterActionOnStart(System.Action action) { RegisterAction(action, 0.0f); }
	public void RegisterActionOnEnd(System.Action action) { RegisterAction(action, -1.0f); }
	public void RegisterAction(System.Action action, float startTime)
	{
		UnimaAction unimaAction = new(startTime, action);
		if (IsValid()) // Code might try to register an action before we've been initialized
		{
			m_Players.Add(unimaAction);
		}
		else
		{
			m_EventsRegisteredPreinitialize.Add(unimaAction);
		}
	}

	public void UnregisterActionOnStart(System.Action action) { UnregisterAction(action, 0.0f); }
	public void UnregisterActionOnEnd(System.Action action) { UnregisterAction(action, -1.0f); }
	public void UnregisterAction(System.Action action, float startTime)
	{
		if (IsValid())
		{
			foreach (IUnimaPlayer player in m_Players)
			{
				if (player is UnimaAction unimaAction &&
					unimaAction.RegisteredAction == action &&
					Core.Util.Approximately(unimaAction.StartTime, startTime))
				{
					// Shouldn't modify m_Players list while playing, this could lead to exceptions when modifying m_Players while iterating over it
					// Instead the best we can do is unregister the action from the UnimaEvent so it's still in m_Players but does nothing
					unimaAction.UnregisterAction(action);
				}
			}
		}
		else
		{
			for (int i = m_EventsRegisteredPreinitialize.Count - 1; i >= 0; --i)
			{
				UnimaAction unimaAction = m_EventsRegisteredPreinitialize[i];
				if (unimaAction.RegisteredAction == action && 
					Core.Util.Approximately(unimaAction.StartTime, startTime))
				{
					m_EventsRegisteredPreinitialize.RemoveAt(i);
				}
			}
		}
	}

	private void StopInternal(bool interrupted)
	{
		if (!m_Playing)
		{
			return;
		}
		m_Playing = false;
		Core.Chrono.Deregister(this);
		m_FrameCount = 0;
		// If an UnimaController is not stopped before objects start getting destroyed this can happen
		// We need to try and stop our beta controllers but it's not safe to call Stop() on our players
		// We don't know what references could be null at this point
		if (IsValid())
		{
			foreach (IUnimaPlayer player in m_Players)
			{
				player.Stop();
			}
		}
		foreach (UnimaController controller in m_BetaControllers)
		{
			controller.StopInternal(interrupted);
		}
	}

	public void OnValidate()
	{
		m_AnimationEvents.Sort();
	}
}
