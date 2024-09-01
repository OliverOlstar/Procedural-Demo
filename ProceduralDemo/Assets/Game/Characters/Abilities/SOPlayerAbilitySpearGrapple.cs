using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Spear Grapple Ability", menuName = "Character/Ability/Player Spear Grapple")]
public class SOPlayerAbilitySpearGrapple : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private SOPoseMontage m_Montage = null;
	[SerializeField]
	private float m_Force = 10.0f;

	public SOPoseMontage Montage => m_Montage;
	public float Force => m_Force;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearGrapple(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearGrapple : CharacterAbility<SOPlayerAbilitySpearGrapple>
{
	public PlayerAbilitySpearGrapple(PlayerRoot pPlayer, SOPlayerAbilitySpearGrapple pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.AbilitySecondary;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;

	protected override bool CanActivate()
	{
		if (Root.OnGround.IsOnGround)
		{
			return false;
		}
		return Root.Spear.State == PlayerSpear.State.Landed || Root.Spear.State == PlayerSpear.State.Thrown;
	}

	protected override void ActivateInternal()
	{
		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);
		Root.Movement.SetVelocityXZ((Root.Spear.Position - Root.Movement.transform.position).normalized * Data.Force);
		Deactivate();
	}

	protected override void DeactivateInternal()
	{
		if (Root.Spear.State == PlayerSpear.State.Thrown)
		{
			Root.Spear.Pull(Root.Movement.transform);
		}
		// Root.Animator.CancelMontage(m_MontageHandle);
	}
}
