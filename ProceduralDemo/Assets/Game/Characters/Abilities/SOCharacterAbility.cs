using System;
using System.Runtime.CompilerServices;
using ODev.Util;
using ODev.Input;
using UnityEngine;
using UnityEngine.Events;
using System.Diagnostics;

public abstract class SOCharacterAbility : ScriptableObject
{
	private enum CooldownTrigger
	{
		OnActivate,
		OnDeactive,
		Both
	}

	[SerializeField]
	private bool m_LogSelf = false;
	[SerializeField]
	private float m_Cooldown = 0.0f;
	[SerializeField]
	private CooldownTrigger m_CooldownTrigger = CooldownTrigger.OnActivate;

	public bool LogSelf => m_LogSelf;
	public float Cooldown => m_Cooldown;
	public bool IsCooldownTrigger(bool pOnActive) => m_CooldownTrigger switch
	{
		CooldownTrigger.OnActivate => pOnActive,
		CooldownTrigger.OnDeactive => !pOnActive,
		CooldownTrigger.Both => true,
		_ => throw new NotImplementedException(),
	};

	public abstract ICharacterAbility CreateInstance(PlayerRoot pRoot, UnityAction<bool> pOnInputRecived);
}

public interface ICharacterAbility
{
	public bool IsActive { get; }
	public InputModule_Toggle InputActivate { get; }

	/// <summary> Calls Activate() if CanActivate() is true </summary>
	public bool TryActivate();
	public bool TryActivateUpdate();
	public void Deactivate();
	public void ActiveTick(float pDeltaTime);
	public void SystemsTick(float pDeltaTime);
	public void Destory();
}

public abstract class CharacterAbility<TData> : ICharacterAbility where TData : SOCharacterAbility
{
	private readonly PlayerRoot m_Root = null;
	private readonly TData m_Data = null;
	private readonly UnityAction<bool> m_OnInputRecieved;
	private bool m_IsActive = false;
	private float m_CooldownTime = 0.0f;

	public PlayerRoot Root => m_Root;
	public TData Data => m_Data;
	public bool IsActive => m_IsActive;
	public virtual InputModule_Toggle InputActivate => null;

	public CharacterAbility(PlayerRoot pRoot, TData pData, UnityAction<bool> pOnInputRecived)
	{
		m_Root = pRoot;
		m_Data = pData;
		m_OnInputRecieved = pOnInputRecived;
		LogMethod();
		InputActivate?.OnChanged.AddListener(m_OnInputRecieved);

		Initalize();
	}

	bool ICharacterAbility.TryActivate()
	{
		if (!CanSystemsCanActive() || !CanActivate())
		{
			return false;
		}
		Activate();
		return true;
	}
	bool ICharacterAbility.TryActivateUpdate()
	{
		if (!CanSystemsCanActive() || !CanActivateUpdate())
		{
			return false;
		}
		Activate();
		return true;
	}
	void ICharacterAbility.Deactivate() => Deactivate();
	void ICharacterAbility.SystemsTick(float pDeltaTime)
	{
		m_CooldownTime -= pDeltaTime;
	}
	void ICharacterAbility.Destory()
	{
		LogMethod();
		InputActivate?.OnChanged.RemoveListener(m_OnInputRecieved);
		DestroyInternal();
	}

	public virtual void ActiveTick(float pDeltaTime) { }
	protected virtual bool CanActivate() { return false; }
	protected virtual bool CanActivateUpdate() { return false; }
	protected abstract void Initalize();
	protected abstract void DestroyInternal();
	protected abstract void ActivateInternal();
	protected abstract void DeactivateInternal();

	private bool CanSystemsCanActive()
	{
		return m_CooldownTime <= 0.0f;
	}

	protected void Activate()
	{
		if (m_IsActive)
		{
			Root.LogError("Ability is already active");
			return;
		}
		m_IsActive = true;
		LogMethod();

		ActivateInternal();
		TriggerCooldown(true);
		m_Root.Abilities.OnAbilityActivated?.Invoke(typeof(TData));
	}

	protected void Deactivate()
	{
		if (!m_IsActive)
		{
			return;
		}
		m_IsActive = false;
		LogMethod();

		DeactivateInternal();
		TriggerCooldown(false);
		m_Root.Abilities.OnAbilityDeactivated?.Invoke(typeof(TData));
	}

	protected void TriggerCooldown(bool pOnActive)
	{
		if (Data.IsCooldownTrigger(pOnActive))
		{
			LogMethod();
			m_CooldownTime = Data.Cooldown;
		}
	}

	#region Helpers
	[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
	private void LogMethod([CallerMemberName] string pMethodName = "")
	{
		if (Data.LogSelf)
		{
			Data.Log("", pMethodName);
		}
	}
	#endregion Helpers
}