using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Spear Pull Ability", menuName = "Character/Ability/Player Spear Pull")]
public class SOPlayerAbilitySpearPull : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private SOPoseMontage m_Montage = null;

	public SOPoseMontage Montage => m_Montage;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearPull(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearPull : CharacterAbility<SOPlayerAbilitySpearPull>
{
	public PlayerAbilitySpearPull(PlayerRoot pPlayer, SOPlayerAbilitySpearPull pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.AbilitySecondary;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;

	protected override bool CanActivate()
	{
		if (!Root.OnGround.IsOnGround)
		{
			return false;
		}
		return Root.Spear.ActiveState == PlayerSpear.State.Landed || Root.Spear.ActiveState == PlayerSpear.State.Thrown;
	}

	protected override void ActivateInternal()
	{
		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);
		Root.Spear.Pull(Root.Animator.transform);
		Deactivate();
	}

	protected override void DeactivateInternal()
	{
		// Root.Animator.CancelMontage(m_MontageHandle);
	}
}
