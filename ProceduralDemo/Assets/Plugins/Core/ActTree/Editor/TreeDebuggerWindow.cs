
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TreeDebuggerWindow : EditorWindow
{
	[MenuItem("Window/Act Tree/Tree Debugger")]
	static TreeDebuggerWindow Get()
	{
		TreeDebuggerWindow window = EditorWindow.GetWindow<TreeDebuggerWindow>("Tree Debug");
		window.Show();
		return window;
	}

	[InitializeOnLoadMethod]
	static void OnLoad()
	{
		Application.quitting += OnQuit;
	}

	static void OnQuit()
	{
		TreeDebuggerData.Get().m_Objects.Clear();
	}

	private static readonly StringSearchWindowContext SEARCH_WINDOW_CONTEXT =
		new StringSearchWindowContext(nameof(TreeDebuggerWindow), nameof(TreeDebuggerWindow), GetDataIds, null, false, true, false);

	private string m_SelectedID = Core.Str.EMPTY;
	private int m_SnapShotIndex = 0;
	private bool m_TimeInFrames = true;
	private List<string> m_DataIds = new List<string>();

	private static List<string> GetDataIds() => new List<string>(Get().m_DataIds);

	private void OnInspectorUpdate()
	{
		Repaint();
	}

	void OnGUI()
	{
		TreeDebuggerData data = TreeDebuggerData.Get();

		EditorGUILayout.BeginHorizontal();

		m_DataIds.Clear();
		m_DataIds.AddRange(data.m_Objects.Keys);
		m_DataIds.Sort();
		if (GUILayout.Button(m_SelectedID) && m_DataIds.Count > 0)
		{
			StringSearchWindowProvider.Show(SEARCH_WINDOW_CONTEXT, OnIDSelected);
		}
		bool isValidSelectedID = m_DataIds.TryGetIndexOf(m_SelectedID, out int index);

		m_TimeInFrames = EditorGUILayout.Toggle("Time in frames", m_TimeInFrames);
		bool clear = false;
		if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
		{
			clear = true;
		}
		EditorGUILayout.EndHorizontal();

		if (!isValidSelectedID)
		{
			m_SelectedID = Core.Str.EMPTY;
			return;
		}

		TreeDebuggerData.SnapShot[] snapShots = data.m_Objects[m_SelectedID].GetSnapShots();
		if (clear)
		{
			for (int i = 1; i < snapShots.Length; i++)
			{
				snapShots[i] = null;
			}
		}

		List<string> snapShotStrings = new List<string>(snapShots.Length);
		for (int i = 0; i < snapShots.Length; i++)
		{
			TreeDebuggerData.SnapShot snapShot = snapShots[i];
			if (snapShot == null)
			{
				continue;
			}
			string snapShotName = i + " " + snapShot.m_SnapShotName + " ";
			float time = -1.0f;
			if (i - 1 < 0)
			{
				time = Time.time - snapShot.m_TimeStamp;
			}
			else
			{
				time = snapShots[i - 1].m_TimeStamp - snapShot.m_TimeStamp;
			}
			snapShotName += m_TimeInFrames ?
				Core.DebugUtil.TruncatedFloatToString(Core.Util.FPS30 * time, 1) :
				Core.DebugUtil.TruncatedFloatToString(time, 2);
			snapShotName += " - " + snapShot.m_TimeStamp;
			snapShotStrings.Add(snapShotName);
		}

		m_SnapShotIndex = EditorGUILayout.Popup(m_SnapShotIndex, snapShotStrings.ToArray());

		TreeDebuggerData.SnapShot shot = data.m_Objects[m_SelectedID].GetSnapShots()[m_SnapShotIndex];
		if (shot == null || shot.m_Sequencer == null)
		{
			return;
		}

		EditorGUILayout.LabelField(shot.m_AnimParamsString.ToString());

		float t = -1.0f;
		if (m_SnapShotIndex == 0)
		{
			t = Time.time - shot.m_TimeStamp;
		}
		else
		{
			t = data.m_Objects[m_SelectedID].GetSnapShots()[m_SnapShotIndex - 1].m_TimeStamp - shot.m_TimeStamp;
		}
		if (m_TimeInFrames)
		{
			EditorGUILayout.LabelField(string.Format("Timer: {0}", Core.Util.FPS30 * t));
		}
		else
		{
			EditorGUILayout.LabelField(string.Format("Timer: {0}", t));
		}

		NodeList(shot.m_Sequencer, 0, t, m_SnapShotIndex == 0);

		foreach (string failString in shot.m_FailParamsStrings)
		{
			EditorGUILayout.LabelField(failString);
		}
	}

	private void OnIDSelected(string id)
	{
		m_SelectedID = id;
	}

	void NodeList(ActSequencer sequencer, int nestDepth, float timer, bool current)
	{
		if (sequencer == null)
		{
			return;
		}
		else
		{
			ActNodeRT currentNode = sequencer.GetNode();

			bool isMaster = false;
			List<ActTrack> tracks = currentNode.GetTracks();
			if (tracks != null)
			{
				for (int j = 0; j < tracks.Count; j++)
				{
					if (tracks[j].IsMaster())
					{
						isMaster = true;
						break;
					}
				}
			}

			GUIStyle selectedNodeStyle = new GUIStyle(GUI.skin.button);
			if (isMaster)
			{
				selectedNodeStyle.fontStyle = FontStyle.Italic;
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(currentNode.GetName(), selectedNodeStyle))
			{
				ActTreeEditorWindow treeWindow = ActTreeEditorWindow.Get();
				treeWindow.SetSelectedNode(currentNode.GetID());
				treeWindow.Focus();
			}
			GUILayout.Label(sequencer.GetTimer().ToString("F2"), GUILayout.ExpandWidth(false));
			GUILayout.Label(sequencer.GetParams().ToString(), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			//			if (mData.mTimeInFrames)
			//			{
			//				GUI.Label(GUIHelpers.ReserveSpace(0.0f, 0.0f, 100.0f), string.Format("Timer: {0}",GameUtil.SecondsToFrames(sequencer.GetTimer())));
			//			}
			//			else
			//			{
			//				GUI.Label(GUIHelpers.ReserveSpace(0.0f, 0.0f, 100.0f), string.Format("Timer: {0:0.00}",sequencer.GetTimer()));
			//			}

			if (tracks != null)
			{
				for (int i = 0; i < tracks.Count; i++)
				{
					ActTrack animEvent = tracks[i];

					GUIStyle colorStyle = new GUIStyle();
					colorStyle.normal.textColor = Color.grey;
					if (!animEvent.HasEndEvent())
					{
						if ((animEvent.GetStartTime() < 0.0f && !current) ||
							(animEvent.GetStartTime() > -Core.Util.EPSILON && animEvent.GetStartTime() < timer))
						{
							colorStyle.normal.textColor = Color.green;
						}
					}
					else if (animEvent.GetStartTime() < timer)
					{
						if ((animEvent.GetEndTime() < 0.0f && !current) ||
							(animEvent.GetEndTime() > -Core.Util.EPSILON && animEvent.GetEndTime() < timer))
						{
							colorStyle.normal.textColor = Color.green;
						}
						else
						{
							colorStyle.normal.textColor = Color.yellow;
						}
					}

					EditorGUILayout.LabelField(animEvent.ToString(), colorStyle);
				}
			}

			EditorGUI.indentLevel++;
			NodeList(sequencer.GetChildSequencer(), nestDepth + 1, timer, current);
			EditorGUI.indentLevel--;
		}
	}
}
