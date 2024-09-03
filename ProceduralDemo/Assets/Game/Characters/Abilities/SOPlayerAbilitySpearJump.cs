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

	public SOPoseMontage Montage => m_Montage;
	public float Force => m_Force;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearJump(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearJump : CharacterAbility<SOPlayerAbilitySpearJump>
{
	public PlayerAbilitySpearJump(PlayerRoot pPlayer, SOPlayerAbilitySpearJump pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Jump;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	protected override bool CanActivate()
	{
		return Root.Spear.PlayerIsInTrigger && Root.Spear.State == PlayerSpear.State.Landed;
	}

	// private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;

	protected override void ActivateInternal()
	{
		if (Data.Montage != null)
		{
			Root.Animator.PlayMontage(Data.Montage);
		}
		Root.Movement.SetVelocityY(Data.Force);
		Root.Spear.Store();
		Deactivate();
	}

	protected override void DeactivateInternal()
	{
		// Root.Animator.CancelMontage(m_MontageHandle);
	}
}
