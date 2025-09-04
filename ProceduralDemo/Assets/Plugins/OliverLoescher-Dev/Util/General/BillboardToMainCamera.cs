using UnityEngine;

public class BillboardToMainCamera : MonoBehaviour
{
	public enum Style
	{
		FaceCamera,
		MatchDirection
	}

	private Transform m_MyCamera = null;
	[SerializeField]
	private Style m_Style = Style.MatchDirection;
	[SerializeField]
	private bool m_LockY = false;

	void Start()
	{
		m_MyCamera = Camera.main.transform;
	}

	void LateUpdate()
	{
		Vector3 direction;

		switch (m_Style)
		{
			case Style.FaceCamera:
				direction = transform.position - m_MyCamera.transform.position;
				break;
			case Style.MatchDirection:
				direction = m_MyCamera.transform.forward;
				break;
			default:
				throw new System.NotImplementedException();
		}

		if (m_LockY)
		{
			direction.y = 0;
		}
		if (direction != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(direction);
		}
	}
}
