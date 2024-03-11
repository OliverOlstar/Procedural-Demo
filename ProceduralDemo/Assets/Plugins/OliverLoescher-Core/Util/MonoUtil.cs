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
		public enum UpdateType
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
			private UpdateType type;
			[SerializeField, DisableInPlayMode]
			private Priorities priority;

			public readonly Action<float> Action => action;
			public readonly UpdateType Type => type;
			public readonly Priorities Priority => priority;

			public Updateable(UpdateType pType, Priorities pPriority)
			{
				action = null;
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

		private static ref List<Updateable> GetUpdatables(UpdateType pType)
		{
			switch (pType)
			{
				case UpdateType.Early:
					return ref earlyUpdatables;
				case UpdateType.Late:
					return ref lateUpdatables;
				case UpdateType.Fixed:
					return ref fixedUpdatables;
				default:
					return ref updatables;
			}
		}

		private void Update()
		{
			Profiler.BeginSample("MonoUtil.EarlyUpdate()");
			foreach (Updateable updatable in earlyUpdatables)
			{
				updatable.Action.Invoke(Time.deltaTime);
			}
			Profiler.EndSample();

			Profiler.BeginSample("MonoUtil.Update()");
			foreach (Updateable updatable in updatables)
			{
				updatable.Action.Invoke(Time.deltaTime);
			}
			Profiler.EndSample();
		}

		private void LateUpdate()
		{
			Profiler.BeginSample("MonoUtil.LateUpdate()");
			foreach (Updateable updatable in lateUpdatables)
			{
				updatable.Action.Invoke(Time.deltaTime);
			}
			Profiler.EndSample();
		}

		private void FixedUpdate()
		{
			Profiler.BeginSample("MonoUtil.FixedUpdate()");
			foreach (Updateable updatable in fixedUpdatables)
			{
				updatable.Action.Invoke(Time.fixedDeltaTime);
			}
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
