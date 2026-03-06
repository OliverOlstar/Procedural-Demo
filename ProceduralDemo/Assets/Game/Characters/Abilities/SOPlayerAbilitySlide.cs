using System;
using ODev.GameStats;
using ODev.Input;
using ODev.Picker;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Slide Ability", menuName = "Character/Ability/Player Slide")]
public class SOPlayerAbilitySlide : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private FloatGameStatModifier m_DragModifier;
	public FloatGameStatModifier DragModifier => m_DragModifier;
	[SerializeField, AssetNonNull]
	private FloatGameStatModifier m_AirDragModifier;
	public FloatGameStatModifier AirDragModifier => m_AirDragModifier;
	[SerializeField, AssetNonNull]
	private FloatGameStatModifier m_GravityModifier;
	public FloatGameStatModifier GravityModifier => m_GravityModifier;

	[Space, SerializeField]
	private float m_StartVelocity = 25.0f;
	public float StartVelocity => m_StartVelocity;
	[SerializeField]
	private float m_NormalAcceleration = 5.0f;
	public float NormalAcceleration => m_NormalAcceleration;
	[SerializeField]
	private float m_MaxAcceleration = 50.0f;
	public float MaxAcceleration => m_MaxAcceleration;

	[Space, SerializeField]
	private float m_RequiredStartVelocity = 10.0f;
	public float RequiredStartVelocity => m_RequiredStartVelocity;
	[SerializeField]
	private float m_RequiredEndVelocity = 5.0f;
	public float RequiredEndVelocity => m_RequiredEndVelocity;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilitySlide(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilitySlide : CharacterAbility<SOPlayerAbilitySlide>
{
	public PlayerAbilitySlide(PlayerRoot pPlayer, SOPlayerAbilitySlide pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	protected override void Initalize() { }
	protected override void DestroyInternal() { }

	protected override bool CanActivateUpdate()
	{
		return Root.Input.Crouch.Input && CanActivate();
	}
	protected override bool CanActivate()
	{
		if (Root.Movement.VelocityXZ.sqrMagnitude > Data.RequiredStartVelocity * Data.RequiredStartVelocity)
		{
			return true;
		}
		float angle = Vector3.Dot(Root.OnGround.GetAverageNormal(), Root.Animator.transform.forward.Horizontalize());
		return angle > 0.02f;
	}

	protected override void ActivateInternal()
	{
		Data.DragModifier.Apply(Root.Movement.Drag);
		Data.AirDragModifier.Apply(Root.Movement.AirDrag);
		Data.GravityModifier.Apply(Root.Movement.DownGravity);
		// m_GravityUpModifierInstance.Apply(Root.Movement.UpGravity);

		Root.Movement.MovementEnabled = false;
		Vector3 direction = Root.Animator.transform.forward.Horizontal().ProjectOnPlane(Root.OnGround.GetAverageNormal()).normalized;
		Root.Movement.AddVelocity(direction * Data.StartVelocity);
	}

	protected override void DeactivateInternal()
	{
		Data.DragModifier.Remove(Root.Movement.Drag);
		Data.AirDragModifier.Remove(Root.Movement.AirDrag);
		Data.GravityModifier.Remove(Root.Movement.DownGravity);
		// m_GravityUpModifierInstance.Remove(Root.Movement.UpGravity);

		Root.Movement.MovementEnabled = true;
	}

	public override void ActiveTick(float pDeltaTime)
	{
		Vector3 direction = Root.Animator.transform.forward.Horizontalize().ProjectOnPlane(Root.OnGround.GetAverageNormal());
		float force = Vector3.Project(Root.OnGround.GetAverageNormal(), Root.Animator.transform.forward.Horizontalize()).magnitude;
		force = Mathf.Min(force * Data.NormalAcceleration, Data.MaxAcceleration);
		Root.Movement.AddVelocityXZ(pDeltaTime * force * direction);

		if (!Root.Input.Crouch.Input || Root.Movement.VelocityXZ.sqrMagnitude < Data.RequiredEndVelocity * Data.RequiredEndVelocity)
		{
			Deactivate();
		}
	}
}
