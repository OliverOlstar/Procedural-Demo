using System.Collections;
using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Spear Grapple Ability", menuName = "Character/Ability/Player Spear Grapple")]
public class SOPlayerAbilitySpearGrapple : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private SOPoseMontage m_Montage = null;
	[SerializeField]
	private float m_Force = 10.0f;
	[SerializeField]
	private float m_Delay = 0.25f;
	[SerializeField]
	private float m_MaxDistance = 10.0f;
	[SerializeField]
	private float m_MinDistance = 2.0f;

	public SOPoseMontage Montage => m_Montage;
	public float Force => m_Force;
	public float Delay => m_Delay;
	public float MaxDistance => m_MaxDistance;
	public float MinDistance => m_MinDistance;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySpearGrapple(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySpearGrapple : CharacterAbility<SOPlayerAbilitySpearGrapple>
{
	public PlayerAbilitySpearGrapple(PlayerRoot pPlayer, SOPlayerAbilitySpearGrapple pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.AbilitySecondary;

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
	private float m_TimeElapsed = 0.0f;

	protected override bool CanActivate()
	{
		if (Root.OnGround.IsOnGround)
		{
			return false;
		}
		if (Root.Spear.State != PlayerSpear.State.Landed && Root.Spear.State != PlayerSpear.State.Thrown)
		{
			return false;
		}
		return true;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(0.0f);
		Root.Movement.GravityEnabled = false;

		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);
		Root.Spear.SetRopeActive(true);

		m_TimeElapsed = 0.0f;
	}

	public override void ActiveTick(float pDeltaTime)
	{
		if (m_TimeElapsed <= Data.Delay)
		{
			m_TimeElapsed += pDeltaTime;
			return;
		}
		if (Root.OnGround.IsOnGround || Root.Spear.State == PlayerSpear.State.Stored || 
			Math.DistanceXZLessThan(Root.Movement.transform.position, Root.Spear.Position, Data.MinDistance) || 
			Math.DistanceXZEqualGreaterThan(Root.Movement.transform.position, Root.Spear.Position, Data.MaxDistance))
		{
			Deactivate();
			return;
		}
		Root.Movement.GravityEnabled = true;
		if (Root.Spear.State == PlayerSpear.State.Thrown)
		{
			Root.Spear.Pull(Root.Movement.transform);
		}
		Vector3 difference = Root.Spear.Position - Root.Movement.transform.position;
		difference.y = 0.0f;
		Vector3 velocity = difference.normalized * Data.Force;
		Root.Movement.SetVelocity(velocity);
	}

	protected override void DeactivateInternal()
	{
		Root.Movement.GravityEnabled = true;
		Root.Animator.CancelMontage(m_MontageHandle);
		Root.Spear.SetRopeActive(false);
	}
}
