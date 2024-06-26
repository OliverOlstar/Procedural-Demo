using UnityEngine;

namespace OCore.Debug2
{
	public class GizmoForward : GizmoBase
	{
		[SerializeField]
		private float m_Magnitude = 1.0f;
		[SerializeField]
		private float m_UpOffset = 0.0f;

		protected override void DrawGizmos()
		{
			base.DrawGizmos();

			Vector3 root = transform.position + (transform.up * m_UpOffset);
			Vector3 end = root + (transform.forward * m_Magnitude);
			Gizmos.DrawLine(root, end);
		}
	}
}