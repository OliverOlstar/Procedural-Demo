using ODev.Input;
using ODev.Picker;
using ODev.PoseAnimator;
using ODev.Util;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Mantle Ability", menuName = "Character/Ability/Player Mantle")]
public class SOPlayerAbilityMantle : SOCharacterAbility
{
	[Space, SerializeField, AssetNonNull]
	private SOPoseMontage m_Montage = null;
	[SerializeField]
	private AnimationCurve m_XZCurve = new();
	[SerializeField]
	private AnimationCurve m_YCurve = new();

	[Space, SerializeField]
	private float m_CompleteVelocity = 0.0f;

	[Header("Raycast")]
	[SerializeField]
	private float m_ForwardDistance = 1.0f;
	[SerializeField]
	private float m_MaxUpDistance = 2.0f;
	[SerializeField]
	private LayerMask m_GroundLayers = new();

	[Header("Valid")]
	[SerializeField]
	private float m_MinTimeOffGround = 0.5f;
	[SerializeField, Range(0.0f, 1.0f)]
	private float m_TopSlopeMin = 0.5f;
	[SerializeField, MinMaxSlider(-1.0f, 1.0f, ShowFields = true)]
	private Vector2 m_SideSlopeLimit = new(-0.5f, 0.5f);

	public SOPoseMontage Montage => m_Montage;
	public AnimationCurve XZCurve => m_XZCurve;
	public AnimationCurve YCurve => m_YCurve;
	public float CompleteVelocity => m_CompleteVelocity;
	public float ForwardDistance => m_ForwardDistance;
	public float MaxUpDistance => m_MaxUpDistance;
	public LayerMask HitLayers => m_GroundLayers;
	public float MinTimeOffGround => m_MinTimeOffGround;
	public float TopSlopeMin => m_TopSlopeMin;
	public Vector2 SideSlopeLimit => m_SideSlopeLimit;

	public override ICharacterAbility CreateInstance(PlayerRoot pPlayer, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) => new PlayerAbilityMantle(pPlayer, this, pOnInputPerformed, pOnInputCanceled);
}

public class PlayerAbilityMantle : CharacterAbility<SOPlayerAbilityMantle>
{
	public PlayerAbilityMantle(PlayerRoot pPlayer, SOPlayerAbilityMantle pData, UnityAction pOnInputPerformed, UnityAction pOnInputCanceled) : base(pPlayer, pData, pOnInputPerformed, pOnInputCanceled) { }

	private Transform Transform => Root.Movement.transform;

	protected override void Initalize()
	{

	}
	protected override void DestroyInternal()
	{

	}

	private Vector3 m_Direction;
	private RaycastHit m_Hit;
	private int m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;

	private float m_TimeElapsed;
	private Vector3 m_CurrentPosition;
	private Vector3 m_FromPosition;
	private Vector3 m_ToPosition;

	protected override bool CanActivateUpdate()
	{
		if (!Root.OnWall.IsOnWall || Root.OnGround.IsOnGround || Root.Movement.VelocityY < -1.0f)
		{
			return false;
		}
		if (Root.OnGround.Times.TimeOffGround < Data.MinTimeOffGround)
		{
			return false;
		}
		if (Root.OnWall.HitInfo.normal.y < Data.SideSlopeLimit.x || Root.OnWall.HitInfo.normal.y > Data.SideSlopeLimit.y)
		{
			return false;
		}
		m_Direction = -Root.OnWall.HitInfo.normal.Horizontalize();
		if (!Physics.Raycast(Transform.position + (m_Direction * Data.ForwardDistance) + (Vector3.up * Data.MaxUpDistance), Vector3.down, out m_Hit, Data.MaxUpDistance, Data.HitLayers))
		{
			return false;
		}
		if (m_Hit.normal.y < Data.TopSlopeMin)
		{
			return false;
		}
		return true;
	}

	protected override void ActivateInternal()
	{
		Root.Movement.enabled = false;
		m_MontageHandle = Root.Animator.PlayMontage(Data.Montage);

		m_TimeElapsed = 0.0f;
		m_CurrentPosition = Vector3.zero;
		m_FromPosition = Transform.position;
		m_ToPosition = m_Hit.point;
	}

	public override void ActiveTick(float pDeltaTime)
	{
		m_TimeElapsed += pDeltaTime;
		float seconds = Data.Montage.TotalSeconds - Data.Montage.FadeOutSeconds;
		if (m_TimeElapsed >= seconds)
		{
			Transform.position = m_ToPosition;
			m_MontageHandle = PoseMontageAnimator.NULL_HANDLE;
			Deactivate();
			return;
		}

		float progress01 = m_TimeElapsed / seconds;
		float xzProgress = Data.XZCurve.Evaluate(progress01);
		float yProgress = Data.YCurve.Evaluate(progress01);

		m_CurrentPosition.x = Mathf.LerpUnclamped(m_FromPosition.x, m_ToPosition.x, xzProgress);
		m_CurrentPosition.y = Mathf.LerpUnclamped(m_FromPosition.y, m_ToPosition.y, yProgress);
		m_CurrentPosition.z = Mathf.LerpUnclamped(m_FromPosition.z, m_ToPosition.z, xzProgress);
		Transform.position = m_CurrentPosition;

		// if (Input.GetKey(KeyCode.Q))
		// {
		// 	this.Log("CANCEL");
		// 	Deactivate();
		// }
	}

	protected override void DeactivateInternal()
	{
		Root.Movement.enabled = true;
		if (!Root.Input.Move.Input.IsNearZero())
		{
			Root.Movement.AddVelocityXZ((m_ToPosition - m_FromPosition).Horizontalize() * Data.CompleteVelocity);
		}
		Root.Animator.CancelMontage(m_MontageHandle);
	}
}
