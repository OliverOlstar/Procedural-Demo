
using System.Collections.Generic;
using UnityEngine;

namespace OCore
{
	public abstract class PoolInstance
	{
		protected Transform m_Transform = null;
		public Transform Transform => m_Transform;

		protected List<IPoolBehaviour> m_Interfaces = new();
		private List<IPoolUpdate> m_Updates = null;
		public bool RequiresUpdate() { return m_Updates != null && m_Updates.Count > 0; }
		private List<IPoolLateUpdate> m_LateUpdates = null;
		public bool RequiresLateUpdate() { return m_LateUpdates != null && m_LateUpdates.Count > 0; }

		private bool m_Active = false;
		private bool m_TryingToStop = false;

		private bool m_Teardown = false;
		public void Teardown() { m_Teardown = true; }

		public void Initialize(
			GameObject prefab,
			Vector3 position,
			Quaternion rotation,
			Transform parent,
			bool autoLifetime,
			Object owner)
		{
			GameObject obj = null;
			if (prefab.transform is RectTransform)
			{
				obj = Object.Instantiate(prefab, position, rotation);
				obj.transform.SetParent(parent, false);
			}
			else
			{
				obj = Object.Instantiate(prefab, position, rotation, parent);
			}
			obj.name = $"{prefab.name}({obj.GetInstanceID()})";
			m_Transform = obj.transform;

			// Passing 'true' in for include inactive, trying to help out by finding disabled interfaces
			// that might want to turn on an object... but maybe this will just cause bugs
			obj.GetComponentsInChildren(true, m_Interfaces);

			// Call play once all interfaces have been collected
			int count = m_Interfaces.Count;
			if (count > 0)
			{
				int firstOrder = (int)PoolOrder.First;
				int lastOrder = (int)PoolOrder.Last;
				for (int i = firstOrder; i <= lastOrder; i++)
				{
					for (int j = 0; j < count; j++)
					{
						IPoolBehaviour fxInterface = m_Interfaces[j];
						if ((int)fxInterface.ExecutionOrder == i)
						{
							fxInterface.OnInitialize();
						}
					}
				}
				for (int i = firstOrder; i <= lastOrder; i++)
				{
					for (int j = 0; j < count; j++)
					{
						IPoolBehaviour fxInterface = m_Interfaces[j];
						if ((int)fxInterface.ExecutionOrder == i)
						{
							fxInterface.OnPlay(owner);
						}
					}
				}
			}
			m_Updates = new List<IPoolUpdate>(m_Interfaces.Count);
			m_LateUpdates = new List<IPoolLateUpdate>(m_Interfaces.Count);
			for (int i = 0; i < m_Interfaces.Count; i++)
			{
				IPoolBehaviour behaviour = m_Interfaces[i];
				if (behaviour is IPoolUpdate update)
				{
					m_Updates.Add(update);
				}
				if (behaviour is IPoolLateUpdate lateUpdate)
				{
					m_LateUpdates.Add(lateUpdate);
				}
			}
			OnInitialize(prefab);
			// If our lifetime is not automatic then someone has to stop us
			m_Active = true;
			m_TryingToStop = autoLifetime;
		}

		public bool IsValid()
		{
			return m_Transform != null;
		}

		public bool IsActive()
		{
			if (!m_Active)
			{
				return false;
			}
			if (!IsValid())
			{
				m_Active = false;
				return false;
			}
			if (!m_TryingToStop)
			{
				return true;
			}
			if (IsActiveInternal())
			{
				return true;
			}
			int count = m_Interfaces.Count;
			for (int i = 0; i < count; i++)
			{
				IPoolBehaviour fxInterface = m_Interfaces[i];
				if (fxInterface == null)
				{
					Debug.LogError("FXInstance.IsActive() " + this + " one of our animators has gone null");
					continue;
				}
				if (fxInterface.KeepAlive())
				{
					return true;
				}
			}
			m_Active = false;
			return false;
		}

		public void Start(
			Vector3 position,
			Quaternion rotation,
			Transform parent,
			bool autoLifetime,
			Object owner)
		{
			m_Active = true;
			m_TryingToStop = autoLifetime;
			// It seems for rect transform we need to assign a parent and then set the position
			m_Transform.SetParent(parent, m_Transform as RectTransform == null);
			m_Transform.SetPositionAndRotation(position, rotation);
			m_Transform.gameObject.SetActive(true);
			int count = m_Interfaces.Count;
			if (count > 0)
			{
				int firstOrder = (int)PoolOrder.First;
				int lastOrder = (int)PoolOrder.Last;
				for (int i = firstOrder; i <= lastOrder; i++)
				{
					for (int j = 0; j < count; j++)
					{
						IPoolBehaviour fxInterface = m_Interfaces[j];
						if (fxInterface == null)
						{
							Debug.LogWarning(this + " FXInterface was destroyed, why?");
							continue;
						}
						if ((int)fxInterface.ExecutionOrder != i)
						{
							continue;
						}
						fxInterface.OnReset();
					}
				}
				for (int i = firstOrder; i <= lastOrder; i++)
				{
					for (int j = 0; j < count; j++)
					{
						IPoolBehaviour fxInterface = m_Interfaces[j];
						if (fxInterface == null)
						{
							continue;
						}
						if ((int)fxInterface.ExecutionOrder != i)
						{
							continue;
						}
						fxInterface.OnPlay(owner);
					}
				}
			}
			OnStart();
		}

		public void Stop(bool kill = false)
		{
			if (m_Teardown)
			{
				return;
			}
			if (kill)
			{
				m_Active = false;
			}
			else
			{
				m_TryingToStop = true;
			}
			for (int i = 0; i < m_Interfaces.Count; i++)
			{
				IPoolBehaviour fxInterface = m_Interfaces[i];
				if (fxInterface != null)
				{
					fxInterface.OnStop(kill);
				}
				else
				{
					Debug.LogWarning(this + " FXInterface was destroyed, why?");
				}
			}
			OnStop(kill);
			if (kill && m_Transform != null)
			{
				m_Transform.gameObject.SetActive(false);
			}
		}

		public void Update()
		{
			int count = m_Updates.Count;
			for (int i = 0; i < count; i++)
			{
				IPoolUpdate update = m_Updates[i];
				if (update != null)
				{
					update.OnUpdate();
				}
			}
		}

		public void LateUpdate()
		{
			int count = m_LateUpdates.Count;
			for (int i = 0; i < count; i++)
			{
				IPoolLateUpdate update = m_LateUpdates[i];
				if (update != null)
				{
					update.OnLateUpdate();
				}
			}
		}

		public override string ToString()
		{
			return m_Transform != null ? m_Transform.name : "InvalidFXInstance";
		}

		public T GetFXInterface<T>() where T : IPoolBehaviour
		{
			foreach (IPoolBehaviour fxInterface in m_Interfaces)
			{
				if (fxInterface is T interfaceOfType)
				{
					return interfaceOfType;
				}
			}
			return default;
		}

		protected abstract bool IsActiveInternal();
		protected abstract void OnInitialize(GameObject prefab);
		protected abstract void OnStart();
		protected abstract void OnStop(bool kill);
	}
}
