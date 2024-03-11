using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PADummyPoint : MonoBehaviour, IPAPoint
{
	Vector3 IPAPoint.Position => transform.position;
	Vector3 IPAPoint.RelativeOriginalPosition => transform.position;
	void IPAPoint.Init(IPACharacter _) { }
	void IPAPoint.DrawGizmos() { }
}
