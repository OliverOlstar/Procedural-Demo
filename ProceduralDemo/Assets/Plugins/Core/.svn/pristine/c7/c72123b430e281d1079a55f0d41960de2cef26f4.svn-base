
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActVarDebugWindow : EditorWindow
{
	[InitializeOnLoadMethod]
	static void OnLoad()
	{
		Application.quitting += OnQuit;
	}

	static void OnQuit()
	{
		ActVarDebugData.s_Info.Clear();
	}

	[MenuItem("Window/Act Tree 2/Var Debugger")]
	static void Init()
	{
		ActVarDebugWindow window = EditorWindow.GetWindow<ActVarDebugWindow>("ActVarDebugger");
		window.Show();
	}

	Vector2 m_ScrollPos = Vector2.zero;

	string m_CurrentID = Core.Str.EMPTY;
	int m_ShotIndex = -1;

	private void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnGUI()
	{
		Dictionary<string, ActVarDebugData.DebugInfo> allInfo = ActVarDebugData.s_Info;
		if (allInfo.Count == 0)
		{
			return;
		}

		string[] names = new string[allInfo.Keys.Count];
		int i = 0;
		int selectedIndex = 0;
		foreach (string behaviour in allInfo.Keys)
		{
			names[i] = behaviour;
			if (Core.Str.Equals(behaviour, m_CurrentID))
			{
				selectedIndex = i;
			}
			i++;
		}
		selectedIndex = EditorGUILayout.Popup(selectedIndex, names);
		string selectedBehaviour = names[selectedIndex];
		ActVarDebugData.DebugInfo info = allInfo[selectedBehaviour];
		if (!Core.Str.Equals(selectedBehaviour, m_CurrentID))
		{
			m_CurrentID = selectedBehaviour;
			m_ShotIndex = info.snapShots.Count - 1;
		}

		if (info.snapShots.Count == 0)
		{
			return;
		}

		string[] shotNames = new string[info.snapShots.Count];
		for (int j = 0; j < shotNames.Length; j++)
		{
			ActVarDebugData.SnapShot snapShot = info.snapShots[j];
			float delta = 0.0f;
			if (j > 0)
			{
				delta = snapShot.TimeStamp - info.snapShots[j - 1].TimeStamp;
			}
			shotNames[j] = Core.Str.Build(
				"t:", Act2.ActTreeDebuggerWindow2.SecondsToString(snapShot.TimeStamp), " ",
				snapShot.VarName, " [",
				Act2.ActTreeDebuggerWindow2.TimeToString(delta), "]");
		}

		EditorGUILayout.BeginHorizontal();
		m_ShotIndex = m_ShotIndex < shotNames.Length ? m_ShotIndex : -1; // Somehow this was getting out of range?
		int index = m_ShotIndex < 0 ? shotNames.Length - 1 : m_ShotIndex;
		int newIndex = EditorGUILayout.Popup(index, shotNames);
		if (newIndex != index && shotNames.Length > 0)
		{
			index = newIndex;
			m_ShotIndex = newIndex == shotNames.Length - 1 ? -1 : newIndex;
		}

		GUI.enabled = index > 0;
		if (GUILayout.Button("\u25c0", GUILayout.ExpandWidth(false)))
		{
			index--;
			m_ShotIndex = index;
		}
		GUI.enabled = index < info.snapShots.Count - 1;
		if (GUILayout.Button("\u25ba", GUILayout.ExpandWidth(false)))
		{
			index++;
			m_ShotIndex = index == shotNames.Length - 1 ? -1 : index;
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();

		bool top = index == info.snapShots.Count - 1;
		ActVarDebugData.SnapShot shot = info.snapShots[index];
		ActVarDebugData.SnapShot? lastShot = index > 0 ? info.snapShots[index - 1] : null;

		EditorGUILayout.LabelField("Time.time: " + shot.TimeStamp + "s");

		EditorGUILayout.BeginScrollView(m_ScrollPos);

		List<KeyValuePair<string, float>> shotList = Core.ListPool<KeyValuePair<string, float>>.Request();
		shotList.AddRange(shot.Vars);
		shotList.Sort(SortSnapshots);
		foreach (KeyValuePair<string, float> var in shotList)
		{
			EditorGUILayout.BeginHorizontal();
			string rootName = GetRootName(var.Key);
			if (rootName != shot.VarName)
			{
				GUI.enabled = false;
				EditorGUILayout.FloatField(var.Key, var.Value);
				GUI.enabled = true;
			}
			else if (!lastShot.HasValue || !lastShot.Value.Vars.TryGetValue(var.Key, out float lastShotValue))
			{
				EditorGUILayout.FloatField(var.Key + " *New*", var.Value);
			}
			else
			{
				EditorGUILayout.FloatField(var.Key, lastShotValue);
				GUILayout.Label("\u27a8", GUILayout.ExpandWidth(false));
				GUILayout.TextField(var.Value.ToString());
			}
			EditorGUILayout.EndHorizontal();
		}

		shotList.Clear();
		shotList.AddRange(shot.Timers);
		shotList.Sort(SortSnapshots);
		foreach (KeyValuePair<string, float> time in shotList)
		{
			string timeString;
			if (top)
			{
				float t = Time.time - time.Value;
				timeString = Act2.ActTreeDebuggerWindow2.TimeToString(t);
			}
			else
			{
				float t = shot.TimeStamp - time.Value;
				timeString = Act2.ActTreeDebuggerWindow2.TimeToString(t);
			}

			if (time.Key != shot.VarName)
			{
				EditorGUILayout.LabelField(time.Key + " = " + timeString);
			}
			else if (!lastShot.HasValue || !lastShot.Value.Timers.TryGetValue(time.Key, out float lastShotValue))
			{
				EditorGUILayout.LabelField("*New* " + time.Key + " = " + timeString);
			}
			else
			{
				float t0 = shot.TimeStamp - lastShotValue;
				EditorGUILayout.LabelField(time.Key + " " + Act2.ActTreeDebuggerWindow2.TimeToString(t0) + " -> " + timeString);
			}
		}
		Core.ListPool<KeyValuePair<string, float>>.Return(shotList);

		List<KeyValuePair<string, string>> refShotList = Core.ListPool<KeyValuePair<string, string>>.Request();
		refShotList.AddRange(shot.References);
		refShotList.Sort(SortSnapshots);
		foreach (KeyValuePair<string, string> var in refShotList)
		{
			EditorGUILayout.BeginHorizontal();
			if (var.Key != shot.VarName)
			{
				GUI.enabled = false;
				EditorGUILayout.TextField(var.Key, var.Value);
				GUI.enabled = true;
			}
			else if (!lastShot.HasValue || !lastShot.Value.Vars.TryGetValue(var.Key, out float lastShotValue))
			{
				EditorGUILayout.TextField(var.Key + " *New*", var.Value);
			}
			else
			{
				EditorGUILayout.FloatField(var.Key, lastShotValue);
				GUILayout.Label("\u27a8", GUILayout.ExpandWidth(false));
				GUILayout.TextField(var.Value.ToString());
			}
			EditorGUILayout.EndHorizontal();
		}
		Core.ListPool<KeyValuePair<string, string>>.Return(refShotList);

		EditorGUILayout.EndScrollView();
	}

	private string GetRootName(string name)
	{
		return name.EndsWith(".x") || name.EndsWith(".y") || name.EndsWith(".z") ?
			name.Substring(0, name.Length - 2) :
			name;
	}

	private int SortSnapshots(KeyValuePair<string, float> a, KeyValuePair<string, float> b)
	{
		return a.Key.CompareTo(b.Key);
	}

	private int SortSnapshots(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
	{
		return a.Key.CompareTo(b.Key);
	}
}
