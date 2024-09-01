
using System;
using System.Runtime.CompilerServices;
using ODev.Util;
using UnityEngine;

public abstract class PlayerSpearController
{
	internal PlayerSpear Spear { get; private set; }
	protected Transform Transform => Spear.transform;

	internal abstract PlayerSpear.State State { get; }
	internal abstract void Stop();

	internal virtual void Setup(PlayerSpear pSpear) { Spear = pSpear; }
	internal virtual void Tick(float pDeltaTime) { }
	internal virtual void DrawGizmos() { }

	protected void Log(string pMessage = "", [CallerMemberName] string pMethodName = "")
	{
		Spear.LogInternal(pMessage, pMethodName);
	}
}
