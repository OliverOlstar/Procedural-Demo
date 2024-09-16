using System;
using ODev.GameStats;
using ODev.Input;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Glide Ability", menuName = "Character/Ability/Player Glide")]
public class SOPlayerAbilityGlide : SOCharacterAbility
{
	[Space, SerializeField]
	private FloatGameStatModifier m_AccelerationModifier = new();
	public FloatGameStatModifier AccelerationModifier => m_AccelerationModifier;
	[SerializeField]
	private FloatGameStatModifier m_DragModifier = new();
	public FloatGameStatModifier DragModifier => m_DragModifier;
	[SerializeField]
	private FloatGameStatModifier m_MaxVelocityModifier = new();
	public FloatGameStatModifier MaxVelocityModifier => m_MaxVelocityModifier;
	[Space, SerializeField]
	private FloatGameStatModifier m_GravityUpModifier = new();
	public FloatGameStatModifier GravityUpModifier => m_GravityUpModifier;
	[SerializeField]
	private FloatGameStatModifier m_GravityDownModifier = new();
	public FloatGameStatModifier GravityDownModifier => m_GravityDownModifier;

	[Space, SerializeField]
	private float m_MinStartYVelocity = 0.0f;
	public float MinStartYVelocity => m_MinStartYVelocity;
	[SerializeField]
	private float m_StartYForce = 0.0f;
	public float StartYForce => m_StartYForce;


	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityGlide(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityGlide : CharacterAbility<SOPlayerAbilityGlide>
{
	public PlayerAbilityGlide(PlayerRoot pPlayer, SOPlayerAbilityGlide pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Jump;

	private FloatGameStatModifier m_AccelerationModifierInstance;
	private FloatGameStatModifier m_DragModifierInstance;
	private FloatGameStatModifier m_MaxVelocityModifierInstance;
	private FloatGameStatModifier m_GravityDownModifierInstance;
	private FloatGameStatModifier m_GravityUpModifierInstance;

	private GameObject m_TempGlideObject = null;
	private bool m_WasOnGround = false;

	protected override void Initalize()
	{
		m_AccelerationModifierInstance = FloatGameStatModifier.CreateCopy(Data.AccelerationModifier);
		m_DragModifierInstance = FloatGameStatModifier.CreateCopy(Data.DragModifier);
		m_MaxVelocityModifierInstance = FloatGameStatModifier.CreateCopy(Data.MaxVelocityModifier);
		m_GravityUpModifierInstance = FloatGameStatModifier.CreateCopy(Data.GravityUpModifier);
		m_GravityDownModifierInstance = FloatGameStatModifier.CreateCopy(Data.GravityDownModifier);

		m_TempGlideObject = GameObject.Find($"{Root.name}-Glide-TestDisplay");
		m_TempGlideObject.SetActive(false);

		Root.OnGround.OnGroundEnterEvent.AddListener(OnGroundEnter);
	}

	protected override void DestroyInternal()
	{
		Root.OnGround.OnGroundEnterEvent.RemoveListener(OnGroundEnter);
	}

	private void OnGroundEnter()
	{
		m_WasOnGround = true;
	}

	protected override bool CanActivate()
	{
		return !Root.OnGround.IsOnGround;
	}

	protected override void ActivateInternal()
	{
		m_AccelerationModifierInstance.Apply(Root.Movement.AirAcceleration);
		m_DragModifierInstance.Apply(Root.Movement.AirDrag);
		m_MaxVelocityModifierInstance.Apply(Root.Movement.AirMaxVelocity);
		m_GravityUpModifierInstance.Apply(Root.Movement.UpGravity);
		m_GravityDownModifierInstance.Apply(Root.Movement.DownGravity);

		Root.OnGround.OnAirExitEvent.AddListener(OnAirExit);

		if (m_TempGlideObject != null)
		{
			m_TempGlideObject.SetActive(true);
		}

		if (m_WasOnGround)
		{
			Root.Movement.SetVelocityY(Mathf.Max(Root.Movement.VelocityY, Data.StartYForce));
		}
		else
		{
			Root.Movement.SetVelocityY(Mathf.Max(Root.Movement.VelocityY, Data.MinStartYVelocity));
		}
		m_WasOnGround = false;
	}

	protected override void DeactivateInternal()
	{
		m_AccelerationModifierInstance.Remove(Root.Movement.AirAcceleration);
		m_DragModifierInstance.Remove(Root.Movement.AirDrag);
		m_MaxVelocityModifierInstance.Remove(Root.Movement.AirMaxVelocity);
		m_GravityUpModifierInstance.Remove(Root.Movement.UpGravity);
		m_GravityDownModifierInstance.Remove(Root.Movement.DownGravity);

		Root.OnGround.OnAirExitEvent.RemoveListener(OnAirExit);

		if (m_TempGlideObject != null)
		{
			m_TempGlideObject.SetActive(false);
		}
	}

	private void OnAirExit() => Deactivate();
}
