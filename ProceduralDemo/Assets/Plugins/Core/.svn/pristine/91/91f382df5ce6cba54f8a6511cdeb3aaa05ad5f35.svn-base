
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Act2
{
	public class ActTreeDebuggerWindow2 : EditorWindow
	{
		private const char TRANSITION_ICON = '\u27a8';

		private static bool s_TimeInFrames = true;

		[MenuItem("Window/Act Tree 2/Tree Debugger")]
		static void Init()
		{
			ActTreeDebuggerWindow2 window = EditorWindow.GetWindow<ActTreeDebuggerWindow2>("Tree Debug 2");
			window.Show();
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

		private string m_SelectedObjectName = null;
		private string m_SelectedTreeName = null;
		private int m_SnapShotIndex = 0;

		Vector2 m_ScrollView = Vector2.zero;

		private void OnInspectorUpdate()
		{
			Repaint();
		}

		void OnGUI()
		{
			TreeDebuggerData data = TreeDebuggerData.Get();

			EditorGUILayout.BeginHorizontal();

			s_TimeInFrames = EditorGUILayout.Toggle("Time in frames", s_TimeInFrames);
			if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
			{
				TreeDebuggerData.Get().m_Objects.Clear();
				m_SelectedObjectName = null;
				m_SelectedTreeName = null;
				m_SnapShotIndex = 0;
			}
			EditorGUILayout.EndHorizontal();

			List<string> objNames = new List<string>(data.m_Objects.Keys);
			objNames.Sort();
			int index = objNames.IndexOf(m_SelectedObjectName);
			index = EditorGUILayout.Popup(index, objNames.ToArray());
			if (index < objNames.Count && index >= 0)
			{
				m_SelectedObjectName = objNames[index];
			}
			else
			{
				m_SelectedObjectName = null;
				return;
			}
			List<string> treeNames = new List<string>(data.m_Objects[m_SelectedObjectName].Keys);
			treeNames.Sort();
			int treeIndex = Mathf.Max(treeNames.IndexOf(m_SelectedTreeName), 0);
			treeIndex = EditorGUILayout.Popup(treeIndex, treeNames.ToArray());
			if (treeIndex < treeNames.Count && treeIndex >= 0)
			{
				m_SelectedTreeName = treeNames[treeIndex];
			}
			else
			{
				m_SelectedTreeName = null;
				return;
			}

			TreeDebuggerData.SnapShot[] snapShots = data.m_Objects[m_SelectedObjectName][m_SelectedTreeName].m_SnapShots;
			List<string> snapShotStrings = new List<string>(snapShots.Length);
			for (int i = 0; i < snapShots.Length; i++)
			{
				TreeDebuggerData.SnapShot snapShot = snapShots[i];
				if (snapShot == null)
				{
					continue;
				}

				float time = i == 0 && snapShot.m_Stopped.Count == 0 ? // If top is not stopped then update with the current game time
					Time.time - snapShot.m_TimeStamp :
					snapShot.GetDuration();

				string snapShotName = Core.Str.Build(
					"t:", SecondsToString(snapShot.m_TimeStamp), " ", snapShot.m_SnapShotName, " [", TimeToString(time), "]");
				snapShotStrings.Add(snapShotName);
			}

			EditorGUILayout.BeginHorizontal();
			m_SnapShotIndex = EditorGUILayout.Popup(m_SnapShotIndex, snapShotStrings.ToArray());

			GUI.enabled = m_SnapShotIndex < snapShots.Length - 1 && snapShots[m_SnapShotIndex + 1] != null;
			if (GUILayout.Button("\u25c0", GUILayout.ExpandWidth(false)))
			{
				m_SnapShotIndex++;
			}
			GUI.enabled = m_SnapShotIndex > 0;
			if (GUILayout.Button("\u25ba", GUILayout.ExpandWidth(false)))
			{
				m_SnapShotIndex--;
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			TreeDebuggerData.SnapShot shot = snapShots[m_SnapShotIndex];
			if (shot == null)
			{
				return;
			}

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Reason for playing: " + shot.m_Action, EditorStyles.boldLabel);
			GUILayout.Label("Time.time: " + shot.m_TimeStamp + "s", GUILayout.ExpandWidth(false));
			EditorGUILayout.EndHorizontal();
			if (shot.m_Transition != null)
			{
				DrawTransition(shot.m_Transition);
			}
			EditorGUILayout.LabelField(shot.m_EventString.ToString());
			EditorGUILayout.EndVertical();

			bool topSelected = m_SnapShotIndex == 0;
			TreeDebuggerData.SnapShot nextShot = topSelected ? null : snapShots[m_SnapShotIndex - 1];
			// If the top snap shot is selected and not stopped then update with current game time
			float deltaTime = topSelected && shot.m_Stopped.Count == 0 ? Time.time - shot.m_TimeStamp : 0.0f;

			foreach (TreeDebuggerData.SequencerSnapShot nodeSnapShot in shot.m_SequencerSnapShots)
			{
				NodeGUI(shot, nodeSnapShot, deltaTime, nextShot, topSelected);
			}

			if (!topSelected)
			{
				if (shot.m_Stopped.Count == 0)
				{
					EditorGUILayout.HelpBox("Never received OnStop event", MessageType.Error);
				}
				else if (shot.m_Stopped.Count > 1)
				{
					EditorGUILayout.HelpBox($"Received {shot.m_Stopped.Count} OnStop events, should only ever receive one", MessageType.Error);
				}
			}

			if (shot.m_FailParamsStrings.Count > 0)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField($"Missed {shot.m_FailParamsStrings.Count} events:");
				m_ScrollView = GUILayout.BeginScrollView(m_ScrollView);
				foreach (TreeDebuggerData.MissedParamsSnapShot failParams in shot.m_FailParamsStrings)
				{
					EditorGUILayout.LabelField($"t:{SecondsToString(failParams.m_TimeStamp)} [{TimeToString(failParams.m_Timer)}] - {failParams.m_Params}");
				}
				GUILayout.EndScrollView();
				GUILayout.FlexibleSpace();
			}

			foreach (TreeDebuggerData.SnapShotStopped stopped in shot.m_Stopped)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Reason for stopping: " + stopped.Action, EditorStyles.boldLabel);
				GUILayout.Label("Time.time: " + stopped.TimeStamp + "s", GUILayout.ExpandWidth(false));
				EditorGUILayout.EndHorizontal();
				if (nextShot != null && nextShot.m_Transition != null)
				{
					DrawTransition(nextShot.m_Transition);
					EditorGUILayout.LabelField(nextShot.m_EventString.ToString());
				}
				EditorGUILayout.EndVertical();
			}
		}

		private static void DrawTransition(TransitionRT transition)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField($"{transition.FromNode.Name} {TRANSITION_ICON} {transition.ToNode.Name}", EditorStyles.boldLabel);
			EditorGUI.indentLevel--;
		}

		public static string SecondsToString(float time)
		{
			return Core.DebugUtil.TruncatedFloatToString(time, 2) + "s";
		}

		public static string TimeToString(float time)
		{
			if (time < 0.0f)
			{
				return "Stopped";
			}
			return s_TimeInFrames ?
				Core.DebugUtil.TruncatedFloatToString(Core.Util.FPS30 * time, 1) :
				SecondsToString(time);
		}

		private void NodeGUI(
			TreeDebuggerData.SnapShot mainSnapShot, 
			TreeDebuggerData.SequencerSnapShot snapShot, 
			float deltaTime, 
			TreeDebuggerData.SnapShot nextShot,
			bool top)
		{
			Rect r = EditorGUILayout.GetControlRect();
			Rect labelRect = r;
			float labelWidth = TrackDrawer.GetActItemWidth(r.width);
			labelRect.width = labelWidth;
			if (GUI.Button(labelRect, string.Empty, GUIStyle.none))
			{
				ActTreeEditorWindow2.OpenTree(mainSnapShot.m_TreePath, snapShot.NodeProperties.ID);
			}
			EditorGUI.DrawRect(labelRect, NodeDrawer.Styles.Root.BackgroundColor.Value);
			GUIStyle nodeLabelStyle = new GUIStyle(EditorStyles.boldLabel);
			nodeLabelStyle.normal.textColor = NodeDrawer.Styles.Root.GetTextColor();
			NodeDrawer.AttachNodeIcon(labelRect, out labelRect, nodeLabelStyle, snapShot.NodeProperties);
			EditorGUI.LabelField(labelRect, snapShot.NodeProperties.Name, nodeLabelStyle);
			if (snapShot.NodeProperties.ID != Node.ROOT_ID)
			{
				nodeLabelStyle.alignment = TextAnchor.MiddleRight;
				nodeLabelStyle.fontStyle = FontStyle.Normal;
				nodeLabelStyle.normal.textColor = NodeDrawer.Styles.Normal.GetTextColor();
				EditorGUI.LabelField(labelRect, "(ID: " + snapShot.NodeProperties.ID + ") " + UberPickerGUI.POINTER_UNICODE + "  ", nodeLabelStyle);
			}

			Rect contentRect = r;
			contentRect.xMin = labelRect.xMax;
			float time = snapShot.StopTime > 0.0f ? snapShot.StopTime : snapShot.StartTime + deltaTime;
			EditorGUI.LabelField(contentRect, "  [" + TimeToString(time) + "] " + snapShot.Params);

			TimedItemProperties timingProperties = TimedItemUtil.GetTimedItemProperties(snapShot.GetTracksAndTransitions());

			GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.normal.textColor = Color.black;
			foreach (TreeDebuggerData.TransitionSnapShot trans in snapShot.TransitionSnapShots)
			{
				Rect itemRect = EditorGUILayout.GetControlRect();
				Rect r1 = itemRect;
				r1.width = labelWidth;
				r1.xMin += NodeDrawer.NODE_ICON_WIDTH;
				Rect r2 = itemRect; // Calculate this before attaching node icon
				r2.xMin = r1.xMax;

				trans.UpdateTakenState(nextShot);

				EditorGUI.DrawRect(r1, NodeTransition.EDITOR_COLOR);
				labelStyle.normal.textColor = !trans.Taken ?
					labelStyle.normal.textColor = Color.black :
					trans._EditorGetColor();
				labelStyle.alignment = TextAnchor.MiddleRight;
				NodeDrawer.AttachNodeIcon(r1, out r1, labelStyle, trans.ToNodeProperties);
				EditorGUI.LabelField(r1, trans.ToNodeProperties.Name + (trans.Taken ? " [Taken]" : ""), labelStyle);

				labelStyle.normal.textColor = Color.black;
				TrackDrawer.DrawTimelineBar(
					r2,
					trans,
					labelStyle,
					timingProperties,
					out Rect validAreaRect);

				float x = Mathf.Lerp(validAreaRect.xMin, validAreaRect.xMax, time / timingProperties.ValidTimeRange);
				GraphEditor.Lines.DrawLine(new Vector2(x, itemRect.yMin - 2.0f), new Vector2(x, itemRect.yMax + 2.0f), Color.cyan);
			}

			foreach (TreeDebuggerData.TrackSnapShot track in snapShot.TrackStates)
			{
				Rect itemRect = EditorGUILayout.GetControlRect();
				Rect r1 = itemRect;
				r1.width = labelWidth;
				r1.xMin += NodeDrawer.NODE_ICON_WIDTH;
				Rect r2 = itemRect;
				r2.xMin = r1.xMax;

				EditorGUI.DrawRect(r1, NodeTransition.EDITOR_COLOR);
				labelStyle.normal.textColor = track.State == TreeDebuggerData.TrackState.Undecided ?
					labelStyle.normal.textColor = Color.black :
					track._EditorGetColor();
				labelStyle.alignment = TextAnchor.MiddleRight;
				EditorGUI.LabelField(r1, track.Name, labelStyle);

				labelStyle.normal.textColor = Color.black;
				TrackDrawer.DrawTimelineBar(
					r2,
					track,
					labelStyle,
					timingProperties,
					out Rect validAreaRect);

				float x = Mathf.Lerp(validAreaRect.xMin, validAreaRect.xMax, time / timingProperties.ValidTimeRange);
				GraphEditor.Lines.DrawLine(new Vector2(x, itemRect.yMin - 1.0f), new Vector2(x, itemRect.yMax + 1.0f), Color.cyan);
			}
		}
	}
}
