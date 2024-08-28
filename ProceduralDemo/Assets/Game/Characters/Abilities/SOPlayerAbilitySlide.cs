using System;
using ODev.GameStats;
using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Slide Ability", menuName = "Character/Ability/Player Slide")]
public class SOPlayerAbilitySlide : SOCharacterAbility
{
	[SerializeField]
	private FloatGameStatModifier m_DragModifier = new();
	public FloatGameStatModifier DragModifier => m_DragModifier;
	[SerializeField]
	private FloatGameStatModifier m_AirDragModifier = new();
	public FloatGameStatModifier AirDragModifier => m_AirDragModifier;
	[SerializeField]
	private FloatGameStatModifier m_GravityModifier = new();
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
	private FloatGameStatModifier m_ModifierInstance;
	private FloatGameStatModifier m_AirModifierInstance;
	private FloatGameStatModifier m_GravityDownModifierInstance;
	private FloatGameStatModifier m_GravityUpModifierInstance;

	public PlayerAbilitySlide(PlayerRoot pPlayer, SOPlayerAbilitySlide pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	protected override void Initalize()
	{
		m_ModifierInstance = FloatGameStatModifier.CreateCopy(Data.DragModifier);
		m_AirModifierInstance = FloatGameStatModifier.CreateCopy(Data.AirDragModifier);
		m_GravityUpModifierInstance = FloatGameStatModifier.CreateCopy(Data.GravityModifier);
		m_GravityDownModifierInstance = FloatGameStatModifier.CreateCopy(Data.GravityModifier);
	}
	protected override void DestroyInternal() { }

	protected override bool CanActivateUpdate()
	{
		return Root.Input.Crouch.Input && CanActivate();
	}
	protected override bool CanActivate()
	{
		return !Root.OnGround.IsInAir && Root.Movement.VelocityXZ.sqrMagnitude > Data.RequiredStartVelocity * Data.RequiredStartVelocity;
	}

	protected override void ActivateInternal()
	{
		m_ModifierInstance.Apply(Root.Movement.Drag);
		m_AirModifierInstance.Apply(Root.Movement.AirDrag);
		m_GravityUpModifierInstance.Apply(Root.Movement.UpGravity);
		m_GravityDownModifierInstance.Apply(Root.Movement.DownGravity);

		Root.Movement.MovementEnabled = false;
		Root.Movement.AddVelocity(Root.Movement.VelocityXZ.Horizontalize() * Data.StartVelocity);
	}

	protected override void DeactivateInternal()
	{
		m_ModifierInstance.Remove(Root.Movement.Drag);
		m_AirModifierInstance.Remove(Root.Movement.AirDrag);
		m_GravityUpModifierInstance.Remove(Root.Movement.UpGravity);
		m_GravityDownModifierInstance.Remove(Root.Movement.DownGravity);

		Root.Movement.MovementEnabled = true;
	}

	public override void ActiveTick(float pDeltaTime)
	{
		Vector3 force = Vector3.Project(Root.OnGround.GetAverageNormal(), Root.Movement.VelocityXZ.Horizontalize());
		force *= Data.NormalAcceleration;
		force = Vector3.ClampMagnitude(force, Data.MaxAcceleration);
		Root.Movement.AddVelocityXZ(pDeltaTime * force);

		if (!Root.Input.Crouch.Input || Root.Movement.VelocityXZ.sqrMagnitude < Data.RequiredEndVelocity * Data.RequiredEndVelocity)
		{
			Deactivate();
		}
	}
}
