using UnityEngine;

namespace ODev.Debug2
{
	public abstract class GizmoBase : MonoBehaviour
	{
		[SerializeField]
		private Color m_Colour = new(0.0f, 0.5f, 1.0f, 1.0f);
		[SerializeField]
		private bool m_AlwaysShow = false;

		private void Awake()
		{
			if (!Application.isEditor)
			{
				Debug.LogWarning($"This {GetType()} exist, destory it. Please clean these up. {Util.Debug.GetPath(transform)}");
				DestroyImmediate(this);
			}
		}

		protected virtual void OnDrawGizmos()
		{
			if (m_AlwaysShow)
			{
				DrawGizmos();
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			if (!m_AlwaysShow)
			{
				DrawGizmos();
			}
		}

		protected virtual void DrawGizmos()
		{
			Gizmos.color = m_Colour;
		}
	}
}