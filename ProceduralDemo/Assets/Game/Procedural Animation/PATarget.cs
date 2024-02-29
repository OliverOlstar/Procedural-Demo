using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverLoescher.Util;
using RootMotion.FinalIK;
using OliverLoescher.Cue;
using Sirenix.OdinInspector;

public class PATarget : MonoBehaviour, IPAPoint
{
	public enum State
	{
		Idle = 0,
		Stepping,
		Falling
	}

	[DisableInPlayMode]
	public Vector3 TargetLocalOffset;
	[SerializeField]
	private float StepDistance = 1.0f;

	[Header("Animation")]
	[SerializeField]
	private Easing.EaseParams EaseStep;
	[SerializeField]
	private Easing.EaseParams EaseHeight;
	[SerializeField, Min(Math.NEARZERO)]
	private float StepSeconds = 0.35f;
	[SerializeField]
	private float UpHeight = 1.0f;

	[Header("References")]
	[SerializeField]
	private CCDIK IK;

	[Header("Linecast")]
	[SerializeField]
	private Vector2 LinecastUpDown = new Vector2(1, -1);
	[SerializeField]
	private LayerMask StepLayer = new LayerMask();

	[Header("Cues")]
	[SerializeField]
	private SOCue OnStepCue;

	public Vector3 CurrentPosition
	{
		get => IK.solver.IKPosition;
		set => IK.solver.IKPosition = value;
	}
	Vector3 IPAPoint.Position => CurrentPosition;
	Vector3 IPAPoint.RelativeOriginalPosition => TargetPosition;

	private PACharacter Character;
	private Vector3 StepOffset;
	private Vector3 TargetCharacterOffset;
	public State CurrentState { get; private set; } = State.Idle;

	public Vector3 TargetPosition => Character.TransformPoint(TargetCharacterOffset);

	void IPAPoint.Init(PACharacter pCharacter)
	{
		Character = pCharacter;
		TargetCharacterOffset = Character.InverseTransformPoint(TargetLocalOffset + transform.position);
	}

	public void TriggerMove()
	{
		StepOffset = CurrentPosition;
		CurrentState = State.Stepping;
		Anim.Play2D(EaseStep, EaseHeight, StepSeconds, StepTick, StepComplete);
	}

	public void TriggerFalling()
	{
		CurrentState = State.Falling;
	}

	private void StepTick(Vector2 pProgress)
	{
		Vector3 endStepPoint = CalculateStepPoint();
		Vector3 up = Character.Up * Math.LerpUnclamped(0, UpHeight, 0, pProgress.y);
		Vector3 horizontal = Vector3.LerpUnclamped(StepOffset, endStepPoint, pProgress.x);
		CurrentPosition = horizontal + up;
	}
	private void StepComplete(Vector2 _)
	{
		CurrentPosition = CalculateStepPoint();
		CurrentState = State.Idle;
		OnStepCue?.Play(new CueContext(CurrentPosition));
	}

	private Vector3 CalculateStepPoint()
	{
		Vector3 stepMotion = Character.MotionForward * -StepDistance;
		Vector3 stepEndPoint = stepMotion + TargetPosition;
		Vector3 upPoint = (LinecastUpDown.x * Character.Up) + stepEndPoint;
		Vector3 downPoint = (LinecastUpDown.y * Character.Up) + stepEndPoint;
		if (Physics.Linecast(upPoint, downPoint, out RaycastHit hit, StepLayer))
		{
			stepMotion = hit.point - TargetPosition;
		}
		return TargetPosition + stepMotion;
	}
	
	void IPAPoint.DrawGizmos()
	{
		// Points
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(CurrentPosition, 0.2f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(TargetPosition, 0.2f);
		Gizmos.DrawWireSphere(TargetPosition, StepDistance);

		// Linecast
		Vector3 stepMotion = Character.MotionForward * -StepDistance;
		Vector3 stepEndPoint = stepMotion + TargetPosition;
		Vector3 upPoint = (LinecastUpDown.x * Character.Up) + stepEndPoint;
		Vector3 downPoint = (LinecastUpDown.y * Character.Up) + stepEndPoint;
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(upPoint, downPoint);
	}
}
