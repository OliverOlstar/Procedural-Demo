using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Act2
{
	public class ActTreeRuntimeDebugController
	{
		private HashSet<int> m_CurrentlyPlayingNodes = new HashSet<int>();
		private HashSet<int> m_PreviouslyPlayingNodes = new HashSet<int>();
		private int m_TransitionFromNodeID = Node.INVALID_ID;

		public bool IsDebugging => m_CurrentlyPlayingNodes.Count > 0;

		public void Update(ActTree2 tree)
		{
			m_CurrentlyPlayingNodes.Clear();
			m_PreviouslyPlayingNodes.Clear();
			m_TransitionFromNodeID = Node.INVALID_ID;

			if (Application.isPlaying && Selection.activeGameObject != null)
			{
				TreeDebuggerData debugger = TreeDebuggerData.Get();
				if (debugger.TryGetCurrentSnapShot(Selection.activeGameObject.name, tree.name, out TreeDebuggerData.SnapShot curSnapShot))
				{
					if (curSnapShot.m_Transition != null)
					{
						m_TransitionFromNodeID = curSnapShot.m_Transition.FromNode.ID;
					}
					foreach (TreeDebuggerData.SequencerSnapShot seq in curSnapShot.m_SequencerSnapShots)
					{
						m_CurrentlyPlayingNodes.Add(seq.NodeProperties.ID);
					}
				}
				if (debugger.TryGetPreviousSnapShot(Selection.activeGameObject.name, tree.name, out TreeDebuggerData.SnapShot prevSnapShot))
				{
					foreach (TreeDebuggerData.SequencerSnapShot seq in prevSnapShot.m_SequencerSnapShots)
					{
						m_PreviouslyPlayingNodes.Add(seq.NodeProperties.ID);
					}
				}
			}
		}

		public bool IsCurrentlyPlayingNode(int nodeID)
		{
			if (!IsDebugging)
			{
				return false;
			}
			return m_CurrentlyPlayingNodes.Contains(nodeID);
		}

		public bool WasPreviouslyPlayingNode(int nodeID)
		{
			if (!IsDebugging)
			{
				return false;
			}
			return m_PreviouslyPlayingNodes.Contains(nodeID);
		}

		public enum TransitionTaken
		{
			NotTaken,
			FromOtherNode,
			FromSelf,
		}
		public bool WasTransitionTakenFromNode(int nodeID)
		{
			if (!IsDebugging)
			{
				return false;
			}
			return m_TransitionFromNodeID == nodeID;
		}
	}
}
