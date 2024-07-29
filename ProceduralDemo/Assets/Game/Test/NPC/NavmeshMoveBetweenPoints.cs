using System.Collections;
using System.Collections.Generic;
using ODev.Util;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshMoveBetweenPoints : MonoBehaviour
{
	[System.Serializable]
	public class PointMotion
	{
		public Vector3 Point = new();
		public float Delay = 0.0f;

		public IEnumerator Play(NavMeshAgent pAgent, Vector3 pToPoint, Vector3 pPointOffset, System.Action pOnComplete)
		{
			yield return new WaitForSeconds(Delay);

			pAgent.SetDestination(pToPoint + pPointOffset);

			while ((pAgent.transform.position - (pToPoint + pPointOffset)).sqrMagnitude > 1.5f * 1.5f)
			{
				yield return new WaitForSeconds(0.5f);
			}

			pOnComplete?.Invoke();
		}
	}

	[SerializeField]
	private PointMotion[] m_Points = new PointMotion[2];
	[SerializeField]
	private NavMeshAgent m_Agent = null;

	private Vector3 m_InitalPosition = new();

	private void Start()
	{
		m_InitalPosition = transform.position;
		PlayPoint(0);
	}

	private void PlayPoint(int pIndex)
	{
		int nextIndex = Math.Loop(pIndex + 1, m_Points.Length);
		StartCoroutine(m_Points[pIndex].Play(m_Agent, m_Points[nextIndex].Point, m_InitalPosition, () => PlayPoint(nextIndex)));
	}

	private void Reset()
	{
		m_Agent = GetComponentInChildren<NavMeshAgent>();
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
			Gizmos.DrawLine(m_Points[i].Point + offset, m_Points[(i + 1).Loop(m_Points.Length)].Point + offset);
			Gizmos.DrawCube(m_Points[i].Point + offset, Vector3.one * 0.5f);
		}
	}
}
