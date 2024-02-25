
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Act2
{
	public class ActItemSearchWindow : EditorWindow
	{
		private System.Type m_ItemType = null;

		private string[] m_Names = null;
		private List<System.Type> m_Types = new List<System.Type>();

		private Vector2 m_Scroll = Vector2.zero;

		[MenuItem("Window/Act Tree 2/Search Trees For Items")]
		static void Init()
		{
			ActItemSearchWindow window = GetWindow<ActItemSearchWindow>("Act Item Search");
			window.Show();
		}

		private void OnEnable()
		{
			SortedActItemNamesAndTypes sorted = new SortedActItemNamesAndTypes();
			foreach (System.Type type in Core.TypeUtility.GetTypesDerivedFrom(typeof(Track)))
			{
				FillItems(type, "Tracks", sorted);
			}
			foreach (System.Type type in Core.TypeUtility.GetTypesDerivedFrom(typeof(Condition)))
			{
				FillItems(type, "Conditions", sorted);
			}
			sorted.Get("Select...", ref m_Types, out m_Names);
		}

		private void FillItems(System.Type type, string prefix, SortedActItemNamesAndTypes namesToFill)
		{
			if (type.IsGenericTypeDefinition || type.IsAbstract)
			{
				return;
			}
			if (System.Attribute.GetCustomAttribute(type, typeof(NodeItemGroupAttribute), true) is NodeItemGroupAttribute groupAtt &&
				!string.IsNullOrEmpty(groupAtt.GroupName))
			{
				prefix += "/" + groupAtt.GroupName;
			}
			namesToFill.Add(prefix + "/" + type.Name, type);
		}

		private void OnGUI()
		{
			int index = EditorGUILayout.Popup(0, m_Names);
			if (index > 0)
			{
				m_ItemType = m_Types[index - 1];
			}
			if (m_ItemType == null)
			{
				return;
			}

			List<ActTree2> trees = new List<ActTree2>();
			Core.AssetDatabaseUtil.LoadAll(trees);

			Rect typeRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 1.5f);

			m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
			Rect treeRect = EditorGUILayout.GetControlRect();
			int totalCount = 0;
			foreach (ActTree2 tree in trees)
			{
				int count = 0;
				EditorGUI.indentLevel++;
				foreach (Node node in tree.Nodes)
				{
					foreach (System.Type itemType in GetItemTypes(node))
					{
						if (itemType != m_ItemType)
						{
							continue;
						}
						count++;
						Rect r = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
						if (ActNodeEditorWindow2.NodeButton(r, tree, node))
						{
							ActTreeEditorWindow2.OpenTree(tree, node.GetID());
						}
						break;
					}
				}
				EditorGUI.indentLevel--;
				if (count > 0)
				{
					EditorGUI.LabelField(treeRect, new GUIContent(tree.name, EditorGUIUtility.GetIconForObject(tree)));
					treeRect = EditorGUILayout.GetControlRect();
					totalCount += count;
				}
			}
			EditorGUILayout.EndScrollView();

			EditorGUI.LabelField(typeRect, GUIContent.none, EditorStyles.helpBox);
			typeRect.xMin += 8;
			EditorGUI.LabelField(typeRect, "Found " + totalCount + " " + m_ItemType.Name + "(s)", EditorStyles.boldLabel);
		}

		private IEnumerable<System.Type> GetItemTypes(Node node)
		{
			if (typeof(Track).IsAssignableFrom(m_ItemType))
			{
				foreach (Track track in node.Tracks)
				{
					if (track != null)
					{
						yield return track.GetType();
					}
				}
			}
			else if (typeof(Condition).IsAssignableFrom(m_ItemType))
			{
				foreach (Condition condition in node.Conditions)
				{
					if (condition != null)
					{
						yield return condition.GetType();
					}
				}
			}
		}
	}
}
