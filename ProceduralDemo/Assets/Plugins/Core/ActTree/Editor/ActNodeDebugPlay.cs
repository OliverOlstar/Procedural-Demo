
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class ActNodeDebugPlay
{
	static ActTree s_Tree = null;
	static List<ActTreeBehaviour> s_Behaviours = new List<ActTreeBehaviour>();

	public static bool CanPlay(ActTree tree)
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
		foreach (ActTreeBehaviour behaviour in s_Behaviours)
		{
			if (behaviour != null)
			{
				valid = true;
				break;
			}
		}
		if (!valid)
		{
			ActTreeBehaviour[] behaviours = Object.FindObjectsOfType<ActTreeBehaviour>();
			foreach (ActTreeBehaviour behaviour in behaviours)
			{
				foreach (ActTreeRT rtTree in behaviour.Controller.GetAllRuntimeTrees())
				{
					if (rtTree.GetSourceTree().GetInstanceID() == s_Tree.GetInstanceID())
					{
						s_Behaviours.Add(behaviour);
					}
				}
			}
		}

		return s_Behaviours.Count > 0;
	}

	public static void Play(ActTree tree, Act.Node node)
	{
		CanPlay(tree); // This needs to be called first

		foreach (ActTreeBehaviour controller in s_Behaviours)
		{
			if (controller != null)
			{
				controller.PlayNode(node.GetID());
			}
		}
	}
}
