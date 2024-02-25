
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Act2
{
	public class ConditionDrawer
	{
		private ActNodeCreateItemDrawer<Condition> m_CreateConditionDrawer = new ActNodeCreateItemDrawer<Condition>();

		private NodeItemContextMenu m_ContextMenu = new NodeItemContextMenu(
			new DeleteContextMenu(),
			new MoveContextMenu(),
			new DuplicateContextMenu(),
			new CopyPasteContextMenu());

		public void OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			m_CreateConditionDrawer.Initialize(tree, node);

			m_ContextMenu.OnGUI(tree, ref sTree, node, ref sNode);

			SerializedProperty sConditions = sNode.FindPropertyRelative("m_Conditions");

			EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);
			Rect rect = EditorGUILayout.GetControlRect();
			float itemWidth = TrackDrawer.GetActItemWidth(rect.width);
			Rect r1 = rect;
			r1.width = itemWidth;

			if (m_CreateConditionDrawer.GUI(r1, out object instance))
			{
				sConditions.arraySize++;
				SerializedProperty ele = sConditions.GetArrayElementAtIndex(sConditions.arraySize - 1);
				ele.managedReferenceValue = instance;
			}

			if (sConditions.arraySize == 0)
			{
				return;
			}

			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.alignment = TextAnchor.MiddleRight;
			style.normal.textColor = Color.grey;
			Rect r2 = rect;
			r2.x += r1.width;
			r2.width -= r1.width;

			if (node.IsEventRequired(out System.Type requiredEventType))
			{
				EditorGUI.LabelField(r2, $"{NodeDrawer.EVENT_ICON} Requires {Util.EventTypeToString(requiredEventType)}", style);
			}
			else
			{
				EditorGUI.LabelField(r2, $"{NodeDrawer.POLL_ICON} Poling", style);
			}

			if (node.Conditions.Count == 0)
			{
				return;
			}

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			for (int i = 0; i < node.Conditions.Count; i++)
			{
				Condition condition = node.Conditions[i];
				if (condition == null)
				{
					Debug.LogWarning(Core.Str.Build(tree.name, " has a null condition"));
					continue;
				}
				SelectionDrawer.SelectionState selected = SelectionDrawer.GetSelectionState(tree, node, NodeItemType.Condition, i);

				GUIContent content = new GUIContent(condition.ToString());
				Rect sourceRect = EditorGUILayout.GetControlRect();
				Rect itemRect = sourceRect;
				GUIStyle itemStyle;
				if (selected != SelectionDrawer.SelectionState.None)
				{
					itemStyle = new GUIStyle(GUI.skin.label);
					itemStyle.fontStyle = FontStyle.Bold;
					itemStyle.alignment = TextAnchor.MiddleLeft;
					itemStyle.normal.textColor = TrackDrawer.SELECTED_TEXT_COLOR;
					TrackDrawer.DrawSelectionRect(sourceRect, selected);
				}
				else
				{
					itemStyle = new GUIStyle(GUI.skin.button);
					itemStyle.alignment = TextAnchor.MiddleLeft;
					if (!condition._EditorIsValid(tree, node, out _))
					{
						itemStyle.normal.textColor = TrackDrawer.ERROR_TEXT_COLOR;
					}
					itemRect.width = Mathf.Max(itemWidth, itemStyle.CalcSize(content).x);
				}
				itemRect.width = Mathf.Max(itemWidth, itemStyle.CalcSize(content).x);
				EditorGUI.LabelField(itemRect, content, itemStyle);
				itemRect.x += itemRect.width;
				itemRect.width = 16.0f;
				style.normal.textColor = itemStyle.normal.textColor;
				style.alignment = TextAnchor.MiddleRight;
				style.fontSize = 10;
				if (condition.IsEventRequired(out _))
				{
					GUI.enabled = false;
					EditorGUI.LabelField(sourceRect, NodeDrawer.EVENT_ICON, style);
					GUI.enabled = true;
				}
				else
				{
					GUI.enabled = false;
					EditorGUI.LabelField(sourceRect, NodeDrawer.POLL_ICON + " " + condition.GetPolingFrequency(), style);
					GUI.enabled = true;
				}

				if (GUI.Button(sourceRect, string.Empty, GUIStyle.none))
				{
					SelectionDrawer.SetSelected(
						tree, 
						node, 
						NodeItemType.Condition, 
						i,
						Event.current.button == 0,
						Event.current.control);
					if (Event.current.button == 1)
					{
						m_ContextMenu.Show(tree, node, condition, i);
					}
				}

				if (SelectionDrawer.IsPrimarySelection(tree, node, NodeItemType.Condition, i))
				{
					EditorGUI.indentLevel++;
					SelectionDrawer.OnGUI(tree, node, ref sNode, out bool visibleProperties);
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();
		}
	}
}
