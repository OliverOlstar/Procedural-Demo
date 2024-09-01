
using UnityEngine;

public abstract class PlayerSpearController
{
	public PlayerSpear Spear { get; private set; }
	protected Transform Transform => Spear.transform;

	public abstract PlayerSpear.State State { get; }
	public abstract void Stop();

	public virtual void Setup(PlayerSpear pSpear) { Spear = pSpear; }
	public virtual void Tick(float pDeltaTime) { }
}
