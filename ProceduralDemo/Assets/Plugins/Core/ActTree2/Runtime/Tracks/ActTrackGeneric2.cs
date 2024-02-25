using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class TrackGeneric<TContext> : Track where TContext : ITreeContext
	{
		protected IActObject m_Tree = null;
		protected IActNodeRuntime m_Node = null;
		protected TContext m_Context = default;

		public override bool IsEventRequired(out System.Type trackEventType)
		{
			trackEventType = null;
			return false;
		}

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

		internal sealed override bool Initialize(IActObject tree, IActNodeRuntime node, ITreeContext context)
		{
			if (NodeItemUtil.TryInitialize(this, node, context, out TContext requiredContext, out string error))
			{
				error = OnValidate();
			}
			if (!string.IsNullOrEmpty(error))
			{
				Debug.LogWarning($"{tree.name} {node} {GetType().Name}.Initialize() {error}");
				return false;
			}
			m_Tree = tree;
			m_Node = node;
			m_Context = requiredContext;
			return OnInitialize();
		}

		protected virtual bool OnInitialize() { return true; }
	}
}
