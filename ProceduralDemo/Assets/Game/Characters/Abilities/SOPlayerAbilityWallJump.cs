using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Wall Jump Ability", menuName = "Character/Ability/Player Wall Jump")]
public class SOPlayerAbilityWallJump : SOCharacterAbility
{
	[Space, SerializeField]
	private float m_PushOffForce = 20.0f;
	[SerializeField]
	private float m_Force = 9.0f;
	[Space, SerializeField, Range(0.0f, 1.0f)]
	private float m_CancelVelocityPercent = 0.5f;
	[SerializeField]
	private float m_CancelMinVelocity = 5.0f;

	public float PushOffForce => m_PushOffForce;
	public float Force => m_Force;
	public float CancelVelocityPercent => m_CancelVelocityPercent;
	public float CancelMinVelocity => m_CancelMinVelocity;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityWallJump(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityWallJump : CharacterAbility<SOPlayerAbilityWallJump>
{
	public PlayerAbilityWallJump(PlayerRoot pPlayer, SOPlayerAbilityWallJump pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Jump;

	protected override void Initalize()
	{

	}
	protected override void DestroyInternal()
	{

	}

	protected override bool CanActivate()
	{
		return !Root.OnGround.IsOnGround && Root.OnWall.IsOnWall;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.SetVelocityY(Data.Force);
		Vector3 direction = Vector3.Reflect(Root.Movement.VelocityXZ, Root.OnWall.HitInfo.normal);
		Root.Movement.SetVelocityXZ(direction.Horizontalize() * Data.PushOffForce);
	}
	protected override void DeactivateInternal()
	{
		if (Root.Movement.VelocityY <= Data.CancelMinVelocity)
		{
			return;
		}
		float velocity = Mathf.Max(Root.Movement.VelocityY * Data.CancelVelocityPercent, Data.CancelMinVelocity);
		Root.Movement.SetVelocityY(velocity);
	}
}
