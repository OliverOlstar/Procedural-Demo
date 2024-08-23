using UnityEngine;

namespace ODev
{
	public abstract class UpdateableMonoBehaviour : MonoBehaviour
	{
		[SerializeField]
		private Util.Mono.Updateable m_Updateable = new(Util.Mono.Type.Default, Util.Mono.Priorities.Default);

		protected virtual void OnEnable() => SetUpdateEnabled(true);
		// protected virtual void OnDestroy() => SetUpdateEnabled(false);
		// protected virtual void OnEnable() => SetUpdateEnabled(true);
		protected virtual void OnDisable() => SetUpdateEnabled(false);

		protected abstract void Tick(float pDeltaTime);

		public void SetUpdateEnabled(bool pEnabled)
		{
			if (pEnabled)
			{
				m_Updateable.Register(Tick);
			}
			else
			{
				m_Updateable.Deregister();
			}
		}
	}
}
