using UnityEngine;

public class SpringDamperTest : MonoBehaviour
{
	[SerializeField]
	private float Spring = 1.0f;
	[SerializeField]
	private float Damper = 10.0f;

	[Space, SerializeField]
	private Transform Target = null;

	private Vector3 Velocity;

	public void Update()
	{
		transform.position = OCore.Util.Func.SpringDamper(transform.position, Target.position, ref Velocity, Spring, Damper, Time.deltaTime);
	}
}
