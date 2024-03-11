using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OliverLoescher
{
	public abstract class UpdateableMonoBehaviour : MonoBehaviour
	{
		[SerializeField]
		private Util.Mono.Updateable updateable = new Util.Mono.Updateable(Util.Mono.UpdateType.Default, Util.Mono.Priorities.Default);

		protected virtual void Start() => SetUpdateEnabled(true);
		// protected virtual void OnDestroy() => SetUpdateEnabled(false);
		// protected virtual void OnEnable() => SetUpdateEnabled(true);
		protected virtual void OnDisable() => SetUpdateEnabled(false);

		protected abstract void Tick(float pDeltaTime);

		public void SetUpdateEnabled(bool pEnabled)
		{
			if (pEnabled)
			{
				updateable.Register(Tick);
			}
			else
			{
				updateable.Deregister();
			}
		}
	}
}
