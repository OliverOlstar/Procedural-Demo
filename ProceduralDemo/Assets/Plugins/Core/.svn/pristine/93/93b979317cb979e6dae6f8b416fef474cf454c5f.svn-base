
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Core
{
	[InitializeOnLoad]
	public class EditorHistory
	{
		static List<Object[]> m_History = new List<Object[]>(100);
		static int m_Index = -1;
		static bool m_Ignore = false;

		static EditorHistory()
		{
			//Debug.LogError("Register");
			Selection.selectionChanged += OnSelectionChanged;
		}

		static void OnSelectionChanged()
		{
			//Debug.LogWarning("GWAR");

			if (m_Ignore)
			{
				//Debug.LogError("Ignore");
				m_Ignore = false;
				return;
			}

			if (Selection.objects.Length == 0)
			{
				return;
			}

//			if (m_History.Count == Selection.objects.Length)
//			{
//				bool duplicate = true;
//				for (int i = 0; i < Selection.objects.Length; i++)
//				{
//					for (int j = 0; j < m_History[m_Index].Length; j++)
//					{
//						if (Selection.objects[i].GetInstanceID() == m_History[m_Index][j].GetInstanceID())
//						{
//							duplicate = false;
//							break;
//						}
//					}
//					if (!duplicate)
//					{
//						break;
//					}
//				}
//				if (duplicate)
//				{
//					return;
//				}
//			}

			if (m_Index < m_History.Count - 1)
			{
				m_History.RemoveRange(m_Index + 1, m_History.Count - m_Index - 1);
			}

			m_History.Add(Selection.objects);
			m_Index++;

//			Debug.LogError(HistoryToString());
//			Debug.LogError(string.Format("OnSelectionChanged {0} {1} {2}", 
//				Selection.objects[0].name, 
//				Selection.objects.Length, 
//				m_Index));

			CleanUpList();
		}

		[MenuItem("Core/Selection/Back %;")]
		static void Back()
		{
//			Debug.LogWarning("GWAR");
//			Selection.activeGameObject = new GameObject("Gwar");

			CleanUpList();
			if (m_Index > 0)
			{
				m_Index--;
				m_Ignore = true;
				Selection.objects = m_History[m_Index];
				EditorGUIUtility.PingObject(m_History[m_Index][0]);

//				Debug.LogWarning(string.Format("History-Back {0} {1} {2}", 
//					Selection.objects[0].name, 
//					Selection.objects.Length, 
//					m_Index));
//				Debug.LogWarning(HistoryToString());
			}
//			else
//			{
//				Debug.LogWarning("History-Back Limit");
//			}
		}

		[MenuItem("Core/Selection/Forward %'")]
		static void Forward()
		{
			CleanUpList();
			if (m_Index + 1 < m_History.Count)
			{
				m_Index++;
				m_Ignore = true;

				Selection.objects = m_History[m_Index];

//				Debug.LogWarning(string.Format("History-Forward {0} {1} {2}", 
//					Selection.objects[0].name, 
//					Selection.objects.Length, 
//					m_Index));
//				Debug.LogWarning(HistoryToString());
			}
//			else
//			{
//				Debug.LogWarning("History-Front Limit");
//			}
		}

		static void CleanUpList()
		{
			for (int i = 0; i < m_History.Count; i++)
			{
				// This is so that if we delete our currently selected objects we can use back to get to our previously selected ones
				if (i == m_Index)
				{
					continue;
				}

				Object[] entry = m_History[i];
				int validObjects = 0;
				foreach (Object obj in entry)
				{
					if (obj != null)
					{
						validObjects++;
					}
				}

				bool dupe = true;
				if (validObjects > 0 && i + 1 < m_History.Count)
				{
					Object[] next = m_History[i + 1];
					int nextValidObjects = 0;
					foreach (Object obj2 in next)
					{
						if (obj2 != null)
						{
							nextValidObjects++;
						}
					}
					// Next entry must have the same number of valid objects
					if (validObjects == nextValidObjects)
					{
						// Search for all valid objects in the next entry
						// Assuming that the order of objects may not be guarenteed
						foreach (Object obj1 in entry)
						{
							if (obj1 == null)
							{
								continue;
							}
							bool found = false;
							foreach (Object obj2 in next)
							{
								if (obj2 != null && obj1.GetInstanceID() == obj2.GetInstanceID())
								{
									found = true;
									break;
								}
							}
							if (!found)
							{
								dupe = false;
								break;
							}
						}
					}
					else
					{
						dupe = false;
					}
				}
				else
				{
					dupe = false;
				}

				if (validObjects == 0 || dupe)
				{
					m_History.RemoveAt(i);
					i--;
					if (m_Index >= i)
					{
						m_Index--;
					}
//					if (validObjects == 0)
//					{
//						Debug.LogWarning("History-RemoveEmpty " + HistoryToString());
//					}
//					else
//					{
//						Debug.LogWarning("History-Duplicate" + HistoryToString());
//					}
				}
			}

			if (m_History.Count > 20)
			{
				m_History.RemoveAt(0);
				m_Index--;
//				Debug.LogWarning("History-Truncate " + HistoryToString());
			}
		}

		static string HistoryToString()
		{
			string s = Core.Str.EMPTY;
			for (int i = 0; i < m_History.Count; i++)
			{
				string name = Core.Str.EMPTY;
				foreach (Object obj in m_History[i])
				{
					if (obj != null)
					{
						if (!Core.Str.IsEmpty(name))
						{
							name += ',';
						}
						name += obj.name;
					}
				}
				if (i == m_Index)
				{
					s += "*" + name + "*";
				}
				else
				{
					s += "[" + name + "]";
				}
			}
			return s;
		}
	}
}
