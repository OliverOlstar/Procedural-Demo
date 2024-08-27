using System;
using ODev;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Climb Ability", menuName = "Character/Ability/Player Climb")]
public class SOPlayerAbilityClimb : SOCharacterAbility
{
	[Space, SerializeField]
	private float m_Force = 20.0f;
	[SerializeField, Range(-1.0f, 1.0f)]
	private float m_MinDot = 0.75f;
	[SerializeField, Range(0.0f, 1.0f)]
	private float m_InputDeadZone01 = 0.2f;
	[SerializeField]
	private float m_AccelerationPercentModify = 0.0f;
	[SerializeField]
	private float m_DragPercentModify = 1.0f;

	public float Force => m_Force;
	public float MinDot => m_MinDot;
	public float InputDeadZone01 => m_InputDeadZone01;
	public float AccelerationPercentModify => m_AccelerationPercentModify;
	public float DragPercentModify => m_DragPercentModify;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityClimb(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityClimb : CharacterAbility<SOPlayerAbilityClimb>
{
	public PlayerAbilityClimb(PlayerRoot pPlayer, SOPlayerAbilityClimb pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	private bool m_IsInputing;

	protected override bool CanActivateUpdate()
	{
		if (Root.OnGround.IsOnGround || !Root.OnWall.IsOnWall)
		{
			return false;
		}
		m_IsInputing = Root.Input.Move.Input.sqrMagnitude > Mathf.Pow(Data.InputDeadZone01, 2);
		if (m_IsInputing)
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
		if (!CanActivateUpdate())
		{
			Deactivate();
			return;
		}
		if (m_IsInputing)
		{
			Vector3 up = Vector3.up.ProjectOnPlane(Root.OnWall.HitInfo.normal).normalized;
			if (Root.Input.Move.Input.y < 0.0f)
			{
				Deactivate();
				return;
			}
			Root.Movement.AddVelocityXZ(Data.Force * pDeltaTime * Root.Input.Move.Input.y * up);
		}
	}

	private int m_ModifyKeyAcceleration;
	private int m_ModifyKeyDrag;
	protected override void ActivateInternal()
	{
		Root.Movement.GravityEnabled = false;
		// Root.Movement.MovementEnabled = false;
		m_ModifyKeyAcceleration = Root.Movement.AirAcceleration.AddPercentModify(Data.DragPercentModify);
		m_ModifyKeyDrag = Root.Movement.AirDrag.AddPercentModify(Data.DragPercentModify);
		Root.Movement.SetVelocityY(0.0f);
	}
	protected override void DeactivateInternal()
	{
		Root.Movement.GravityEnabled = true;
		// Root.Movement.MovementEnabled = true;
		Root.Movement.AirAcceleration.TryRemovePercentModify(m_ModifyKeyAcceleration);
		Root.Movement.AirDrag.TryRemovePercentModify(m_ModifyKeyDrag);
	}
}
