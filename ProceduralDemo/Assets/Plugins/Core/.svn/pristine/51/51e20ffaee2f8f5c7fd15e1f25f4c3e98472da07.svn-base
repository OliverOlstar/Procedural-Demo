
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Core
{
	public class ReferencesWindow : EditorWindow
	{
		public enum SortType
		{
			InUse = 0,
			FileName,
			AssetType
		}

		public class RefData
		{
			public string path = Core.Str.EMPTY;
			public string name = Core.Str.EMPTY;
			public Object obj;
			public bool inUse = false;
			public HashSet<string> refernces = null;
			public bool showReferences = false;
		}

		static Dictionary<string, HashSet<string>> s_DepDB = null;

		List<RefData> m_Objects = new List<RefData>();

		Vector2 m_ScrollPos1 = Vector2.zero;
		SortType m_SortType = SortType.InUse;

		[MenuItem("Assets/Check References", false, 51)]
		static void Menu()
		{
			ReferencesWindow window = EditorWindow.GetWindow<ReferencesWindow>(true, "References");
			window.Init();
		}

		public void Init()
		{
			m_Objects.Clear();

			if (s_DepDB == null)
			{
				BuildDB();
				if (s_DepDB == null) // Building DB could be cancelled
				{
					return;
				}
			}
			foreach (string guid in Selection.assetGUIDs)
			{
				CollectReferences(AssetDatabase.GUIDToAssetPath(guid));
			}
			Sort();
		}

		void CollectReferences(string path)
		{
			bool isFolder = !Path.HasExtension(path);
			string[] files = isFolder ?
				Directory.GetFiles(path, "*.*", SearchOption.AllDirectories) :
				new string[] { path };
			m_Objects = new List<RefData>(files.Length);
			foreach (string file in files)
			{
				Object obj = AssetDatabase.LoadAssetAtPath<Object>(file);
				if (obj != null)
				{
					RefData data = new RefData();
					data.obj = obj;
					data.path = file.Replace("\\", "/");
					data.name = Path.GetFileName(data.path);
					data.inUse = s_DepDB.ContainsKey(data.path);
					if (data.inUse)
					{
						data.refernces = s_DepDB[data.path];
					}
					m_Objects.Add(data);
				}
			}
		}

		static bool Deep
		{
			get { return EditorPrefs.GetBool("CheckReferences_Deep", true); }
			set { EditorPrefs.SetBool("CheckReferences_Deep", value); }
		}
		static bool IncludeScenes
		{
			get { return EditorPrefs.GetBool("CheckReferences_Scenes", true); }
			set { EditorPrefs.SetBool("CheckReferences_Scenes", value); }
		}
		static bool IncludeResources
		{
			get { return EditorPrefs.GetBool("CheckReferences_Resources", true); }
			set { EditorPrefs.SetBool("CheckReferences_Resources", value); }
		}
		static bool IncludeAssetBundleScenes
		{
			get { return EditorPrefs.GetBool("CheckReferences_AssetBundleScenes", true); }
			set { EditorPrefs.SetBool("CheckReferences_AssetBundleScenes", value); }
		}
		static bool IncludeAssetBundleAssets
		{
			get { return EditorPrefs.GetBool("CheckReferences_AssetBundleAssets", true); }
			set { EditorPrefs.SetBool("CheckReferences_AssetBundleAssets", value); }
		}

		static void BuildDB()
		{
			EditorUtility.DisplayProgressBar("References Window", "Collecting dependiences...", 0.0f);

			 // Make sure latest changes are saved before building
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

			HashSet<string> assetPaths = new HashSet<string>();

			if (IncludeScenes)
			{
				EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
				for (int i = 0; i < scenes.Length; i++)
				{
					EditorBuildSettingsScene scene = scenes[i];
					string path = scene.path;
					if (!assetPaths.Contains(path))
					{
						assetPaths.Add(path);
					}
				}
			}

			string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
			foreach (string bundle in bundleNames)
			{
				string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);
				foreach (string asset in assets)
				{
					if (assetPaths.Contains(asset))
					{
						continue;
					}
					bool isScene = asset.EndsWith(".unity");
					if ((isScene && IncludeAssetBundleScenes) ||
						(!isScene && IncludeAssetBundleAssets))
					{
						assetPaths.Add(asset);
					}
				}
			}

			if (IncludeResources)
			{
				string[] guids = AssetDatabase.FindAssets("", new string[] { "Assets/Resources" });
				foreach (string guid in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					if (!assetPaths.Contains(path))
					{
						assetPaths.Add(path);
					}
				}
			}

			s_DepDB = new Dictionary<string, HashSet<string>>();

			if (Deep)
			{
				int count = 0;
				int total = assetPaths.Count;
				foreach (string assetPath in assetPaths)
				{
					count++;
					if (EditorUtility.DisplayCancelableProgressBar(
							"References Window",
							Core.Str.Build("Collecting dependiences ", count.ToString(), "/", total.ToString()),
							(float)count / total))
					{
						s_DepDB = null;
						break;
					}
					string[] dependencyPaths = AssetDatabase.GetDependencies(assetPath);
					foreach (string dep in dependencyPaths)
					{
						HashSet<string> refs = null;
						if (s_DepDB.TryGetValue(dep, out refs))
						{
							refs.Add(assetPath);
						}
						else
						{
							refs = new HashSet<string>();
							refs.Add(assetPath);
							s_DepDB.Add(dep, refs);
						}
					}
				}
			}
			else
			{
				EditorUtility.DisplayProgressBar("References Window", "Collecting dependiences...", 0.5f);
				string[] dependencyPaths = AssetDatabase.GetDependencies(assetPaths.ToArray());
				foreach (string dependencyPath in dependencyPaths)
				{
					s_DepDB.Add(dependencyPath, null);
				}
			}

			EditorUtility.ClearProgressBar();
		}

		void Sort()
		{
			if (m_SortType == SortType.FileName)
			{
				m_Objects = m_Objects.OrderBy(x => x.obj.name).ToList();
			}
			else if (m_SortType == SortType.AssetType)
			{
				m_Objects = m_Objects.OrderBy(x => (x.obj == null ? "" : x.obj.GetType().ToString())).ToList();
			}
			else if (m_SortType == SortType.InUse)
			{
				m_Objects = m_Objects.OrderBy(x => x.inUse).ToList();
			}
		}

		void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Build dependencies catalogue", GUILayout.ExpandWidth(false)))
			{
				BuildDB();
				m_Objects.Clear();
			}
			Deep = GUILayout.Toggle(Deep, "Deep", GUILayout.ExpandWidth(false));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Incude ", GUILayout.ExpandWidth(false));
			IncludeScenes = GUILayout.Toggle(IncludeScenes, "Scenes", GUILayout.ExpandWidth(false));
			IncludeResources = GUILayout.Toggle(IncludeResources, "Resources", GUILayout.ExpandWidth(false));
			IncludeAssetBundleScenes = GUILayout.Toggle(IncludeAssetBundleScenes, "Asset bundle scenes", GUILayout.ExpandWidth(false));
			IncludeAssetBundleAssets = GUILayout.Toggle(IncludeAssetBundleAssets, "Asset bundle assets", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			if (s_DepDB == null)
			{
				return;
			}

			GUILayout.Label(Core.Str.Build("Selected ", m_Objects.Count.ToString()));

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
			foreach (RefData data in m_Objects)
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

			GUILayout.BeginHorizontal();
			if (GUILayout.Toggle(selectAll, "Select All", GUILayout.ExpandWidth(false)) != selectAll)
			{
				if (!selectAll)
				{
					Selection.objects = allObjects.ToArray();
				}
				else
				{
					Selection.objects = new Object[] { };
				}
			}
			if (GUILayout.Button("Select referenced", GUILayout.ExpandWidth(false)))
			{
				List<Object> used = new List<Object>(m_Objects.Count);
				foreach (RefData data in m_Objects)
				{
					if (data.obj != null && data.inUse)
					{
						used.Add(data.obj);
					}
				}
				Selection.objects = used.ToArray();
			}
			if (GUILayout.Button("Select not referenced", GUILayout.ExpandWidth(false)))
			{
				List<Object> notUsed = new List<Object>(m_Objects.Count);
				foreach (RefData data in m_Objects)
				{
					if (data.obj != null && !data.inUse)
					{
						notUsed.Add(data.obj);
					}
				}
				Selection.objects = notUsed.ToArray();
			}
			GUILayout.EndHorizontal();

			m_ScrollPos1 = GUILayout.BeginScrollView(m_ScrollPos1);
			bool dirty = false;
			List<Object> objs = new List<Object>(Selection.objects.Length);
			foreach (RefData data in m_Objects)
			{
				if (data.obj != null)
				{
					GUILayout.BeginHorizontal();
					bool selected = Selection.objects.Contains(data.obj);
					if (GUILayout.Toggle(selected, "", GUILayout.ExpandWidth(false)) != selected)
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
					GUILayout.Label(data.inUse ? "\u2714" : "\u25CB", GUILayout.Width(16));
					EditorGUILayout.ObjectField(
						data.obj,
						data.obj.GetType(),
						false,
						GUILayout.Width(0.3f * position.width));
					string name = data.name;
					if (selected)
					{
						EditorGUILayout.LabelField(name, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
						//GUILayout.Label(name, EditorStyles.boldLabel, GUILayout.Width(0.65f * position.width));
					}
					else
					{
						EditorGUILayout.LabelField(name, GUILayout.ExpandWidth(false));
						//GUILayout.Label(name, GUILayout.Width(0.65f * position.width));
					}
					if (data.refernces != null &&
						GUILayout.Button(data.showReferences ? "Hide references" : "Show references " + data.refernces.Count, GUILayout.ExpandWidth(false)))
					{
						data.showReferences = !data.showReferences;
					}
					GUILayout.EndHorizontal();
					if (data.refernces != null && data.showReferences)
					{
						EditorGUI.indentLevel++;
						foreach (string s in data.refernces)
						{
							EditorGUILayout.LabelField(s);
						}
						EditorGUI.indentLevel--;
					}
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
