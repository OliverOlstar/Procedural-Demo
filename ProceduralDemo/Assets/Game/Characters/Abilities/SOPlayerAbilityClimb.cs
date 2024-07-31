using System;
using ODev;
using ODev.Util;
using UnityEngine;

[CreateAssetMenu(fileName = "New Climb Ability", menuName = "Character/Ability/Player Climb")]
public class SOPlayerAbilityClimb : SOCharacterAbility
{
	[SerializeField]
	private float m_ClimbForce = 20.0f;
	[SerializeField]
	private ODev.Util.Mono.Type m_UpdateType = ODev.Util.Mono.Type.Fixed;
	[SerializeField]
	private ODev.Util.Mono.Priorities m_UpdatePriority = ODev.Util.Mono.Priorities.CharacterAbility;

	public float ClimbForce => m_ClimbForce;
	public ODev.Util.Mono.Type UpdateType => m_UpdateType;
	public ODev.Util.Mono.Priorities UpdatePriority => m_UpdatePriority;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityClimb(pPlayer, this);
}

public class PlayerAbilityClimb : CharacterAbility<SOPlayerAbilityClimb>
{
	[SerializeField]
	private ODev.Util.Mono.Updateable m_Updateable = new();
	
	public PlayerAbilityClimb(PlayerRoot pPlayer, SOPlayerAbilityClimb pData) : base(pPlayer, pData) { }

	public override void Initalize()
	{
		m_Updateable.SetProperties(Data.UpdateType, Data.UpdatePriority);
		m_Updateable.Register(Tick);
	}

	public override void Destroy()
	{
		m_Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		if (Root.OnGround.IsOnGround || !Root.OnWall.IsOnWall)
		{
			return;
		}

		Vector3 input = Root.Input.Move.Input.y * MainCamera.Camera.transform.forward.ProjectOnPlane(Vector3.up);
		input += Root.Input.Move.Input.x * MainCamera.Camera.transform.right.ProjectOnPlane(Vector3.up);
		if (Vector3.Dot(input, -Root.OnWall.HitInfo.normal.Horizontal()) < 0.75f)
		{
			return;
		}
		
		Root.Movement.SetVelocityY(Data.ClimbForce);
	}
}
