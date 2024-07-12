using UnityEngine;

namespace ODev.Debug2
{
	public class GizmoSphere : GizmoBase
	{
		[SerializeField, Min(Util.Math.NEARZERO)]
		private float m_Radius = 1;

		protected override void DrawGizmos()
		{
			base.DrawGizmos();

			Gizmos.DrawWireSphere(transform.position, m_Radius);
		}
	}
}