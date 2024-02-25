
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class PoolLifetime
	{
		PoolInstance m_Instance = null;
		float m_TimeStamp = 0.0f;
		bool m_KillOnEnd = false;

		public PoolLifetime(PoolInstance instance, float lifetime, bool killOnEnd)
		{
			m_Instance = instance;
			m_TimeStamp = Time.time + lifetime;
			m_KillOnEnd = killOnEnd;
		}

		public bool Update()
		{
			if (Time.time > m_TimeStamp)
			{
				if (m_Instance != null)
				{
					m_Instance.Stop(m_KillOnEnd);
				}
				return false;
			}
			return true;
		}
	}

	public interface IPoolUpdate
	{
		void OnUpdate();
	}

	public interface IPoolLateUpdate
	{
		void OnLateUpdate();
	}

	public interface IPoolDirector
	{

	}

	public abstract class PoolDirectorBase : IDirector
	{
		public abstract void OnCreate();

		public abstract void OnDestroy();

		public abstract Dictionary<int, string>.KeyCollection GetIDs();
		public abstract void GetDebugItems(int key, out string name, List<PoolInstance> instances);
	}

	public abstract class PoolDirector<TInstance, TSingleton> : PoolDirectorBase, IUpdatable, ILateUpdatable
		where TInstance : PoolInstance, new()
		where TSingleton : PoolDirector<TInstance, TSingleton>, IDirector, new()
	{
		public static readonly int MAX_INSTANCES = 30;

		public static bool Exists() { return Director.Exists<TSingleton>(); }
		public static TSingleton Instance => Director.GetOrCreate<TSingleton>();

		private Dictionary<int, PoolList<TInstance>> m_Instances = new Dictionary<int, PoolList<TInstance>>();
		private Dictionary<int, string> m_DebugPrefabNames = new Dictionary<int, string>();
		private PoolList<TInstance> m_UpdateInstances = new PoolList<TInstance>(100);
		private PoolList<TInstance> m_LateUpdateInstances = new PoolList<TInstance>(100);
		private PoolList<PoolLifetime> m_Lifetimes = new PoolList<PoolLifetime>(30);
		Transform m_Transform = null;

		public PoolDirector() { }

		public override void OnCreate()
		{
			Chrono.Register(this);
			Chrono.RegisterLate(this);
		}

		public override void OnDestroy()
		{
			Chrono.Deregister(this);
			Chrono.DeregisterLate(this);
			TeardownInternal();
		}

		public Transform Transform
		{
			get
			{
				if (m_Transform == null)
				{
					m_Transform = new GameObject(GetType().Name).transform;
					if (PersistAcrossScenes)
					{
						Object.DontDestroyOnLoad(m_Transform.gameObject);
					}
				
				}
				return m_Transform;
			}
		}

		public static void ProtectInstancesFromParentDestruction()
		{
			TSingleton manager = Director.GetOrCreate<TSingleton>();
			foreach (PoolList<TInstance> instances in manager.m_Instances.Values)
			{
				foreach (TInstance instance in instances)
				{
					if (instance != null)
					{
						if (instance.Transform == null)
						{
							continue;
						}
						if (instance.IsActive())
						{
							instance.Stop(true);
						}
						instance.Transform.SetParent(manager.Transform, false);
					}
				}
			}
		}

		public static TInstance Spawn(
			GameObject prefab,
			Transform locator,
			bool child,
			bool autoLifetime,
			UnityEngine.Object owner = null)
		{
			return Spawn(prefab, locator.position, locator.rotation, child ? locator : null, autoLifetime, owner);
		}

		public static TInstance Spawn(
			GameObject prefab,
			Transform parent,
			Vector3 offsetPosition,
			Quaternion offsetOrientation,
			bool autoLifetime,
			UnityEngine.Object owner = null)
		{
			if (parent == null)
			{
				return Spawn(prefab, offsetPosition, offsetOrientation, null, autoLifetime, owner);
			}
			else
			{
				Vector3 pos = (parent.rotation * offsetPosition) + parent.position;
				Quaternion rot = parent.rotation * offsetOrientation;
				return Spawn(prefab, pos, rot, parent, autoLifetime, owner);
			}
		}

		public static bool Exists(GameObject prefab)
		{
			TSingleton manager = Director.GetOrCreate<TSingleton>();
			return manager.m_Instances.ContainsKey(prefab.GetInstanceID());
		}

		public static TInstance Spawn(
			GameObject prefab,
			Vector3 position,
			Quaternion rotation,
			Transform parent,
			bool autoLifetime,
			UnityEngine.Object owner = null)
		{
			if (prefab == null)
			{
				return null;
			}

			TSingleton manager = Director.GetOrCreate<TSingleton>();
			PoolList<TInstance> instances = null;
			if (!manager.m_Instances.TryGetValue(prefab.GetInstanceID(), out instances))
			{
				instances = new PoolList<TInstance>(MAX_INSTANCES);
				int id = prefab.GetInstanceID();
				manager.m_Instances.Add(id, instances);
				
				#if DEBUG
				manager.m_DebugPrefabNames.Add(id, prefab.name);
				#endif
			}

			if (parent == null)
			{
				parent = manager.Transform;
			}

			// See if there is an instance we can re-use
			TInstance instance = null;
			int count = instances.Count;
			for (int i = 0; i < count; i++)
			{
				TInstance inst = instances[i];
				if (!inst.IsValid())
				{
					Debug.LogWarning(manager.GetType().Name + ".Instantiate() Re-initializing " + prefab.name + " instance, ideally instances should never be invalidated");
					inst.Initialize(
						prefab,
						position,
						rotation,
						parent,
						autoLifetime,
						owner);
					instance = inst;
					break;
				}
				else if (!inst.IsActive())
				{
					instance = inst;
					instance.Start(
						position,
						rotation,
						parent,
						autoLifetime, 
						owner);
					break;
				}
			}

			if (instance == null)
			{
				// Create a new instance
				// It's important to instantiate the prefab in location, some Particle Systems features need
				// to be in the correct position when Awake() gets called
				instance = new TInstance();
				instance.Initialize(
					prefab,
					position,
					rotation,
					parent,
					autoLifetime,
					owner);
				instances.Add(instance);
				if (instance.RequiresUpdate())
				{
					manager.m_UpdateInstances.Add(instance);
				}
				if (instance.RequiresLateUpdate())
				{
					manager.m_LateUpdateInstances.Add(instance);
				}
			}

			return instance;
		}

		public static void SetDuration(TInstance instance, float duration, bool killOnEnd = false)
		{
			TSingleton manager = Instance;
			manager.m_Lifetimes.Add(new PoolLifetime(instance, duration, killOnEnd));
		}

		public double DeltaTime => 0d;

		public void OnRegistered() { }

		public void OnDeregistered() { }

		public void OnUpdate(double deltaTime)
		{
			int lifeTimeCount = m_Lifetimes.Count;
			for (int i = 0; i < lifeTimeCount; i++)
			{
				PoolLifetime lifeTime = m_Lifetimes[i];
				if (lifeTime != null && !lifeTime.Update())
				{
					m_Lifetimes.RemoveAt(i);
				}
			}
			int count = m_UpdateInstances.Count;
			for (int i = 0; i < count; i++)
			{
				TInstance instance = m_UpdateInstances[i];
				if (instance.IsActive())
				{
					instance.Update();
				}
			}
		}

		public void OnLateUpdate(double deltaTime)
		{
			int count = m_LateUpdateInstances.Count;
			for (int i = 0; i < count; i++)
			{
				TInstance instance = m_LateUpdateInstances[i];
				if (instance.IsActive())
				{
					instance.LateUpdate();
				}
			}
		}

		public override Dictionary<int, string>.KeyCollection GetIDs() { return m_DebugPrefabNames.Keys; }
		public override void GetDebugItems(int key, out string name, List<PoolInstance> instances)
		{
			m_DebugPrefabNames.TryGetValue(key, out name);
			m_Instances.TryGetValue(key, out PoolList<TInstance> list);
			instances.Clear();
			foreach (TInstance instance in list)
			{
				if (instance != null)
				{
					instances.Add(instance);
				}
			}
		}

		private void TeardownInternal()
		{
			foreach (PoolList<TInstance> instances in m_Instances.Values)
			{
				foreach (TInstance instance in instances)
				{
					if (instance != null)
					{
						instance.Teardown();
					}
				}
			}
			m_Instances.Clear();
			m_DebugPrefabNames.Clear();
			m_Lifetimes.Clear();
		}

		protected virtual bool PersistAcrossScenes => false;
	}
}
