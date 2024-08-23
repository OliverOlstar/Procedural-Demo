using UnityEngine;

namespace ODev
{
	public class PoolElement : MonoBehaviour
	{
		private string m_PoolKey = string.Empty;
		public string PoolKey => string.IsNullOrEmpty(m_PoolKey) ? gameObject.name : m_PoolKey;
		public Transform Parent { get; private set; } = null;

		public virtual void Init(string pPoolKey, Transform pParent)
		{
			m_PoolKey = pPoolKey;
			Parent = pParent;
		}

		public virtual void ReturnToPool()
		{
			ObjectPoolDictionary.Return(this);
		}

		public virtual void OnExitPool()
		{

		}

		private void OnDestroy()
		{
			ObjectPoolDictionary.ObjectDestroyed(gameObject, this);
		}
	}
}