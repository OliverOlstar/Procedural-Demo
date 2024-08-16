using System;
using System.Runtime.CompilerServices;
using ODev.Util;
using UnityEngine;

public abstract class SOCharacterAbility : ScriptableObject
{
	// TODO Cooldown
	// TODO Buffer
	public bool LogSelf = false;
	public abstract ICharacterAbility CreateInstance(PlayerRoot pRoot);
}

public interface ICharacterAbility
{
	public bool IsActive { get; }

	public void Destory();
	/// <summary> Calls Activate() if CanActivate() is true </summary>
	public bool TryActivate();
	public void Deactivate();
	public void ActiveTick(float pDeltaTime);
}

public abstract class CharacterAbility<TData> : ICharacterAbility where TData : SOCharacterAbility
{
	private readonly PlayerRoot m_Root = null;
	private readonly TData m_Data = null;
	private bool m_IsActive = false;

	public PlayerRoot Root => m_Root;
	public TData Data => m_Data;
	public bool IsActive => m_IsActive;

	public CharacterAbility(PlayerRoot pRoot, TData pData)
	{
		m_Root = pRoot;
		m_Data = pData;
		LogMethod();
		Initalize();
	}

	void ICharacterAbility.Destory()
	{
		LogMethod();
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
	void ICharacterAbility.Deactivate() => Deactivate();

	public virtual void ActiveTick(float pDeltaTime) { }
	protected virtual bool CanActivate() { return false; }
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