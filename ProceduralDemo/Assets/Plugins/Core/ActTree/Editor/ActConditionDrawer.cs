
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class ActConditionDrawer
{
	List<ActCondition> m_Conditions = new List<ActCondition>();
	List<System.Type> m_AllTypes = new List<System.Type>();
	List<System.Type> m_Types = new List<System.Type>();
	string[] m_ClassNames = {};

	int m_Context = 0;
	int m_MoveUpID = 0;
	int m_MoveDownID = 0;
	int m_DeleteID = 0;

	public void OnGUI(ActTree tree, ref SerializedObject sTree, Act.Node node)
	{
		Initialize(tree);

		PerformActions(tree, ref sTree);

		int polingFrequency = int.MaxValue;
		m_Conditions.Clear();
		foreach (ActCondition condition in tree.GetConditions())
		{
			if (condition == null)
			{
				Debug.LogWarning(Core.Str.Build(tree.name, " has a null condition"));
				continue;
			}
			if (condition.GetNodeID() != node.GetID())
			{
				continue;
			}
			m_Conditions.Add(condition);
			if (condition.RequiresEvent())
			{
				polingFrequency = -1;
			}
			else if (condition.GetPolingFrequency() < polingFrequency)
			{
				polingFrequency = condition.GetPolingFrequency();
			}
		}

		EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

		Rect rect = EditorGUILayout.GetControlRect();
		Rect r1 = rect;
		r1.width = ActTrackDrawer.GetActItemWidth(rect.width);
		int typeIndex = EditorGUI.Popup(r1, 0, m_ClassNames);
		if (typeIndex > 0)
		{
			ActCondition newCondtion = (ActCondition)ScriptableObject.CreateInstance(m_Types[typeIndex - 1]);
			newCondtion.name = m_ClassNames[typeIndex];
			newCondtion.EditorSetNodeID(node.GetID());
			Undo.RegisterCreatedObjectUndo(newCondtion, "AddCondition");
			Undo.RecordObject(tree, "AddCondition");
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
			AssetDatabase.AddObjectToAsset(newCondtion, tree);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newCondtion)); // Update AssetDatabase
			RebuildConditionsList(tree, ref sTree);
		}
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.alignment = TextAnchor.MiddleRight;
		style.normal.textColor = Color.grey;
		if (m_Conditions.Count > 0)
		{
			Rect r2 = rect;
			r2.x += r1.width;
			r2.width -= r1.width;
			EditorGUI.LabelField(r2, polingFrequency < 0 ? "Requires event" : "Poling", style);
		}

		if (m_Conditions.Count == 0)
		{
			return;
		}

		GUILayout.BeginVertical(GUI.skin.box);
		foreach (ActCondition condition in m_Conditions)
		{
			bool selected = ActSelectionDrawer.IsConditionSelected(condition.GetInstanceID());
			GUILayout.BeginHorizontal();
			GUIStyle buttonStyle;
			if (selected)
			{
				buttonStyle = new GUIStyle(GUI.skin.label);
				buttonStyle.fontStyle = FontStyle.Bold;
			}
			else
			{
				buttonStyle = new GUIStyle(GUI.skin.button);
			}
			if (GUILayout.Button(condition.ToString(), buttonStyle, GUILayout.ExpandWidth(false)))
			{
				if (Event.current.button == 0)
				{
					ActSelectionDrawer.SelectedCondition(condition.GetInstanceID());
				}
				else
				{
					OpenContext(condition);
				}
			}
			if (condition.RequiresEvent())
			{
				style.normal.textColor = Color.black;
				style.fontSize = 14;
				EditorGUILayout.LabelField("\u21AF", style);
			}
			GUILayout.EndHorizontal();
			if (selected)
			{
				ActSelectionDrawer.OnGUI(tree, ref sTree, node);
			}
		}
		GUILayout.EndVertical();
	}

	private void Initialize(ActTree tree)
	{
		if (m_AllTypes.Count == 0)
		{
			m_AllTypes = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
						  from assemblyType in domainAssembly.GetTypes()
						  where assemblyType.IsSubclassOf(typeof(ActCondition)) && 
							!assemblyType.IsGenericTypeDefinition && 
							!assemblyType.IsAbstract &&
							assemblyType.GetCustomAttributes(typeof(System.ObsoleteAttribute), false).Length == 0
						  select assemblyType).ToList();
			m_AllTypes.Sort(new System.Comparison<System.Type>(CompareTypes));
		}

		if (m_Context != tree.GetContextMask())
		{
			m_Context = tree.GetContextMask();
			m_Types.Clear();
			foreach (System.Type type in m_AllTypes)
			{
				System.Reflection.MethodInfo m = type.GetMethod(
					"GetContextMask",
					System.Reflection.BindingFlags.Static |
					System.Reflection.BindingFlags.FlattenHierarchy |
					System.Reflection.BindingFlags.Public);
				int trackMask = (int)m.Invoke(null, new object[] { });
				if ((trackMask & m_Context) != 0)
				{
					m_Types.Add(type);
				}
			}

			m_ClassNames = new string[m_Types.Count + 1];
			m_ClassNames[0] = "Add Condition...";
			for (int i = 0; i < m_Types.Count; i++)
			{
				m_ClassNames[i + 1] = m_Types[i].Name;
			}
		}
	}

	private static int CompareTypes(System.Type t1, System.Type t2)
	{
		return t1.Name.CompareTo(t2.Name);
	}

	private void PerformActions(ActTree tree, ref SerializedObject sTree)
	{
		if (m_MoveUpID != 0)
		{
			ActCondition condition = GetCondition(tree, m_MoveUpID);
			if (condition != null)
			{
				List<ActCondition> firstConditions = new List<ActCondition>();
				List<ActCondition> conditions = tree.GetConditions();
				for (int i = 0; i < conditions.Count; i++)
				{
					ActCondition c = conditions[i];
					if (c != null &&
						c.GetNodeID() == condition.GetNodeID())
					{
						if (c.GetInstanceID() != condition.GetInstanceID())
						{
							firstConditions.Add(c);
							continue;
						}
						if (firstConditions.Count > 0)
						{
							int order = firstConditions[firstConditions.Count - 1].GetOrder() + 1;
							SetOrder(c, order);
							for (int j = 1; j < firstConditions.Count; j++)
							{
								SetOrder(firstConditions[firstConditions.Count - j - 1], order + j);
							}
							Undo.RecordObject(tree, "MoveUp");
							Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
							SortConditionList(tree, ref sTree);
						}
						break;
					}
				}
			}
			m_MoveUpID = 0;
		}

		if (m_MoveDownID != 0)
		{
			ActCondition condition = GetCondition(tree, m_MoveDownID);
			if (condition != null)
			{
				List<ActCondition> lastConditions = new List<ActCondition>();
				List<ActCondition> conditions = tree.GetConditions();
				for (int i = conditions.Count - 1; i >= 0; i--)
				{
					ActCondition c = conditions[i];
					if (c != null &&
						c.GetNodeID() == condition.GetNodeID())
					{
						if (c.GetInstanceID() != condition.GetInstanceID())
						{
							lastConditions.Add(c);
							continue;
						}
						if (lastConditions.Count > 0)
						{
							int order = lastConditions[lastConditions.Count - 1].GetOrder() - 1;
							SetOrder(c, order);
							for (int j = 1; j < lastConditions.Count; j++)
							{
								SetOrder(lastConditions[lastConditions.Count - j - 1], order - j);
							}
							Undo.RecordObject(tree, "MoveDown");
							Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
							SortConditionList(tree, ref sTree);
						}
						break;
					}
				}
			}
			m_MoveDownID = 0;
		}

		if (m_DeleteID != 0)
		{
			ActCondition condition = GetCondition(tree, m_DeleteID);
			if (condition != null)
			{
				Undo.RecordObject(tree, "DeleteCondition");
				Undo.DestroyObjectImmediate(condition);
				Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tree));
				EditorUtility.SetDirty(tree);
				RebuildConditionsList(tree, ref sTree);
			}
			m_DeleteID = 0;
		}
	}

	private void SetOrder(ActCondition condition, int order)
	{
		SerializedObject sCondition = new SerializedObject(condition);
		sCondition.FindProperty("m_Order").intValue = order;
		sCondition.ApplyModifiedProperties();
	}

	private ActCondition GetCondition(ActTree tree, int conditionID)
	{
		foreach (ActCondition condition in tree.GetConditions())
		{
			if (condition != null && condition.GetInstanceID() == conditionID)
			{
				return condition;
			}
		}
		return null;
	}

	private void MoveUpContext(object obj)
	{
		ActCondition condition = obj as ActCondition;
		m_MoveUpID = condition.GetInstanceID();
	}

	private void MoveDownContext(object obj)
	{
		ActCondition condition = obj as ActCondition;
		m_MoveDownID = condition.GetInstanceID();
	}

	private void DeleteContext(object obj)
	{
		ActCondition condition = obj as ActCondition;
		m_DeleteID = condition.GetInstanceID();
	}

	private void OpenContext(ActCondition condition)
	{
		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Delete"), false, DeleteContext, condition);
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Move Up"), false, MoveUpContext, condition);
		menu.AddItem(new GUIContent("Move Down"), false, MoveDownContext, condition);
		menu.ShowAsContext();
	}

	public static void RebuildConditionsList(ActTree tree, ref SerializedObject sTree)
	{
		List<ActCondition> conditionList = tree.GetConditions();
		conditionList.Clear();
		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(tree));
		foreach (Object asset in assets)
		{
			ActCondition condition = asset as ActCondition;
			if (condition != null)
			{
				conditionList.Add(condition);
			}
		}
		SortConditionList(tree, ref sTree);
	}

	public static void SortConditionList(ActTree tree, ref SerializedObject sTree)
	{
		tree.GetConditions().Sort(); // Order is important for conditions
		EditorUtility.SetDirty(tree);
		sTree = new SerializedObject(tree);
		ActTreeEditorWindow.Get().UpdateTree(); // Need to inform tree window that tree has changed
	}
}
