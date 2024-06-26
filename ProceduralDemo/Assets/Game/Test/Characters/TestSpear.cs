using UnityEngine;
using OCore;
using OCore.Util;
using OCore.Cue;

public class TestSpear : MonoBehaviour
{
	[SerializeField]
	private float Force = 5.0f;
	[SerializeField]
	private float Gravity = -9.81f;
	[SerializeField]
	private float GravityDelay = 3.0f;
	[SerializeField]
	private LayerMask HitLayer = new();
	[SerializeField]
	private Easing.EaseParams RecallEase = new();
	[SerializeField]
	private float RecallSeconds = 5.0f;

	[SerializeField]
	private float JumpScalarXZ = 8.0f;
	[SerializeField]
	private float JumpScalarY = 8.0f;
	[SerializeField]
	private float JumpForceOffsetXZ = 4.0f;
	[SerializeField]
	private float JumpForceOffsetY = 4.0f;
	[SerializeField]
	private float MaxChargeSeconds = 2.0f;

	[SerializeField]
	private float JumpRotationSpeed = 5.0f;
	[SerializeField]
	private LineRenderer LineRenderer = null;
	[SerializeField]
	private Transform HighlightSpear = null;

	[Space, SerializeField]
	private Easing.EaseParams playerSnapEase = new();
	[SerializeField]
	private float playerSnapEaseSeconds = 1.0f;

	[Space, SerializeField]
	private SOCue hitCue;

	private Transform Camera = null;
	private TestThrow Thrower = null;
	private Vector3 Velocity;
	private float MoveTime = 0.0f;
	private bool isAnimating = false;
	private bool isAiming = false;
	private TestCharacter character;
	private Collider trigger = null;
	private readonly TransformFollower follower = new();

	private Vector3 CharacterStandPoint => transform.position + (0.5f * transform.localScale.y * Vector3.up) + (0.45f * transform.localScale.z * -transform.forward);

	private Vector3 SpearBack() => transform.position - (0.5f * transform.localScale.z * transform.forward);
	private Vector3 SpearBack(Vector3 pPosition) => pPosition - (0.5f * transform.localScale.z * transform.forward);
	private Vector3 SpearFront() => transform.position + (0.5f * transform.localScale.z * transform.forward);

	public void Init(Transform pCamera, TestThrow pThrow)
	{
		Camera = pCamera;
		Thrower = pThrow;
		trigger = GetComponent<Collider>();
	}

	public void Aim()
	{
		transform.SetParent(Camera, false);
		transform.localPosition = Vector3.zero;
		transform.rotation = Quaternion.LookRotation(GetThrowDirection());
		MoveTime = -1.0f;
		isAiming = true;
		HighlightSpear.gameObject.SetActive(true);
	}

	public bool CanThrow() => !isAnimating;
	public void Throw()
	{
		transform.SetParent(null);
		Velocity = GetThrowDirection() * Force;
		MoveTime = 0.0f;
		isAiming = false;
		HighlightSpear.gameObject.SetActive(false);
		lastPosition = transform.position;
	}

	private Vector3 GetThrowPoint()
	{
		Vector3 toPoint = Physics.Raycast(MainCamera.Camera.transform.position, MainCamera.Camera.transform.forward, out RaycastHit hit, 40.0f, HitLayer)
			? hit.point : MainCamera.Position + (MainCamera.Forward * 40.0f);
		return toPoint;
	}
	private Vector3 GetThrowDirection()
	{
		return GetThrowPoint() - transform.position;
	}

	public void Recall()
	{
		if (character != null)
		{
			character.SetUpdateEnabled(true);
			character = null;
		}
		transform.SetParent(null);
		MoveTime = -1.0f;

		Vector3 recallStartPosition = transform.position;
		isAnimating = true;
		float seconds = Vector3.Distance(recallStartPosition, Camera.position) * RecallSeconds;
		Anim.Play(RecallEase, Mathf.Min(seconds * 10.0f, 0.2f),
		(pProgress) => // OnTick
		{
			transform.position = Vector3.LerpUnclamped(recallStartPosition, Camera.position, pProgress);
		},
		(_) => // OnComplete
		{
			transform.position = Vector3.one * 5000.0f;
			isAnimating = false;
			Thrower.OnRecallComplete();
		});

		follower.Stop();
	}

	private float jumpCharge = -1.0f;
	private void Update()
	{
		if (character != null)
		{
			DoJumpCharge();
		}
		else if (MoveTime > -1.0f)
		{
			DoThrownUpdate();
		}
		else if (isAiming)
		{
			Vector3 point = GetThrowPoint();
			Quaternion direction = Quaternion.LookRotation(point - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, direction, Time.deltaTime * 35.0f);
			LineRenderer.SetPosition(0, SpearFront());
			LineRenderer.SetPosition(1, point);
			HighlightSpear.transform.SetPositionAndRotation(point - (0.4f * transform.localScale.z * transform.forward), direction);
		}
	}

