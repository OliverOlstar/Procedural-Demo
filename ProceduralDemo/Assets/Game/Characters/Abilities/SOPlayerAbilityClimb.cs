using System;
using ODev;
using ODev.Util;
using UnityEngine;

[CreateAssetMenu(fileName = "New Climb Ability", menuName = "Character/Ability/Player Climb")]
public class SOPlayerAbilityClimb : SOCharacterAbility
{
	[SerializeField]
	private float m_Force = 20.0f;
	[SerializeField, Range(-1.0f, 1.0f)]
	private float m_MinDot = 0.75f;
	[SerializeField, Range(0.0f, 1.0f)]
	private float m_InputDeadZone01 = 0.2f;

	public float Force => m_Force;
	public float MinDot => m_MinDot;
	public float InputDeadZone01 => m_InputDeadZone01;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer) => new PlayerAbilityClimb(pPlayer, this);
}

public class PlayerAbilityClimb : CharacterAbility<SOPlayerAbilityClimb>
{
	public PlayerAbilityClimb(PlayerRoot pPlayer, SOPlayerAbilityClimb pData) : base(pPlayer, pData) { }

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	protected override bool CanActivate()
	{
		if (Root.OnGround.IsOnGround || !Root.OnWall.IsOnWall)
		{
			return false;
		}
		if (Root.Input.Move.Input.sqrMagnitude > Data.InputDeadZone01)
		{
			Vector3 input = Root.Input.Move.Input.y * MainCamera.Camera.transform.forward.Horizontal();
			input += Root.Input.Move.Input.x * MainCamera.Camera.transform.right.Horizontal();
			if (Vector3.Dot(input, -Root.OnWall.HitInfo.normal.Horizontal()) < Data.MinDot)
			{
				return false;
			}
		}
		
		return true;
	}

	public override void ActiveTick(float pDeltaTime)
	{
		if (!CanActivate())
		{
			Deactivate();
			return;
		}
		bool isInputing = Root.Input.Move.Input.sqrMagnitude > Data.InputDeadZone01;
		if (isInputing)
		{
			Root.Movement.SetVelocityY(Data.Force);
		}
		else
		{
			Root.Movement.SetVelocityY(0.0f);
			Root.Movement.SetVelocityXZ(Vector3.zero);
		}
	}

	protected override void ActivateInternal()
	{

	}
	protected override void DeactivateInternal()
	{
		
	}
}
