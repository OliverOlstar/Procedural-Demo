
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Core
{
	public class ObjPoolDebugWindow : EditorWindow
	{
		[MenuItem("Window/Debug/Object Pool Debugger")]
		private static void CreateWizard()
		{
			ObjPoolDebugWindow window = GetWindow<ObjPoolDebugWindow>("Object Pools");
			window.Show();
		}

		private Vector2 m_Scroll = Vector2.zero;

		private ObjectPoolBase m_InspectActive = null;
		private ObjectPoolBase m_InspectPooled = null;
		private object m_InspectPooledObject = null;

		public void OnEnable()
		{
			ObjectPoolBase._EditorDebugMode = true;
		}
		public void OnDisable()
		{
			ObjectPoolBase._EditorDebugMode = false;
		}

		private void OnInspectorUpdate()
		{
			Repaint();
		}

		private string Parse(string trace)
		{
			string[] traces = trace.Split('\n');
			foreach (string t in traces)
			{
				int i1 = t.IndexOf('(');
				string s1 = i1 >= 0 ? t.Substring(0, i1) : Core.Str.EMPTY;
				int i2 = t.LastIndexOf('\\');
				string s2 = i2 >= 0 ? t.Substring(i2 + 1, t.Length - i2 - 1) : Core.Str.EMPTY;
				Core.Str.AddLine(s1, "  -  ", s2);
			}
			return Core.Str.Finish();
		}

		private void OnGUI()
		{
			if (m_InspectActive != null)
			{
				if (GUILayout.Button("Back to Pools", GUILayout.ExpandWidth(false)))
				{
					m_InspectActive = null;
					return;
				}
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"{m_InspectActive._EditorName} Stack Traces for Active Instances");
				EditorGUILayout.EndHorizontal();

				m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
				Dictionary<string, int> dict = DictionaryPool<string, int>.Request();
				foreach (string stack in m_InspectActive._EditorActiveStackTraces)
				{
					if (dict.ContainsKey(stack))
					{
						dict[stack]++;
					}
					else
					{
						dict.Add(stack, 1);
					}
				}
				foreach (KeyValuePair<string, int> pair in dict)
				{
					EditorGUILayout.LabelField("Instance Count: " + pair.Value);
					EditorGUILayout.TextArea(Parse(pair.Key));
				}
				DictionaryPool<string, int>.Return(dict);
				EditorGUILayout.EndScrollView();
			}
			if (m_InspectPooled != null)
			{
				if (GUILayout.Button("Back to Pools", GUILayout.ExpandWidth(false)))
				{
					m_InspectPooled = null;
					m_InspectPooledObject = null;
					return;
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"{m_InspectPooled._EditorName} Stack Traces for Active Instances");
				EditorGUILayout.EndHorizontal();

				m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
				foreach (KeyValuePair<object, string> pair in m_InspectPooled._EditorPooledObjects)
				{
					bool selected = pair.Key == m_InspectPooledObject;
					if (EditorGUILayout.Foldout(selected, pair.Key.ToString()))
					{
						m_InspectPooledObject = pair.Key;
					}
					if (selected)
					{
						EditorGUILayout.LabelField(pair.Key.ToString());
						EditorGUI.indentLevel++;
						EditorGUILayout.TextArea(Parse(pair.Value));
						EditorGUI.indentLevel--;
					}
				}
				EditorGUILayout.EndScrollView();
			}
			else
			{
				foreach (ObjectPoolBase pool in ObjectPoolBase._EditorPools)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField($"{pool._EditorName} - Total: {pool._EditorActiveCount + pool.PooledCount} Active: {pool._EditorActiveCount} Pooled: {pool.PooledCount}", GUILayout.Width(400.0f));
					if (GUILayout.Button("Inspect Active", GUILayout.ExpandWidth(false)))
					{
						m_InspectActive = pool;
						m_Scroll = Vector2.zero;
					}
					if (GUILayout.Button("Inspect Pooled", GUILayout.ExpandWidth(false)))
					{
						m_InspectPooled = pool;
						m_Scroll = Vector2.zero;
					}
					EditorGUILayout.EndHorizontal();
				}
			}
		}
	}
}
