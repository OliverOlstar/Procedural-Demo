using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace ODev.Util
{
	/// <summary>
	/// Public access point for any class to access MonoBehaviour functions
	/// </summary>
	public class Mono : MonoBehaviourSingletonAuto<Mono>
	{
		protected override void OnDestroy()
		{
			StopAllCoroutines();
			base.OnDestroy();
		}

		#region Updatables
		public enum Type
		{
			Default = 0,
			Early,
			Late,
			Fixed
		}

		public enum Priorities
		{
			First = int.MinValue,
			Input = -2000,
			UI = -1000,
			World = -200,
			Default = 0,
			OnGround = 350,
			CharacterAbility = 375,
			CharacterController = 400,
			ModelController = 500,
			Interactator = 700,
			Camera = 1000,
			PoseAnimator = 5000,
			Last = int.MaxValue
		}

		[Serializable]
		public struct Updateable
		{
			private Action<float> m_Action;
			[SerializeField, DisableInPlayMode]
			private Type m_Type;
			[SerializeField, DisableInPlayMode]
			private Priorities m_Priority;

			public readonly Action<float> Action => m_Action;
			public readonly Type Type => m_Type;
			public readonly Priorities Priority => m_Priority;
			public readonly bool IsRegistered => m_Action != null;

			public Updateable(Type pType, Priorities pPriority)
			{
				m_Action = null;
				m_Type = pType;
				m_Priority = pPriority;
			}

			public void SetProperties(Type pType, Priorities pPriority)
			{
				if (m_Action != null)
				{
					LogError("Tried setting properties when already registered");
					return;
				}
				m_Type = pType;
				m_Priority = pPriority;
			}

			public void Register(Action<float> pAction)
			{
				if (pAction == null)
				{
					LogExeception("Was passed a null action");
					return;
				}
				if (IsRegistered)
				{
					if (m_Action.Method == pAction.Method)
					{
						LogWarning("Was passed the same method which was already registered, returning.");
						return;
					}
					Deregister(); // Remove old action before registering the new one
				}
				m_Action = pAction;
				RegisterUpdate(this);
			}

			public void Deregister()
			{
				if (m_Action == null)
				{
					LogWarning("Tried deregistering when not registered");
					return;
				}
				DeregisterUpdate(this);
				m_Action = null;
			}

			public override readonly string ToString()
			{
				if (m_Action == null)
				{
					return $"Updateable(Action: NULL, Type: {m_Type}, Priority: {m_Priority})";
				}
				return $"Updateable(Action: {m_Action.Target} - {m_Action.Method.Name}, Type: {m_Type}, Priority: {m_Priority})";
			}
		}

		private static List<Updateable> s_Updatables = new();
		private static List<Updateable> s_EarlyUpdatables = new();
		private static List<Updateable> s_LateUpdatables = new();
		private static List<Updateable> s_FixedUpdatables = new();

#if UNITY_EDITOR
		public static IEnumerable<Updateable> GetAllUpdateables()
		{
			foreach (Updateable updateable in s_EarlyUpdatables)
			{
				yield return updateable;
			}
			foreach (Updateable updateable in s_Updatables)
			{
				yield return updateable;
			}
			foreach (Updateable updateable in s_LateUpdatables)
			{
				yield return updateable;
			}
			foreach (Updateable updateable in s_FixedUpdatables)
			{
				yield return updateable;
			}
		}
#endif

		private static void RegisterUpdate(in Updateable pUpdatable)
		{
			TryCreate();

			ref List<Updateable> items = ref GetUpdatables(pUpdatable.Type);
			int index;
			for (index = 0; index < items.Count; index++)
			{
				if (items[index].Priority > pUpdatable.Priority)
				{
					break;
				}
			}
			items.Insert(index, pUpdatable);
		}

		private static void DeregisterUpdate(in Updateable pUpdatable)
		{
			if (!GetUpdatables(pUpdatable.Type).Remove(pUpdatable))
			{
				LogError($"Failed to remove {pUpdatable}.");
			}
		}

		private static ref List<Updateable> GetUpdatables(Type pType)
		{
			switch (pType)
			{
				case Type.Early:
					return ref s_EarlyUpdatables;
				case Type.Late:
					return ref s_LateUpdatables;
				case Type.Fixed:
					return ref s_FixedUpdatables;
				default:
					return ref s_Updatables;
			}
		}

		private void Update()
		{
			UpdateInternal(s_EarlyUpdatables, Time.deltaTime, "MonoUtil.EarlyUpdate()");
			UpdateInternal(s_Updatables, Time.deltaTime, "MonoUtil.Update()");
		}

		private void LateUpdate()
		{
			UpdateInternal(s_LateUpdatables, Time.deltaTime, "MonoUtil.LateUpdate()");
		}

		private void FixedUpdate()
		{
			UpdateInternal(s_FixedUpdatables, Time.fixedDeltaTime, "MonoUtil.FixedUpdate()");
		}

		private readonly List<Updateable> m_Updateables = new(); // Resuse instead of making new every frame
		private void UpdateInternal(List<Updateable> pUpdatables, in float pDeltaTime, string pProfilerName)
		{
			Profiler.BeginSample(pProfilerName);
			m_Updateables.AddRange(pUpdatables); // Copy over incase it changes
			foreach (Updateable updatable in m_Updateables)
			{
				updatable.Action.Invoke(pDeltaTime);
			}
			m_Updateables.Clear();
			Profiler.EndSample();
		}
		#endregion Updatables

		#region Coroutines
		public static Coroutine Start(in IEnumerator pEnumerator)
		{
			return Instance.StartCoroutine(pEnumerator);
		}
		public static void Stop(ref Coroutine pCoroutine)
		{
			if (pCoroutine == null)
			{
				return;
			}
			Instance.StopCoroutine(pCoroutine);
			pCoroutine = null;
		}
		#endregion Coroutines
	}
}
