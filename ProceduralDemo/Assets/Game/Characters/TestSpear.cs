using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;
using System;
using OliverLoescher.Cue;

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
	private float JumpScalar = 5.0f;
	[SerializeField]
	private float MaxJumpForce = 999.0f;

	[SerializeField]
	private SOCue hitCue;

	private Transform Camera = null;
	private TestThrow Thrower = null;
	private Vector3 Velocity;
	private float MoveTime = 0.0f;
	private Vector3 RecallStartPosition = Vector3.zero;
	private bool IsAnimating = false;
	private TestCharacter character;

	public void Init(Transform pCamera, TestThrow pThrow) { Camera = pCamera; Thrower = pThrow; }

	public void Aim()
	{
		transform.SetParent(Camera, false);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		MoveTime = -1.0f;
	}

	public void Throw()
	{
		transform.SetParent(null);
		Velocity = Camera.forward * Force;
		MoveTime = 0.0f;
	}

	public bool CanThrow() => !IsAnimating;

	public void Recall()
	{
		if (character != null)
		{
			character.SetUpdateEnabled(true);
			character = null;
		}
		transform.SetParent(null);
		MoveTime = -1.0f;
		RecallStartPosition = transform.position;
		IsAnimating = true;
		float seconds = Vector3.Distance(RecallStartPosition, Camera.position) * RecallSeconds;
		Anim.Play(RecallEase, Mathf.Min(seconds, 2.0f), RecallTick, RecallComplete);
	}

	private void RecallComplete(float pValue)
	{
		transform.position = Vector3.one * 5000.0f;
		IsAnimating = false;
		Thrower.OnRecallComplete();
	}

	private void RecallTick(float pProgress)
	{
		transform.position = Vector3.LerpUnclamped(RecallStartPosition, Camera.position, pProgress);
	}

	private float jumpCharge = 0.0f;
	private void Update()
	{
		if (character != null)
		{
			if (Input.GetKey(KeyCode.Space))
			{
				jumpCharge += Time.deltaTime;
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				character.SetUpdateEnabled(true);
				character.Velocity = Mathf.Min(jumpCharge * JumpScalar, MaxJumpForce) * Vector3.up;
				jumpCharge = 0.0f;
				character = null;
			}
			return;
		}

		if (MoveTime <= -1.0f)
		{
			return;
		}

		Vector3 from = transform.position + transform.forward;
		if (Physics.Raycast(from, transform.forward, out RaycastHit hit, transform.localScale.z * 0.5f, HitLayer))
		{
			MoveTime = -2.0f;
			transform.position = hit.point - (transform.forward * transform.localScale.z * 0.5f);
			hitCue?.Play(new CueContext(hit.point));
			return;
		}
		
		MoveTime += Time.deltaTime;
		if (MoveTime >= GravityDelay)
		{
			Velocity.y += Gravity * Time.deltaTime;
		}
		transform.position += Velocity * Time.deltaTime;
		if (Velocity.NotNearZero())
			transform.rotation = Quaternion.LookRotation(Velocity);
	}
	
	private void OnDrawGizmos()
	{
		Vector3 from = transform.position + transform.forward;
		Vector3 end = from + transform.forward * (transform.localScale.z * 0.5f);
		Gizmos.DrawLine(from, end);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (MoveTime > -2.0f || !other.TryGetComponent(out character))
		{
			return;
		}
		character.SetUpdateEnabled(false);
		Vector3 startPosition = character.transform.position;
		Anim.Play(Easing.Method.Sine, Easing.Direction.In, 0.2f,
		(float pProgress) => 
		{
			if (character != null)
				character.transform.position = Vector3.LerpUnclamped(character.transform.position, transform.position + (0.5f * transform.localScale.y * Vector3.up), pProgress);
		});
	}
}
