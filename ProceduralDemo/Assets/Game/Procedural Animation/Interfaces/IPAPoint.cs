using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPAPoint
{
	Vector3 Position { get; }
	Vector3 RelativeOriginalPosition { get; }
	
	void Init(IPACharacter pCharacter);
	void DrawGizmos();
}
