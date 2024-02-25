
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ActSelectionDrawer
{
	private static Dictionary<int, int> m_TrackHistory = new Dictionary<int, int>();
	private static int m_ConditionID = 0;
	private static int m_TrackID = 0;
	private static int m_OpportunityIndex = -1;

	public static bool IsConditionSelected(int conditionID) { return m_ConditionID == conditionID; }
	public static void SelectedCondition(int conditionID)
	{
		m_ConditionID = m_ConditionID != conditionID ? conditionID : 0;
		m_TrackID = 0;
		m_OpportunityIndex = -1;
	}

	public static bool IsTrackSelected(int trackID) { return m_TrackID == trackID; }
	public static void ClearTrack() { m_TrackID = 0; }
	public static void SelectTrack(ActTrack track)
	{
		int trackID = track.GetInstanceID();
		if (!m_TrackHistory.ContainsKey(track.GetNodeID()))
		{
			m_TrackHistory.Add(track.GetNodeID(), trackID);
		}
		else
		{
			m_TrackHistory[track.GetNodeID()] = trackID;
		}
		SelectTrackInternal(trackID);
	}
	private static void SelectTrackInternal(int trackID)
	{
		m_TrackID = m_TrackID != trackID ? trackID : 0;
		m_ConditionID = 0;
		m_OpportunityIndex = -1;
	}

	public static bool IsOpportunitySelected(int index) { return m_OpportunityIndex == index; }
	public static void SelectOpportunity(int index)
	{
		m_OpportunityIndex = m_OpportunityIndex != index ? index : -1;
		m_ConditionID = 0;
		m_TrackID = 0;
	}

	public static void ClearSelection()
	{
		m_ConditionID = 0;
		m_TrackID = 0;
		m_OpportunityIndex = -1;
	}

	public static void ValidateSelected(ActTree tree, Act.Node node)
	{
		ActTrack track = GetTrack(tree, node);
		if (track != null)
		{
			return;
		}
		Act.NodeSequence seq = GetOpportunity(tree, node);
		if (seq != null)
		{
			return;
		}
		ActCondition cond = GetCondition(tree, node);
		if (cond != null)
		{
			return;
		}
		if (m_TrackHistory.TryGetValue(node.GetID(), out int trackID))
		{
			SelectTrackInternal(trackID);
			return;
		}
		// If there is currently no valid selection select first track in the node
		foreach (ActTrack t in tree.GetTracks())
		{
			if (t != null && t.GetNodeID() == node.GetID())
			{
				SelectTrack(t);
				return;
			}
		}
	}

	public static ActTrack GetTrack(ActTree tree, Act.Node node)
	{
		if (m_TrackID == 0)
		{
			return null;
		}
		foreach (ActTrack track in tree.GetTracks())
		{
			if (track != null && 
				track.GetInstanceID() == m_TrackID &&
				track.GetNodeID() == node.GetID())
			{
				return track;
			}
		}
		return null;
	}

	public static ActCondition GetCondition(ActTree tree, Act.Node node)
	{
		if (m_ConditionID == 0)
		{
			return null;
		}
		foreach (ActCondition condition in tree.GetConditions())
		{
			if (condition != null &&
				condition.GetInstanceID() == m_ConditionID &&
				condition.GetNodeID() == node.GetID())
			{
				return condition;
			}
		}
		return null;
	}

	public static Act.NodeSequence GetOpportunity(ActTree tree, Act.Node node)
	{
		List<Act.NodeSequence> seqs = tree.GetAllOpportunities();
		if (m_OpportunityIndex < 0 || m_OpportunityIndex >= seqs.Count)
		{
			return null;
		}
		Act.NodeSequence seq = seqs[m_OpportunityIndex];
		if (seq == null || seq.GetNodeID() != node.GetID())
		{
			return null;
		}
		return seq;
	}

	public static void OnGUI(ActTree tree, ref SerializedObject sTree, Act.Node node)
	{
		Act.ITrack selectedTrack = null;
		SerializedObject serObj = null;
		SerializedProperty startTime = null;
		SerializedProperty endTime = null;
		string name = string.Empty;
		bool selected = false;

		ActTrack track = GetTrack(tree, node);
		if (track != null)
		{
			selected = true;
			selectedTrack = track;
			serObj = new SerializedObject(track);
			startTime = serObj.FindProperty("m_StartTime");
			endTime = serObj.FindProperty("m_EndTime");
			name = track.EditorDisplayName();
		}

		Act.NodeSequence seq = GetOpportunity(tree, node);
		if (seq != null)
		{
			selected = true;
			selectedTrack = seq;
			SerializedProperty sSeq = sTree.FindProperty("m_Opportunities").GetArrayElementAtIndex(m_OpportunityIndex);
			startTime = sSeq.FindPropertyRelative("m_StartTime");
			endTime = sSeq.FindPropertyRelative("m_EndTime");
			Act.Node toNode = tree.GetNode(seq.GetToID());
			if (toNode != null)
			{
				name = toNode.GetName();
			}
		}

		ActCondition cond = GetCondition(tree, node);
		if (cond != null)
		{
			selected = true;
			serObj = new SerializedObject(cond);
			name = serObj.targetObject.ToString();
		}

		bool drawProperty = false;
		SerializedProperty property = null;
		if (serObj != null)
		{
			property = serObj.GetIterator();
			property.NextVisible(true); // Skip the first visible property as this is always the script reference
			if (property.NextVisible(false))
			{
				drawProperty = true;
			}
		}

		bool somethingToDraw = selectedTrack != null || drawProperty;
		if (!selected || !somethingToDraw)
		{
			return;
		}

		GUILayout.BeginVertical(GUI.skin.box);
		//GUILayout.Label(name, EditorStyles.boldLabel);
		// Draw timing for ITracks
		if (selectedTrack != null)
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
			GUILayout.Label("Start Time", GUILayout.ExpandWidth(false));
			float start = Core.Util.FramesToSeconds(
				EditorGUILayout.IntField(Core.Util.SecondsToFrames(startTime.floatValue), GUILayout.Width(32.0f)));
			if (selectedTrack.HasEndEvent())
			{
				GUILayout.Label("End Time", GUILayout.ExpandWidth(false));
				float end = endTime.floatValue;
				if (selectedTrack.GetEndEventType() == ActTrack.EndEventType.NegativeEndTime)
				{
					end = -1;
					EditorGUILayout.LabelField(
						Core.Str.Build("-1"),
						GUILayout.Width(48.0f));
				}
				else
				{
					end = Core.Util.FramesToSeconds(
						EditorGUILayout.IntField(Core.Util.SecondsToFrames(endTime.floatValue), GUILayout.Width(32.0f)));
				}
				start = Mathf.Max(0.0f, start);
				if (end > 0.0f || selectedTrack.GetEndEventType() == ActTrack.EndEventType.PositiveEndTime)
				{
					end = Mathf.Max(start + Core.Util.SPF30, end);
				}
				endTime.floatValue = end;
			}
			startTime.floatValue = start;
			EditorGUILayout.EndHorizontal();
		}
		// Draw content for tracks and conditions
		if (drawProperty)
		{
			// For some reason labels don't seem to automatically resize for the property fields bellow
			EditorGUIUtility.labelWidth = Mathf.Max(EditorGUIUtility.labelWidth, 0.4f * EditorGUIUtility.currentViewWidth);
			do
			{
				EditorGUILayout.PropertyField(property, true);
			}
			while (property.NextVisible(false));
		}
		GUILayout.EndVertical();
		if (serObj != null && serObj.hasModifiedProperties)
		{
			serObj.ApplyModifiedProperties();
			ActTreeDirtyTimestamps.SetDirty(tree);
		}
	}
}
