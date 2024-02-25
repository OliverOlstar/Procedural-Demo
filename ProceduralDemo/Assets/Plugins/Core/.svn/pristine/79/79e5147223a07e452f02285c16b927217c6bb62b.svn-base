
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Act2
{
	public class TrackDrawer
	{
		private static readonly float TIMING_LABEL_WIDTH = 32.0f;

		private NodeItemContextMenu m_ContextMenu = new NodeItemContextMenu(
			new DeleteContextMenu(),
			new DuplicateContextMenu(),
			new CopyPasteContextMenu());

		private NodeItemContextMenu m_EmptyContextMenu = new NodeItemContextMenu(new CopyPasteContextMenu());

		public void OnGUI(IActObject tree, ref SerializedObject sTree, IActNode node, ref SerializedProperty sNode)
		{
			m_ContextMenu.OnGUI(tree, ref sTree, node, ref sNode);
			m_EmptyContextMenu.OnGUI(tree, ref sTree, node, ref sNode);

			Rect sourceRect = EditorGUILayout.GetControlRect();
			DrawItemLabel(sourceRect, SelectionDrawer.SelectionState.None, out GUIStyle labelStyle, out Rect labelRect, out Rect trackAreaRect);
			labelStyle.fontStyle = FontStyle.Bold;
			labelStyle.alignment = TextAnchor.MiddleLeft;
			if (GUI.Button(sourceRect, string.Empty, GUIStyle.none) && Event.current.button != 0)
			{
				m_EmptyContextMenu.Show(tree, node, null, -1);
			}
			EditorGUI.LabelField(labelRect, "Tracks", labelStyle);

			TimedItemProperties timingProperties = TimedItemUtil.GetTimedItemProperties(node.GetAllTimedItems());
			List<(Track, int)> tracks = TimedItemUtil.GetSortedTracks(node.Tracks);
			foreach ((Track, int) pair in tracks)
			{
				Track track = pair.Item1;
				int index = pair.Item2;
				DrawTrack(
					tree,
					node,
					track,
					index,
					timingProperties,
					out bool rightClicked);
				if (rightClicked)
				{
					m_ContextMenu.Show(tree, node, track, index);
				}
			}
		}

		public static float GetActItemWidth(float totalWidth)
		{
			return Mathf.Max(132.0f, 0.33f * totalWidth);
		}

		public static readonly Color32 COPIED_BG_COLOR = new Color32(170, 193, 221, 255);
		public static readonly Color32 SELECTED_BG_COLOR = new Color32(62, 125, 231, 255);
		public static readonly Color SELECTED_TEXT_COLOR = Color.white;
		public static readonly Color ERROR_TEXT_COLOR = new Color32(255, 50, 40, 255);

		public static void DrawSelectionRect(Rect rect, SelectionDrawer.SelectionState selected)
		{
			EditorGUI.DrawRect(rect, selected == SelectionDrawer.SelectionState.Copied ? COPIED_BG_COLOR : SELECTED_BG_COLOR);
		}

		public static void DrawItemLabel(
			Rect sourceRect,
			SelectionDrawer.SelectionState selected,
			out GUIStyle labelStyle,
			out Rect labelRect,
			out Rect contentRect)
		{
			Color textColor = selected == SelectionDrawer.SelectionState.None ? GUIStyle.none.normal.textColor : SELECTED_TEXT_COLOR;
			labelStyle = new GUIStyle(GUI.skin.label);
			labelStyle.alignment = TextAnchor.MiddleRight;
			labelStyle.normal.textColor = textColor;
			labelRect = sourceRect;
			labelRect.width = GetActItemWidth(sourceRect.width);
			if (selected == SelectionDrawer.SelectionState.None)
			{
				EditorGUI.DrawRect(labelRect, new Color(0.6f, 0.6f, 0.6f));
			}
			else
			{
				Rect highlightedRect = new Rect(sourceRect);
				//highlightedRect.xMin -= 4.0f;
				//highlightedRect.yMin -= 1.0f;
				//highlightedRect.xMax += 4.0f;
				//highlightedRect.yMax += 1.0f;
				DrawSelectionRect(highlightedRect, selected);
			}

			contentRect = sourceRect;
			contentRect.x += labelRect.width;
			contentRect.width -= labelRect.width;
		}

		public static bool DrawTrack(
			IActObject obj,
			IActNode node,
			Track track,
			int index,
			TimedItemProperties timingProperties,
			out bool rightClicked)
		{
			if (track == null)
			{
				Debug.LogWarning(Core.Str.Build(obj.name, " has a null track"));
				rightClicked = false;
				return false;
			}
			bool isValid = track._EditorIsValid(obj, node, out _);
			DrawTimedItem(
				obj, 
				node, 
				track, 
				NodeItemType.Track,
				index, 
				timingProperties, 
				isValid, 
				out GUIStyle labelStyle, 
				out Rect labelRect, 
				out rightClicked);
			string trackName = track.ToString();
			EditorGUI.LabelField(labelRect, trackName, labelStyle);
			return true;
		}

		public static void DrawTimedItem(
			IActObject tree, 
			IActNode node, 
			ITimedItem item,
			NodeItemType itemType,
			int index, 
			TimedItemProperties 
			timingProperties, 
			bool isValid,
			out GUIStyle labelStyle,
			out Rect labelRect,
			out bool rightClicked)
		{
			SelectionDrawer.SelectionState selected = SelectionDrawer.GetSelectionState(tree, node, itemType, index);
			Rect sourceRect = EditorGUILayout.GetControlRect();
			DrawItemLabel(sourceRect, selected, out GUIStyle textStyle, out labelRect, out Rect trackAreaRect);

			labelStyle = new GUIStyle(textStyle);
			if (!isValid)
			{
				labelStyle.normal.textColor = ERROR_TEXT_COLOR;
			}

			if (GUI.Button(sourceRect, string.Empty, GUIStyle.none))
			{
				// Use this to fix the annoying bug where we switch displayed objects but the values for the previous one is still in the text field
				EditorGUIUtility.editingTextField = false;
				SelectionDrawer.SetSelected(
					tree,
					node,
					itemType,
					index,
					Event.current.button == 0,
					Event.current.control);
				rightClicked = Event.current.button == 1;
			}
			else
			{
				rightClicked = false;
			}

			bool isTimingValid = ValidateTiming(item, timingProperties);
			DrawTimelineBar(trackAreaRect, item, textStyle, timingProperties, out _, isValid, isTimingValid);
		}

		private static bool ValidateTiming(ITimedItem item, TimedItemProperties properties)
		{
			float startTime = item.GetStartTime();
			float endTime = item._EditorDisplayEndTime();
			if (startTime < 0.0f)
			{
				return true;
			}
			if (startTime > properties.ValidTimeRange + Core.Util.EPSILON) // Track will never play if start time is after all master tracks have ended
			{
				return false;
			}
			if (!item.HasEndEvent())
			{
				return true;
			}
			if (endTime < 0.0f)
			{
				return true;
			}
			if (endTime > properties.ValidTimeRange + Core.Util.EPSILON) // Track can never finish properly if end time is after all master tracks have ended
			{
				return false;
			}
			return true;
		}

		public static void DrawTimelineBar(
			Rect sourceRect, 
			ITimedItem item,
			GUIStyle labelStyle,
			TimedItemProperties properties,
			out Rect trackAreaRect,
			bool isValid = true,
			bool isTimingValid = true)
		{
			float startTime = item.GetStartTime();
			float endTime = item._EditorDisplayEndTime();

			trackAreaRect = sourceRect;
			trackAreaRect.width -= 2.0f * TIMING_LABEL_WIDTH;
			trackAreaRect.x += TIMING_LABEL_WIDTH;

			Rect validAreaRect = trackAreaRect;
			validAreaRect.width *= properties.ValidTimeRange / properties.TotalTimeRange;

			Rect trackRect = trackAreaRect;
			if (!item.IsMajor())
			{
				if (!item.HasEndEvent())
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

			if (startTime < 0.0f)
			{
				trackRect.width = item.IsMajor() ? 0.33f * trackRect.height : trackRect.height;
				trackRect.x = validAreaRect.xMax;
			}
			else
			{
				trackRect.x = Mathf.Lerp(trackAreaRect.xMin, trackAreaRect.xMax, startTime / properties.TotalTimeRange);
				if (!item.HasEndEvent())
				{
					trackRect.width = item.IsMajor() ? 0.33f * trackRect.height : trackRect.height;
				}
				else if (endTime < 0.0f)
				{
					trackRect.xMax = validAreaRect.xMax;
					trackRect.width = Mathf.Max(trackRect.width, 4);
				}
				else
				{
					trackRect.xMax = Mathf.Lerp(trackAreaRect.xMin, trackAreaRect.xMax, endTime / properties.TotalTimeRange);
					trackRect.width = Mathf.Max(trackRect.width, 4);
				}
			}

			// Sub Times Rects
			List<float> subTimes = Core.ListPool<float>.Request();
			List<Rect> subTimeRects = Core.ListPool<Rect>.Request();
			item._EditorAddSubTimes(subTimes);
			if (subTimes != null && subTimes.Count > 0)
			{
				for (int i = 0; i < subTimes.Count; i++)
				{
					Rect trackSubRect = trackRect;

					float x = (subTimes[i] / properties.TotalTimeRange) * trackRect.width;
					trackSubRect.x += x;
					trackSubRect.width = 6.0f;
					trackRect.width = Mathf.Max(trackRect.width, x + trackSubRect.width); // Extend width if sub timers track is longer

					trackSubRect.y += trackRect.height;
					if (item.IsMajor())
					{
						trackSubRect.height *= 0.33f;
						trackSubRect.y -= trackSubRect.height;
					}

					subTimeRects.Add(trackSubRect);
				}
			}
			Core.ListPool<float>.Return(subTimes);

			// Outline.
			EditorGUI.DrawRect(trackRect, labelStyle.normal.textColor);
			// Track.
			Color trackColor =
				!isValid || !isTimingValid ? Color.red :
				!item.IsActive() ? Color.grey : 
				item._EditorGetColor();
			EditorGUI.DrawRect(new Rect(trackRect.position + Vector2.one, trackRect.size - Vector2.one * 2.0f), trackColor);
			// Sub Track & Outline.
			foreach (Rect subRect in subTimeRects)
			{
				EditorGUI.DrawRect(subRect, labelStyle.normal.textColor);
				EditorGUI.DrawRect(new Rect(subRect.position + Vector2.one, subRect.size - Vector2.one * 2.0f), trackColor);
			}
			Core.ListPool<Rect>.Return(subTimeRects);

			// Labels
			Rect r2 = trackRect;
			r2.y = trackAreaRect.y;
			r2.height = trackAreaRect.height;
			r2.width = TIMING_LABEL_WIDTH;

			bool timeAfter = item.HasEndEvent() || item.GetStartTime() < 0.0f;
			bool timeBefore = item.HasEndEvent() || !timeAfter;
			Color originalTextColor = labelStyle.normal.textColor;
			labelStyle.normal.textColor = isTimingValid ? labelStyle.normal.textColor : ERROR_TEXT_COLOR;
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
			labelStyle.normal.textColor = originalTextColor;
		}
	}
}
