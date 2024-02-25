using System;
using System.Collections;

namespace Core
{
	public abstract class TaskRunnerBase
	{
	}

	public class TaskRunner : TaskRunnerBase
	{
		public delegate IEnumerator RunDelegate();
		public delegate IEnumerator RunDelegate<T>(T arg1);
		public delegate IEnumerator RunDelegate<T1, T2>(T1 arg1, T2 arg2);
		public delegate IEnumerator RunDelegate<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
		public delegate IEnumerator RunDelegate<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

		public static void Run(RunDelegate task)
		{
			Run(task, null);
		}

		public static void Run(RunDelegate task, Action onComplete)
		{
			new TaskRunner(task).Run(onComplete);
		}

		public static void Run<T>(RunDelegate<T> task, T arg1)
		{
			Run(task, arg1, null);
		}

		public static void Run<T>(RunDelegate<T> task, T arg1, Action onComplete)
		{
			new TaskRunner<T>(task, arg1).Run(onComplete);
		}

		public static void Run<T1, T2>(RunDelegate<T1, T2> task, T1 arg1, T2 arg2)
		{
			Run(task, arg1, arg2, null);
		}

		public static void Run<T1, T2>(RunDelegate<T1, T2> task, T1 arg1, T2 arg2, Action onComplete)
		{
			new TaskRunner<T1, T2>(task, arg1, arg2).Run(onComplete);
		}

		public static void Run<T1, T2, T3>(RunDelegate<T1, T2, T3> task, T1 arg1, T2 arg2, T3 arg3)
		{
			Run(task, arg1, arg2, arg3);
		}

		public static void Run<T1, T2, T3>(RunDelegate<T1, T2, T3> task, T1 arg1, T2 arg2, T3 arg3, Action onComplete)
		{
			new TaskRunner<T1, T2, T3>(task, arg1, arg2, arg3).Run(onComplete);
		}

		public static void Run<T1, T2, T3, T4>(RunDelegate<T1, T2, T3, T4> task, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			Run(task, arg1, arg2, arg3, arg4, null);
		}

		public static void Run<T1, T2, T3, T4>(RunDelegate<T1, T2, T3, T4> task, T1 arg1, T2 arg2, T3 arg3, T4 arg4, Action onComplete)
		{
			new TaskRunner<T1, T2, T3, T4>(task, arg1, arg2, arg3, arg4).Run(onComplete);
		}

		private RunDelegate m_Task;

		internal TaskRunner(RunDelegate task)
		{
			m_Task = task;
		}

		internal void Run()
		{
			Run(null);
		}

		internal void Run(Action onComplete)
		{
			TaskManager.Instance.CreateTask(m_Task()).Start(onComplete);
		}
	}

	public class TaskRunner<T> : TaskRunnerBase
	{
		private TaskRunner.RunDelegate<T> m_Task;
		private T m_Arg1;

		internal TaskRunner(TaskRunner.RunDelegate<T> task, T arg1)
		{
			m_Task = task;
			m_Arg1 = arg1;
		}

		internal void Run()
		{
			Run(null);
		}

		internal void Run(Action onComplete)
		{
			TaskManager.Instance.CreateTask(m_Task(m_Arg1)).Start(onComplete);
		}
	}

	public class TaskRunner<T1, T2> : TaskRunnerBase
	{
		private TaskRunner.RunDelegate<T1, T2> m_Task;
		private T1 m_Arg1;
		private T2 m_Arg2;

		internal TaskRunner(TaskRunner.RunDelegate<T1, T2> task, T1 arg1, T2 arg2)
		{
			m_Task = task;
			m_Arg1 = arg1;
			m_Arg2 = arg2;
		}

		internal void Run()
		{
			Run(null);
		}

		internal void Run(Action onComplete)
		{
			TaskManager.Instance.CreateTask(m_Task(m_Arg1, m_Arg2)).Start(onComplete);
		}
	}

	public class TaskRunner<T1, T2, T3> : TaskRunnerBase
	{
		private TaskRunner.RunDelegate<T1, T2, T3> m_Task;
		private T1 m_Arg1;
		private T2 m_Arg2;
		private T3 m_Arg3;

		internal TaskRunner(TaskRunner.RunDelegate<T1, T2, T3> task, T1 arg1, T2 arg2, T3 arg3)
		{
			m_Task = task;
			m_Arg1 = arg1;
			m_Arg2 = arg2;
			m_Arg3 = arg3;
		}

		internal void Run()
		{
			Run(null);
		}

		internal void Run(Action onComplete)
		{
			TaskManager.Instance.CreateTask(m_Task(m_Arg1, m_Arg2, m_Arg3)).Start(onComplete);
		}
	}

	public class TaskRunner<T1, T2, T3, T4> : TaskRunnerBase
	{
		private TaskRunner.RunDelegate<T1, T2, T3, T4> m_Task;
		private T1 m_Arg1;
		private T2 m_Arg2;
		private T3 m_Arg3;
		private T4 m_Arg4;

		internal TaskRunner(TaskRunner.RunDelegate<T1, T2, T3, T4> task, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			m_Task = task;
			m_Arg1 = arg1;
			m_Arg2 = arg2;
			m_Arg3 = arg3;
			m_Arg4 = arg4;
		}

		internal void Run()
		{
			Run(null);
		}

		internal void Run(Action onComplete)
		{
			TaskManager.Instance.CreateTask(m_Task(m_Arg1, m_Arg2, m_Arg3, m_Arg4)).Start(onComplete);
		}
	}
}
