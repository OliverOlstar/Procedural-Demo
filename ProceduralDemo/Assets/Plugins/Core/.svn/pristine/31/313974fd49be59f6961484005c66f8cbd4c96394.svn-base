
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core
{
	public class PoolEditorWindow : EditorWindow
	{
		private Vector2 m_ScrollPosition = Vector2.zero;

		[MenuItem("Window/Debug/Pool Debugger")]
		private static void CreateWizard()
		{
			PoolEditorWindow window = GetWindow<PoolEditorWindow>("Pools");
			window.Show();
		}

		private List<PoolDirectorBase> m_Directors = new List<PoolDirectorBase>();
		private List<PoolInstance> m_Instances = new List<PoolInstance>();
		private HashSet<int> m_FoldOuts = new HashSet<int>();
		private System.Type m_SelectedType = default;

		[MenuItem("Window/Debug/Pool Debugger Write To CSV")]
		public static void WriteSnapshotToCSV()
		{
			List<PoolDirectorBase> directors = new List<PoolDirectorBase>();
			Director.GetAll<PoolDirectorBase>(directors);
			if (directors.Count == 0)
			{
				return;
			}

			string path = EditorUtility.SaveFilePanel("Save Snapshot File", Application.dataPath, "PoolDebugSnapshot", "csv");
			using (StreamWriter writer = new StreamWriter(path))
			{

				writer.WriteLine("Director,Asset,Count");
				foreach (PoolDirectorBase director in directors)
				{
					string directorName = director.GetType().Name;
					foreach (int id in director.GetIDs())
					{
						List<PoolInstance> instances = new List<PoolInstance>();
						director.GetDebugItems(id, out string name, instances);
						writer.WriteLine(directorName + "," + name + "," + instances.Count);
					}
				}
			}
		}

		private void OnInspectorUpdate()
		{
			Repaint();
		}

		private void OnGUI()
		{
			m_Directors.Clear();
			Director.GetAll<PoolDirectorBase>(m_Directors);
			if (m_Directors.Count == 0)
			{
				return;
			}

			if (GUILayout.Button("Write To CSV"))
			{
				WriteSnapshotToCSV();
			}

			int selectedIndex = 0;
			string[] names = new string[m_Directors.Count];
			for (int i = 0; i < m_Directors.Count; i++)
			{
				PoolDirectorBase director = m_Directors[i];
				if (director.GetType() == m_SelectedType)
				{
					selectedIndex = i;
				}
				names[i] = director.GetType().Name;
			}

			selectedIndex = EditorGUILayout.Popup(selectedIndex, names);
			PoolDirectorBase manager = m_Directors[selectedIndex];
			m_SelectedType = manager.GetType();

			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

			foreach (int id in manager.GetIDs())
			{
				int active = 0;
				int total = 0;
				manager.GetDebugItems(id, out string name, m_Instances);
				for (int i = 0; i < m_Instances.Count; i++)
				{
					PoolInstance instance = m_Instances[i];
					if (instance == null || !instance.IsValid())
					{
						continue;
					}
					total++;
					if (instance.IsActive())
					{
						active++;
					}
				}

				bool foldOut = m_FoldOuts.Contains(id);
				if (!EditorGUILayout.Foldout(foldOut, name + " (" + active + "/" + total + ")"))
				{
					if (foldOut)
					{
						m_FoldOuts.Remove(id);
					}
					continue;
				}
				if (!foldOut)
				{
					m_FoldOuts.Add(id);
				}

				EditorGUI.indentLevel++;
				for (int i = 0; i < m_Instances.Count; i++)
				{
					PoolInstance instance = m_Instances[i];
					if (instance == null || !instance.IsValid())
					{
						continue;
					}
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.ObjectField(instance.Transform.name, instance.Transform, typeof(Transform), true);
					if (instance.IsActive())
					{
						GUILayout.Label("active");
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndScrollView();
		}
	}
}
