using ODev;
using UnityEngine;

public class SpearSingleton : MonoBehaviourSingleton<SpearSingleton>
{
	public enum State
	{
		Stored,
		Flying,
		
	}
}
