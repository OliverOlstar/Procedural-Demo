using UnityEngine;

namespace Core
{
	public class MonoDirectorBehaviour : MonoBehaviour
	{
		protected virtual void Awake()
		{

		}

		protected virtual void OnEnable()
		{
			MonoDirector.Register(this);
		}

		protected virtual void OnDisable()
		{
			MonoDirector.Deregister(this);
		}
	}
}

