using UnityEngine;
using Sirenix.OdinInspector;
using OCore.Util;

namespace OCore
{
	public class MoveBetweenPoints : MonoBehaviour
	{
		[System.Serializable]
		public class PointMotion
		{
			public Vector3 Point = new();
			public Vector3 Rotate = new();
			public float Seconds = 1.0f;
			public float Delay = 0.0f;
			public Easing.EaseParams Ease = new(Easing.Method.Sine, Easing.Direction.InOut);

			public void Play(Transform pTransform, Vector3 pToPoint, Vector3 pOffset, System.Action pOnComplete)
			{
				Anim.Play(Ease, Seconds,
				(float pProgress) => // OnTick
				{
					pTransform.position = Vector3.LerpUnclamped(Point, pToPoint, pProgress) + pOffset;
				},
				(_) => // OnComplete
				{
					pTransform.position = pToPoint + pOffset;
					pOnComplete.Invoke();
				}, Delay);
			}
		}

		[SerializeField, InfoBox("ERROR: Required 2 or more points.", InfoMessageType.Error, "@m_Points.Length < 2")]
		private PointMotion[] m_Points = new PointMotion[2];
		[SerializeField]
		private Transform m_MoveTransform = null;

		private Vector3 m_InitalPosition = new();

		private void Start()
		{
			m_InitalPosition = transform.position;
			m_MoveTransform.position = m_Points[0].Point + m_InitalPosition;
			PlayPoint(0);
		}

		private void PlayPoint(int pIndex)
		{
			int nextIndex = Math.Loop(pIndex + 1, m_Points.Length);
			m_Points[pIndex].Play(m_MoveTransform, m_Points[nextIndex].Point, m_InitalPosition, () => PlayPoint(nextIndex));
		}

		private void Reset()
		{
			m_MoveTransform = transform;
		}

		private void OnDrawGizmosSelected()
		{
			if (m_Points.Length < 2)
			{
				return;
			}

			Vector3 offset = Application.isPlaying ? m_InitalPosition : transform.position;

			Gizmos.color = Color.cyan;
			for (int i = 0; i < m_Points.Length; i++)
			{
				Gizmos.DrawCube(m_Points[i].Point + offset, Vector3.one * 0.1f);
				Gizmos.DrawLine(m_Points[i].Point + offset, m_Points[(i + 1).Loop(m_Points.Length)].Point + offset);
			}
		}
	}
}