using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
	public class SceneWindow : EditorWindow
	{
		class EditorScene
		{
			public string AssetPath { get; private set; }
			public string AssetName { get; private set; }

			public EditorScene(string guid)
			{
				AssetPath = AssetDatabase.GUIDToAssetPath(guid);
				AssetName = Path.GetFileName(AssetPath);
			}

			public bool DrawGUI(GUIStyle style, string currentScene)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					using (new EditorGUI.DisabledGroupScope(AssetPath == currentScene))
					{
						if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(100f)))
						{
							return true;
						}
					}
					return GUILayout.Button(AssetName, style);
				}
			}
		}

		[MenuItem("Window/Tools/Scene Window %#W", false, 100)]
		private static void ShowDialog()
		{
			SceneWindow window = GetWindow<SceneWindow>("Scene Window");
			window.Setup();
			window.Show();
		}

		private delegate void KeyDownDelegate();

		private const string SEARCH_KEY = "SCENE_TOOL_SEARCH";
		private const string SELECTED_KEY = "SCENE_TOOL_SELECTED";
		private const string SEARCH_CONTROL_NAME = "SCENE_TOOL_SEARCH_FIELD";

		private GUIStyle m_SearchTextFieldStyle;
		private GUIStyle m_SearchCancelButtonStyle;
		private GUIStyle m_SelectedActiveStyle;
		private GUIStyle m_SelectedInactiveStyle;
		private Dictionary<KeyCode, KeyDownDelegate> m_KeyDownHandlers;

		private EditorPrefsString m_Search;
		private EditorPrefsString m_SelectedScene;

		private string m_CurrentScene;
		private string m_FocusedControl;
		private int m_SelectedIndex;
		private List<EditorScene> m_Scenes;
		private Vector2 m_ScrollPosition;

		private void Awake()
		{
			m_SearchTextFieldStyle = "ToolbarSearchTextField";
			m_SearchCancelButtonStyle = "ToolbarSearchCancelButton";
			m_SelectedActiveStyle = "LODSliderRangeSelected";
			m_SelectedInactiveStyle = "LODBlackBox";
			m_KeyDownHandlers = new Dictionary<KeyCode, KeyDownDelegate>()
			{
				{ KeyCode.UpArrow, OnUpArrowDown },
				{ KeyCode.DownArrow, OnDownArrowDown },
				{ KeyCode.Escape, OnEscapeDown },
				{ KeyCode.Return, OnReturnDown },
				{ KeyCode.KeypadEnter, OnReturnDown },
			};
		}
		
		private void OnEnable()
		{
			m_Search = new EditorPrefsString(SEARCH_KEY);
			m_SelectedScene = new EditorPrefsString(SELECTED_KEY);
		}

		private void Setup()
		{
			m_Scenes = new List<EditorScene>();
			m_SelectedIndex = -1;
			string[] guids = AssetDatabase.FindAssets("t:Scene, " + m_Search);
			for (int i = 0; i < guids.Length; ++i)
			{
				EditorScene editorScene = new(guids[i]);
				m_Scenes.Add(editorScene);
				if (m_SelectedScene == editorScene.AssetPath)
				{
					m_SelectedIndex = i;
					m_SelectedScene.Value = editorScene.AssetPath;
				}
			}
			if (m_SelectedIndex == -1)
			{
				m_SelectedScene.Value = string.Empty;
			}
			EditorApplication.update -= Repaint;
			EditorApplication.update += Repaint;
		}

		private void OnGUI()
		{
			m_CurrentScene = SceneManager.GetActiveScene().path;
			HandleKeyboardEvents();
			DrawSearchBar();
			DrawScenes();
		}

		private void HandleKeyboardEvents()
		{
			if (Event.current.type != EventType.KeyDown)
			{
				return;
			}
			m_FocusedControl = GUI.GetNameOfFocusedControl();
			KeyDownDelegate handler;
			if (!m_KeyDownHandlers.TryGetValue(Event.current.keyCode, out handler))
			{
				return;
			}
			handler();
		}

		private void OnUpArrowDown()
		{
			m_SelectedIndex = Mathf.Max(0, m_SelectedIndex - 1);
			m_SelectedScene.Value = m_Scenes[m_SelectedIndex].AssetPath;
			if (m_FocusedControl == SEARCH_CONTROL_NAME)
			{
				GUI.FocusControl(null);
			}
		}

		private void OnDownArrowDown()
		{
			m_SelectedIndex = Mathf.Min(m_Scenes.Count - 1, m_SelectedIndex + 1);
			m_SelectedScene.Value = m_Scenes[m_SelectedIndex].AssetPath;
			if (m_FocusedControl == SEARCH_CONTROL_NAME)
			{
				GUI.FocusControl(null);
			}
		}

		private void OnEscapeDown()
		{
			if (m_FocusedControl != SEARCH_CONTROL_NAME)
			{
				GUI.FocusControl(SEARCH_CONTROL_NAME);
			}
		}

		private void OnReturnDown()
		{
			if (m_FocusedControl != SEARCH_CONTROL_NAME && m_SelectedIndex >= 0 && m_SelectedIndex <= m_Scenes.Count - 1)
			{
				string scene = m_Scenes[m_SelectedIndex].AssetPath;
				if (scene != m_CurrentScene)
				{
					EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
					Close();
				}
			}
		}

		private void DrawSearchBar()
		{
			EditorGUILayout.Space();
			using (new EditorGUILayout.HorizontalScope())
			{
				GUI.SetNextControlName(SEARCH_CONTROL_NAME);
				string search = EditorGUILayout.TextField(GUIContent.none, m_Search, m_SearchTextFieldStyle);
				if (m_Search != search)
				{
					m_Search.Value = search;
					Setup();
				}
				if (GUILayout.Button(GUIContent.none, m_SearchCancelButtonStyle))
				{
					GUI.FocusControl(null);
					m_Search.Value = string.Empty;
					Setup();
				}
			}
		}

		private void DrawScenes()
		{
			if (m_Scenes == null)
			{
				return;
			}
			string focusedControl = GUI.GetNameOfFocusedControl();
			using (EditorGUILayout.ScrollViewScope scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
			{
				m_ScrollPosition = scroll.scrollPosition;
				for (int i = 0; i < m_Scenes.Count; ++i)
				{
					GUIStyle style = m_Scenes[i].AssetPath == m_SelectedScene ?
						(focusedControl == SEARCH_CONTROL_NAME ? m_SelectedInactiveStyle : m_SelectedActiveStyle) :
						(EditorStyles.label);
					if (m_Scenes[i].DrawGUI(style, m_CurrentScene))
					{
						m_SelectedScene.Value = m_Scenes[i].AssetPath;
						m_SelectedIndex = i;
						GUI.FocusControl(null);
						EditorSceneManager.OpenScene(m_SelectedScene, OpenSceneMode.Single);
						Close();
					}
				}
			}
		}
	}
}
