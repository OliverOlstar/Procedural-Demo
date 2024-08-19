using System;
using System.Runtime.CompilerServices;
using ODev.Util;
using ODev.Input;
using UnityEngine;
using System;
using UnityEngine.Events;

public abstract class SOCharacterAbility : ScriptableObject
{
	// TODO Cooldown
	// TODO Buffer
	public bool LogSelf = false;
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
	public void Destory();
}

public abstract class CharacterAbility<TData> : ICharacterAbility where TData : SOCharacterAbility
{
	private readonly PlayerRoot m_Root = null;
	private readonly TData m_Data = null;
	private readonly UnityAction<bool> m_OnInputRecieved;
	private bool m_IsActive = false;

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

	void ICharacterAbility.Destory()
	{
		LogMethod();
		InputActivate?.OnChanged.RemoveListener(m_OnInputRecieved);
		DestroyInternal();
	}
	bool ICharacterAbility.TryActivate()
	{
		if (!CanActivate())
		{
			return false;
		}
		Activate();
		return true;
	}
	bool ICharacterAbility.TryActivateUpdate()
	{
		if (!CanActivateUpdate())
		{
			return false;
		}
		Activate();
		return true;
	}
	void ICharacterAbility.Deactivate() => Deactivate();

	public virtual void ActiveTick(float pDeltaTime) { }
	protected virtual bool CanActivate() { return false; }
	protected virtual bool CanActivateUpdate() { return false; }
	protected abstract void Initalize();
	protected abstract void DestroyInternal();
	protected abstract void ActivateInternal();
	protected abstract void DeactivateInternal();

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
		m_Root.Abilities.OnAbilityActivated?.Invoke(typeof(TData));
	}

	protected void Deactivate()
	{
		if (!m_IsActive)
		{
			Root.LogError("Ability is not active");
			return;
		}
		m_IsActive = false;
		LogMethod();
		DeactivateInternal();
		m_Root.Abilities.OnAbilityDeactivated?.Invoke(typeof(TData));
	}

	#region Helpers
	private void LogMethod([CallerMemberName] string pMethodName = "")
	{
		if (Data.LogSelf)
		{
			ODev.Util.Debug.Log("", GetType(), pMethodName);
		}
	}
	#endregion Helpers
}