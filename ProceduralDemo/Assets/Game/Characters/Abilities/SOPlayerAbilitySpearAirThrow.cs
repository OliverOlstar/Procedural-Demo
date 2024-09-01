using ODev;
using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Spear Air Throw Ability", menuName = "Character/Ability/Player Spear Air Throw")]
public class SOPlayerAbilitySpearAirThrow : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private SOPoseMontage m_Montage = null;
	[SerializeField]
	private float m_TimeScale = 0.15f;
	[SerializeField]
	private float m_TimeScaleDampening = 1.0f;

	public SOPoseMontage Montage => m_Montage;
	public float TimeScale => m_TimeScale;
	public float TimeScaleDampening => m_TimeScaleDampening;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearAirThrow(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearAirThrow : CharacterAbility<SOPlayerAbilitySpearAirThrow>
{
	public PlayerAbilitySpearAirThrow(PlayerRoot pPlayer, SOPlayerAbilitySpearAirThrow pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.AbilitySecondary;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
	private int m_TimeScaleHandle = TimeScaleManager.INVALID_HANDLE;
	private float m_TimeScale = 1.0f;

	protected override bool CanActivate()
	{
		return Root.Spear.State == PlayerSpear.State.Stored && !Root.OnGround.IsOnGround;
	}

	protected override void ActivateInternal()
	{
		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);
		m_TimeScale = 1.0f;
		m_TimeScaleHandle = TimeScaleManager.StartTimeEvent(m_TimeScale);
	}

	public override void ActiveTick(float pDeltaTime)
	{
		m_TimeScale = Mathf.Lerp(m_TimeScale, Data.TimeScale, pDeltaTime * Data.TimeScaleDampening);
		TimeScaleManager.UpdateTimeEvent(m_TimeScaleHandle, m_TimeScale);
	}

	protected override void DeactivateInternal()
	{
		Root.Animator.CancelMontage(m_MontageHandle);
		TimeScaleManager.EndTimeEvent(m_TimeScaleHandle);
		m_TimeScaleHandle = TimeScaleManager.INVALID_HANDLE;
		
		Vector3 direction = Root.Camera.transform.forward;
		Root.Spear.Throw(Root.Movement.transform.position + (Vector3.up * 2.0f), direction);
	}
}
