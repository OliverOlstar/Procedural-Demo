using UnityEditor;
using UnityEngine;

namespace ActCore
{
	public class ActGraphController
	{
		public const float ZOOM_SPEED = 0.02f;
		public const float MAX_ZOOM = 1.0f;
		public const float MIN_ZOOM = 0.25f;

		private Rect m_GraphPosition = new();
		public Rect Position => m_GraphPosition;

		private Vector2 m_GraphOffset = Vector2.zero;
		public Vector2 Offset => m_GraphOffset;

		private float m_GraphScale = 1.0f;
		public float Scale => m_GraphScale;

		private Rect m_ClippedWindowsRect = Rect.zero;

		public void Begin(out bool repaint)
		{
			repaint = false;
			Event e = Event.current;
			if (e.button == 2 && e.type == EventType.MouseDrag)
			{
				m_GraphOffset += e.delta / m_GraphScale;
				repaint = true;
			}

			if (e.isScrollWheel)
			{
				Vector2 mousePos = Event.current.mousePosition;
				Vector2 delta = Event.current.delta;
				Vector2 graphMousePos = ConvertScreenPositionToGraphPosition(mousePos);
				float scaleDelta = delta.y * ZOOM_SPEED;
				float oldScale = m_GraphScale;
				m_GraphScale = Mathf.Clamp(m_GraphScale - scaleDelta, MIN_ZOOM, MAX_ZOOM);
				Vector2 toMouse = graphMousePos - m_GraphOffset;
				float scaleAmount = oldScale / m_GraphScale;
				m_GraphOffset -= toMouse - scaleAmount * toMouse;
				repaint = true;
			}

			Rect graphRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			ActGraphEditorUtil.DrawGrid(graphRect, m_GraphOffset, m_GraphScale);
			ActGraphEditorUtil.DrawGradient(graphRect);
			m_GraphPosition = graphRect;
			EditorWindowScaleUtil.Begin(m_GraphPosition, m_GraphScale);

			m_ClippedWindowsRect =
				EditorWindowScaleUtil.ScaleSizeBy(m_GraphPosition, 1.0f / m_GraphScale, m_GraphPosition.min);
			m_ClippedWindowsRect.y -= EditorWindowScaleUtil.EDITOR_WINDOW_TAB_HEIGHT;
		}

		private Vector2 ConvertScreenPositionToGraphPosition(Vector2 position)
		{
			return (position - m_GraphPosition.min) / m_GraphScale + m_GraphOffset;
		}

		public void End()
		{
			EditorWindowScaleUtil.End();
		}

		public bool IsVisible(Rect position)
		{
			return m_ClippedWindowsRect.Overlaps(position);
		}
	}
}
