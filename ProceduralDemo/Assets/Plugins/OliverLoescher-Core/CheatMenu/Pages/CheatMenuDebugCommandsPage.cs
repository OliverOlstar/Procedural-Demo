using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace OCore.CheatMenu
{
	public class CheatMenuDebugCommandsPage : CheatMenuPage
	{
		public override CheatMenuGroup Group => CheatMenuCoreGroups.OCore;
		public override string Name => "Commands";

		private string m_Category;
		private List<string> m_Categories;
		private Dictionary<string, List<(DebugCommandAttribute, MethodInfo)>> m_Methods;

		protected override void OnInitialize()
		{
			m_Category = DebugCommandAttribute.DefaultCategory;
			m_Categories = new List<string>();
			m_Methods = new Dictionary<string, List<(DebugCommandAttribute, MethodInfo)>>();
			foreach (Type type in TypeUtility.GetAllTypes())
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
						m_Categories.AddUniqueItem(attribute.Category);
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
		}

		public override void DrawGUI()
		{
			if (m_Methods.Count <= 0)
			{
				GUILayout.Label("No Debug Functions found");
				return;
			}
			int index = 0;
			for (int i = 0; i < m_Categories.Count; ++i)
			{
				if (m_Categories[i] == m_Category)
				{
					index = i;
					break;
				}
			}
			index = GUILayout.SelectionGrid(index, m_Categories.ToArray(), 3);
			m_Category = m_Categories[index];
			GUILayout.Space(10f);
			if (!m_Methods.TryGetValue(m_Category, out List<(DebugCommandAttribute, MethodInfo)> methods))
			{
				return;
			}
			foreach ((DebugCommandAttribute, MethodInfo) method in methods)
			{
				if (GUILayout.Button(method.Item1.Label))
				{
					method.Item2.Invoke(null, null);
					Close();
				}
			}
		}
	}
}
