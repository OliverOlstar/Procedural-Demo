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
	[SerializeField]
	private float TimeScaleSmoothTime = 0.2f;

	private State state = State.Has;
	private float timeScaleVelocity = 0.0f;

	private void Start()
	{
		Spear.Init(SpearPivot, this);
	}

	void Update()
	{
		bool isAiming = Input.GetKey(KeyCode.Mouse0) && (state == State.Has || state == State.Aiming);
		AimCamera.SetActive(isAiming);
		Time.timeScale = Mathf.SmoothDamp(Time.timeScale, isAiming ? AimTimeScale : 1.0f, ref timeScaleVelocity, TimeScaleSmoothTime);
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
				Spear.Recall();
				Time.timeScale = 1.0f;
				break;

			case State.Aiming:
				Spear.Aim();
				
				break;

			case State.Thrown:
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
