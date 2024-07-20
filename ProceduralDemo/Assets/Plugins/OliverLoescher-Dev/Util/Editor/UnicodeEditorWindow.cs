using UnityEditor;
using UnityEngine;

namespace ODev
{
	public class UnicodeEditorWindow : EditorWindow
	{
		[MenuItem("ODev/Unicode")]
		public static UnicodeEditorWindow Get()
		{
			UnicodeEditorWindow window = GetWindow<UnicodeEditorWindow>("Unicode");
			window.Show();
			return window;
		}

		private static readonly int START = 0;
		private static readonly int END = 0xFFFF;
		private static readonly int ITEMS_PER_PAGE = 150;

		private Vector2 m_ScrollPos = Vector2.zero;
		private int m_Page = 1;
		private bool m_ShowCodes = true;

		private void OnGUI()
		{
			using (Util.GUI.UsableHorizontal.Use())
			{
				m_Page = EditorGUILayout.IntSlider("Page", m_Page, 1, (END - START) / ITEMS_PER_PAGE);
				if (GUILayout.Button("Previous"))
				{
					m_Page--;
				}
				if (GUILayout.Button("Next"))
				{
					m_Page++;
				}
				GUILayout.Space(32.0f);
				if (GUILayout.Button("Toggle Codes"))
				{
					m_ShowCodes = !m_ShowCodes;
				}
			}

			GUIStyle style = new(GUI.skin.label)
			{
				fontSize = 12
			};

			using (Util.GUI.UsableScrollRect.Use(ref m_ScrollPos))
			{
				int start = START + (m_Page - 1) * ITEMS_PER_PAGE;
				int end = START + m_Page * ITEMS_PER_PAGE;
				int dif = end - start;
				int columns = 5;
				int countPerColumn = Mathf.CeilToInt((float)dif / columns);

				using (Util.GUI.UsableHorizontal.Use())
				{
					for (int i = 0; i < columns; i++)
					{
						DrawColumn(style, start, countPerColumn, i);
					}
				}
			}
		}

		private void DrawColumn(GUIStyle pStyle, int pStart, int pCountPerColumn, int pIndex)
		{
			using (Util.GUI.UsableVertical.Use())
			{
				int columnStart = pStart + pIndex * pCountPerColumn;
				int columnEnd = pStart + (pIndex + 1) * pCountPerColumn;
				for (int j = columnStart; j < columnEnd; j++)
				{
					using (Util.GUI.UsableHorizontal.Use())
					{
						string hex = j.ToString("x");
						string uni =
							j >= 0xD800 && j <= 0xDFFF ? string.Empty : // I guess these aren't valid
							char.ConvertFromUtf32(j);
						EditorGUILayout.LabelField(uni, pStyle, GUILayout.Width(20.0f), GUILayout.Height(20.0f));
						if (m_ShowCodes)
						{
							EditorGUILayout.LabelField(hex, pStyle);
						}
					}
				}
			}
		}
	}
}