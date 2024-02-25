
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Act2
{
	public static class NodeDebugPlay
	{
		static ActTree2 s_Tree = null;
		static List<ActTreeBehaviourBase2> s_Behaviours = new List<ActTreeBehaviourBase2>();

		public static bool CanPlay(ActTree2 tree)
		{
			if (!Application.isPlaying)
			{
				return false;
			}

			if (s_Tree != null && s_Tree.GetInstanceID() != tree.GetInstanceID())
			{
				s_Behaviours.Clear();
			}
			s_Tree = tree;

			bool valid = false;
			foreach (ActTreeBehaviourBase2 behaviour in s_Behaviours)
			{
				if (behaviour != null)
				{
					valid = true;
					break;
				}
			}
			if (!valid)
			{
				ActTreeBehaviourBase2[] behaviours = Object.FindObjectsOfType<ActTreeBehaviourBase2>();
				foreach (ActTreeBehaviourBase2 behaviour in behaviours)
				{
					if (behaviour.Controller == null) // Invalid case but possible if behaviour has no tree to play
					{
						continue;
					}
					foreach (TreeRT rtTree in behaviour.Controller.GetAllRuntimeTrees())
					{
						if (rtTree.SourceTree.GetInstanceID() == s_Tree.GetInstanceID())
						{
							s_Behaviours.Add(behaviour);
						}
					}
				}
			}

			return s_Behaviours.Count > 0;
		}

		public static void Play(ActTree2 tree, Act2.Node node)
		{
			CanPlay(tree); // This needs to be called first

			foreach (ActTreeBehaviourBase2 controller in s_Behaviours)
			{
				if (controller != null)
				{
					controller.PlayNodeID(node.GetID(), ignoreConditions:true);
				}
			}
		}
	}
}
