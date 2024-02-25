using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	/// <summary>
	/// Basic ActTreeBehaviour that can be attached to any GameObject and work "out of the box"
	/// It's recommended to use ActTreeBehaviourGeneric which can be constrained to specific tree types and contexts for bigger projects
	/// </summary>
	public class ActTreeBehaviour2 : ActTreeBehaviourBase2
	{
		[SerializeField, UberPicker.AssetNonNull]
		private ActTree2 m_Tree = null;
		[SerializeField, UberPicker.AssetNonNull]
		private ActTree2[] m_SecondaryTrees = { };

		public override ActTree2 MainTree => m_Tree;
		public override IEnumerable<ActTree2> SecondaryTrees => m_SecondaryTrees;
		public override ITreeContext GetContext() => new GOContext(gameObject);
	}
}
