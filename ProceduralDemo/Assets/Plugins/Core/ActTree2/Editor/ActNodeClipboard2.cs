
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public static class NodeClipboard
	{
		private static ActTree2 s_CopiedTree = null;
		private static int s_CopiedNode = Node.INVALID_ID;

		public static bool IsCopied(Node node, ActTree2 tree)
		{
			return s_CopiedNode == node.GetID() &&
				s_CopiedTree != null &&
				s_CopiedTree.GetInstanceID() == tree.GetInstanceID();
		}
		public static bool IsCopied(ActTree2 tree)
        {
        	return s_CopiedTree != null && s_CopiedTree.GetInstanceID() == tree.GetInstanceID();
        }

		public static void Copy(Node node, ActTree2 tree)
		{
			if (node == null || tree == null)
			{
				return;
			}

			s_CopiedTree = tree;
			s_CopiedNode = node.GetID();
		}

		public static bool CanPaste()
		{
			return s_CopiedTree != null && s_CopiedNode != Node.INVALID_ID;
		}

		public static void Clear()
		{
			s_CopiedTree = null;
			s_CopiedNode = Node.INVALID_ID;
		}

		public static void Paste(Node destParent, ActTree2 desTree, ref SerializedObject sDestTree, Vector2? graphPosition = null)
		{
			if (destParent == null || desTree == null || sDestTree == null)
			{
				return;
			}
			if (s_CopiedTree == null)
			{
				Clear(); // Tree was deleted?
				return;
			}
			if (!s_CopiedTree.TryGetNode(s_CopiedNode, out Node copiedNode))
			{
				Clear(); // Nodes doesn't exist anymore?
				return;
			}

			TreeDrawer.SetNodeCollapsed(desTree, destParent, false);

			SerializedProperty sNodeID = sDestTree.FindProperty("m_NextNodeID");
			SerializedProperty sNodes = sDestTree.FindProperty("m_Nodes");

			Dictionary<int, int> idMappings = new Dictionary<int, int>();
			List<int> pastedChildIndexes = new List<int>();
			PasteNode(
				copiedNode, 
				s_CopiedTree, 
				destParent.GetID(), 
				desTree,
				sDestTree, sNodeID, sNodes, 
				idMappings,
				pastedChildIndexes);

			// Once all the children have been copied then update their transition IDs
			for (int i = 0; i < pastedChildIndexes.Count; i++)
			{
				int pastedChildIndex = pastedChildIndexes[i];
				SerializedProperty sNode = sNodes.GetArrayElementAtIndex(pastedChildIndex);
				SerializedProperty sTransitions = sNode.FindPropertyRelative("m_Transitions");
				if (i == 0) // Assume first node is root of copied tree, it can't have any transitions since it will have new siblings
				{
					sTransitions.arraySize = 0;
					// Rename the top node so we can see where it was pasted easily
					ValidateName(desTree, destParent.GetID(), Node.INVALID_ID, copiedNode.Name, out string validName);
					sNode.FindPropertyRelative("m_Name").stringValue = validName;
					if (graphPosition.HasValue)
					{
						sNode.FindPropertyRelative("m_GraphPositionChild").vector2Value = graphPosition.Value;
					}
					continue;
				}
				for (int j = 0; j < sTransitions.arraySize; j++)
				{
					SerializedProperty sTransition = sTransitions.GetArrayElementAtIndex(j);
					SerializedProperty sToID = sTransition.FindPropertyRelative("m_ToID");
					if (idMappings.TryGetValue(sToID.intValue, out int toID))
					{
						sToID.intValue = toID;
					}
				}
			}

			sDestTree.ApplyModifiedProperties();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(desTree)); // Update AssetDatabase
		}

		private static bool HasNumSuffix(string fullName, out string name, out int suffixNumber)
		{
			int i = 0;
			suffixNumber = 0;
			for (; i < fullName.Length - 1; i++)
			{
				int index = fullName.Length - (i + 1);
				char c = fullName[index];
				if (c < '0' || c > '9')
				{
					break;
				}
				int value = c - '0';
				suffixNumber += value * (i + 1);
			}
			if (i == 0)
			{
				name = fullName;
				suffixNumber = -1;
				return false;
			}
			name = fullName.Substring(0, fullName.Length - i);
			return true;
		}

		public static bool ValidateName(ActTree2 tree, Node node, string newName, out string validName)
		{
			return ValidateName(tree, node.GetParentID(), node.GetID(), newName, out validName);
		}

		public static bool ValidateName(ActTree2 tree, int parentNodeID, int nodeID, string newName, out string validName)
		{
			bool wasEmpty = string.IsNullOrEmpty(newName);
			if (wasEmpty)
			{
				newName = "Node01";
			}
			bool unique = true;
			List<Node> children = tree.GetChildren(parentNodeID);
			foreach (Node child in children)
			{
				if (child.GetID() != nodeID && child.Name == newName)
				{
					unique = false;
					break;
				}
			}
			if (unique)
			{
				validName = newName;
				return !wasEmpty; // If our name was originally empty then it's changed despite being unique
			}

			HasNumSuffix(newName, out string shortName, out _);
			int max = 0;
			for (int i = 0; i < children.Count; i++)
			{
				Node child = children[i];
				if (child.GetID() != nodeID &&
					HasNumSuffix(child.Name, out string name, out int suffixNumber) &&
					shortName == name &&
					suffixNumber > max)
				{
					max = suffixNumber;
				}
			}
			if (max == 0)
			{
				validName = newName + "02";
			}
			else
			{
				validName = shortName + (max + 1).ToString("00");
			}
			return false;
		}

		private static void PasteNode(
			Node copiedNode,
			ActTree2 copiedTree,
			int destParentID,
			ActTree2 destTree,
			SerializedObject sDestTree,
			SerializedProperty sDestNodeID,
			SerializedProperty sDestNodes,
			Dictionary<int, int> idMappings,
			List<int> pastedChildIndexes)
		{
			sDestNodes.arraySize++;
			int pastedNodeIndex = sDestNodes.arraySize - 1;
			pastedChildIndexes.Add(pastedNodeIndex);
			SerializedProperty pastedNode = sDestNodes.GetArrayElementAtIndex(pastedNodeIndex);
			pastedNode.managedReferenceValue = CopySerializedReferenceUtil.CopyObject(copiedNode);

			int pastedID = sDestNodeID.intValue;
			idMappings.Add(copiedNode.GetID(), pastedID);
			sDestNodeID.intValue++;
			pastedNode.FindPropertyRelative("m_ID").intValue = pastedID;
			pastedNode.FindPropertyRelative("m_ParentID").intValue = destParentID;

			// If we're copying within the same tree, then copy pointer,
			// otherwise if we are copying to a new try then break pointer reference
			pastedNode.FindPropertyRelative("m_PointerID").intValue =
				copiedTree.GetInstanceID() == destTree.GetInstanceID() ?
				copiedNode.GetPointerID() : Node.INVALID_ID;

			List<Node> children = copiedTree.GetChildren(copiedNode.GetID());
			foreach (Node child in children)
			{
				PasteNode(
					child,
					copiedTree,
					pastedID,
					destTree,
					sDestTree, sDestNodeID, sDestNodes,
					idMappings,
					pastedChildIndexes);
			}
		}
	}
}
