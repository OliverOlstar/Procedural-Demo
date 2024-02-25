
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core
{
	public class DependencyWindow : EditorWindow
	{
		public enum SortType
		{
			FileName = 0,
			AssetType
		}

		public class DependencyData
		{
			public Object obj;
			public string path;
			public string label;
			public ulong size;

			public DependencyData(string dependencyPath)
			{
				obj = AssetDatabase.LoadAssetAtPath(dependencyPath, typeof(Object));
				path = dependencyPath.Substring("Assets/".Length);
				label = path;
				if (obj is Texture2D tex)
				{
					size = EditorUtil.EstimateTextureSize(tex, dependencyPath);
					label += " " + Mathf.RoundToInt(tex.width) + "x" + Mathf.RoundToInt(tex.height);
				}
			}
		}
		
		List<DependencyData> m_Objects = new List<DependencyData>();
		string m_RootPath = string.Empty;
		Vector2 m_ScrollPos1 = Vector2.zero;
		SortType m_SortType = SortType.FileName;

		[MenuItem("Assets/Open in Dependencies Window", false, 51)]
		static void Menu()
		{
			DependencyWindow window = EditorWindow.GetWindow<DependencyWindow>(true, "Dependenices");
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			window.Init(path);
		}

		public void Init(string rootPath = "")
		{
			if (rootPath != string.Empty)
			{
				m_RootPath = rootPath;
			}
			if (m_RootPath == string.Empty)
			{
				return;
			}

			bool isFolder = !Path.HasExtension(m_RootPath);
			string[] files = isFolder ?
				Directory.GetFiles(m_RootPath, "*.*", SearchOption.AllDirectories) :
				new string[] { m_RootPath };
			string[] dependencyPaths = AssetDatabase.GetDependencies(files);

			m_Objects = new List<DependencyData>(dependencyPaths.Length);
			foreach (string dependencyPath in dependencyPaths)
			{
				if (dependencyPath.Length < m_RootPath.Length ||
				    dependencyPath.Substring(0, m_RootPath.Length) != m_RootPath)
				{
					DependencyData data = new DependencyData(dependencyPath);
					m_Objects.Add(data);
				}
			}
			Sort();
		}

		void Sort()
		{
			if (m_SortType == SortType.FileName)
			{
				m_Objects = m_Objects.OrderBy(x => x.path).ToList();
			}
			else
			{
				m_Objects = 
					m_Objects.OrderBy(x => (x.obj == null ? "" : x.obj.GetType().ToString())).ToList();
			}
		}

		void OnGUI()
		{
			GUILayout.Label(m_RootPath + " Dependenices " + m_Objects.Count);

			if (GUILayout.Button("Refresh", GUILayout.Width(0.25f * position.width)))
			{
				Init();
			}
			SortType newType = (SortType)EditorGUILayout.EnumPopup(
				m_SortType,
				GUILayout.Width(0.25f * position.width));
			if (newType != m_SortType)
			{
				m_SortType = newType;
				Sort();
			}

			bool selectAll = Selection.objects.Length > 0;
			List<Object> allObjects = new List<Object>(m_Objects.Count);
			foreach (DependencyData data in m_Objects)
			{
				if (data.obj != null)
				{
					allObjects.Add(data.obj);
					if (selectAll && !Selection.objects.Contains(data.obj))
					{
						selectAll = false;
					}
				}
			}
			if (GUILayout.Toggle(selectAll, "Select All", GUILayout.Width(0.25f * position.width)) != selectAll)
			{
				if (!selectAll)
				{
					Selection.objects = allObjects.ToArray();
				}
				else
				{
					Selection.objects = new Object[] {};
				}
			}

			m_ScrollPos1 = GUILayout.BeginScrollView(m_ScrollPos1);
			bool dirty = false;
			List<Object> objs = new List<Object>(Selection.objects.Length);
			foreach (DependencyData data in m_Objects)
			{
				if (data.obj != null)
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.ObjectField(
						data.obj, 
						data.obj.GetType(), 
						false, 
						GUILayout.Width(0.3f * position.width));
					bool selected = Selection.objects.Contains(data.obj);
					if (selected)
					{
						GUILayout.Label(data.label, EditorStyles.boldLabel, GUILayout.Width(0.55f * position.width));
						GUILayout.Label(
							data.size > 0 ? Core.UIUtil.BytesToString(data.size) : Core.Str.EMPTY,
							EditorStyles.boldLabel,
							GUILayout.Width(0.1f * position.width));
					}
					else
					{
						GUILayout.Label(data.label, GUILayout.Width(0.55f * position.width));
						GUILayout.Label(
							data.size > 0 ? Core.UIUtil.BytesToString(data.size) : Core.Str.EMPTY,
							GUILayout.Width(0.1f * position.width));
					}
					if (GUILayout.Toggle(selected, "") != selected)
					{
						dirty = true;
						if (!selected)
						{
							objs.Add(data.obj);
						}
					}
					else if (selected)
					{
						objs.Add(data.obj);
					}
					GUILayout.EndHorizontal();
				}
			}
			if (dirty)
			{
				Selection.objects = objs.ToArray();
			}
			GUILayout.EndScrollView();
		}
	}
}
