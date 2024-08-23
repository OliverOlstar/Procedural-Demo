using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace GraphEditor
{
	public class EditorWindowInput
	{
		private static readonly int s_KeyCodeCount;

		static EditorWindowInput()
		{
			s_KeyCodeCount = Enum.GetValues(typeof(KeyCode)).Length;
		}

		public const int MOUSE_LEFT = 0;
		public const int MOUSE_RIGHT = 1;
		public const int MOUSE_MIDDLE = 2;

		private const int BUTTON_COUNT = 3;

		public delegate void MouseDownDelegate(Event guiEvent);
		public delegate void MouseUpDelegate(Event guiEvent);
		public delegate void MouseClickDelegate(Event guiEvent);
		public delegate void MouseDragDelegate(Event guiEvent);
		public delegate void KeyDownDelegate(Event guiEvent);
		public delegate void KeyUpDelegate(Event guiEvent);
		public delegate void KeyPressDelegate(Event guiEvent);

		private readonly Rect[] m_IgnoreRects;
		private readonly MouseDownDelegate[] m_MouseDownEvents;
		private readonly MouseUpDelegate[] m_MouseUpEvents;
		private readonly MouseClickDelegate[] m_MouseClickEvents;
		private readonly MouseDragDelegate[] m_MouseBeginDragEvents;
		private readonly MouseDragDelegate[] m_MouseDragEvents;
		private readonly MouseDragDelegate[] m_MouseEndDragEvents;
		private readonly bool[] m_MouseDownStates;
		private readonly bool[] m_MouseDragStates;
		private readonly Vector2?[] m_MouseDownPositions;
		private readonly KeyDownDelegate[] m_KeyDownEvents;
		private readonly KeyUpDelegate[] m_KeyUpEvents;
		private readonly KeyPressDelegate[] m_KeyPressEvents;
		private readonly bool[] m_KeyDownStates;

		public EditorWindowInput(params Rect[] ignoreRects)
		{
			m_IgnoreRects = ignoreRects;
			m_MouseDownEvents = new MouseDownDelegate[BUTTON_COUNT];
			m_MouseUpEvents = new MouseUpDelegate[BUTTON_COUNT];
			m_MouseClickEvents = new MouseClickDelegate[BUTTON_COUNT];
			m_MouseBeginDragEvents = new MouseDragDelegate[BUTTON_COUNT];
			m_MouseDragEvents = new MouseDragDelegate[BUTTON_COUNT];
			m_MouseEndDragEvents = new MouseDragDelegate[BUTTON_COUNT];
			m_MouseDownStates = new bool[BUTTON_COUNT];
			m_MouseDragStates = new bool[BUTTON_COUNT];
			m_MouseDownPositions = new Vector2?[BUTTON_COUNT];
			m_KeyDownEvents = new KeyDownDelegate[s_KeyCodeCount];
			m_KeyUpEvents = new KeyUpDelegate[s_KeyCodeCount];
			m_KeyPressEvents = new KeyPressDelegate[s_KeyCodeCount];
			m_KeyDownStates = new bool[s_KeyCodeCount];
		}

		public void Update(Event guiEvent)
		{
			switch (guiEvent.type)
			{
				case EventType.MouseDown:
					if (PointIsInIgnoreRect(guiEvent.mousePosition))
					{
						break;
					}
					m_MouseDownPositions[guiEvent.button] = guiEvent.mousePosition;
					m_MouseDownEvents[guiEvent.button]?.Invoke(guiEvent);
					m_MouseDownStates[guiEvent.button] = true;
					break;
				case EventType.MouseUp:
					if (!m_MouseDownStates[guiEvent.button])
					{
						break;
					}
					if (m_MouseDragStates[guiEvent.button])
					{
						m_MouseDragStates[guiEvent.button] = false;
						m_MouseEndDragEvents[guiEvent.button]?.Invoke(guiEvent);
					}
					m_MouseUpEvents[guiEvent.button]?.Invoke(guiEvent);
					if (m_MouseDownPositions[guiEvent.button] == guiEvent.mousePosition)
					{
						m_MouseClickEvents[guiEvent.button]?.Invoke(guiEvent);
					}
					m_MouseDownStates[guiEvent.button] = false;
					break;
				case EventType.MouseDrag:
					if (!m_MouseDownStates[guiEvent.button])
					{
						break;
					}
					m_MouseDownPositions[guiEvent.button] = null;
					if (!m_MouseDragStates[guiEvent.button])
					{
						m_MouseDragStates[guiEvent.button] = true;
						m_MouseBeginDragEvents[guiEvent.button]?.Invoke(guiEvent);
					}
					m_MouseDragEvents[guiEvent.button]?.Invoke(guiEvent);
					break;
				case EventType.KeyDown:
					if (m_KeyDownStates[(int)guiEvent.keyCode])
					{
						break;
					}
					m_KeyDownEvents[(int)guiEvent.keyCode]?.Invoke(guiEvent);
					m_KeyDownStates[(int)guiEvent.keyCode] = true;
					break;
				case EventType.KeyUp:
					m_KeyUpEvents[(int)guiEvent.keyCode]?.Invoke(guiEvent);
					if (m_KeyDownStates[(int)guiEvent.keyCode])
					{
						m_KeyPressEvents[(int)guiEvent.keyCode]?.Invoke(guiEvent);
					}
					m_KeyDownStates[(int)guiEvent.keyCode] = false;
					break;
				case EventType.MouseMove:
				case EventType.ScrollWheel:
				case EventType.Repaint:
				case EventType.Layout:
				case EventType.DragUpdated:
				case EventType.DragPerform:
				case EventType.DragExited:
				case EventType.Ignore:
				case EventType.Used:
				case EventType.ValidateCommand:
				case EventType.ExecuteCommand:
				case EventType.ContextClick:
				case EventType.MouseEnterWindow:
				case EventType.MouseLeaveWindow:
				default:
					break;
			}
		}

		public void SubscribeMouseDown(int button, MouseDownDelegate listener)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseDownEvents[button] = listener;
		}

		public void UnsubscribeMouseDown(int button)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseDownEvents[button] = null;
		}

		public void SubscribeMouseUp(int button, MouseUpDelegate listener)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseUpEvents[button] = listener;
		}

		public void UnsubscribeMouseUp(int button)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseUpEvents[button] = null;
		}

		public void SubscribeMouseClick(int button, MouseClickDelegate listener)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseClickEvents[button] = listener;
		}

		public void UnsubscribeMouseClick(int button)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseClickEvents[button] = null;
		}

		public void SubscribeMouseBeginDrag(int button, MouseDragDelegate listener)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseBeginDragEvents[button] = listener;
		}

		public void UnsubscribeMouseBeginDrag(int button)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseBeginDragEvents[button] = null;
		}

		public void SubscribeMouseDrag(int button, MouseDragDelegate listener)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseDragEvents[button] += listener;
		}

		public void UnSubscribeMouseDrag(int button)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseDragEvents[button] = null;
		}

		public void SubscribeMouseEndDrag(int button, MouseDragDelegate listener)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseEndDragEvents[button] = listener;
		}

		public void UnsubscribeMouseEndDrag(int button)
		{
			Assert.IsTrue(button >= 0 && button < BUTTON_COUNT);
			m_MouseEndDragEvents[button] = null;
		}

		public void SubscribeKeyDown(KeyCode key, KeyDownDelegate listener)
		{
			m_KeyDownEvents[(int)key] = listener;
		}

		public void UnsubscribeKeyDown(KeyCode key)
		{
			m_KeyDownEvents[(int)key] = null;
		}

		public void SubscribeKeyUp(KeyCode key, KeyUpDelegate listener)
		{
			m_KeyUpEvents[(int)key] = listener;
		}

		public void UnsubscribeKeyUp(KeyCode key)
		{
			m_KeyUpEvents[(int)key] = null;
		}

		public void SubscribeKeyPress(KeyCode key, KeyPressDelegate listener)
		{
			m_KeyPressEvents[(int)key] = listener;
		}

		public void UnsubscribeKeyPress(KeyCode key)
		{
			m_KeyPressEvents[(int)key] = null;
		}

		public bool PointIsInIgnoreRect(Vector2 point)
		{
			for (int i = 0; i < m_IgnoreRects.Length; ++i)
			{
				if (m_IgnoreRects[i].Contains(point))
				{
					return true;
				}
			}
			return false;
		}
	}
}