	private Vector3 StartPosition = Vector3.zero;
	private Quaternion StartRotation = Quaternion.identity;
	private Vector2 JumpInput;
	private Vector2 LastJumpInput;
	private void DoJumpCharge()
	{
		if (jumpCharge < 0.0f)
		{
			if (!Input.GetKeyDown(KeyCode.S) && !Input.GetKeyDown(KeyCode.D) && !Input.GetKeyDown(KeyCode.A))
			{
				return;
			}
			jumpCharge = 0.0f;
			StartRotation = transform.rotation;
			StartPosition = transform.position;
		}
		float x = Input.GetKey(KeyCode.S) ? -1 : 0;
		float y = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
		JumpInput = Vector2.Lerp(JumpInput, Vector2.ClampMagnitude(new Vector2(x, y), 1.0f), Time.deltaTime * 5.0f);
		if (jumpCharge < MaxChargeSeconds)
		{
			// Charging
			jumpCharge += Time.deltaTime;
			jumpCharge.ClampMax(MaxChargeSeconds);
		}

		// Rotate
		transform.SetPositionAndRotation(StartPosition, StartRotation);
		transform.RotateAround(SpearFront(), transform.right, -JumpRotationSpeed * -JumpInput.x * jumpCharge);
		transform.RotateAround(SpearFront(), transform.up, -JumpRotationSpeed * JumpInput.y * jumpCharge);
		character.transform.position = CharacterStandPoint;

		if (x == 0 && y == 0)
		{
			// Jump
			transform.SetPositionAndRotation(StartPosition, StartRotation);
			character.Move(new Vector3(0.0f, CharacterStandPoint.y - character.transform.position.y, 0.0f));

			character.SetUpdateEnabled(true);
			Vector3 dir = (transform.up * -LastJumpInput.x) + (transform.right * -LastJumpInput.y);
			character.Velocity = Math.Scale(dir, JumpForceOffsetXZ + (jumpCharge * JumpScalarXZ), JumpForceOffsetY + (jumpCharge * JumpScalarY));

			jumpCharge = -1.0f;
			character = null;

			trigger.enabled = false;
			Invoke(nameof(SetColliderEnabled), 0.5f);
		}
		LastJumpInput = JumpInput;
	}
	private void SetColliderEnabled() => trigger.enabled = true;

	private Vector3 lastPosition = Vector3.zero;
	private void DoThrownUpdate()
	{
		if (Physics.Linecast(SpearBack(lastPosition), SpearFront(), out RaycastHit hit, HitLayer))
		{
			// Hit
			MoveTime = -2.0f;
			if (hit.collider.CompareTag("CanNotSpear"))
			{
				Recall();
			}
			else
			{
				trigger.enabled = false;
				transform.position = hit.point - (0.4f * transform.localScale.z * transform.forward);
				// transform.SetPositionAndRotation(hit.point - (0.4f * transform.localScale.z * transform.forward), Quaternion.LookRotation(-hit.normal));
				trigger.enabled = true;

				follower.Start(hit.transform, transform, hit.point, OnAttachedMoved, true, OCore.Util.Mono.Type.Default, OCore.Util.Mono.Priorities.CharacterController, this);
			}
			SOCue.Play(hitCue, new CueContext(hit.point));
			return;
		}
		lastPosition = transform.position;

		// Moving
		MoveTime += Time.deltaTime;
		if (MoveTime >= GravityDelay)
		{
			Velocity.y += Gravity * Time.deltaTime;
		}
		transform.position += Velocity * Time.deltaTime;
		if (Velocity.NotNearZero())
		{
			transform.rotation = Quaternion.LookRotation(Velocity);
		}
	}

	private void OnAttachedMoved(Vector3 pDeltaPosition)
	{
		if (character != null && !isAnimating)
		{
			character.transform.position = CharacterStandPoint;
			StartPosition += pDeltaPosition;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (MoveTime > -2.0f || character || !other.TryGetComponent(out character))
		{
			return;
		}
		character.SetUpdateEnabled(false);
		Vector3 startPosition = character.transform.position;
		isAnimating = true;
		Anim.Play(playerSnapEase, playerSnapEaseSeconds,
		(float pProgress) =>
		{
			if (character != null)
			{
				character.transform.position = Vector3.LerpUnclamped(startPosition, CharacterStandPoint, pProgress);
			}
		},
		(float _) =>
		{
			if (character != null)
			{
				character.transform.position = CharacterStandPoint;
			}

			isAnimating = false;
		});
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(SpearBack(), SpearFront());
		follower.OnDrawGizmos();
	}

	private void OnDestroy() => follower.OnDestroy();
}
