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

	public SOPoseMontage Montage => m_Montage;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearThrow(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearThrow : CharacterAbility<SOPlayerAbilitySpearThrow>
{
	public PlayerAbilitySpearThrow(PlayerRoot pPlayer, SOPlayerAbilitySpearThrow pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.AbilitySecondary;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;

	protected override bool CanActivate()
	{
		return Root.Spear.ActiveState == PlayerSpear.State.Stored;
	}

	protected override void ActivateInternal()
	{
		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);
		Vector3 direction = Root.Camera.transform.forward;
		if (Root.OnGround.IsOnGround)
		{
			direction = direction.Horizontalize();
		}
		Root.Spear.Throw(Root.Movement.transform.position + (Vector3.up * 2.0f), direction);
		Deactivate();
	}

	protected override void DeactivateInternal()
	{
		// Root.Animator.CancelMontage(m_MontageHandle);
	}
}
