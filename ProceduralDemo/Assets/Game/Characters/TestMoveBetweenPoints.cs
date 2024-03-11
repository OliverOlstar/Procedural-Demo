using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OliverLoescher.Util;
using UnityEngine;

public class TestMoveBetweenPoints : MonoBehaviour, IPACharacter
{
	[System.Serializable]
	protected struct MoveTo
	{
		public Vector3 Point;
		public float Seconds;
	}

    [SerializeField]
	private Easing.EaseParams Ease;
	[SerializeField]
	private MoveTo[] Points = new MoveTo[2];

	private int pCurrIndex = 0;
	private int pNextIndex = 1;
	
	Vector3 IPACharacter.Up => transform.up;
	Vector3 IPACharacter.Position => transform.position;
	Vector3 IPACharacter.Forward => transform.forward;
	Vector3 IPACharacter.TransformPoint(in Vector3 pVector) => transform.TransformPoint(pVector);
	Vector3 IPACharacter.InverseTransformPoint(in Vector3 pVector) => transform.InverseTransformPoint(pVector);
	Vector3 IPACharacter.MotionForward => transform.forward;

	private void Start()
	{
		Anim.Play(Ease, Points[pCurrIndex].Seconds, OnTick, OnComplete);
	}

	private void OnTick(float pProgress)
	{
		transform.position = Points[pCurrIndex].Point;
		transform.position = Vector3.Lerp(Points[pCurrIndex].Point, Points[pNextIndex].Point, pProgress);
	}

	private void OnComplete(float _)
	{
		pCurrIndex = pNextIndex;
		pNextIndex++;
		if (pNextIndex >= Points.Length)
		{
			pNextIndex = 0;
		}
		Start();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		for (int i = 0; i < Points.Length; i++)
		{
			Gizmos.DrawWireSphere(Points[i].Point, 0.5f);
			Gizmos.DrawLine(Points[i].Point, Points[i + 1 >= Points.Length ? 0 : i + 1].Point);
		}
	}
}
