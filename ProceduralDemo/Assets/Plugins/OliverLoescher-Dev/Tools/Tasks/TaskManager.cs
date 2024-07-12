using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ODev
{
	public class TaskManager : MonoBehaviour
	{
		public const string DEFAULT_CATEGORY = "DEFAULT_TASK_MANANGER_CATEGORY";

		private static TaskManager s_Instance;

		public static TaskManager Instance
		{
			get
			{
				if (s_Instance == null)
				{
					GameObject singleton = new("TaskManager");
					DontDestroyOnLoad(singleton);
					s_Instance = singleton.AddComponent<TaskManager>();
				}
				return s_Instance;
			}
		}

		private Dictionary<string, HashSet<Task>> m_Tasks;

		TaskManager()
		{
			m_Tasks = new Dictionary<string, HashSet<Task>>();
		}

		public Task CreateTask(IEnumerator coroutine)
		{
			return CreateTask(DEFAULT_CATEGORY, coroutine);
		}

		public Task CreateTask(string category, IEnumerator coroutine)
		{
			Task task = new(category, coroutine);
			HashSet<Task> tasks = null;
			if (!m_Tasks.TryGetValue(category, out tasks))
			{
				m_Tasks[category] = new HashSet<Task>();
			}
			m_Tasks[category].Add(task);
			return task;
		}
		
		public void StopTasks(string category)
		{
			HashSet<Task> tasks = null;
			if (m_Tasks.TryGetValue(category, out tasks))
			{
				foreach (Task task in tasks)
				{
					task.Stop();
				}
				tasks.Clear();
			}
		}

		public void StopAllTasks()
		{
			foreach (string category in m_Tasks.Keys)
			{
				HashSet<Task> tasks = m_Tasks[category];
				foreach (Task task in tasks)
				{
					task.Stop();
				}
				tasks.Clear();
			}
		}

		public IEnumerator WaitForTasks(params Task[] tasks)
		{
			yield return WaitForTasks(tasks as IEnumerable<Task>);
		}

		public IEnumerator WaitForTasks(IEnumerable<Task> tasks)
		{
			bool yielding;
			do
			{
				yielding = false;
				foreach (Task task in tasks)
				{
					if (task != null && !task.Complete)
					{
						yielding = true;
						break;
					}
				}
				if (yielding)
				{
					yield return null;
				}
			} while (yielding);
		}

		private Coroutine StartTask(IEnumerator enumerator, Action onComplete)
		{
			return StartCoroutine(DoTask(enumerator, onComplete));
		}

		private IEnumerator DoTask(IEnumerator enumerator, Action onComplete)
		{
			yield return enumerator;
			if (onComplete != null)
			{
				onComplete();
			}
		}

		private void RemoveTask(Task task)
		{
			HashSet<Task> tasks = null;
			if (m_Tasks.TryGetValue(task.Category, out tasks))
			{
				tasks.Remove(task);
			}
		}

		public class Task
		{
			private bool m_Started;
			private IEnumerator m_Enumerator;
			private Action m_OnComplete;
			private Coroutine m_Coroutine;

			public event Action Completed;

			public string Category { get; private set; }
			public bool Complete { get; private set; }

			internal Task(string category, IEnumerator coroutine)
			{
				Category = category;
				Complete = false;
				m_Started = false;
				m_Enumerator = coroutine;
			}

			public Task Start()
			{
				Start(null);
				return this; // So you can do this m_Task = ODev.TaskManager.Instance.CreateTask(TaskCoroutine()).Start();
			}

			public void Start(Action onComplete)
			{
				if (!m_Started)
				{
					m_OnComplete = onComplete;
					m_Coroutine = Instance.StartTask(m_Enumerator, OnTaskComplete);
					m_Started = true;
				}
			}

			public void Stop()
			{
				if (m_Started)
				{
					Instance.StopCoroutine(m_Coroutine);
					m_Started = false;
				}
			}

			private void OnTaskComplete()
			{
				Complete = true;
				Instance.RemoveTask(this);
				Completed?.Invoke();
				if (m_OnComplete != null)
				{
					m_OnComplete();
				}
			}
		}
	}
}
