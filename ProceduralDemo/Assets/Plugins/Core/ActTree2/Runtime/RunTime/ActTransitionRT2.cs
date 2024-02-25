using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Act2
{
	public class TransitionRT
	{
		private NodeTransition m_Transition = null;
		public NodeTransition Transition => m_Transition;

		private NodeRT m_FromNode = null;
		public NodeRT FromNode => m_FromNode;

		private NodeRT m_ToNode = null;
		public NodeRT ToNode => m_ToNode;

		public TransitionRT(NodeTransition transition, NodeRT fromNode, NodeRT toNode)
		{
			m_Transition = transition;
			m_FromNode = fromNode;
			m_ToNode = toNode;
		}

		public bool IsAvailable(Sequencer.ActionType sequencerAction, float time)
		{
			switch (sequencerAction)
			{
				case Sequencer.ActionType.TransitionOnEnd:
					if (!m_Transition.IsAvailableOnEnd())
					{
						return false;
					}
					if (m_ToNode.IsAvailableOnlyOnEventTransition())
					{
						return false;
					}
					break;
				case Sequencer.ActionType.TransitionPoling:
					if (!m_Transition.IsAvailable(time))
					{
						return false;
					}
					if (m_ToNode.IsAvailableOnlyOnEventTransition())
					{
						return false;
					}
					break;
				case Sequencer.ActionType.EventTransition:
					if (!m_Transition.IsAvailable(time))
					{
						return false;
					}
					if (!m_ToNode.IsAvailableOnlyOnEventTransition())
					{
						return false;
					}
					break;
				default:
					Core.DebugUtil.DevException($"TransitionRT.IsAvailable() Does not support unexpected action {sequencerAction}");
					return false;
			}
			return true;
		}
	}
}
