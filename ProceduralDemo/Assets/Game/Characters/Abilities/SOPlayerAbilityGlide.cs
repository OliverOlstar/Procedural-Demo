using ODev.GameStats;
using ODev.Input;
using ODev.Picker;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Glide Ability", menuName = "Character/Ability/Player Glide")]
public class SOPlayerAbilityGlide : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private FloatGameStatModifier m_AccelerationModifier;
	public FloatGameStatModifier AccelerationModifier => m_AccelerationModifier;
	[SerializeField, AssetNonNull]
	private FloatGameStatModifier m_DragModifier;
	public FloatGameStatModifier DragModifier => m_DragModifier;
	[SerializeField, AssetNonNull]
	private FloatGameStatModifier m_MaxVelocityModifier;
	public FloatGameStatModifier MaxVelocityModifier => m_MaxVelocityModifier;
	[Space, SerializeField, AssetNonNull]
	private FloatGameStatModifier m_GravityUpModifier;
	public FloatGameStatModifier GravityUpModifier => m_GravityUpModifier;
	[SerializeField, AssetNonNull]
	private FloatGameStatModifier m_GravityDownModifier;
	public FloatGameStatModifier GravityDownModifier => m_GravityDownModifier;

	[Space, SerializeField]
	private float m_MinStartYVelocity = 0.0f;
	public float MinStartYVelocity => m_MinStartYVelocity;
	[SerializeField]
	private float m_StartYForce = 0.0f;
	public float StartYForce => m_StartYForce;

	public override ICharacterAbility CreateInstance(PlayerRoot pRoot, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityGlide(pRoot, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityGlide : CharacterAbility<SOPlayerAbilityGlide>
{
	public PlayerAbilityGlide(PlayerRoot pPlayer, SOPlayerAbilityGlide pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	public override IInputTrigger InputActivate => Root.Input.Jump;

	private GameObject m_TempGlideObject = null;
	private bool m_WasOnGround = false;

	protected override void Initalize()
	{
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

	protected override void ActivateInternal()
	{
		Data.AccelerationModifier.Apply(Root.Movement.AirAcceleration);
		Data.DragModifier.Apply(Root.Movement.AirDrag);
		Data.MaxVelocityModifier.Apply(Root.Movement.AirMaxVelocity);
		Data.GravityUpModifier.Apply(Root.Movement.UpGravity);
		Data.GravityDownModifier.Apply(Root.Movement.DownGravity);

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
		Data.AccelerationModifier.Remove(Root.Movement.AirAcceleration);
		Data.DragModifier.Remove(Root.Movement.AirDrag);
		Data.MaxVelocityModifier.Remove(Root.Movement.AirMaxVelocity);
		Data.GravityUpModifier.Remove(Root.Movement.UpGravity);
		Data.GravityDownModifier.Remove(Root.Movement.DownGravity);

		if (m_TempGlideObject != null)
		{
			m_TempGlideObject.SetActive(false);
		}
	}
}
