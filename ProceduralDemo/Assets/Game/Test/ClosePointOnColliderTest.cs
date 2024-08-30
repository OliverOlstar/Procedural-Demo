using UnityEngine;

public class ClosePointOnColliderTest : MonoBehaviour
{
	[Space, SerializeField]
	private Collider m_Target = null;

	public void OnDrawGizmos()
	{
		if (m_Target == null)
		{
			return;
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(m_Target.ClosestPointOnBounds(transform.position), 0.5f);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(m_Target.ClosestPoint(transform.position), 0.5f);
	}
}
