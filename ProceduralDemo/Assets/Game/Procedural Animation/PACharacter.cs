using System.Collections;
using System.Collections.Generic;
using OliverLoescher.Util;
using UnityEngine;
using Sirenix.OdinInspector;

public class PACharacter : MonoBehaviour, IPACharacter
{
	[SerializeField, DisableInPlayMode]
	private OliverLoescher.Util.Mono.Updateable Updateable = new OliverLoescher.Util.Mono.Updateable(OliverLoescher.Util.Mono.Type.Fixed, OliverLoescher.Util.Mono.Priorities.CharacterController);

	Vector3 IPACharacter.Up => transform.up;
	Vector3 IPACharacter.Position => transform.position;
	Vector3 IPACharacter.Forward => transform.forward;
	Vector3 IPACharacter.TransformPoint(in Vector3 pVector) => transform.TransformPoint(pVector);
	Vector3 IPACharacter.InverseTransformPoint(in Vector3 pVector) => transform.InverseTransformPoint(pVector);

	public Vector3 MotionForward { get; private set; } = Vector3.forward; // IPACharacter.MotionForward { get; }
	public Vector3 Motion { get; private set; } = Vector3.zero;

	Quaternion IPACharacter.Rotation => transform.rotation;
	Vector3 IPACharacter.Veclocity => Motion;

	[Header("Motion")]
	[SerializeField]
	private CharacterController Controller;
	[SerializeField]
	private float Speed = 10.0f;
	
	[Header("Rotation")]
	[SerializeField]
	private float RotationDampening = 4.0f;
	[SerializeField]
	private float MotionForwardDampening = 5.0f;

	private void Start()
	{
		Updateable.Register(Tick);
	}
	private void OnDestroy()
	{
		Updateable.Deregister();
	}

	private void Tick(float pDeltaTime)
	{
		Vector3 input = ((Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)) * Math.Horizontalize(OliverLoescher.MainCamera.Camera.transform.forward);
		input += ((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0)) * Math.Horizontalize(OliverLoescher.MainCamera.Camera.transform.right);
		
		float sqrMag = input.sqrMagnitude;
		if (sqrMag > 1.0f)
		{
			input.Normalize();
		}

		if (sqrMag > 0.01f)
		{
			Motion = input * Speed;
			Vector3 clampedMotion = Vector3.ClampMagnitude(Motion, 1.0f);
			MotionForward = Vector3.Lerp(MotionForward, clampedMotion, pDeltaTime * MotionForwardDampening);

			Controller.SimpleMove(Motion * pDeltaTime);
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(clampedMotion), pDeltaTime * RotationDampening);
		}
		else
		{
			Motion = Vector3.zero;
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 point = transform.position + Vector3.up;
		Gizmos.DrawLine(point, point + MotionForward);
	}
}
