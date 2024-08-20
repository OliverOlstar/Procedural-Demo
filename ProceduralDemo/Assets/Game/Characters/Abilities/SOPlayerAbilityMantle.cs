using ODev.Input;
using ODev.Util;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Mantle Ability", menuName = "Character/Ability/Player Mantle")]
public class SOPlayerAbilityMantle : SOCharacterAbility
{
	[SerializeField]
	private float m_ForwardDistance = 1.0f;
	[SerializeField]
	private float m_MaxUpDistance = 2.0f;
	[SerializeField]
	private LayerMask m_GroundLayers = new();

	public float ForwardDistance => m_ForwardDistance;
	public float MaxUpDistance => m_MaxUpDistance;
	public LayerMask HitLayers => m_GroundLayers;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction<bool> pOnInputRecived) => new PlayerAbilityMantle(pPlayer, this, pOnInputRecived);
}

public class PlayerAbilityMantle : CharacterAbility<SOPlayerAbilityMantle>
{
	public PlayerAbilityMantle(PlayerRoot pPlayer, SOPlayerAbilityMantle pData, UnityAction<bool> pOnInputRecived) : base(pPlayer, pData, pOnInputRecived) { }

	public override InputModule_Toggle InputActivate => Root.Input.Jump;
	private Transform Transform => Root.Movement.transform;

	protected override void Initalize()
	{

	}
	protected override void DestroyInternal()
	{

	}

	private Vector3 m_Direction;
	private RaycastHit m_Hit;

	protected override bool CanActivateUpdate()
	{
		if (!Root.OnWall.IsOnWall || Root.OnGround.IsOnGround || Root.Movement.VelocityY < -1.0f)
		{
			return false;
		}
		m_Direction = -Root.OnWall.HitInfo.normal.Horizontalize();
		if (Physics.Raycast(Transform.position + (m_Direction * Data.ForwardDistance) + (Vector3.up * Data.MaxUpDistance), Vector3.down, out m_Hit, Data.MaxUpDistance, Data.HitLayers))
		{
			return true;
		}
		return false;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.AddDisplacement(m_Hit.point - Transform.position, Quaternion.identity);
		Root.Movement.SetVelocityY(-1.0f);
		Deactivate();
	}
	protected override void DeactivateInternal()
	{
		
	}
}
