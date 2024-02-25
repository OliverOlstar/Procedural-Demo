
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ActSequenceContext
{
	public class Parameters
	{
		public SerializedObject Tree { get; private set; }
		public ActTree ActTree { get; private set; }
		public Act.Node Node { get; private set; }
		public List<Act.NodeSequence> Sequences { get; private set; }
		public int Index { get; private set; }

		public Parameters(
			SerializedObject tree,
			Act.Node node,
			List<Act.NodeSequence> sequences,
			int index)
		{
			Tree = tree;
			Node = node;
			Sequences = sequences;
			Index = index;
		}

		public Parameters(
			SerializedObject sTree,
			ActTree tree,
			Act.Node node,
			Act.NodeSequence sequence)
		{
			Tree = sTree;
			ActTree = tree;
			Node = node;
			Sequences = tree.GetAllOpportunities();
			for (int i = 0; i < Sequences.Count; i++)
			{
				Act.NodeSequence seq = Sequences[i];
				if (seq.GetFromID() == sequence.GetFromID() &&
					seq.GetToID() == sequence.GetToID())
				{
					Index = i;
					break;
				}
			}
		}
	}

	public interface IActContext
	{
		void Execute();
	}

	public class Delete : IActContext
	{
		private Parameters m_Params = null;

		public Delete(Parameters param) { m_Params = param; }

		public void Execute()
		{
			SerializedProperty serSequences = m_Params.Tree.FindProperty("m_Opportunities");
			serSequences.DeleteArrayElementAtIndex(m_Params.Index);
			m_Params.Tree.ApplyModifiedProperties();
			ActTreeDirtyTimestamps.SetDirty(m_Params.ActTree);
		}
	}

	public class MoveUp : IActContext
	{
		private Parameters m_Params = null;

		public MoveUp(Parameters param) { m_Params = param; }

		public void Execute()
		{
			SerializedProperty serSequences = m_Params.Tree.FindProperty("m_Opportunities");
			for (int i = m_Params.Index - 1; i >= 0; i--)
			{
				if (m_Params.Sequences[i].GetFromID() == m_Params.Node.GetID())
				{
					serSequences.MoveArrayElement(m_Params.Index, i);
					m_Params.Tree.ApplyModifiedProperties();
					ActTreeDirtyTimestamps.SetDirty(m_Params.ActTree);
					break;
				}
			}
		}
	}

	public class MoveDown : IActContext
	{
		private Parameters m_Params = null;

		public MoveDown(Parameters param) { m_Params = param; }

		public void Execute()
		{
			SerializedProperty serSequences = m_Params.Tree.FindProperty("m_Opportunities");
			for (int i = m_Params.Index + 1; i < m_Params.Sequences.Count; i++)
			{
				if (m_Params.Sequences[i].GetFromID() == m_Params.Node.GetID())
				{
					serSequences.MoveArrayElement(m_Params.Index, i);
					m_Params.Tree.ApplyModifiedProperties();
					ActTreeDirtyTimestamps.SetDirty(m_Params.ActTree);
					break;
				}
			}
		}
	}

	public static void OpenContext(Parameters param)
	{
		GenericMenu menu = new GenericMenu();
		menu.allowDuplicateNames = true;
		menu.AddItem(new GUIContent("Delete"), false, Execute, new Delete(param));
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Move Up"), false, Execute, new MoveUp(param));
		menu.AddItem(new GUIContent("Move Down"), false, Execute, new MoveDown(param));
		menu.ShowAsContext();
	}

	public static void Execute(object obj)
	{
		if (obj is IActContext context)
		{
			context.Execute();
		}
	}
}
