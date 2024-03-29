using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace OliverLoescher.Util
{
	/// <summary>
	/// Public access point for any class to access MonoBehaviour functions
	/// </summary>
    public class Mono : MonoBehaviourSingleton<Mono>
    {
		private void Awake()
		{
			DontDestroyOnLoad(this);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			StopAllCoroutines();
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
			Default = 0,
			CharacterController = 400,
			OnGround = 425,
			ModelController = 500,
			Camera = 1000,
			Last = int.MaxValue
		}

		[Serializable]
		public struct Updateable
		{
			private Action<float> action;
			[SerializeField, DisableInPlayMode]
			private Type type;
			[SerializeField, DisableInPlayMode]
			private Priorities priority;

			public readonly Action<float> Action => action;
			public readonly Type Type => type;
			public readonly Priorities Priority => priority;

			public Updateable(Type pType, Priorities pPriority)
			{
				action = null;
				type = pType;
				priority = pPriority;
			}

			public void SetProperties(Type pType, Priorities pPriority)
			{
				if (action != null)
				{
					LogError("Tried setting properties when already registered", "SetProperties");
					return;
				}
				type = pType;
				priority = pPriority;
			}

			public void Register(Action<float> pAction)
			{
				if (pAction == null)
				{
					LogExeception("Was passed a null action", "Register");
					return;
				}
				if (action != null)
				{
					if (action.Method == pAction.Method)
					{
						LogWarning("Was passed the same method which was already registered, returning.", "Register");
						return;
					}
					Deregister(); // Remove old action before registering the new one
				}
				action = pAction;
				RegisterUpdate(this);
			}

			public void Deregister()
			{
				if (action == null)
				{
					LogWarning("Tried deregistering when not registered", "Deregister");
					return;
				}
				DeregisterUpdate(this);
				action = null;
			}

			public override string ToString()
			{
				if (action == null)
				{
					return $"Updateable(Action: NULL, Type: {type}, Priority: {priority})";
				}
				return $"Updateable(Action: {action.Target} - {action.Method.Name}, Type: {type}, Priority: {priority})";
			}
		}

		private static List<Updateable> updatables = new List<Updateable>();
		private static List<Updateable> earlyUpdatables = new List<Updateable>();
		private static List<Updateable> lateUpdatables = new List<Updateable>();
		private static List<Updateable> fixedUpdatables = new List<Updateable>();

		private static void RegisterUpdate(in Updateable pUpdatable)
		{
			TryCreate();

			ref List<Updateable> items = ref GetUpdatables(pUpdatable.Type);
			int index;
			for (index = 0; index < items.Count; index++)
			{
				if (items[index].Priority <= pUpdatable.Priority)
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
				LogError($"Failed to remove {pUpdatable}.", "DeregisterUpdate");
			}
		}

		private static ref List<Updateable> GetUpdatables(Type pType)
		{
			switch (pType)
			{
				case Type.Early:
					return ref earlyUpdatables;
				case Type.Late:
					return ref lateUpdatables;
				case Type.Fixed:
					return ref fixedUpdatables;
				default:
					return ref updatables;
			}
		}

		private void Update()
		{
			UpdateInternal(earlyUpdatables, Time.deltaTime, "MonoUtil.EarlyUpdate()");
			UpdateInternal(updatables, Time.deltaTime, "MonoUtil.Update()");
		}

		private void LateUpdate()
		{
			UpdateInternal(lateUpdatables, Time.deltaTime, "MonoUtil.LateUpdate()");
		}

		private void FixedUpdate()
		{
			UpdateInternal(fixedUpdatables, Time.fixedDeltaTime, "MonoUtil.FixedUpdate()");
		}

		private readonly List<Updateable> updateables = new List<Updateable>();
		private void UpdateInternal(List<Updateable> pUpdatables, in float pDeltaTime, string pProfilerName)
		{
			Profiler.BeginSample(pProfilerName);
			updateables.AddRange(pUpdatables); // Copy over incase it changes
			foreach (Updateable updatable in updateables)
			{
				updatable.Action.Invoke(pDeltaTime);
			}
			updateables.Clear();
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
