using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public abstract class ActTreeBehaviourGeneric2<TTree, TContext> : ActTreeBehaviourBase2
		where TTree : ActTreeGeneric2<TContext>
		where TContext : class, ITreeContext // class constraint is because casting to ITreeContext in GetContext() would create garbage if TContext was a struct
	{
		[SerializeField, UberPicker.AssetNonNull]
		private TTree m_Tree = null;
		[SerializeField, UberPicker.AssetNonNull]
		private TTree[] m_SecondaryTrees = { };

		public override ActTree2 MainTree => m_Tree;
		public override IEnumerable<ActTree2> SecondaryTrees => m_SecondaryTrees;
		public override ITreeContext GetContext() => GetContextInternal();

		protected abstract TContext GetContextInternal();
	}
}
