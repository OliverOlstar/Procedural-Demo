using System.Collections;
using System.Collections.Generic;
using OliverLoescher;
using OliverLoescher.Util;
using UnityEngine;

public class TestCharacter : MonoBehaviour
{
	[SerializeField]
	private float Speed = 10.0f;
	[SerializeField]
	private float Drag = 1.0f;
	[SerializeField]
	private float AirSpeed = 5.0f;
	[SerializeField]
	private float AirDrag = 0.1f;
	[SerializeField]
	private float Jump = 10.0f;
	[SerializeField]
	private float Gravity = -9.81f;
	[SerializeField]
	private float TerminalVelocity = -10.0f;

	private Vector3 Velocity = Vector3.zero;

	[Header("Climb")]
	[SerializeField]
	private LayerMask ClimbLayer;
	[SerializeField]
	private float ClimbDistance = 2.0f;

	[Header("References")]
	[SerializeField]
	private CharacterController Controller = null;
	[SerializeField]
	private OnGround Grounded = null;

	public void Update()
	{
		Vector3 input = ((Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)) * Math.Horizontalize(MainCamera.Camera.transform.forward);
		input += ((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0)) * Math.Horizontalize(MainCamera.Camera.transform.right);

		float sqrMag = input.sqrMagnitude;
		if (sqrMag > 1.0f)
		{
			input.Normalize();
		}

		float speed = Grounded.isGrounded ? Speed : AirSpeed;
		Velocity += speed * input * Time.deltaTime;
		
		float drag = Grounded.isGrounded ? Drag : AirDrag;
		Velocity -= Math.Horizontal(Velocity) * drag * Time.deltaTime;

		if (Grounded.isGrounded)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Velocity.y = Jump;
			}
		}
		else
		{
			Velocity.y += Gravity * Time.deltaTime;
			Velocity.y = Mathf.Max(Velocity.y, TerminalVelocity);
		}

		Controller.Move(Velocity * Time.deltaTime);

		if (sqrMag > 0.01f && !Grounded.isGrounded && Physics.SphereCast(transform.position + Controller.center, Controller.radius, input.normalized, out RaycastHit hit, ClimbDistance, ClimbLayer))
		{
			if (Physics.SphereCast(hit.point + Vector3.up * 2, Controller.radius, Vector3.down, out RaycastHit hit2, 4, ClimbLayer))
			{
				Velocity = Vector3.zero;
				transform.position = hit2.point;
			}
		}
	}
}
