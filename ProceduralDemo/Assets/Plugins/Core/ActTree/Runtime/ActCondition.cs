
using UnityEngine;

public class ActCondition : ScriptableObject, System.IComparable
{
	public static int GetContextMask() { return 0x1; }

	[SerializeField][HideInInspector]
	int m_NodeID = 0;
	public int GetNodeID() { return m_NodeID; }
#if UNITY_EDITOR
	public void EditorSetNodeID(int nodeID) { m_NodeID = nodeID; EditorUpdateName(); } // Update name from node ID
	// HACK: Unity has a bug where nested objects are sorted by name and then the first object in the sort order becomes the root
	// This makes it impossible to duplicate a tree unless we guarentee the tree's name always sorts to the top
	// Prefixing with "zz" should guarentee this is always the case
	public void EditorUpdateName() { name = $"zzNode{m_NodeID}_{GetType().Name}"; }
#endif

	[SerializeField][HideInInspector]
	int m_Order = 0;
	public int GetOrder() { return m_Order; }

	protected ActTreeRT m_Tree = null;
	protected ActNodeRT m_Node = null;

	public int CompareTo(object obj)
	{
		ActCondition condition = obj as ActCondition;
		if (condition == null)
		{
			return 0;
		}
		if (condition.m_Order != m_Order)
		{
			return condition.m_Order - m_Order;
		}

		// We need a deterministic way to settle ties.
		// This doesn't really work as conditions with the same name can still tie, but it will avoid most bugs.
		// I kind of want to redo this whole order thing... It's not the best
		return name.CompareTo(condition.name);
	}

	public virtual bool RequiresEvent() { return false; }
	public virtual int GetPolingFrequency() { return 2; }

	public void Initialize(ActTreeRT tree, ActNodeRT node, ActParams actParams)
	{
		m_Tree = tree;
		m_Node = node;
		OnInitialize(actParams);
	}

	protected virtual void OnInitialize(ActParams actParams) { }

	public virtual bool Evaluate(ActParams param)
	{
		return true;
	}

	public void StateEnter(ActParams param)
	{
		OnStart(param);
	}

	public void StateExit()
	{
		OnEnd();
	}

	protected virtual void OnStart(ActParams param) { }

	protected virtual void OnEnd() { }

	public override string ToString()
	{
		return GetType().ToString();
	}
}
