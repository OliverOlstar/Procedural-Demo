
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class ActTrackDrawer
{
	readonly float DEFAULT_TIME_RANGE = 2.0f;

	Vector2 m_ScrollPos = Vector2.zero;

	int m_DeleteID = 0;
	int m_DuplicateID = 0;
	bool m_Paste = false;

	float GetTimingRange(ActTree tree, Act.Node node, ref bool negativeEnd)
	{
		negativeEnd = false;
		float totalRange = 0.0f;
		foreach (Act.ITrack track in tree.GetTrackAndOpportunities())
		{
			if (track != null && 
				track.GetNodeID() == node.GetID() && 
				(track.IsMaster() || track._EditorDisplayEndTime() > 0.0f) && 
				track.HasEndEvent())
			{
				float time = track._EditorDisplayEndTime();
				if (time < 0.0f)
				{
					negativeEnd = true;
					time = track.GetStartTime() + DEFAULT_TIME_RANGE;
				}
				if (time > totalRange)
				{
					totalRange = time;
				}
			}
		}
		if (totalRange < Core.Util.EPSILON)
		{
			totalRange = DEFAULT_TIME_RANGE; // If all the master tracks have a negative end time use a default
		}
		if (negativeEnd)
		{
			foreach (Act.ITrack track in tree.GetTrackAndOpportunities())
			{
				if (track != null)
				{
					float time =
						!track.HasEndEvent() ? track.GetStartTime() :
						!track.HasNegativeEndTime() ? track._EditorDisplayEndTime() :
						track.GetStartTime() + DEFAULT_TIME_RANGE;
					if (time > totalRange)
					{
						totalRange = time;
					}
				}
			}
		}
		return totalRange;
	}

	public void OnGUI(ActTree tree, ref SerializedObject sTree, Act.Node node)
	{
		if (m_DeleteID != 0)
		{
			foreach (ActTrack track in tree.GetTracks())
			{
				if (track != null && track.GetInstanceID() == m_DeleteID)
				{
					Undo.RecordObject(tree, "DeleteTrack");
					Undo.DestroyObjectImmediate(track);
					Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tree));
					EditorUtility.SetDirty(tree);
					UpdateTrackList(tree, ref sTree);
					break;
				}
			}
			m_DeleteID = 0;
		}

		if (m_DuplicateID != 0)
		{
			foreach (ActTrack track in tree.GetTracks())
			{
				if (track != null && track.GetInstanceID() == m_DuplicateID)
				{
					ActTrack dup = Object.Instantiate<ActTrack>(track);
					dup.name = track.name;
					Undo.RegisterCreatedObjectUndo(dup, "DupTrack");
					Undo.RecordObject(tree, "DupTrack");
					Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
					AssetDatabase.AddObjectToAsset(dup, tree);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(dup)); // Update AssetDatabase
					UpdateTrackList(tree, ref sTree);
					break;
				}
			}
			m_DuplicateID = 0;
		}

		if (m_Paste)
		{
			m_Paste = false;
			ActTrackClipboard.Paste(node.GetID(), tree, ref sTree);
		}

		m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
		GUILayout.BeginVertical(GUI.skin.box);

		bool negativeMaster = false;
		float totalRange = GetTimingRange(tree, node, ref negativeMaster);
		tree.GetTracks().Sort();

		bool hasOpps = false;
		List<Act.NodeSequence> opps = tree.GetAllOpportunities();
		foreach (Act.NodeSequence opp in opps)
		{
			if (opp.GetFromID() == node.GetID())
			{
				hasOpps = true;
				break;
			}
		}
		if (hasOpps)
		{
			Rect oppHeaderRect = EditorGUILayout.GetControlRect();
			DrawActItem(false, oppHeaderRect, out GUIStyle oppHeaderStyle, out Color oppHeaderColor, out Rect oppHeaderLabelRect, out Rect oppHeaderContentRect);
			oppHeaderStyle.fontStyle = FontStyle.Bold;
			oppHeaderStyle.alignment = TextAnchor.MiddleLeft;
			EditorGUI.LabelField(oppHeaderLabelRect, "Transitions", oppHeaderStyle);
			for (int i = 0; i < opps.Count; i++)
			{
				Act.NodeSequence opp = opps[i];
				DrawTrack(
					ref sTree,
					tree,
					node,
					opp,
					ActSelectionDrawer.IsOpportunitySelected(i),
					totalRange,
					negativeMaster,
					out bool selected);
				if (selected)
				{
					ActSelectionDrawer.SelectOpportunity(i);
				}
				if (ActSelectionDrawer.IsOpportunitySelected(i))
				{
					ActSelectionDrawer.OnGUI(tree, ref sTree, node);
				}
			}
		}

		Rect sourceRect = EditorGUILayout.GetControlRect();
		DrawActItem(false, sourceRect, out GUIStyle labelStyle, out Color textColor, out Rect labelRect, out Rect trackAreaRect);
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.alignment = TextAnchor.MiddleLeft;
		if (GUI.Button(sourceRect, string.Empty, GUIStyle.none) && Event.current.button != 0)
		{
			EmptyContext();
		}
		EditorGUI.LabelField(labelRect, "Tracks", labelStyle);
		foreach (ActTrack track in tree.GetTracks())
		{
			int id = track.GetInstanceID();
			DrawTrack(
				ref sTree,
				tree,
				node,
				track,
				ActSelectionDrawer.IsTrackSelected(id),
				totalRange,
				negativeMaster,
				out bool selected);
			if (selected)
			{
				ActSelectionDrawer.SelectTrack(track);
			}
		}

		if (Event.current.type == EventType.KeyDown)
		{
			switch (Event.current.keyCode)
			{
				case KeyCode.UpArrow:
					break;
				case KeyCode.DownArrow:
					break;
				case KeyCode.Delete:
					ActTrack selectedTrack = ActSelectionDrawer.GetTrack(tree, node);
					if (selectedTrack != null)
					{
						m_DeleteID = selectedTrack.GetInstanceID();
					}
					break;
			}
		}

		GUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
	}

	public static float GetActItemWidth(float totalWidth)
	{
		return Mathf.Max(132.0f, 0.33f * totalWidth);
	}

	public static void DrawActItem(
		bool isSelected, 
		Rect sourceRect, 
		out GUIStyle labelStyle, 
		out Color textColor, 
		out Rect labelRect,
		out Rect contentRect)
	{
		textColor = isSelected ? Color.white : GUIStyle.none.normal.textColor;
		labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.MiddleRight;
		labelStyle.normal.textColor = textColor;
		if (isSelected)
		{
			Rect highlightedRect = new Rect(sourceRect);
			//highlightedRect.xMin -= 4.0f;
			//highlightedRect.yMin -= 1.0f;
			//highlightedRect.xMax += 4.0f;
			//highlightedRect.yMax += 1.0f;
			ColorUtility.TryParseHtmlString("#3E7DE7FF", out Color color);
			EditorGUI.DrawRect(highlightedRect, color);
		}
		labelRect = sourceRect;
		labelRect.width = GetActItemWidth(sourceRect.width);
		if (!isSelected)
		{
			EditorGUI.DrawRect(labelRect, new Color(0.6f, 0.6f, 0.6f));
		}

		contentRect = sourceRect;
		contentRect.x += labelRect.width;
		contentRect.width -= labelRect.width;
	}

	bool DrawTrack(
		ref SerializedObject sTree,
		ActTree tree,
		Act.Node node,
		Act.ITrack track, 
		bool isSelected,
		float totalRange, 
		bool negativeMaster,
		out bool selected)
	{
		selected = false;
		if (track == null)
		{
			Debug.LogWarning(Core.Str.Build(tree.name, " has a null track"));
			return false;
		}
		if (track.GetNodeID() != node.GetID())
		{
			return false;
		}

		ActTrack actTrack = track as ActTrack;
		Act.NodeSequence seq = actTrack == null ? track as Act.NodeSequence : null;
		Act.Node sib = seq != null ? tree.GetNode(seq.GetToID()) : null;
		if (seq != null && sib == null)
		{
			return false; // Can't draw a sequence without a valid sibiling reference
		}

		EditorGUILayout.BeginHorizontal();

		Rect sourceRect = EditorGUILayout.GetControlRect();
		DrawActItem(isSelected, sourceRect, out GUIStyle labelStyle, out Color textColor, out Rect labelRect, out Rect trackAreaRect);

		if (actTrack != null)
		{
			string name = actTrack.EditorDisplayName();
			EditorGUI.LabelField(labelRect, name, labelStyle);
		}
		if (seq != null)
		{
			ActNodeDrawer.GetNodeInformation(tree, sib, out bool isMaster, out bool hasTrack, out Act.Node referencedNode, out string icon, out int iconSize);
			string name = sib.GetName();
			labelRect.width -= 16;
			Rect iconRect = labelRect;
			iconRect.width = 16;
			iconRect.x += labelRect.width;
			GUIStyle iconStyle = new GUIStyle(labelStyle);
			iconStyle.fontSize = iconSize;
			EditorGUI.LabelField(iconRect, icon, iconStyle);
			EditorGUI.LabelField(labelRect, name, labelStyle);
		}
		
		Rect trackRect = trackAreaRect;
		//EditorGUI.DrawRect(r, track._EditorGetColor()); 
		//r1.width = 18.0f;
		//r1.x = r.x + r.width - r1.width;
		//GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
		//buttonStyle.normal.textColor = SelectionTint(
		//	track.IsActive() ? track._EditorGetColor() : Color.grey, 
		//	currentSelectedID == track.GetInstanceID());
		//buttonStyle.fontStyle = FontStyle.Bold;
		//if (GUI.Button(r1, "X", buttonStyle))
		//{
		//	m_DeleteID = track.GetInstanceID();
		//}
		//r1.x -= r1.width;
		//if (GUI.Button(r1, "D", buttonStyle))
		//{
		//	m_DuplicateID = track.GetInstanceID();
		//}

		//r1.x = r.x;
		//r1.width = r.width - 18.0f - 18.0f;
		if (GUI.Button(sourceRect, string.Empty, GUIStyle.none))
		{
			if (Event.current.button == 0)
			{
				// Use this to fix the annoying bug where we switch displayed objects but the values for the previous one is still in the text field
				EditorGUIUtility.editingTextField = false;
				selected = true;
			}
			else if (seq != null)
			{
				ActSequenceContext.OpenContext(new ActSequenceContext.Parameters(sTree, tree, node, seq));
			}
			else if (actTrack != null)
			{
				OpenContext(actTrack);
			}
		}

		if (!track.IsMaster())
		{
			if (!track.HasEndEvent())
			{
				trackRect.height *= 0.33f;
				trackRect.y += trackRect.height;
			}
			else
			{
				trackRect.height *= 0.33f;
				trackRect.y += trackRect.height;
			}
		}

		float startTime = track.GetStartTime();
		float endTime = track._EditorDisplayEndTime();

		float width = trackAreaRect.width - 32.0f - 32.0f - 24.0f;
		if (startTime < 0.0f)
		{
			trackRect.width = trackRect.height;
			float negativeOffset = negativeMaster ? 24.0f : 0.0f;
			trackRect.x = trackAreaRect.x + width + 32.0f + negativeOffset - trackRect.width;
		}
		else
		{
			float start = (startTime / totalRange) * width;
			trackRect.x = trackAreaRect.x + 32.0f + start;
			if (!track.HasEndEvent())
			{
				trackRect.width = trackRect.height;
				trackRect.x = Mathf.Min(trackRect.x, trackAreaRect.x + width + 32.0f - trackRect.width);
			}
			else if (endTime < 0.0f)
			{
				float negativeOffset = negativeMaster ? 24.0f : 0.0f;
				trackRect.width = width - start + negativeOffset;
			}
			else
			{
				float end = (endTime / totalRange) * width;
				trackRect.width = Mathf.Max(end - start, 4);
			}
		}

		// Sub Times Rects
		List<float> subTimes = Core.ListPool<float>.Request();
		List<Rect> subTimeRects = Core.ListPool<Rect>.Request();
		track._EditorAddSubTimes(subTimes);
		if (subTimes != null && subTimes.Count > 0)
		{
			for (int i = 0; i < subTimes.Count; i++)
			{
				Rect trackSubRect = trackRect;

				float x = (subTimes[i] / totalRange) * width;
				trackSubRect.x += x;
				trackSubRect.width = 6.0f;
				trackRect.width = Mathf.Max(trackRect.width, x + trackSubRect.width); // Extend width if sub timers track is longer

				trackSubRect.y += trackRect.height;
				if (track.IsMaster())
				{
					trackSubRect.height *= 0.33f;
					trackSubRect.y -= trackSubRect.height;
				}

				subTimeRects.Add(trackSubRect);
			}
		}
		Core.ListPool<float>.Return(subTimes);

		// Outline.
		EditorGUI.DrawRect(trackRect, textColor);
		// Track.
		EditorGUI.DrawRect(new Rect(trackRect.position + Vector2.one, trackRect.size - Vector2.one * 2.0f), track.IsActive() ? track._EditorGetColor() : Color.grey);

		// Sub Track & Outline.
		foreach (Rect subRect in subTimeRects)
		{
			EditorGUI.DrawRect(subRect, textColor);
			EditorGUI.DrawRect(new Rect(subRect.position + Vector2.one, subRect.size - Vector2.one * 2.0f), track.IsActive() ? track._EditorGetColor() : Color.grey);
		}
		Core.ListPool<Rect>.Return(subTimeRects);

		// Labels
		Rect r2 = trackRect;
		r2.y = trackAreaRect.y;
		r2.height = trackAreaRect.height;
		r2.width = 32.0f;
		
		bool timeAfter = track.HasEndEvent() || track.GetStartTime() < 0.0f;
		bool timeBefore = track.HasEndEvent() || !timeAfter;
		if (timeBefore)
		{
			r2.x = trackRect.x - r2.width;
			labelStyle.alignment = TextAnchor.MiddleRight;
			GUI.Label(r2, Core.Util.SecondsToFrames(startTime).ToString(), labelStyle);
		}
		if (timeAfter)
		{
			r2.x = trackRect.x + trackRect.width;
			labelStyle.alignment = TextAnchor.MiddleLeft;
			GUI.Label(r2, endTime < 0.0f ? "-1" : Core.Util.SecondsToFrames(endTime).ToString(), labelStyle);
		}

		EditorGUILayout.EndHorizontal();
		return true;
	}

	void DeleteContext(object obj)
	{
		ActTrack track = obj as ActTrack;
		m_DeleteID = track.GetInstanceID();
	}

	void DuplicateContext(object obj)
	{
		ActTrack track = obj as ActTrack;
		m_DuplicateID = track.GetInstanceID();
	}

	void CopyContext(object obj)
	{
		ActTrack track = obj as ActTrack;
		ActTrackClipboard.Clear();
		ActTrackClipboard.Add(track);
	}

	void AddContext(object obj)
	{
		ActTrack track = obj as ActTrack;
		ActTrackClipboard.Add(track);
	}

	void PasteContext()
	{
		m_Paste = true;
	}

	void OpenContext(ActTrack track)
	{
		GenericMenu menu = new GenericMenu();
		menu.allowDuplicateNames = true;
		menu.AddDisabledItem(new GUIContent(track.ToString()));
		menu.AddItem(new GUIContent("Delete"), false, DeleteContext, track);
		menu.AddSeparator("");
		menu.AddItem(new GUIContent("Duplicate"), false, DuplicateContext, track);
		menu.AddItem(new GUIContent("Copy"), false, CopyContext, track);
		if (!ActTrackClipboard.IsEmpty())
		{
			menu.AddSeparator("");
			menu.AddDisabledItem(new GUIContent("Clipboard"));
			foreach (ActTrack clipboard in ActTrackClipboard.GetTracks())
			{
				if (clipboard != null)
				{
					menu.AddDisabledItem(new GUIContent(clipboard.ToString()));
				}
			}
			if (ActTrackClipboard.CanAdd(track))
			{
				menu.AddItem(new GUIContent("Add"), false, AddContext, track);
			}
			menu.AddItem(new GUIContent("Paste"), false, PasteContext);
		}
		menu.ShowAsContext();
	}

	void EmptyContext()
	{
		GenericMenu menu = new GenericMenu();
		menu.allowDuplicateNames = true;
		if (ActTrackClipboard.GetTracks().Count > 0)
		{
			menu.AddSeparator("");
			menu.AddDisabledItem(new GUIContent("Clipboard"));
			menu.AddSeparator("");
			foreach (ActTrack clipboard in ActTrackClipboard.GetTracks())
			{
				menu.AddDisabledItem(new GUIContent(clipboard.ToString()));
			}
			menu.AddItem(new GUIContent("Paste"), false, PasteContext);
		}
		menu.ShowAsContext();
	}

	public static void UpdateTrackList(ActTree tree, ref SerializedObject sTree)
	{
		List<ActTrack> trackList = tree.GetTracks();
		trackList.Clear();
		Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(tree));
		foreach (Object asset in assets)
		{
			ActTrack track = asset as ActTrack;
			if (track != null)
			{
				trackList.Add(track);
			}
		}

        trackList = trackList.OrderBy(x => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(x))).ToList();

        EditorUtility.SetDirty(tree);
		sTree = new SerializedObject(tree);
		ActTreeEditorWindow.Get().UpdateTree(); // Need to inform tree window that tree has changed
	}
}
