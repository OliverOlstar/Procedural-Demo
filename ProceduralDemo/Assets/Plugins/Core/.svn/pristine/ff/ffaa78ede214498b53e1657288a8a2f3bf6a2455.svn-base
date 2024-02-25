
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public static class SelectionDrawer
	{
		private class SelectionList : List<(NodeItemType, int)> { }
		private static Dictionary<(int, int), SelectionList> m_History = new Dictionary<(int, int), SelectionList>();

		private static (int, int)? m_CopiedKey = null;
		public static bool HasCopiedItems() => 
			m_CopiedKey.HasValue && 
			m_History.TryGetValue(m_CopiedKey.Value, out SelectionList list) && 
			list.Count > 0;
		public static void ClearCopiedItems() => m_CopiedKey = null;

		private static bool IsValid(IActNode node, (NodeItemType, int) pair)
		{
			int itemIndex = pair.Item2;
			if (itemIndex < 0) // Probably not possible
			{
				return false;
			}
			NodeItemType itemType = pair.Item1;
			switch (itemType)
			{
				case NodeItemType.Condition:
					return itemIndex < node.Conditions.Count;
				case NodeItemType.Transition:
					return itemIndex < node.Transitions.Count;
				case NodeItemType.Track:
					return itemIndex < node.Tracks.Count;
				default:
					return false;
			}
		}

		public static bool TryGetPrimary(IActObject tree, IActNode node, out (NodeItemType, int) selected)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				selected = default;
				return false;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				(NodeItemType, int) pair = list[i];
				if (IsValid(node, pair))
				{
					selected = pair;
					return true;
				}
			}
			selected = default;
			return false;
		}

		public static bool IsPrimarySelection(IActObject tree, IActNode node, NodeItemType? select = null, int? index = null) =>
			TryGetPrimary(tree, node, out (NodeItemType, int) pair) &&
			(!select.HasValue || select.Value == pair.Item1) &&
			(!index.HasValue || index.Value == pair.Item2);

		public static bool IsSeleted(IActObject tree, IActNode node, NodeItemType? select = null, int? index = null) =>
			GetSelectionState(tree, node, select, index) != SelectionState.None;

		public enum SelectionState
		{
			None,
			Selected,
			Copied,
		}

		public static SelectionState GetSelectionState(IActObject tree, IActNode node, NodeItemType? select = null, int? index = null)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list) || list.Count == 0)
			{
				return SelectionState.None;
			}
			foreach ((NodeItemType, int) pair in list)
			{
				NodeItemType itemType = pair.Item1;
				if (select.HasValue && select.Value != itemType)
				{
					continue;
				}
				int itemIndex = pair.Item2;
				if (index.HasValue && index.Value != itemIndex)
				{
					continue;
				}
				if (!IsValid(node, pair))
				{
					continue;
				}
				bool isCopied = m_CopiedKey.HasValue &&
					key.Item1 == m_CopiedKey.Value.Item1 &&
					key.Item2 == m_CopiedKey.Value.Item2;
				return isCopied ? SelectionState.Copied : SelectionState.Selected;
			}
			return SelectionState.None;
		}

		public static void SetSelected(IActObject tree, IActNode node, NodeItemType select, int index, bool toggle, bool additive)
		{
			// Special case transitions, they cannot be combined with other types
			if (select != NodeItemType.Transition)
			{
				Clear(tree, node, NodeItemType.Transition);
			}
			else if (additive)
			{
				additive = false;
			}
			if (!IsSeleted(tree, node, select, index))
			{
				Select(tree.GetInstanceID(), node.ID, select, index, additive);
			}
			else if (toggle)
			{
				if (additive)
				{
					Clear(tree, node, select, index);
				}
				else
				{
					Clear(tree, node);
				}
			}
		}

		private static void Select(int treeID, int nodeID, NodeItemType select, int index, bool additive)
		{
			(int, int) key = (treeID, nodeID);
			if (key == m_CopiedKey) // If we select something else from the same node then clear copy
			{
				m_CopiedKey = null;
			}
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				list = new SelectionList();
				m_History.Add(key, list);
			}
			if (additive)
			{
				// Remove duplicates, want selection to be last in the list so it becomes "primary"
				for (int i = list.Count - 1; i >= 0; i--)
				{
					(NodeItemType, int) pair = list[i];
					if (pair.Item1 == select && pair.Item2 == index)
					{
						list.RemoveAt(i);
					}
				}
			}
			else
			{
				list.Clear();
			}
			list.Add((select, index));
		}

		public static void AutoSelectTrack(IActObject tree, IActNode node)
		{
			if (node.Tracks.Count > 0 && // Obviously can't select tracks if node doesn't have any
				!IsPrimarySelection(tree, node, NodeItemType.Track) && // If we don't have a track selected, select the first track by default
				!HasCopiedItems()) // Don't select anything if we have copied items otherwise our copied items will be cleared
			{
				List<(Track, int)> tracks = TimedItemUtil.GetSortedTracks(node.Tracks);
				Select(tree.GetInstanceID(), node.ID, NodeItemType.Track, tracks[0].Item2, false);
			}
		}

		public static ITimedItem TryGetTimedItem(IActObject tree, IActNode node)
		{
			if (!TryGetPrimary(tree, node, out (NodeItemType, int) selected))
			{
				return null;
			}
			NodeItemType type = selected.Item1;
			int index = selected.Item2;
			switch (type)
			{
				case NodeItemType.Transition:
					return node.Transitions[index];
				case NodeItemType.Track:
					return node.Tracks[index];
				default:
					return null;
			}
		}

		public static INodeItem TryGetNodeItem(IActObject tree, IActNode node)
		{
			if (!TryGetPrimary(tree, node, out (NodeItemType, int) selected))
			{
				return null;
			}
			NodeItemType type = selected.Item1;
			int index = selected.Item2;
			switch (type)
			{
				case NodeItemType.Condition:
					return node.Conditions[index];
				case NodeItemType.Track:
					return node.Tracks[index];
				default:
					return null;
			}
		}

		public static bool Copy(IActObject tree, IActNode node)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list) || list.Count == 0)
			{
				return false;
			}
			m_CopiedKey = key;
			return true;
		}

		public static IEnumerable<object> GetCopyableItems(IActObject tree)
		{
			if (!m_CopiedKey.HasValue || !m_History.TryGetValue(m_CopiedKey.Value, out SelectionList list))
			{
				yield break;
			}

			int treeInstanceID = m_CopiedKey.Value.Item1;
			if (tree.GetInstanceID() != treeInstanceID)
			{
				// If passed in tree is not where the copied items are located, try to find an object in the asset database that has them
				if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(treeInstanceID, out string guid, out long localID))
				{
					yield break;
				}
				tree = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid)) as IActObject;
				if (tree == null)
				{
					yield break;
				}
			}
			
			int nodeID = m_CopiedKey.Value.Item2;
			if (!tree.TryGetNode(nodeID, out IActNode node))
			{
				yield break;
			}
			foreach ((NodeItemType, int) pair in list)
			{
				int index = pair.Item2;
				switch (pair.Item1)
				{
					case NodeItemType.Condition:
						if (index >= 0 && index < node.Conditions.Count)
						{
							yield return node.Conditions[index];
						}
						break;
					case NodeItemType.Track:
						if (index >= 0 && index < node.Tracks.Count)
						{
							yield return node.Tracks[index];
						}
						break;
				}
			}
		}

		public static IEnumerable<(NodeItemType, int)> GetSelectedPairs(IActObject tree, IActNode node)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				yield break;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				(NodeItemType, int) pair = list[i];
				if (IsValid(node, pair))
				{
					yield return pair;
				}
			}
		}

		public static SerializedProperty GetPrimarySelectedSerailizedProperty(IActObject tree, IActNode node, SerializedProperty sNode)
		{
			if (!TryGetPrimary(tree, node, out (NodeItemType, int) selected))
			{
				return null;
			}
			NodeItemType type = selected.Item1;
			SerializedProperty arrayProperty;
			switch (type)
			{
				case NodeItemType.Condition:
					arrayProperty = sNode.FindPropertyRelative("m_Conditions");
					break;
				case NodeItemType.Transition:
					arrayProperty = sNode.FindPropertyRelative("m_Transitions");
					break;
				case NodeItemType.Track:
					arrayProperty = sNode.FindPropertyRelative("m_Tracks");
					break;
				default:
					return null;
			}
			int index = selected.Item2;
			if (index < 0 || index >= arrayProperty.arraySize)
			{
				return null;
			}
			return arrayProperty.GetArrayElementAtIndex(index);
		}

		public static void OnGUI(IActObject tree, IActNode node, ref SerializedProperty sNode, out bool visibleProperties)
		{
			SerializedProperty sItem = GetPrimarySelectedSerailizedProperty(tree, node, sNode);
			if (sItem == null)
			{
				visibleProperties = false;
				return;
			}

			// Draw timing for ITracks
			// For some reason labels don't seem to automatically resize for the property fields bellow
			EditorGUIUtility.labelWidth = Mathf.Max(EditorGUIUtility.labelWidth, 0.4f * EditorGUIUtility.currentViewWidth);

			INodeItem item = TryGetNodeItem(tree, node);
			if (item != null)
			{
				if (!item._EditorIsValid(tree, node, out string error))
				{
					EditorGUILayout.HelpBox(error, MessageType.Error);
				}
			}

			ITimedItem selectedItem = TryGetTimedItem(tree, node);
			if (selectedItem != null)
			{
				if (selectedItem is Track track)
				{
					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					EditorGUILayout.PropertyField(sItem.FindPropertyRelative("m_Active"));
					SerializedProperty masterSlave = sItem.FindPropertyRelative("m_Major");
					NodeType trackType = (NodeType)EditorGUILayout.EnumPopup(
						"Track Type",
						masterSlave.boolValue ? NodeType.Major : NodeType.Minor);
					masterSlave.boolValue = trackType == NodeType.Major ? true : false;
					EditorGUILayout.EndVertical();
				}

				SerializedProperty startTime = sItem.FindPropertyRelative("m_StartTime");
				SerializedProperty endTime = sItem.FindPropertyRelative("m_EndTime");
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
				GUILayout.Label("Start Time", GUILayout.ExpandWidth(false));
				int startFrames = EditorGUILayout.IntField(Core.Util.SecondsToFrames(startTime.floatValue), GUILayout.Width(32.0f));
				float start = Core.Util.FramesToSeconds(startFrames);
				if (selectedItem.HasEndEvent())
				{
					GUILayout.Label("End Time", GUILayout.ExpandWidth(false));
					float end = -1.0f;
					if (selectedItem.GetEndEventType() == Track.EndEventType.NegativeEndTime)
					{
						EditorGUILayout.LabelField("-1", GUILayout.Width(48.0f));
					}
					else
					{
						int endFrames = EditorGUILayout.IntField(Core.Util.SecondsToFrames(endTime.floatValue), GUILayout.Width(32.0f));
						end = Core.Util.FramesToSeconds(endFrames);
					}
					start = Mathf.Max(0.0f, start);
					if (end > 0.0f || selectedItem.GetEndEventType() == Track.EndEventType.PositiveEndTime)
					{
						end = Mathf.Max(start + Core.Util.SPF30, end);
					}
					endTime.floatValue = end;
				}
				startTime.floatValue = start;
				EditorGUILayout.EndHorizontal();
			}

			// Draw content for tracks and transitions
			visibleProperties = sItem.hasVisibleChildren;
			if (visibleProperties)
			{
				sItem.NextVisible(true);
				int depth = sItem.depth;
				do
				{
					EditorGUILayout.PropertyField(sItem, true);
				}
				while (sItem.NextVisible(false) && sItem.depth == depth);
			}
		}

		public static void SetSelectedIndex(IActObject tree, IActNode node, NodeItemType select, int originalIndex, int newIndex)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				return;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				(NodeItemType, int) pair = list[i];
				if (pair.Item1 != select)
				{
					continue;
				}
				int index = pair.Item2;
				if (index == originalIndex)
				{
					list[i] = (select, newIndex);
				}
			}
		}

		public static void OnInsert(IActObject tree, IActNode node, NodeItemType select, int insertedAtIndex)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				return;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				(NodeItemType, int) pair = list[i];
				if (pair.Item1 != select)
				{
					continue;
				}
				int index = pair.Item2;
				if (index >= insertedAtIndex)
				{
					list[i] = (select, ++index);
				}
			}
		}
		public static void OnDeleted(IActObject tree, IActNode node, NodeItemType select, int deletedIndex)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				return;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				(NodeItemType, int) pair = list[i];
				if (pair.Item1 != select)
				{
					continue;
				}
				int index = pair.Item2;
				if (index == deletedIndex)
				{
					list.RemoveAt(i);
				}
				else if (index > deletedIndex)
				{
					list[i] = (select, --index);
				}
			}
		}

		public static void Clear(IActObject tree, IActNode node, NodeItemType? select = null, int? index = null)
		{
			(int, int) key = (tree.GetInstanceID(), node.ID);
			if (!m_History.TryGetValue(key, out SelectionList list))
			{
				return;
			}
			if (!select.HasValue)
			{
				list.Clear(); // Early out if we don't need to do filtering
				return;
			}
			for (int i = list.Count - 1; i >= 0; i--)
			{
				(NodeItemType, int) pair = list[i];
				if (select.HasValue && select.Value != pair.Item1)
				{
					continue;
				}
				if (index.HasValue && index.Value != pair.Item2)
				{
					continue;
				}
				list.RemoveAt(i);
			}
		}

		public static void ClearEverything()
		{
			m_History.Clear();
			m_CopiedKey = null;
		}
	}
}
