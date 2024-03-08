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

	private State state = State.Has;

	private void Start()
	{
		Spear.Init(SpearPivot);
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
				break;

			case State.Aiming:
				AimCamera.SetActive(true);
				Spear.Aim();
				break;

			case State.Thrown:
				AimCamera.SetActive(false);
				Spear.Throw();
				break;
		}
		state = pToState;
	}
}
