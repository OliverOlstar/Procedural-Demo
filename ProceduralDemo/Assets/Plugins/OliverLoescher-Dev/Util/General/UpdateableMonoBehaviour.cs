using ODev.Updateables;
using UnityEngine;

namespace ODev.Update
{
	public abstract class UpdateableMonoBehaviour : MonoBehaviour
	{
		[SerializeField]
		private Updateable m_Updateable = new(UpdateableType.Default, UpdateablePriority.Default);

		protected virtual void OnEnable() => SetUpdateEnabled(true);
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
