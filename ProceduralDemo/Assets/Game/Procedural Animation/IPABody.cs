using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPABody
{
	void Init(PARoot pRoot);
	void Tick(float pDeltaTime);
	void DrawGizmos();
}
