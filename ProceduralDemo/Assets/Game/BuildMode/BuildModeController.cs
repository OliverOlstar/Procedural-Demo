using System;
using ODev.Util;
using UnityEngine;

public class BuildModeController : MonoBehaviour
{
	public abstract class StateBase
	{
		private BuildModeRoot m_Root;
		protected BuildModeRoot Root => m_Root;

		internal void Initalize(BuildModeRoot pRoot) => m_Root = pRoot;

		internal abstract void Tick(float pDeltaTime);
		internal abstract void Disable();
		internal virtual void OnPositiveInput() { }
		internal virtual void OnNegativeInput() { }
	}

	private enum State
	{
		Selecting,
		Placing,
		Moving
	}

	[SerializeField]
	private BuildModeControllerSelecting m_SelectingState = new();
	[SerializeField]
	private BuildModeControllerPlacing m_PlacingState = new();
	[SerializeField]
	private BuildModeControllerMoving m_MovingState = new();

	private StateBase m_CurrentState = null;

	[SerializeField]
	private BuildModeRoot m_Root = null;
	[SerializeField]
	private Mono.Updateable m_Updateable = new();

	private void OnEnable()
	{
		m_Updateable.Register(Tick);
	}
	private void OnDisable()
	{
		m_Updateable.Deregister();
	}

	private void Start()
	{
		m_Root.Input.Exit.RegisterOnPerformed(ExitMode);
		m_Root.Input.Positive.RegisterOnPerformed(OnPositiveInput);
		m_Root.Input.Negative.RegisterOnPerformed(OnNegativeInput);

		m_SelectingState.Initalize(m_Root);
		m_PlacingState.Initalize(m_Root);
		m_MovingState.Initalize(m_Root);
		SwitchToStateSelecting();
	}
	private void OnDestroy()
	{
		m_Root.Input.Exit.DeregisterOnPerformed(ExitMode);
		m_Root.Input.Positive.DeregisterOnPerformed(OnPositiveInput);
		m_Root.Input.Negative.DeregisterOnPerformed(OnNegativeInput);
	}

	public void OnExitingMode()
	{
		if (m_CurrentState != null && m_CurrentState != m_SelectingState)
		{
			SwitchToStateSelecting();
		}
	}

	private void Tick(float pDeltaTime) => m_CurrentState?.Tick(pDeltaTime);

	private void ExitMode() => m_Root.Mode.SwitchToPlayer();
	private void OnPositiveInput() => m_CurrentState?.OnPositiveInput();
	private void OnNegativeInput() => m_CurrentState?.OnNegativeInput();

	private void SetState(StateBase pState, Action pEnableAction)
	{

		m_CurrentState?.Disable();
		m_CurrentState = pState;
		pEnableAction?.Invoke();
	}
	public void SwitchToStateSelecting() => SetState(m_SelectingState, m_SelectingState.Enable);
	public void SwitchToStatePlacing(SOBuildingItem pItem) => SetState(m_PlacingState, () => m_PlacingState.Enable(pItem));
	public void SwitchToStateMoving(BuildModeInstance pBuilding) => SetState(m_MovingState, () => m_MovingState.Enable(pBuilding));
}
