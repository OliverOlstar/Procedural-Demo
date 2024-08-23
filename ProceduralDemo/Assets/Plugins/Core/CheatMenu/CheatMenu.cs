using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.CheatMenu
{
	public class CheatMenu : MonoBehaviour
	{
		[AttributeUsage(AttributeTargets.Class)]
		public class IgnorePageAttribute : Attribute { }

		[DebugOptionList]
		public static class DebugOptions
		{
			public static DebugOption LogCheatMenu = new DebugOption.Toggle(DebugOption.Group.Log, "Log CheatMenu", DebugOption.DefaultSetting.OnDevice);
		}

		private const int BUTTONS_PER_ROW = 3;
		private const float MAX_BUTTON_WIDTH = 99.0f;
		private const int SPACE_PRIORITY_THRESHOLD = 1000;
		private const string CHEAT_MENU_GROUP_PREF = "CHEAT_MENU_GROUP";

		private const float OPEN_TIME = 0.5f;

		[SerializeField, Tooltip("Increase or decrease the scale of the cheat menu GUI. " +
			"This is especially for mobile games in order to make buttons easier to press")]
		private float m_ScaleGUI = 3.33f;
		[SerializeField, Tooltip("Screen resolution that will be use primarily during testing/development.\n\n" +
			"This value is used to re-target GUI scale for other resolutions in order to maintain a consistent 'real world' size of buttons/text.\n\n" +
			"ie. We don't want the GUI to be 2x smaller when working on a 4k screen vs and HD screen")]
		private float m_ReferenceScreenWidth = 1080.0f;

		private float Ratio => Screen.width / m_ReferenceScreenWidth;
		private float NormalizedScale => Ratio * m_ScaleGUI;
		private float ScaledWidth => Screen.width / NormalizedScale;
		private float ScaledHeight => Screen.height / NormalizedScale;
		
#pragma warning disable CS0414
		[SerializeField, Percent]
		private float m_HorizScreenSpace = 1.0f;
		[SerializeField, Percent]
		private float m_VertScreenSpace = 0.95f;
#pragma warning restore

#if RELEASE
		public static void Open() { }
		public static void Close() { }
		public static void CloseGroup() { }
		public static bool IsOpen => false;
#else
		private static CheatMenu s_Instance = null;

		public static void Open() { if (s_Instance != null) { s_Instance.OpenInternal(); } }
		public static void Close() { if (s_Instance != null) { s_Instance.CloseInternal(); } }
		public static void CloseGroup() { if (s_Instance != null) { s_Instance.CloseGroupInternal(); } }
		public static bool IsOpen => s_Instance != null ? s_Instance.m_IsOpen : false;

		private bool m_IsOpen = false;
		private PlayerPrefsString m_CurrentGroupName = null;
		private Vector2 m_ScrollPosition = Vector2.zero;
		private static CheatMenuGroup[] s_GroupArray = null;
		private static Dictionary<string, CheatMenuGroup> s_GroupDict;
		private int m_TimeScaleHandle = TimeScaleManager.INVALID_HANDLE;
		private GUIStyle m_HeaderStyle = null;

#pragma warning disable CS0414 // No referenced in RELEASE
		[SerializeField]
		private GameObject m_InputBlocker = null;
#pragma warning restore CS0414

		private float m_OpenTimer = 0.0f;

		protected virtual void OnAwake() { }
		protected virtual void OnOpened() { }
		protected virtual void OnClosed() { }

		void Awake()
		{
			if (s_Instance != null)
			{
				DebugUtil.DevException("A new CheatMenu was just created but one already existed. CheatMenu is a singleton, this should never happen.");
			}
			s_Instance = this;

			DontDestroyOnLoad(gameObject);
			m_InputBlocker.gameObject.SetActive(false);

			if (s_GroupArray == null)
			{
				s_GroupDict = new Dictionary<string, CheatMenuGroup>();
				List<CheatMenuGroup> groupList = ListPool<CheatMenuGroup>.Request();
				foreach (Type type in TypeUtility.GetTypesDerivedFrom(typeof(CheatMenuPage)))
				{
					if (type.IsAbstract || type.GetCustomAttributes(typeof(IgnorePageAttribute), true).Length > 0)
					{
						continue;
					}
					CheatMenuPage page = Activator.CreateInstance(type) as CheatMenuPage;
					if (!s_GroupDict.TryGetValue(page.Group.Name, out CheatMenuGroup group))
					{
						group = page.Group;
						s_GroupDict.Add(group.Name, group);
						groupList.Add(group);
					}
					group.AddPage(page);
				}
				s_GroupArray = groupList.ToArray();
				ListPool<CheatMenuGroup>.Return(groupList);
				Array.Sort(s_GroupArray, ComparePageGroupsByPriority);
			}

			m_CurrentGroupName = new PlayerPrefsString(CHEAT_MENU_GROUP_PREF);

			foreach (CheatMenuGroup group in s_GroupArray)
			{
				group.OnInitialize();
			}
			OnAwake();
		}

		void OnDestroy()
		{
			if (s_Instance == this)
			{
				s_Instance = null;
			}
			foreach (CheatMenuGroup group in s_GroupArray)
			{
				group.OnDestroy();
			}
		}

		protected virtual CheatMenuGUI.ControlInput GetControlInput()
		{
			if (!IsOpen)
			{
				if ((Application.isEditor && !Input.GetMouseButton(1)) ||
				    (Application.isMobilePlatform && Input.touchCount < 2))
				{
					m_OpenTimer = 0.0f;
					return CheatMenuGUI.ControlInput.None;
				}
				if (Input.mousePosition.y < 0.75f * Screen.height)
				{
					m_OpenTimer = 0.0f;
					return CheatMenuGUI.ControlInput.None;
				}
				m_OpenTimer += Time.unscaledDeltaTime;
				if (m_OpenTimer < OPEN_TIME)
				{
					return CheatMenuGUI.ControlInput.None;
				}
				m_OpenTimer = 0.0f;
				return CheatMenuGUI.ControlInput.OpenMenu;
			}
			CheatMenuGUI.ControlInput input =
				Input.GetKeyDown(KeyCode.Escape) ? CheatMenuGUI.ControlInput.CloseMenu :
				Input.GetKeyDown(KeyCode.Return) ? CheatMenuGUI.ControlInput.Select :
				Input.GetKeyDown(KeyCode.RightArrow) ? CheatMenuGUI.ControlInput.NavigateForward :
				Input.GetKeyDown(KeyCode.LeftArrow) ? CheatMenuGUI.ControlInput.NavigateBackward :
				Input.GetKeyDown(KeyCode.UpArrow) ? CheatMenuGUI.ControlInput.ScrollUp :
				Input.GetKeyDown(KeyCode.DownArrow) ? CheatMenuGUI.ControlInput.ScrollDown :
				CheatMenuGUI.ControlInput.None;
			return input;
		}

		private void Update()
		{
			CheatMenuGUI.ControlInput controlInput = GetControlInput();
			switch (controlInput)
			{
				case CheatMenuGUI.ControlInput.None:
					break;
				case CheatMenuGUI.ControlInput.OpenMenu:
					if (!IsOpen)
					{
						Open();
					}
					break;
				case CheatMenuGUI.ControlInput.CloseMenu:
					if (IsOpen)
					{
						Close();
					}
					break;
				case CheatMenuGUI.ControlInput.ScrollUp:
					if (IsOpen)
					{
						float amount = 0.05f * ScaledHeight;
						m_ScrollPosition.y -= amount;
					}
					break;
				case CheatMenuGUI.ControlInput.ScrollDown:
					if (IsOpen)
					{
						float amount = 0.05f * ScaledHeight;
						m_ScrollPosition.y += amount;
					}
					break;
				default:
					if (IsOpen)
					{
						CheatMenuGUI.UpdateControls(controlInput);
					}
					break;
			}
		}

		private void OnGUI()
		{
			if (!IsOpen)
			{
				return;
			}
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.TRS(
				Vector3.zero,
				Quaternion.identity,
				new Vector3(NormalizedScale, NormalizedScale, 1.0f));

			// Scale up buttons and text fields to make them more pressable on mobile devices
			GUIStyle originalButton = GUI.skin.button;
			GUIStyle originalTextField = GUI.skin.textField;
			GUIStyle newButton = new(GUI.skin.button);
			GUIStyle newTextField = new(GUI.skin.textField);
			newButton.fixedHeight = 26;
			newTextField.fixedHeight = 26;
			GUI.skin.button = newButton;
			GUI.skin.textField = newTextField;

			if (m_HeaderStyle == null)
			{
				m_HeaderStyle = new GUIStyle(GUI.skin.label);
				m_HeaderStyle.alignment = TextAnchor.LowerCenter;
				m_HeaderStyle.fontStyle = FontStyle.Bold;
			}

			CheatMenuGUI.ResetControls();

			float width = ScaledWidth;
			float height = ScaledHeight;
			GUI.Box(new Rect(0f, 0f, width, height), string.Empty); // Background
			
			float widthScale = m_HorizScreenSpace;
			float heightScale = m_VertScreenSpace;
			Rect rect = new(
				0.5f * (1.0f - widthScale) * width,
				0.5f * (1.0f - heightScale) * height,
				widthScale * width,
				heightScale * height);
			using (GUIUtil.UsableArea.Use(rect))
			{
				// Check current group
				bool hasCurrentGroup = false;
				if (TryGetCurrentCurrentGroup(out CheatMenuGroup currentGroup))
				{
					hasCurrentGroup = currentGroup.UpdateIsAvailable();
					if (!hasCurrentGroup)
					{
						ClearCurrentGroup();
					}
				}
				// Draw
				if (hasCurrentGroup)
				{
					DrawGroupMenuButtons(currentGroup);
					bool pageChanged = currentGroup.DrawPageSelection();
					if (pageChanged)
					{
						m_ScrollPosition = Vector2.zero;
					}
					m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUI.skin.box);
					currentGroup.DrawPage();
					GUILayout.EndScrollView();
				}
				else
				{
					DrawMenuButtons();
					m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUI.skin.box);
					DrawPageSelection();
					GUILayout.EndScrollView();
				}
			}
			GUI.skin.button = originalButton;
			GUI.skin.textField = originalTextField;
			GUI.matrix = matrix;
		}

		private void DrawGroupMenuButtons(CheatMenuGroup currentGroup)
		{
			using (GUIUtil.UsableHorizontal.Use(GUI.skin.box, GUILayout.ExpandHeight(false)))
			{
				CheatMenuGUI.SetNextControlID("CheatMenu.DrawGroupMenuButtons.Back");
				if (CheatMenuGUI.Button("Back", GUILayout.Width(85.0f)))
				{
					currentGroup.OnPostClose();
					ClearCurrentGroup();
				}
				GUILayout.Label(TryGetCurrentGroupName(out string groupName) ? groupName : string.Empty, m_HeaderStyle);
				CheatMenuGUI.SetNextControlID("CheatMenu.DrawGroupMenuButtons.Close");
				if (CheatMenuGUI.Button("Close", GUILayout.Width(85.0f)))
				{
					Close();
				}
			}
		}

		private void DrawMenuButtons()
		{
			using (GUIUtil.UsableHorizontal.Use(GUI.skin.box, GUILayout.ExpandHeight(false)))
			{
				CheatMenuGUI.SetNextControlID("CheatMenu.DrawMenuButtons.Close");
				if (CheatMenuGUI.Button("Close"))
				{
					Close();
				}
			}
		}

		private void DrawPageSelection()
		{
			int index = 0;
			bool newSection = false;

			for (int row = 0; index < s_GroupArray.Length && row < 99; row += BUTTONS_PER_ROW)
			{
				using (GUIUtil.UsableHorizontal.Use())
				{
					for (int column = 0; column < BUTTONS_PER_ROW && index < s_GroupArray.Length; column++)
					{
						CheatMenuGroup group = s_GroupArray[index];
						if (index > 0 && !newSection &&
						    s_GroupArray[index - 1].Priority < group.Priority - SPACE_PRIORITY_THRESHOLD)
						{
							newSection = true;
							break;
						}
						newSection = false;
						GUI.enabled = group.IsAvailable();
						if (!GUI.enabled && group.HideWhenNotAvailable)
						{
							index++;
							column--;
							continue;
						}
						CheatMenuGUI.SetNextControlID("CheatMenu.DrawPageSelection." + group.Name);
						if (CheatMenuGUI.Button(group.Name, GUILayout.Width(MAX_BUTTON_WIDTH)))
						{
							SetCurrentGroup(group.Name);
							group.OnBecameActive();
						}
						index++;
					}
				}
				if (newSection)
				{
					GUILayout.Space(6.0f);
				}
			}
			GUI.enabled = true;
		}

		private bool TryGetCurrentGroupName(out string groupName)
		{
			groupName = m_CurrentGroupName.Value;
			return !string.IsNullOrEmpty(groupName);
		}
		private bool TryGetCurrentCurrentGroup(out CheatMenuGroup group) => s_GroupDict.TryGetValue(m_CurrentGroupName.Value, out group);
		private void ClearCurrentGroup() => SetCurrentGroup(string.Empty);
		private void SetCurrentGroup(string groupName)
		{
			m_CurrentGroupName.Value = groupName;
			m_ScrollPosition = Vector2.zero;
		}

		private void OpenInternal()
		{
			if (IsOpen)
			{
				return;
			}
			Log("Open");
			m_IsOpen = true;
			m_InputBlocker.gameObject.SetActive(true);
			m_TimeScaleHandle = TimeScaleManager.StartTimeEvent(0.0f);
			OnOpened();
			if (!TryGetCurrentCurrentGroup(out CheatMenuGroup currentGroup))
			{
				return;
			}
			if (!currentGroup.UpdateIsAvailable())
			{
				ClearCurrentGroup();
				return;
			}
			currentGroup.OnBecameActive();
		}

		private void CloseInternal()
		{
			if (!m_IsOpen)
			{
				return;
			}
			Log("Close");
			m_IsOpen = false;
			m_InputBlocker.gameObject.SetActive(false);
			if (m_TimeScaleHandle != TimeScaleManager.INVALID_HANDLE)
			{
				TimeScaleManager.EndTimeEvent(m_TimeScaleHandle);
			}
			OnClosed();
			if (TryGetCurrentCurrentGroup(out CheatMenuGroup currentGroup))
			{
				currentGroup.OnPostClose();
			}
		}

		private void CloseGroupInternal()
		{
			if (!m_IsOpen || !TryGetCurrentCurrentGroup(out CheatMenuGroup currentGroup))
			{
				return;
			}
			currentGroup.OnPostClose();
			ClearCurrentGroup();
		}

		private int ComparePageGroupsByPriority(CheatMenuGroup x, CheatMenuGroup y)
		{
			if (x.Priority == y.Priority)
			{
				return x.Name.CompareTo(y.Name);
			}
			return x.Priority.CompareTo(y.Priority);
		}

		private void Log(string message)
		{
			if (DebugOptions.LogCheatMenu.IsSet())
			{
				Debug.Log($"[CheatMenu] {message}");
			}
		}
#endif
	}
}
