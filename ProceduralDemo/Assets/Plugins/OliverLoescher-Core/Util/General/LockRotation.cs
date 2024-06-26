using UnityEngine;

public class LockRotation : MonoBehaviour
{
	public bool LockXRotation = false;
	public bool LockYRotation = false;
	public bool LockZRotation = false;
	Vector3 m_InitialRotation;

	void Start() 
	{
		m_InitialRotation = transform.eulerAngles;
	}

	void DoLockRotation()
	{
		if (!LockXRotation && !LockYRotation && !LockZRotation)
		{
			return;
		}
		Vector3 currentRotation = transform.eulerAngles;
		transform.eulerAngles = new Vector3(LockXRotation ? m_InitialRotation.x : currentRotation.x, LockYRotation ? m_InitialRotation.y : currentRotation.y, LockZRotation ? m_InitialRotation.z : currentRotation.z);
	}

	private void FixedUpdate() 
	{
		DoLockRotation();
	}

	private void Update() 
	{
		DoLockRotation();
	}
}