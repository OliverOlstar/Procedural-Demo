using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Spear Throw Ability", menuName = "Character/Ability/Player Spear Throw")]
public class SOPlayerAbilitySpearThrow : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private SOPoseMontage m_Montage = null;

	[Space, SerializeField]
	private AnimationCurve m_VelocityCurve = new();
	[SerializeField]
	private float m_Velocity = 0.0f;

	public SOPoseMontage Montage => m_Montage;
	public AnimationCurve VelocityCurve => m_VelocityCurve;
	public float Velocity => m_Velocity;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearThrow(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearThrow : CharacterAbility<SOPlayerAbilitySpearThrow>
{
	public PlayerAbilitySpearThrow(PlayerRoot pPlayer, SOPlayerAbilitySpearThrow pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.AbilityPrimary;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private Vector3 m_Direction;
	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
	private float m_TimeElapsed;

	protected override bool CanActivate()
	{
		return Root.OnGround.IsOnGround;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.MovementEnabled = false;
		m_Direction = Root.Animator.transform.forward.Horizontalize();
		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);
		m_TimeElapsed = 0.0f;
	}

	public override void ActiveTick(float pDeltaTime)
	{
		m_TimeElapsed += pDeltaTime;
		float seconds = Data.Montage.TotalSeconds - Data.Montage.FadeOutSeconds;
		if (m_TimeElapsed >= seconds)
		{
			m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
			Deactivate();
			return;
		}
		float progress01 = m_TimeElapsed / seconds;
		float velocity = Data.VelocityCurve.Evaluate(progress01) * Data.Velocity;
		Root.Movement.AddVelocityXZ(m_Direction * velocity);
	}

	protected override void DeactivateInternal()
	{
		Root.Movement.MovementEnabled = true;
		Root.Animator.CancelMontage(m_MontageHandle);
	}
}
