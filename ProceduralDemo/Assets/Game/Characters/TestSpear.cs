using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;
using OliverLoescher.Cue;
using OliverLoescher;

public class TestSpear : MonoBehaviour
{
	[SerializeField]
	private float Force = 5.0f;
	[SerializeField]
	private float Gravity = -9.81f;
	[SerializeField]
	private float GravityDelay = 3.0f;
	[SerializeField]
	private LayerMask HitLayer = new LayerMask();
	[SerializeField]
	private Easing.EaseParams RecallEase = new Easing.EaseParams();
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
	private SOCue hitCue;

	private Transform Camera = null;
	private TestThrow Thrower = null;
	private Vector3 Velocity;
	private float MoveTime = 0.0f;
	private bool isAnimating = false;
	private bool isAiming = false;
	private TestCharacter character;

	private Vector3 CharacterStandPoint => transform.position + (0.5f * transform.localScale.y * Vector3.up) + (0.45f * transform.localScale.z * -transform.forward);


	private Vector3 SpearBack() => transform.position - (0.5f * transform.localScale.z * transform.forward);
	private Vector3 SpearFront() => transform.position + (0.5f * transform.localScale.z * transform.forward);

	public void Init(Transform pCamera, TestThrow pThrow) { Camera = pCamera; Thrower = pThrow; }

	public void Aim()
	{
		transform.SetParent(Camera, false);
		transform.localPosition = Vector3.zero;
		transform.rotation = Quaternion.LookRotation(GetThrowDirection());
		MoveTime = -1.0f;
		isAiming = true;
	}

	public bool CanThrow() => !isAnimating;
	public void Throw()
	{
		transform.SetParent(null);
		Velocity = GetThrowDirection() * Force;
		MoveTime = 0.0f;
		isAiming = false;
	}

	private Vector3 GetThrowDirection()
	{
		Vector3 toPoint = Physics.Raycast(MainCamera.Camera.transform.position, MainCamera.Camera.transform.forward, out RaycastHit hit, 40.0f, HitLayer)
					? hit.point : MainCamera.Position + (MainCamera.Forward * 40.0f);
		return toPoint - transform.position;
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
		Anim.Play(RecallEase, Mathf.Min(seconds, 2.0f),
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
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(GetThrowDirection()), Time.deltaTime * 35.0f);
		}
	}

	private void DoJumpCharge()
	{
		if (jumpCharge < 0.0f)
		{
			if (!Input.GetKeyDown(KeyCode.Space))
			{
				return;
			}
			jumpCharge = 0.0f;
		}
		if (Input.GetKey(KeyCode.Space) && jumpCharge < MaxChargeSeconds)
		{
			// Charging
			jumpCharge += Time.deltaTime;
			jumpCharge.ClampMax(MaxChargeSeconds);
			transform.RotateAround(SpearFront(), transform.right, -JumpRotationSpeed * Time.deltaTime);
			character.transform.position = CharacterStandPoint;
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			// Jump
			transform.RotateAround(SpearFront(), transform.right, JumpRotationSpeed * jumpCharge);
			character.Move(new Vector3(0.0f, CharacterStandPoint.y - character.transform.position.y, 0.0f));

			character.SetUpdateEnabled(true);
			character.Velocity = Math.Scale(transform.up, JumpForceOffsetXZ + (jumpCharge * JumpScalarXZ), JumpForceOffsetY + (jumpCharge * JumpScalarY));

			jumpCharge = -1.0f;
			character = null;
		}
	}

	private void DoThrownUpdate()
	{
		if (Physics.Raycast(SpearBack(), transform.forward, out RaycastHit hit, transform.localScale.z, HitLayer))
		{
			// Hit
			MoveTime = -2.0f;
			if (hit.collider.CompareTag("CanNotSpear"))
			{
				Recall();
			}
			else
			{
				transform.position = hit.point - (0.4f * transform.localScale.z * transform.forward);
			}
			if (hitCue != null)
				SOCue.Play(hitCue, new CueContext(hit.point));
			return;
		}

		// Moving
		MoveTime += Time.deltaTime;
		if (MoveTime >= GravityDelay)
		{
			Velocity.y += Gravity * Time.deltaTime;
		}
		transform.position += Velocity * Time.deltaTime;
		if (Velocity.NotNearZero())
			transform.rotation = Quaternion.LookRotation(Velocity);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (MoveTime > -2.0f || !other.TryGetComponent(out character))
		{
			return;
		}
		character.SetUpdateEnabled(false);
		Vector3 startPosition = character.transform.position;
		Anim.Play(Easing.Method.Sine, Easing.Direction.In, 0.35f,
		(float pProgress) =>
		{
			if (character != null)
				character.transform.position = Vector3.LerpUnclamped(character.transform.position, CharacterStandPoint, pProgress);
		});
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(SpearBack(), SpearFront());
	}
}
