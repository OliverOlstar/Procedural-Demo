using System.Diagnostics;
using System.Runtime.CompilerServices;
using BandoWare.GameplayTags;
using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

public abstract class CharacterAbility<TData> : ICharacterAbility where TData : SOCharacterAbility
{
	private readonly PlayerRoot m_Root = null;
	private readonly TData m_Data = null;
	private readonly UnityAction m_OnInputPerformed;
	private readonly UnityAction m_OnInputCanceled;
	private bool m_IsActive = false;
	private float m_CooldownTime = 0.0f;

	public PlayerRoot Root => m_Root;
	protected TData Data => m_Data;
	public bool IsActive => m_IsActive;
	public virtual IInputTrigger InputActivate => null;

	public CharacterAbility(PlayerRoot pRoot, TData pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled)
	{
		m_Root = pRoot;
		m_Data = pData;
		LogMethod();
		m_OnInputPerformed = pOnInputPerformed;
		m_OnInputCanceled = pOnInputCanceled;

		if (InputActivate != null)
		{
			InputActivate.RegisterOnPerformed(m_OnInputPerformed);
			if (InputActivate is IInputBool InputActivateBool)
			{
				InputActivateBool.RegisterOnCanceled(m_OnInputCanceled);
			}
		}
		Initalize();
	}

	bool ICharacterAbility.TryActivate(GameplayTagContainer pActiveTags, GameplayTagContainer pBlockedTags)
	{
		if (!CanSystemsCanActive(pActiveTags, pBlockedTags) || !CanActivate())
		{
			return false;
		}
		Activate();
		return true;
	}
	bool ICharacterAbility.TryActivateUpdate(GameplayTagContainer pActiveTags, GameplayTagContainer pBlockedTags)
	{
		if (!CanSystemsCanActive(pActiveTags, pBlockedTags) || !CanActivateUpdate())
		{
			return false;
		}
		Activate();
		return true;
	}
	void ICharacterAbility.TryCancel(GameplayTagContainer pActiveTags, GameplayTagContainer pCancelTags)
	{
		if (Data.ShouldCancel(pActiveTags, pCancelTags))
		{
			LogMethod("Canceled");
			Deactivate();
		}
	}
	void ICharacterAbility.Deactivate() => Deactivate();
	void ICharacterAbility.SystemsTick(float pDeltaTime)
	{
		m_CooldownTime -= pDeltaTime;
	}
	void ICharacterAbility.Destory()
	{
		LogMethod();
		if (InputActivate != null)
		{
			InputActivate.DeregisterOnPerformed(m_OnInputPerformed);
			if (InputActivate is IInputBool InputActivateBool)
			{
				InputActivateBool.DeregisterOnCanceled(m_OnInputCanceled);
			}
		}
		DestroyInternal();
	}

	public virtual void ActiveTick(float pDeltaTime) { }
	protected virtual bool CanActivate() { return true; }
	protected virtual bool CanActivateUpdate() { return false; }
	protected abstract void Initalize();
	protected abstract void DestroyInternal();
	protected abstract void ActivateInternal();
	protected abstract void DeactivateInternal();

	private bool CanSystemsCanActive(GameplayTagContainer pActiveTags, GameplayTagContainer pBlockedTags)
	{
		if (m_CooldownTime > 0.0f)
		{
			return false;
		}
		if (Data.ShouldBlock(pActiveTags, pBlockedTags))
		{
			// LogMethod("Blocked");
			return false;
		}
		return true;
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

		m_Root.Abilities.RecievedAbilityActivated(this);
		TriggerCooldown(true);
		ActivateInternal();
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
		m_Root.Abilities.RecievedAbilityDeactivated(this);
	}

	protected void TriggerCooldown(bool pOnActive)
	{
		if (Data.IsCooldownTrigger(pOnActive))
		{
			LogMethod();
			m_CooldownTime = Data.Cooldown;
		}
	}

	public void AddTags(ref GameplayTagContainer rActiveTags, ref GameplayTagContainer rBlockedTags)
	{
		Data.AddTags(ref rActiveTags, ref rBlockedTags);
	}

	public void GetTags(out GameplayTagContainer oTags, out GameplayTagContainer oCancelTags)
	{
		Data.GetTags(out oTags, out oCancelTags);
	}

	#region Helpers
	[Conditional("ENABLE_DEBUG_LOGGING"), HideInCallstack]
	private void LogMethod(string pMessage = "", [CallerMemberName] string pMethodName = "")
	{
		if (Data.LogSelf)
		{
			Data.Log("", pMethodName);
		}
	}
	#endregion Helpers
}
