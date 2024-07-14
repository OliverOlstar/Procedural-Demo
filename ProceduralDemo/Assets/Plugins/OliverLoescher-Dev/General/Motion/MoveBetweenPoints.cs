using UnityEngine;
using Sirenix.OdinInspector;
using ODev.Util;

namespace ODev
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
			public Easing.EaseParams PointEase = new(Easing.Method.Sine, Easing.Direction.InOut);
			public Easing.EaseParams RotateEase = new(Easing.Method.Sine, Easing.Direction.InOut);

			public void Play(Transform pTransform, Vector3 pToPoint, Vector3 pPointOffset, Vector3 pToEuler, Quaternion pRotationOffset, System.Action pOnComplete)
			{
				Anim.Play2D(PointEase, RotateEase, Seconds, Anim.Type.Physics,
				(Vector2 pProgress) => // OnTick
				{
					pTransform.SetPositionAndRotation(
						Vector3.LerpUnclamped(Point, pToPoint, pProgress.x) + pPointOffset, 
						Quaternion.Euler(Vector3.LerpUnclamped(Rotate, pToEuler, pProgress.y)) * pRotationOffset);
				},
				(_) => // OnComplete
				{
					pTransform.position = pToPoint + pPointOffset;
					pOnComplete.Invoke();
				}, Delay);
			}
		}

		[SerializeField, InfoBox("ERROR: Required 2 or more points.", InfoMessageType.Error, "@m_Points.Length < 2")]
		private PointMotion[] m_Points = new PointMotion[2];
		[SerializeField]
		private Transform m_MoveTransform = null;

		private Vector3 m_InitalPosition = new();
		private Quaternion m_InitalRotation = new();

		private void Start()
		{
			m_InitalPosition = transform.position;
			m_InitalRotation = transform.rotation;
			m_MoveTransform.position = m_Points[0].Point + m_InitalPosition;
			PlayPoint(0);
		}

		private void PlayPoint(int pIndex)
		{
			int nextIndex = Math.Loop(pIndex + 1, m_Points.Length);
			m_Points[pIndex].Play(m_MoveTransform, m_Points[nextIndex].Point, m_InitalPosition, m_Points[nextIndex].Rotate, m_InitalRotation, () => PlayPoint(nextIndex));
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
			Quaternion rotation = Application.isPlaying ? m_InitalRotation : transform.rotation;

			Gizmos.color = Color.cyan;
			for (int i = 0; i < m_Points.Length; i++)
			{
				Gizmos.DrawLine(m_Points[i].Point + offset, m_Points[(i + 1).Loop(m_Points.Length)].Point + offset);
				if (TryGetComponent(out MeshFilter mesh))
				{
					Gizmos.DrawMesh(mesh.sharedMesh, 0, m_Points[i].Point + offset, Quaternion.Euler(m_Points[i].Rotate) * rotation, transform.localScale);
					return;
				}
				Gizmos.DrawCube(m_Points[i].Point + offset, Vector3.one * 0.1f);
			}
		}
	}
}