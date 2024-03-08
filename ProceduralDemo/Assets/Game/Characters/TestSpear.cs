using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;
using System;

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

	private Transform Camera = null;
	private Vector3 Velocity;
	private float MoveTime = 0.0f;
	private Vector3 RecallStartPosition = Vector3.zero;
	private bool IsAnimating = false;

	public void Init(Transform pCamera) => Camera = pCamera;

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
		transform.SetParent(null);
		MoveTime = 0.0f;
		RecallStartPosition = transform.position;
		IsAnimating = true;
		Anim.Play(RecallEase, Vector3.Distance(RecallStartPosition, Camera.position) * RecallSeconds, RecallTick, RecallComplete);
	}

	private void RecallComplete(float pValue)
	{
		transform.position = Vector3.one * 5000.0f;
		IsAnimating = false;
	}

	private void RecallTick(float pProgress)
	{
		transform.position = Vector3.LerpUnclamped(RecallStartPosition, Camera.position, pProgress);
	}

	private void Update()
	{
		if (MoveTime <= -1.0f)
		{
			return;
		}

		Vector3 from = transform.position + transform.forward;
		if (Physics.Raycast(from, transform.forward, out RaycastHit hit, transform.localScale.z * 0.5f, HitLayer))
		{
			MoveTime = -1.0f;
			transform.position = hit.point - (transform.forward * transform.localScale.z * 0.5f);
			// transform.forward = hit.normal;
			return;
		}
		
		MoveTime += Time.deltaTime;
		if (MoveTime >= GravityDelay)
		{
			Velocity.y += Gravity * Time.deltaTime;
		}
		transform.position += Velocity * Time.deltaTime;
		transform.rotation = Quaternion.LookRotation(Velocity);
	}
	
	private void OnDrawGizmos()
	{
		Vector3 from = transform.position + transform.forward;
		Vector3 end = from + transform.forward * (transform.localScale.z * 0.5f);
		Gizmos.DrawLine(from, end);
	}
}
