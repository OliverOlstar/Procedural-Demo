using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class ActConditionFab<TConditionFab, TContext> : ConditionGeneric<TContext>, ITreeOwner
		where TConditionFab : SOActConditionFab<TContext>
		where TContext : ITreeContext
	{
		[SerializeField, UberPicker.AssetNonNull]
		private TConditionFab m_ConditionFab = null;

		private TreeRT m_FabTree = null;

		string ITreeOwner.name => m_Node.Name;
		ITreeContext ITreeOwner.GetContext() => m_Context;

		public override bool IsEventRequired(out System.Type eventType)
		{
			if (m_ConditionFab == null)
			{
				eventType = null;
				return false;
			}
			return m_ConditionFab.RootNode.IsEventRequired(out eventType);
		}

		protected override void OnInitialize(ref bool? failedToInitializeReturnValue)
		{
			if (m_ConditionFab == null)
			{
				failedToInitializeReturnValue = true;
				return;
			}
			m_FabTree = TreeRT.CreateAndInitialize(m_ConditionFab, this);
		}

		protected override bool OnEvaluate(ITreeEvent treeEvent)
		{
#if UNITY_EDITOR
			m_FabTree._EditorRebuildIfDirty();
#endif
			return m_FabTree.RootNode.EvaluateConditions(treeEvent);
		}

		public override string ToString() => m_ConditionFab != null ? m_ConditionFab.name : $"{GetType().Name}(NULL)";
	}
}
