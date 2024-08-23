using UnityEngine;
using ODev.Debug;

namespace ODev.CheatMenu.Pages
{
	public class CheatMenuDebugOptionsPage : CheatMenuPage
	{
		public override CheatMenuGroup Group => CheatMenuODevGroups.Debug;
		public override string Name => "Options";
		public override int Priority => -1;
		public override bool IsAvailable() => true;

		private DebugOption m_ActiveTooltip = null;
		private DebugOption m_ActivePresets = null;
		private GUIStyle m_TitleStyle;
		private GUIStyle m_ToggleStyle;
		private GUIStyle m_ToggleTipStyle;
		private GUIStyle m_ButtonStyle;

		public override void DrawGUI()
		{
			TimeScaleManagerGUI();
			InitalizeStyles();

			// Debug Options
			foreach (string groupName in DebugOption.GetGroupNames())
			{
				if (DebugOption.Group.EditorOnlyGroups.Contains(groupName))
				{
					continue;
				}

				GUILayout.Label(groupName, m_TitleStyle);
				foreach (DebugOption op in DebugOption.GetGroupOptions(groupName))
				{
					if (!op.CanShow())
					{
						continue;
					}

					bool hasTooltip = !string.IsNullOrEmpty(op.Tooltip);
					bool isTooltipOpen = m_ActiveTooltip == op;

					// Big button
					bool set = DebugOption.IsSet(op);
					Rect r = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.button);
					if (hasTooltip)
					{
						r.width -= 20f;
					}
					CheatMenuGUI.SetNextControlID("CheatMenuDebugOptionsPage." + op.Name);
					if (CheatMenuGUI.Button(r, "", GUI.skin.box))
					{
						if (set)
						{
							op.Clear();
						}
						else
						{
							op.Set();
						}
					}

					// Big button Icon
					Rect r1 = r;
					r1.y -= 2.0f;
					r1.width = 16.0f;
					r1.x += 4.0f;
					GUI.Label(r1, set ? "\u25cf" : "\u25cb", m_ToggleStyle);
					float toggleOffset = (r1.x + r1.width) * 0.5f;

					// Tooltip Button
					if (hasTooltip)
					{
						Rect r3 = r;
						r3.width = 20.0f;
						r3.x += r.width;
						if (GUI.Button(r3, isTooltipOpen ? "\u25bc" : "\u25c0", m_ToggleTipStyle))
						{
							m_ActiveTooltip = isTooltipOpen ? null : op;
							isTooltipOpen = !isTooltipOpen;
						}
					}

					// Big button Label
					Rect r2 = r;
					r2.x += toggleOffset;
					r2.width -= toggleOffset;
					GUI.Label(r2, op.Name, m_ButtonStyle);

					string arg = DebugOption.GetArg(op);
					int intArg = DebugOption.GetInt(op);
					string newArg = arg;

					string[] argPresets;
					int currentIndex;

					if (set)
					{
						switch (op)
						{
							case DebugOption.String _:
								CheatMenuGUI.StringField(arg, ref newArg, toggleOffset, 0.0f);
								break;

							case DebugOption.Int _:
								CheatMenuGUI.IntField(intArg, ref newArg, toggleOffset, 0.0f);
								break;

							case DebugOption.Slider opSlider:
								CheatMenuGUI.Slider(opSlider, intArg, ref newArg);
								break;

							case DebugOption.StringWithDropdown opStringDrop:
								bool hasArgPresets = opStringDrop.TryGetDropdownItems(out argPresets, out currentIndex);
								r = CheatMenuGUI.StringField(arg, ref newArg, toggleOffset, hasArgPresets ? 25.0f : 0.0f);

								if (hasArgPresets)
								{
									r.x += r.width;
									r.width = 25.0f;
									bool isPresetsOpen = m_ActivePresets == op;
									if (GUI.Button(r, "-"))
									{
										m_ActivePresets = isPresetsOpen ? null : op;
										isPresetsOpen = !isPresetsOpen;
									}
									if (isPresetsOpen)
									{
										CheatMenuGUI.Dropdown(ref newArg, argPresets, currentIndex, 7);
									}
								}
								break;

							case DebugOption.Dropdown opDrop:
								if (!opDrop.TryGetDropdownItems(out argPresets, out currentIndex))
								{
									Util.Debug.LogWarning($"{op.Name} is of type {nameof(DebugOption.Dropdown)} but arg presets is null or empty. This shouldn't happen", typeof(CheatMenuDebugOptionsPage));
									CheatMenuGUI.StringField(arg, ref newArg, toggleOffset, 0.0f);
									break;
								}
								CheatMenuGUI.Dropdown(ref newArg, argPresets, currentIndex, 9);
								break;
						}
					}

					if (arg != newArg)
					{
						DebugOption.SetArg(op, newArg);
					}

					// Tooltip
					if (isTooltipOpen)
					{
						r = GUILayoutUtility.GetRect(new GUIContent(op.Tooltip), CheatMenuStyles.SmallLabel);
						r.x += 4.0f;
						r.width -= 4.0f;
						GUI.Label(r, op.Tooltip, CheatMenuStyles.SmallLabel);
					}
				}
			}

			if (GUILayout.Button("Reset all options"))
			{
				DebugOption.Reset();
			}
		}

		private void InitalizeStyles()
		{
			m_TitleStyle ??= new(GUI.skin.label)
			{
				fontStyle = FontStyle.Bold
			};
			m_ToggleStyle = new(GUI.skin.label)
			{
				fontSize = 24,
				alignment = TextAnchor.MiddleLeft
			};
			m_ToggleTipStyle = new(GUI.skin.label)
			{
				fontSize = 16,
				alignment = TextAnchor.MiddleLeft
			};
			m_ButtonStyle = new(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleLeft
			};
		}

		private static void TimeScaleManagerGUI()
		{
			TimeScaleManager timeScaleManager = TimeScaleManager.Instance;

			GUILayout.BeginHorizontal();
			CheatMenuGUI.SetNextControlID("CheatMenuDebugOptionsPage.DecTime");
			if (CheatMenuGUI.Button(" < "))
			{
				timeScaleManager.EditorDec();
			}
			GUILayout.Label($"Time Scale {timeScaleManager.EditorSlowMo}%");
			CheatMenuGUI.SetNextControlID("CheatMenuDebugOptionsPage.IncTime");
			if (CheatMenuGUI.Button(" > "))
			{
				timeScaleManager.EditorInc();
			}
			GUILayout.EndHorizontal();
		}
	}
}
