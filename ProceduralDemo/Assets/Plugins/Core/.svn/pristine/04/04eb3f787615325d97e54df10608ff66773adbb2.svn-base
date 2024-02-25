using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class ConditionGeneric<TContext> : Condition where TContext : ITreeContext
	{
		public new static System.Type _EditorGetContext() => typeof(TContext);

		protected IActObject m_Tree = null;
		protected IActNodeRuntime m_Node = null;
		protected TContext m_Context = default;

		private bool? m_FailedToInitializeReturnValue = null;

		public sealed override bool _EditorIsValid(IActObject tree, IActNodeRuntime node, out string error)
		{
			if (!NodeItemUtil._EditorIsValid<TContext>(this, tree, node, out error))
			{
				return false;
			}
			error = OnValidate();
			return string.IsNullOrEmpty(error);
		}

		protected virtual string OnValidate()
		{
			return null;
		}

		public sealed override void Initialize(IActObject tree, IActNodeRuntime node, ITreeContext context)
		{
			if (NodeItemUtil.TryInitialize(this, node, context, out TContext requiredContext, out string error))
			{
				error = OnValidate();
			}
			if (!string.IsNullOrEmpty(error))
			{
				m_FailedToInitializeReturnValue = false;
				Debug.LogWarning($"{tree.name} {node} {GetType().Name}.Initialize() " +
					$"This condition will only return 'false' because of validation error:\n{error}");
				return;
			}
			m_Tree = tree;
			m_Node = node;
			m_Context = requiredContext;
			OnInitialize(ref m_FailedToInitializeReturnValue);
			if (m_FailedToInitializeReturnValue.HasValue)
			{
				Debug.LogWarning($"{tree.name} {node} {GetType().Name}.Initialize() " +
					$"Failed to initialize this condition will only return '{m_FailedToInitializeReturnValue.Value}'");
			}
		}

		protected virtual void OnInitialize(ref bool? failedToInitializeReturnValue) { }

		public sealed override bool Evaluate(ITreeEvent treeEvent)
		{
			if (m_FailedToInitializeReturnValue.HasValue)
			{
				return m_FailedToInitializeReturnValue.Value;
			}
			return OnEvaluate(treeEvent);
		}

		protected abstract bool OnEvaluate(ITreeEvent treeEvent);
	}
}
