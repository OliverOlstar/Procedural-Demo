
using System.Collections.Generic;
using UnityEngine;

namespace ContentTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Core")]
	public class HasContentNodeCondition : Act2.ConditionPoling<Act2.IGOContext>
	{
		[SerializeField, UberPicker.AssetNonNull]
		private SOContentSource m_ContentSource = null;
		[SerializeField, UberPicker.Asset]
		private SOContentNode m_ContentNode = null;

		private ContentTreeBehaviour m_ContentBehaviour = null;

		protected override void OnInitialize(ref bool? failedToInitializeReturnValue)
		{
			if (!m_Context.GameObject.TryGetComponent(out m_ContentBehaviour))
			{
				m_ContentBehaviour = m_Context.GameObject.AddComponent<ContentTreeBehaviour>();
			}
		}

		protected override bool EvaluatePoling()
		{
			return m_ContentBehaviour.ContainsContentNode(m_ContentSource, m_ContentNode);
		}

		public override string ToString()
		{
			return Core.Str.Build("HasContentNode(", m_ContentSource?.name, ", ", m_ContentNode?.Name, ")");
		}
	}
}
