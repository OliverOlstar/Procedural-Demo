using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestThrow : MonoBehaviour
{
	private enum State
	{
		Has,
		Aiming,
		Thrown
	}

	[SerializeField]
	private GameObject AimCamera = null;
	[SerializeField]
	private TestSpear Spear = null;
	[SerializeField]
	private Transform SpearPivot = null;
	[SerializeField]
	private float AimTimeScale = 0.2f;

	private State state = State.Has;

	private void Start()
	{
		Spear.Init(SpearPivot, this);
	}

	void Update()
	{
		switch (state)
		{
			case State.Has:
				if (Spear.CanThrow() && Input.GetKeyDown(KeyCode.Mouse0))
				{
					SetState(State.Aiming);
				}
				break;

			case State.Aiming:
				if (Input.GetKeyUp(KeyCode.Mouse0))
				{
					SetState(State.Thrown);
				}
				break;

			case State.Thrown:
				if (Input.GetKeyDown(KeyCode.Mouse0))
				{
					SetState(State.Has);
				}
				break;
		}
	}

	private void SetState(State pToState)
	{
		switch (pToState)
		{
			case State.Has:
				AimCamera.SetActive(false);
				Spear.Recall();
				Time.timeScale = 1.0f;
				break;

			case State.Aiming:
				AimCamera.SetActive(true);
				Spear.Aim();
				Time.timeScale = AimTimeScale;
				break;

			case State.Thrown:
				AimCamera.SetActive(false);
				Spear.Throw();
				Time.timeScale = 1.0f;
				break;
		}
		state = pToState;
	}

	public void OnRecallComplete()
	{
		if (Spear.CanThrow() && Input.GetKey(KeyCode.Mouse0))
		{
			SetState(State.Aiming);
		}
		else
		{
			state = State.Has; // If not already, ensure set to has because recall finished
		}
	}
}
