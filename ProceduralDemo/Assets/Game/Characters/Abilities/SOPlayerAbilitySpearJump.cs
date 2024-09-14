using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Spear Jump Ability", menuName = "Character/Ability/Player Spear Jump")]
public class SOPlayerAbilitySpearJump : SOCharacterAbility
{
	[Space, SerializeField, Asset]
	private SOPoseMontage m_Montage = null;
	[SerializeField]
	private float m_Force = 15.0f;
	[SerializeField]
	private float m_Dampening = 5.0f;
	[SerializeField]
	private float m_ChargeSeconds = 0.6f;

	public SOPoseMontage Montage => m_Montage;
	public float Force => m_Force;
	public float Dampening => m_Dampening;
	public float ChargeSeconds => m_ChargeSeconds;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearJump(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearJump : CharacterAbility<SOPlayerAbilitySpearJump>
{
	public PlayerAbilitySpearJump(PlayerRoot pPlayer, SOPlayerAbilitySpearJump pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	//public override IInputTrigger InputActivate => Root.Input.Jump;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	protected override bool CanActivateUpdate()
	{
		return Root.Spear.PlayerIsInTrigger && Root.Spear.State == PlayerSpear.State.Landed;
	}

	// private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
	private float m_HeldTimeElapsed = 0.0f;

	protected override void ActivateInternal()
	{
		Root.Movement.GravityEnabled = false;
		Root.Movement.MovementEnabled = false;
		m_HeldTimeElapsed = 0.0f;

		Root.Input.Jump.RegisterOnCanceled(Deactivate);
	}

	public override void ActiveTick(float pDeltaTime)
	{
		Vector3 offset = Vector3.Lerp(Root.Movement.transform.position, Root.Spear.Position + Vector3.up, pDeltaTime * Data.Dampening) - Root.Movement.transform.position;
		Root.Movement.AddDisplacement(offset, Quaternion.identity);

		if (Root.Input.Jump.Input)
		{
			m_HeldTimeElapsed += pDeltaTime;
		}
	}

	protected override void DeactivateInternal()
	{
		Root.Movement.GravityEnabled = true;
		Root.Movement.MovementEnabled = true;

		// Root.Animator.CancelMontage(m_MontageHandle);
		Root.Input.Jump.DeregisterOnCanceled(Deactivate);
		Root.Spear.ClearInTrigger();

		if (!m_HeldTimeElapsed.IsNearZero())
		{
			if (Data.Montage != null)
			{
				Root.Animator.PlayMontage(Data.Montage);
			}
			Root.Movement.SetVelocityY(Data.Force * (Mathf.Min(m_HeldTimeElapsed, Data.ChargeSeconds) / Data.ChargeSeconds));
		}
		// Root.Spear.Store();
	}
}
