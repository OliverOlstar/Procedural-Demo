using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ODev
{
	public class DebugCommandGUI
	{
		private Dictionary<string, List<(DebugCommandAttribute, MethodInfo)>> m_Methods = new();
		private string[] m_Categories = null;
		private int m_CategoryIndex = 0;

		public DebugCommandGUI()
		{
			foreach (System.Type type in Util.Types.GetAllTypes())
			{
				try
				{
					MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					for (int i = 0; i < methods.Length; ++i)
					{
						MethodInfo method = methods[i];
						// static void MethodName() signatures only
						if (!method.IsStatic || method.ReturnType != typeof(void) || method.GetParameters().Length > 0)
						{
							continue;
						}
						DebugCommandAttribute attribute = method.GetCustomAttribute<DebugCommandAttribute>();
						if (attribute == null)
						{
							continue;
						}

						if (!m_Methods.TryGetValue(attribute.Category, out List<(DebugCommandAttribute, MethodInfo)> list))
						{
							list = new List<(DebugCommandAttribute, MethodInfo)>();
							m_Methods[attribute.Category] = list;
						}
						list.Add((attribute, methods[i]));
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError(e);
				}
			}
			m_Categories = new string[m_Methods.Count];
			int index = 0;
			foreach (string catagory in m_Methods.Keys)
			{
				m_Categories[index] = catagory;
				index++;
			}
		}

		public void DrawGUI()
		{
			if (m_Methods.Count <= 0)
			{
				GUILayout.Label("No Debug Functions found");
				return;
			}
			if (m_Categories.Length > 1)
			{
				int xCount = Mathf.Min(m_Categories.Length, 3);
				m_CategoryIndex = GUILayout.SelectionGrid(m_CategoryIndex, m_Categories, xCount);
			}
			GUILayout.Space(10f);
			string selectedCatagory = m_Categories[m_CategoryIndex];
			foreach ((DebugCommandAttribute, MethodInfo) method in m_Methods[selectedCatagory])
			{
				bool enabled = Application.isPlaying;
				GUIContent content = new(method.Item1.Label, enabled ? method.Item1.Tooltip : "Editor must be in playing");
				GUI.enabled = enabled;
				bool pressed = GUILayout.Button(content);
				GUI.enabled = true;

				if (pressed)
				{
					method.Item2.Invoke(null, null);
				}
			}
		}
	}

	public class DebugCommandAttribute : System.Attribute
	{
		public const string DefaultCategory = "Default";

		public string Category { get; private set; }
		public string Label { get; private set; }
		public string Tooltip { get; private set; }

		public DebugCommandAttribute(string label, string category = DefaultCategory, string tooltip = null)
		{
			Label = label;
			Category = string.IsNullOrEmpty(category) ? DefaultCategory : category;
			Tooltip = tooltip;
		}
	}
}
