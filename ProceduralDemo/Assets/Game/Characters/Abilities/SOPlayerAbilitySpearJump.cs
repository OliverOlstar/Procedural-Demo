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

	[Header("Position")]
	[SerializeField]
	private float m_UpOffset = 0.5f;
	[SerializeField]
	private float m_BackOffset = 0.5f;

	public SOPoseMontage Montage => m_Montage;
	public float Force => m_Force;
	public float Dampening => m_Dampening;
	public float ChargeSeconds => m_ChargeSeconds;
	public float UpOffset => m_UpOffset;
	public float BackOffset => m_BackOffset;

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
		return Root.Spear.PlayerIsInTrigger && Root.Spear.State == PlayerSpear.State.Landed && Root.Movement.VelocityY < 1.0f && Root.OnGround.IsInAir;
	}

	// private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
	private float m_HeldTimeElapsed = -1.0f;

	protected override void ActivateInternal()
	{
		Root.Movement.GravityEnabled = false;
		Root.Movement.MovementEnabled = false;
		Root.Movement.SetVelocity(Vector3.zero);
		m_HeldTimeElapsed = -1.0f;

		Root.Input.Jump.RegisterOnPerformed(OnJumpInputPreformed);
		Root.Input.Jump.RegisterOnCanceled(OnJumpInputCanceled);
	}

	private void OnJumpInputPreformed()
	{
		m_HeldTimeElapsed = 0.0f;
	}

	private void OnJumpInputCanceled()
	{
		if (m_HeldTimeElapsed >= 0.0f)
		{
			Deactivate();
		}
	}

	public override void ActiveTick(float pDeltaTime)
	{
		if (!Root.Movement.Velocity.IsNearZero())
		{
			m_HeldTimeElapsed = -1.0f;
			Deactivate();
			return;
		}

		Vector3 targetPositon = Root.Spear.Position + new Vector3(0.0f, Data.UpOffset, 0.0f) + (-Root.Spear.Forward * Data.BackOffset);
		Vector3 offset = Vector3.Lerp(Root.Movement.transform.position, targetPositon, pDeltaTime * Data.Dampening) - Root.Movement.transform.position;
		Root.Movement.AddDisplacement(offset, Quaternion.identity);

		if (m_HeldTimeElapsed >= 0.0f && Root.Input.Jump.Input)
		{
			m_HeldTimeElapsed += pDeltaTime;
		}
	}

	protected override void DeactivateInternal()
	{
		Root.Movement.GravityEnabled = true;
		Root.Movement.MovementEnabled = true;

		// Root.Animator.CancelMontage(m_MontageHandle);
		Root.Input.Jump.DeregisterOnPerformed(OnJumpInputPreformed);
		Root.Input.Jump.DeregisterOnCanceled(OnJumpInputCanceled);
		Root.Spear.ClearInTrigger();

		if (m_HeldTimeElapsed > 0.0f)
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
