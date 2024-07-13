using System.Collections.Generic;
using UnityEngine;
using ODev.Debug;

namespace ODev.CheatMenu
{
	public static class CheatMenuGUI
	{
		public static GUIStyle DefaultButtonStyle => GUI.skin.button;

		public static bool Button(string name, GUIStyle style, params GUILayoutOption[] options)
		{
			GUIContent content = new(name);
			Rect r = GUILayoutUtility.GetRect(content, style, options);
			return Button(r, name, style);
		}
		public static bool Button(string name, params GUILayoutOption[] options) => Button(name, DefaultButtonStyle, options);

		public static bool Button(Rect position, string name, GUIStyle style = null)
		{
			bool selected = false;
			bool pressed = false;
			if (!string.IsNullOrEmpty(s_NextControlID))
			{
				if (!s_Controls.Contains(s_NextControlID))
				{
					s_Controls.Add(s_NextControlID);
					selected = s_NextControlID == s_SelectedControlID;
					pressed = s_NextControlID == s_PressedControlID;
					if (pressed)
					{
						s_PressedControlID = null;
					}
				}
				else
				{
					Util.Debug.LogWarning($"CheatMenuGUI.Button() There is already a control registered to ID {s_NextControlID}, duplicates will not be selectable by game pad", typeof(CheatMenuGUI));
				}
				s_NextControlID = null;
			}
			pressed |= GUI.Button(position, name, style == null ? DefaultButtonStyle : style);
			if (selected)
			{
				Vector2 size = new(15, 20);
				Rect rhs = new(
					position.xMax - 0.5f * size.x,
					position.center.y - 0.5f * size.y,
					size.x,
					size.y);
				Rect lhs = new(
					position.xMin - 0.5f * size.x,
					position.center.y - 0.5f * size.y,
					size.x,
					size.y);

				Color temp = GUI.color;
				GUI.color = Color.green;
				GUI.Label(lhs, "\u25ba");
				GUI.Label(rhs, "\u25c0");
				GUI.color = temp;
			}
			return pressed;
		}

		public static Rect StringField(in string arg, ref string newArg, in float xOffset, in float widthOffset)
		{
			Rect r = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.textField);
			r.x += xOffset;
			r.width -= xOffset + widthOffset;
			newArg = GUI.TextField(r, arg);
			return r;
		}

		public static void IntField(in int intArg, ref string newArg, in float xOffset, in float widthOffset)
		{
			Rect r = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.textField);
			r.x += xOffset;
			r.width -= xOffset + widthOffset;
			newArg = GUI.TextField(r, intArg.ToString());
		}

		public static void Slider(in DebugOption.Slider slider, in int intArg, ref string newArg)
		{
			Rect r = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.textField);
			r.width -= 45.0f;
			slider.GetSliderItems(out int min, out int max);
			newArg = Mathf.RoundToInt(GUI.HorizontalSlider(r, intArg, min, max)).ToString();
			r.x += r.width;
			r.width = 45.0f;
			newArg = GUI.TextField(r, newArg);
		}

		public static int SelectionGrid(int selectedIndex, string[] texts, int xCount)
		{
			string rootControlID = s_NextControlID;
			s_NextControlID = null;

			int rows = Mathf.CeilToInt((float)texts.Length / xCount);
			int index = 0;
			for (int i = 0; i < rows; i++)
			{
				GUILayout.BeginHorizontal();
				for (int j = 0; j < xCount; j++)
				{
					Color temp = GUI.color;
					GUI.color = index == selectedIndex ? Color.white : Color.grey;
					if (!string.IsNullOrEmpty(rootControlID))
					{
						SetNextControlID(rootControlID + "." + texts[index]);
					}
					if (Button(texts[index]))
					{
						selectedIndex = index;
					}
					GUI.color = temp;
					index++;
					if (index >= texts.Length)
					{
						break;
					}
				}
				GUILayout.EndHorizontal();
			}
			return selectedIndex;
		}

		public static void Dropdown(ref string newArg, string[] argPresets, int currentIndex, int itemsPerRow = -1)
		{
			int rows = 1;
			if (itemsPerRow <= 0)
			{
				itemsPerRow = argPresets.Length;
			}
			else
			{
				// Calculate rows required then rebalance items per row
				rows = Mathf.CeilToInt((float)argPresets.Length / itemsPerRow);
				itemsPerRow = Mathf.CeilToInt((float)argPresets.Length / rows);
			}

			string rootControlID = s_NextControlID;
			s_NextControlID = null;

			for (int row = 0; row < rows; row++)
			{
				Rect r = GUILayoutUtility.GetRect(new GUIContent(), CheatMenuStyles.ExtraSmallButtonStyle);
				int startIndex = row * itemsPerRow;
				int endIndex = Mathf.Min(startIndex + itemsPerRow, argPresets.Length);
				r.width = r.width / (endIndex - startIndex);
				for (int i = startIndex; i < endIndex; i++)
				{
					if (currentIndex == i)
					{
						GUI.color = Color.yellow;
					}
					if (!string.IsNullOrEmpty(rootControlID))
					{
						SetNextControlID(rootControlID + "." + argPresets[i]);
					}
					if (Button(r, argPresets[i], CheatMenuStyles.ExtraSmallButtonStyle))
					{
						newArg = argPresets[i];
					}
					GUI.color = Color.white;
					r.x += r.width;
				}
			}
		}

		public static void EnumDropdown<TEnum>(ref TEnum arg, int itemsPerRow = -1)
		{
			string current = arg.ToString();
			string[] names = System.Enum.GetNames(typeof(TEnum));
			int index = -1;
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i] == current)
				{
					index = i;
					break;
				}
			}
			Dropdown(ref current, names, index, itemsPerRow);
			arg = (TEnum)System.Enum.Parse(typeof(TEnum), current);
		}

		private static List<string> s_Controls = new();
		public static void ResetControls()
		{
			if (s_SelectedControlID != null && !s_Controls.Contains(s_SelectedControlID))
			{
				s_SelectedControlID = null;
			}
			if (s_PressedControlID != null && !s_Controls.Contains(s_PressedControlID))
			{
				s_PressedControlID = null;
			}
			s_Controls.Clear();
		}

		private static string s_NextControlID = null;
		public static void SetNextControlID(string controlName) => s_NextControlID = controlName;

		public static string s_SelectedControlID = null;

		public static string s_PressedControlID = null;

		public enum ControlInput
		{
			None = 0,
			OpenMenu,
			CloseMenu,
			Select,
			NavigateForward,
			NavigateBackward,
			ScrollUp,
			ScrollDown
		}
		public static void UpdateControls(ControlInput input)
		{
			if (s_Controls.Count == 0)
			{
				return;
			}
			if (s_SelectedControlID == null)
			{
				s_SelectedControlID = s_Controls[0];
				return;
			}
			if (input == ControlInput.Select)
			{
				s_PressedControlID = s_SelectedControlID;
				return;
			}
			int index = s_Controls.IndexOf(s_SelectedControlID);
			if (index < 0)
			{
				index = 0;
			}
			int last = s_Controls.Count - 1;
			if (input == ControlInput.NavigateBackward)
			{
				index = index > 0 ? --index : last;
			}
			else if (input == ControlInput.NavigateForward)
			{
				index = index < last ? ++index : 0;
			}
			s_SelectedControlID = s_Controls[index];
		}
	}
}
