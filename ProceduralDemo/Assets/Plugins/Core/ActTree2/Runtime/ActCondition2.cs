
using UnityEngine;

namespace Act2
{
	[System.Serializable]
	public abstract class Condition : INodeItem
	{
		// Called by Editor scripts through reflection
		public static System.Type _EditorGetContext() => typeof(IGOContext);
		public static System.Type _EditorGetEvent() => null;

		[SerializeField]
		[HideInInspector]
		private int m_Order = 0;
		public int GetOrder() { return m_Order; }

		public abstract bool IsEventRequired(out System.Type eventType);
		public virtual int GetPolingFrequency() { return 2; }

		public abstract bool _EditorIsValid(IActObject tree, IActNodeRuntime node, out string error);

		public abstract void Initialize(IActObject tree, IActNodeRuntime node, ITreeContext context);

		public abstract bool Evaluate(ITreeEvent treeEvent);

		public void StateEnter(ITreeEvent treeEvent)
		{
			OnStart(treeEvent);
		}

		public void StateExit()
		{
			OnEnd();
		}

		protected virtual void OnStart(ITreeEvent treeEvent) { }

		protected virtual void OnEnd() { }

		public override string ToString()
		{
			string name = GetType().Name;
			if (name.EndsWith("Condition"))
			{
				name = name.Substring(0, name.Length - 9);
			}
			return name;
		}
	}
}
