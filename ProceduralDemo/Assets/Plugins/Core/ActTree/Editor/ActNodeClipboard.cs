
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ActNodeClipboard
{
	static ActTreeRT s_SourceTree = null;
	static int s_SourceNodeID = Act.Node.INVALID_ID;

	public static void Copy(Act.Node node, ActTree tree)
	{
		if (node == null || tree == null)
		{
			return;
		}

		s_SourceTree = new ActTreeRT(tree);
		s_SourceNodeID = node.GetID();
	}

	public static bool CanPaste()
	{
		return s_SourceTree != null && s_SourceNodeID != Act.Node.INVALID_ID;
	}

	public static void Clear()
	{
		s_SourceTree = null;
		s_SourceNodeID = Act.Node.INVALID_ID;
	}

	public static void Paste(Act.Node destParent, ActTree desTree, ref SerializedObject sDestTree)
	{
		if (destParent == null || desTree == null || sDestTree == null)
		{
			return;
		}
		if (s_SourceTree == null)
		{
			Clear(); // Tree was deleted?
			return;
		}
		ActNodeRT source = s_SourceTree.GetNode(s_SourceNodeID);
		if (source == null)
		{
			Clear(); // Nodes doesn't exist anymore?
			return;
		}

		SerializedProperty serNodeID = sDestTree.FindProperty("m_NextNodeID");
		SerializedProperty serNodes = sDestTree.FindProperty("m_Nodes");
		SerializedProperty serLinks = sDestTree.FindProperty("m_NodeLinks");
		SerializedProperty serOpps = sDestTree.FindProperty("m_Opportunities");

		Dictionary<int, int> idMappings = new Dictionary<int, int>();
		AddNode(source.GetID(), s_SourceTree, destParent.GetID(), desTree, 
			serNodeID, serNodes, serLinks, serOpps, ref idMappings);
		
		sDestTree.ApplyModifiedProperties();
		AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(desTree)); // Update AssetDatabase
		ActConditionDrawer.RebuildConditionsList(desTree, ref sDestTree);
		ActTrackDrawer.UpdateTrackList(desTree, ref sDestTree);
	}

	static void AddNode(
		int sourceNodeID,
		ActTreeRT sourceTree,
		int destParentID,
		ActTree destTree,
		SerializedProperty serNodeID, 
		SerializedProperty serNodes, 
		SerializedProperty serLinks,
		SerializedProperty serOpps,
		ref Dictionary<int, int> idMappings)
	{
		ActNodeRT sourceNode = sourceTree.GetNode(sourceNodeID);
		if (sourceNode == null)
		{
			return;
		}

		// Find safe name.
		string name = sourceNode.GetName();
		string nameBase = sourceNode.GetName();
		string numberSuffix = string.Empty;

		// Does the name end with a number?
		for (int i = name.Length - 1; 0 <= i; i--)
		{
			if (char.IsDigit(name[i]))
			{
				numberSuffix = name[i] + numberSuffix;
			}
			else break;
		}

		int nameIndex = 0;
		if (0 < numberSuffix.Length)
		{
			nameIndex = int.Parse(numberSuffix);
			nameBase = name.Remove(name.Length - numberSuffix.Length);
		}

		List<Act.NodeLink> list = destTree.GetAllNodeLinks();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Act.NodeLink link = list[i];
			Act.Node child = destTree.GetNode(link.GetChildID());
			if (destParentID == link.GetParentID() 
				&& child != null 
				&& child.GetName() == name)
			{
				i = 0;
				name = nameBase + (++nameIndex).ToString();
			}
		}

		int newID = serNodeID.intValue;
		idMappings.Add(sourceNode.GetID(), newID);
		serNodeID.intValue++;
		serNodes.arraySize++;
		SerializedProperty newNode = serNodes.GetArrayElementAtIndex(serNodes.arraySize - 1);
		newNode.FindPropertyRelative("m_ID").intValue = newID;
		// Rename the top node so we can see where it was pasted easily
		newNode.FindPropertyRelative("m_Name").stringValue = name;

		// If we're copying within the same tree, then copy pointer,
		// otherwise if we are copying to a new try then break pointer reference
		newNode.FindPropertyRelative("m_PointerID").intValue =
			sourceTree.GetSourceTree().GetInstanceID() == destTree.GetInstanceID() ?
			sourceNode.GetNode().GetPointerID() : Act.Node.INVALID_ID;

		serLinks.arraySize++;
		SerializedProperty newLink = serLinks.GetArrayElementAtIndex(serLinks.arraySize - 1);
		newLink.FindPropertyRelative("m_ParentID").intValue = destParentID;
		newLink.FindPropertyRelative("m_ChildID").intValue = newID;

		List<ActNodeRT> children = sourceTree.GetChildren(sourceNode.GetID());
		if (children != null)
		{
			foreach (ActNodeRT child in children)
			{
				AddNode(child.GetID(), sourceTree, newID, destTree,
					serNodeID, serNodes, serLinks, serOpps, ref idMappings);
			}

			// Once all of the duplicate nodes children have been copied we can try to copy any sequences
			foreach (ActNodeRT child in children)
			{
				CopySequences(child.GetID(), sourceTree.GetOpportunities(child.GetID()), serOpps, ref idMappings);
			}
		}

		Undo.RecordObject(destTree, "DupConditionsAndTrack");
		ActConditionGroup conditions = sourceTree.GetConditions(sourceNode.GetID());
		if (conditions != null)
		{
			foreach (ActCondition condition in conditions.GetConditions())
			{
				ActCondition dup = Object.Instantiate<ActCondition>(condition);
				dup.name = condition.name;
				dup.EditorSetNodeID(newID);
				Undo.RegisterCreatedObjectUndo(dup, "DupConditionsAndTrack");
				Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
				AssetDatabase.AddObjectToAsset(dup, destTree);
			}
		}
		List<ActTrack> tracks = sourceTree.GetTracks(sourceNode.GetID());
		if (tracks != null)
		{
			foreach (ActTrack track in tracks)
			{
				ActTrack dup = Object.Instantiate<ActTrack>(track);
				dup.EditorSetNodeID(newID);
				Undo.RegisterCreatedObjectUndo(dup, "DupConditionsAndTrack");
				AssetDatabase.AddObjectToAsset(dup, destTree);
			}
		}
		Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
	}

	static void CopySequences(
		int sourceNodeID,
		List<ActSequenceRT> sequences,
		SerializedProperty serSequences, 
		ref Dictionary<int, int> idMappings)
	{
		if (sequences == null)
		{
			return;
		}

		int destNodeID = Act.Node.INVALID_ID;
		if (!idMappings.TryGetValue(sourceNodeID, out destNodeID))
		{
			return;
		}

		foreach (ActSequenceRT sourceSeq in sequences)
		{
			if (idMappings.ContainsKey(sourceSeq.GetSequence().GetToID()))
			{
				serSequences.arraySize++;
				SerializedProperty newSequence = serSequences.GetArrayElementAtIndex(serSequences.arraySize - 1);
				newSequence.FindPropertyRelative("m_FromID").intValue = destNodeID;
				newSequence.FindPropertyRelative("m_ToID").intValue = idMappings[sourceSeq.GetSequence().GetToID()]; // The new ID of this node
				newSequence.FindPropertyRelative("m_StartTime").floatValue = sourceSeq.GetSequence().GetStartTime();
				newSequence.FindPropertyRelative("m_EndTime").floatValue = sourceSeq.GetSequence().GetEndTime();
			}
		}
	}
}
