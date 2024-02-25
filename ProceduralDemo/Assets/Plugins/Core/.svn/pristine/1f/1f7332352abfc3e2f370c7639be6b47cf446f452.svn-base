using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core
{
	public class ABMTools : EditorWindow
	{
		[MenuItem("Assets/AssetBundles/ABM Tools")]
		static void ShowDialog()
		{
			GetWindow<ABMTools>("ABM Tools").Show();
		}

		private const float TOGGLE_WIDTH = 150f;
		private const float BUTTON_WIDTH = 100f;

		private Vector2 m_ScrollPosition = Vector2.zero;
		private string m_ManagerName = Str.EMPTY;
		private string m_StateKey = Str.EMPTY;

		void OnGUI()
		{
			DrawHeader();
			DrawMainInfo();
		}

		private void DrawHeader()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				bool simMode = AssetBundleUtil.IsSimMode();
				simMode = EditorGUILayout.ToggleLeft("Simulation Mode", simMode, GUILayout.Width(TOGGLE_WIDTH));
				AssetBundleUtil.SetSimulationMode(simMode);
				StreamingBundlesManager.Enabled = EditorGUILayout.ToggleLeft("Streaming Bundles", StreamingBundlesManager.Enabled, GUILayout.Width(TOGGLE_WIDTH));
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				AssetBundleUtil.UseLocalBundlePath = EditorGUILayout.ToggleLeft("Local Bundles", AssetBundleUtil.UseLocalBundlePath, GUILayout.Width(TOGGLE_WIDTH));
				using (new EditorGUI.DisabledGroupScope(!AssetBundleUtil.UseLocalBundlePath))
				{
					string path = EditorGUILayout.DelayedTextField(AssetBundleUtil.LocalBundlePath);
					if (System.IO.Directory.Exists(path))
					{
						AssetBundleUtil.LocalBundlePath = path;
					}
					if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(BUTTON_WIDTH)))
					{
						path = EditorUtility.OpenFolderPanel("Asset Bundles", AssetBundleUtil.LocalBundlePath, string.Empty);
						if (System.IO.Directory.Exists(path))
						{
							AssetBundleUtil.LocalBundlePath = path;
						}
					}
					if (GUILayout.Button("Default", EditorStyles.miniButton, GUILayout.Width(BUTTON_WIDTH)))
					{
						AssetBundleUtil.UseDefaultLocalBundlePath();
					}
				}
			}
			if (GUILayout.Button("Clear Cache", EditorStyles.miniButton, GUILayout.Width(BUTTON_WIDTH)))
			{
				Caching.ClearCache();
			}
		}

		private void DrawMainInfo()
		{
			List<string> names = ABMs.GetManagerNames();
			if (names.Count == 0)
			{
				m_StateKey = Str.EMPTY;
				return;
			}
			int index = names.IndexOf(m_ManagerName);
			int newIndex = EditorGUILayout.Popup(index, names.ToArray());
			newIndex = Mathf.Clamp(newIndex, 0, names.Count - 1);
			m_ManagerName = names[newIndex];
			if (newIndex != index)
			{
				m_StateKey = Core.Str.EMPTY;
			}
			ABMBase abm = ABMs.Get(newIndex);
			List<string> states = abm.GetDebugStates();
			if (states.Count == 0)
			{
				m_StateKey = Core.Str.EMPTY;
				return;
			}
			List<float> times = abm.GetDebugTimeStamps();
			string[] popupList = new string[times.Count];
			for (int i = 0; i < times.Count; i++)
			{
				popupList[i] = times[i].ToString("F2");
			}
			int stateIndex = !Core.Str.IsEmpty(m_StateKey) ? ArrayUtility.IndexOf<string>(popupList, m_StateKey) : -1;
			if (stateIndex < 0)
			{
				stateIndex = popupList.Length - 1;
			}
			int newStateIndex = EditorGUILayout.Popup(stateIndex, popupList);
			m_StateKey = newStateIndex == popupList.Length - 1 ? Core.Str.EMPTY : popupList[newStateIndex];
			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
			EditorGUILayout.TextArea(states[newStateIndex]);
			EditorGUILayout.EndScrollView();
		}

		void Update()
		{
			for (int i = 0; i < ABMs.GetManagerCount(); i++)
			{
				ABMs.Get(i).SetDebugMode(true);
			}
		}

		void OnDestroy()
		{
			for (int i = 0; i < ABMs.GetManagerCount(); i++)
			{
				ABMs.Get(i).SetDebugMode(false);
			}
		}

		void OnInspectorUpdate()
		{
			if (Str.IsEmpty(m_StateKey))
			{
				Repaint();
			}
		}
	}
}
