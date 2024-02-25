using UnityEditor;
using UnityEngine;

namespace Act2
{
	public class NodeDrawer
	{
		public const int SIDEBAR_WIDTH = 17;
		public const string TRANSITION_ARROW_LEFT = "\u2190";
		public const string TRANSITION_ARROW_RIGHT = "\u2192";
		public const string TRANSITION_ARROW_BOTH = "\u2194";
		public const string TRANSITION_ARROW_LOOP = "\u21BB";

		public enum ParentsProperties
		{
			None = 0,
			Reference = 1 << 0,
			False = 1 << 1,
		}

		public class Style
		{
			public static readonly Color FOLDOUT_COLOR = new Color32(104, 104, 104, 255);

			public Color? SidebarColor = null;
			public Color? BackgroundColor = null;
			public float? TextBrightness = null;
			public bool AllowTextColor = true;

			public Color GetTextColor(Color? requestColor = null)
			{
				Color color = AllowTextColor && requestColor.HasValue ? 
					requestColor.Value :
					GUI.skin.label.normal.textColor;
				if (TextBrightness.HasValue)
				{
					color *= TextBrightness.Value;
				}
				return color;
			}

			public Color GetFoldoutColor()
			{
				Color color = FOLDOUT_COLOR;
				if (TextBrightness.HasValue)
				{
					color *= TextBrightness.Value;
				}
				return color;
			}
		}

		public static class Styles
		{
			public static readonly Style Normal = new Style
			{
				SidebarColor = new Color32(45, 45, 45, 255),
				BackgroundColor = null,
				TextBrightness = null,
				AllowTextColor = true,
			};

			public static readonly Style Selected = new Style
			{
				SidebarColor = new Color32(52, 83, 125, 255),
				BackgroundColor = new Color32(37, 80, 114, 255),
				TextBrightness = 1.25f,
				AllowTextColor = true,
			};

			public static readonly Style Copied = new Style
			{
				SidebarColor = new Color32(109, 137, 160, 255),
				BackgroundColor = new Color32(133, 156, 175, 255),
				TextBrightness = 1.25f,
				AllowTextColor = false,
			};

			public static readonly Style Reference = new Style
			{
				SidebarColor = Normal.SidebarColor,
				BackgroundColor = null,
				TextBrightness = 0.75f,
				AllowTextColor = true,
			};

			public static readonly Style Root = new Style
			{
				SidebarColor = null,
				BackgroundColor = new Color32(104, 104, 104, 255),
				TextBrightness = 1.4f,
				AllowTextColor = false,
			};

			public static readonly Style Transition = new Style
			{
				SidebarColor = new Color32(66, 66, 66, 255),
				BackgroundColor = new Color32(77, 77, 77, 255),
				TextBrightness = null,
				AllowTextColor = true,
			};
		}

		private static GUIStyle m_BaseStyle = null;
		private static GUIStyle m_NodeLabelStyle = null;
		private static GUIStyle m_SequenceIndicatorStyle = null;
		private static GUIStyle m_NodeFoldoutStyle = null;

		private static void Initialize()
		{
			if (m_BaseStyle != null)
			{
				return;
			}

			m_BaseStyle = new GUIStyle(GUIStyle.none);
			m_BaseStyle.alignment = TextAnchor.MiddleLeft;
			m_BaseStyle.fixedHeight = 16;
			m_BaseStyle.contentOffset = Vector2.zero;
			m_BaseStyle.richText = true;

			m_NodeLabelStyle = new GUIStyle(m_BaseStyle);

			m_SequenceIndicatorStyle = new GUIStyle(m_NodeLabelStyle);
			m_SequenceIndicatorStyle.alignment = TextAnchor.MiddleCenter;

			m_NodeFoldoutStyle = new GUIStyle(m_BaseStyle);
			m_NodeFoldoutStyle.fontSize = 10;
		}

		public bool OnGUILayout(
			ActTreeEditorWindow2 window,
			TreeDrawer drawer,
			Node node,
			ParentsProperties parentNodes,
			out Node.Properties nodeProperties,
			out Rect nodeRect,
			out Rect controlRect)
		{
			Initialize();
			controlRect = EditorGUILayout.GetControlRect(false, m_BaseStyle.fixedHeight, m_BaseStyle);
			return OnGUI(
				controlRect,
				window,
				drawer,
				node,
				parentNodes,
				out nodeProperties,
				out nodeRect);
		}

		public bool OnGUI(
			Rect controlRect,
			ActTreeEditorWindow2 window,
			TreeDrawer drawer,
			Node node,
			ParentsProperties parentsProperties,
			out Node.Properties nodeProperties,
			out Rect nodeRect)
		{
			Initialize();
			ActTree2 tree = window.Tree;
			int selectedNodeID = window.SelectedNodeID;
			nodeProperties = Node.GetProperties(tree, node);
			bool isCollapsed = TreeDrawer.IsNodeCollapsed(tree, node);
			bool isReferencedChild = parentsProperties.HasFlag(ParentsProperties.Reference);
			bool isSelected = !isReferencedChild && node.GetID() == selectedNodeID;
			bool isCopied = !isReferencedChild && NodeClipboard.IsCopied(node, tree);
			bool comesFromSelected = false;
			bool goesToSelected = false;
			if (!drawer.IsDebugging) // Don't draw transition if we're debugging, it's too noisy
			{
				if (!isReferencedChild && selectedNodeID != Node.INVALID_ID)
				{
					foreach (NodeTransition transition in node.Transitions)
					{
						if (transition.GetToID() == selectedNodeID)
						{
							goesToSelected = true;
						}
					}
					Node selectedNode = tree.GetNode(selectedNodeID);
					foreach (NodeTransition transition in selectedNode.Transitions)
					{
						if (transition.GetToID() == node.GetID())
						{
							comesFromSelected = true;
						}
					}
				}
			}

			if (nodeProperties.IsRoot)
			{
				controlRect.position += Vector2.down;
			}

			Rect sidebarRect = new Rect(controlRect);
			sidebarRect.xMax = sidebarRect.xMin + SIDEBAR_WIDTH;

			Rect sequenceIconRect = new Rect(sidebarRect);
			sequenceIconRect.position += 3.0f * Vector2.right + Vector2.down;

			Rect foldoutRect = new Rect(controlRect);
			foldoutRect.position = isCollapsed ? foldoutRect.position : foldoutRect.position + Vector2.up;
			foldoutRect.xMin = 14.0f * EditorGUI.indentLevel + 18.0f;
			foldoutRect.xMax = foldoutRect.xMin + m_NodeFoldoutStyle.fontSize;

			Style nodeStyle = Styles.Normal;
			if (isCopied)
			{
				nodeStyle = Styles.Copied;
			}
			else if (isSelected)
			{
				nodeStyle = Styles.Selected;
			}
			else if (nodeProperties.IsRoot)
			{
				nodeStyle = Styles.Root;
			}
			else if (comesFromSelected || goesToSelected)
			{
				nodeStyle = Styles.Transition;
			}
			else if (isReferencedChild)
			{
				nodeStyle = Styles.Reference;
			}

			// Background & sidebar
			if (nodeStyle.BackgroundColor.HasValue)
			{
				EditorGUI.DrawRect(controlRect, nodeStyle.BackgroundColor.Value);
			}
			if (nodeStyle.SidebarColor.HasValue)
			{
				EditorGUI.DrawRect(sidebarRect, nodeStyle.SidebarColor.Value);
			}

			// Sequence indication
			m_SequenceIndicatorStyle.normal.textColor = nodeStyle.GetTextColor();
			if (isSelected && goesToSelected)
			{
				m_SequenceIndicatorStyle.fontSize = 14;
				GUI.Label(sequenceIconRect, TRANSITION_ARROW_LOOP, m_SequenceIndicatorStyle);
			}
			else if (comesFromSelected && goesToSelected)
			{
				m_SequenceIndicatorStyle.fontSize = 12;
				GUI.Label(sequenceIconRect, TRANSITION_ARROW_BOTH, m_SequenceIndicatorStyle);
			}
			else if (comesFromSelected)
			{
				m_SequenceIndicatorStyle.fontSize = 12;
				GUI.Label(sequenceIconRect, TRANSITION_ARROW_RIGHT, m_SequenceIndicatorStyle);
			}
			else if (goesToSelected)
			{
				m_SequenceIndicatorStyle.fontSize = 12;
				GUI.Label(sequenceIconRect, TRANSITION_ARROW_LEFT, m_SequenceIndicatorStyle);
			}

			// Foldout button
			m_NodeFoldoutStyle.normal.textColor = nodeStyle.GetFoldoutColor();
			bool hasFoldout = !nodeProperties.IsLeaf; 
			if (hasFoldout && GUI.Button(foldoutRect, isCollapsed ? "\u25BA" : "\u25BC", m_NodeFoldoutStyle))
			{
				isCollapsed = !isCollapsed;
				TreeDrawer.SetNodeCollapsed(tree, node, isCollapsed, Event.current.alt);
			}
            
			Rect labelRect = new Rect(controlRect);
			labelRect.xMin = foldoutRect.xMax;

			DrawNodeLabel(window, labelRect, m_NodeLabelStyle, nodeProperties, parentsProperties, nodeStyle);

			nodeRect = new Rect(controlRect);
			nodeRect.xMin = hasFoldout ? foldoutRect.xMin : foldoutRect.xMax;
			return isCollapsed;
		}

		public static void DrawNodeLabel(
			ActTreeEditorWindow2 window,
			Rect position,
			GUIStyle labelStyle,
			Node.Properties nodeProperties,
			ParentsProperties parentsProperties = ParentsProperties.None,
			Style nodeDrawerStyle = null)
		{
			AttachNodeIcon(position, out Rect labelRect, labelStyle, nodeProperties, parentsProperties, nodeDrawerStyle);
			if (window.RenameNodeID == nodeProperties.ID)
			{
				string ogName = nodeProperties.Name;
				GUI.SetNextControlName("NodeRenameTextField");
				string newName = GUI.TextField(labelRect, nodeProperties.Name);
				GUI.FocusControl("NodeRenameTextField");
				if (newName != ogName)
				{
					window.RenameNode(nodeProperties.ID, newName);
				}
			}
			else
			{
				labelStyle.fontStyle = nodeProperties.IsRoot ? FontStyle.Bold : FontStyle.Normal;
				GUI.Label(labelRect, nodeProperties.IsRoot ? nodeProperties.TreeName : nodeProperties.Name, labelStyle);
			}
		}

		public static readonly float NODE_ICON_WIDTH = 19.0f;
		public static readonly Color NODE_BLUE = new Color32(127, 214, 252, 255);
		public static readonly Color LABEL_BLUE = new Color32(125, 173, 243, 255);
		public static readonly Color NODE_GREEN = new Color32(141, 210, 138, 255);
		public static readonly Color LABEL_GREEN = new Color32(78, 176, 110, 255);
		public static readonly Color NODE_GREY = new Color32(0, 5, 10, 255);
		public static readonly Color LABEL_GREY = new Color32(0, 5, 10, 255);
		public static readonly Color NODE_WHITE = new Color32(200, 200, 200, 255);
		public static readonly Color LABEL_WHITE = new Color32(200, 200, 200, 255);

		public static void AttachNodeIcon(
			Rect controlRect,
			out Rect adjustedRect,
			GUIStyle labelStyle,
			Node.Properties properties,
			ParentsProperties parentsProperties = ParentsProperties.None,
			Style nodeDrawerStyle = null)
		{
			Rect nodeIconRect = new Rect(controlRect);

			float iconWidth = NODE_ICON_WIDTH;
			if (labelStyle.fontSize != 0 && labelStyle.fontSize != GUI.skin.label.fontSize)
			{
				float percent = (float)labelStyle.fontSize / GUI.skin.label.fontSize;
				iconWidth = Mathf.RoundToInt(percent * iconWidth);
			}

			switch (labelStyle.alignment)
			{
				case TextAnchor.UpperRight:
				case TextAnchor.MiddleRight:
				case TextAnchor.LowerRight:
					nodeIconRect.xMin = nodeIconRect.xMax - iconWidth;
					break;
				default:
					nodeIconRect.xMax = controlRect.xMin + iconWidth;
					break;
			}

			Color nodeColor;
			Color textColor;
			if (properties.HasFalseCondtion || parentsProperties.HasFlag(ParentsProperties.False))
			{
				nodeColor = NODE_GREY;
				textColor = LABEL_GREY;
			}
			else
			{
				switch (properties.Availibility)
				{
					case Node.Available.TransitionsOnly:
						nodeColor = NODE_WHITE;
						textColor = LABEL_WHITE;
						break;
					case Node.Available.EventTransitionsOnly:
						nodeColor = NODE_GREEN;
						textColor = LABEL_GREEN;
						break;
					default:
						nodeColor = NODE_BLUE;
						textColor = LABEL_BLUE;
						break;
				}
			}

			if (properties.IsRoot)
			{
				GUI.DrawTexture(nodeIconRect, EditorUtil.ActTreeIcon, ScaleMode.ScaleToFit);
			}
			else
			{
				GetNodeIcon(properties, labelStyle, out string icon, out int iconSize, out float yOffset);
				nodeIconRect.y += yOffset;

				// Node icon.
				int originalFontSize = labelStyle.fontSize;
				TextAnchor originalAlignment = labelStyle.alignment;
				FontStyle originalFontStyle = labelStyle.fontStyle;

				labelStyle.fontSize = iconSize;
				labelStyle.alignment = TextAnchor.MiddleCenter;
				labelStyle.fontStyle = FontStyle.Normal; // Some Unicode characters look weird bolded

				if (nodeDrawerStyle != null)
				{
					labelStyle.normal.textColor = nodeDrawerStyle.GetTextColor(nodeColor);
				}

				GUI.Label(nodeIconRect, icon, labelStyle);

				labelStyle.fontSize = originalFontSize;
				labelStyle.alignment = originalAlignment;
				labelStyle.fontStyle = originalFontStyle;
			}

			// Fill out things we need to return adjustedRect and possibly labelStyle color
			switch (labelStyle.alignment)
			{
				case TextAnchor.UpperRight:
				case TextAnchor.MiddleRight:
				case TextAnchor.LowerRight:
					adjustedRect = new Rect(controlRect);
					adjustedRect.xMax = nodeIconRect.xMin;
					break;
				default:
					adjustedRect = new Rect(controlRect);
					adjustedRect.xMin = nodeIconRect.xMax;
					break;
			}
			if (nodeDrawerStyle != null) // If a node drawer style was specified then modify the callers label to add color if the node style requires
			{
				labelStyle.normal.textColor = nodeDrawerStyle.GetTextColor(textColor);
			}
		}

		public static void AttachNodeIcon(
			Rect controlRect,
			out Rect adjustedRect,
			GUIStyle style,
			ActTree2 tree,
			Node node,
			NodeDrawer.Style nodeStyle = null)
		{
			Node.Properties props = Node.GetProperties(tree, node);
			AttachNodeIcon(controlRect, out adjustedRect, style, props, ParentsProperties.None, nodeStyle);
		}

		public static readonly string EVENT_ICON = "\u25C6";
		public static readonly string POLL_ICON = "\u25CF";

		private static void GetNodeIcon(
			Node.Properties properties,
			GUIStyle style,
			out string unicode,
			out int iconSize,
			out float yOffset)
		{
			yOffset = 0.0f;
			if (properties.ReferencedNode != null)
			{
				iconSize = 18;
				unicode = "\u260D";
			}
			else if (properties.HasFalseCondtion)
			{
				iconSize = 7;
				unicode = "\u24bb";
			}
			else if (properties.HasMajorTrack)
			{
				iconSize = GUI.skin.label.fontSize;
				if (properties.IsEvent)
				{
					unicode = EVENT_ICON;
				}
				else if (properties.HasConditions)
				{
					unicode = POLL_ICON;
				}
				else
				{
					yOffset = -3.0f;
					iconSize = 20;
					unicode = "\u25a0";
				}
			}
			else if (properties.HasTrack)
			{
				iconSize = GUI.skin.label.fontSize;
				if (properties.IsEvent)
				{
					//iconSize = 26;
					yOffset -= 1;
					iconSize = 13;
					unicode = "\u25C8";
				}
				else if (properties.HasConditions)
				{
					iconSize = 23;
					unicode = "\u25C9";
				}
				else
				{
					//iconSize = 24;
					yOffset -= 1;
					unicode = "\u25a3";
				}
			}
			else if (properties.IsLeaf)
			{
				iconSize = GUI.skin.label.fontSize;
				unicode = "\u2715";
			}
			else
			{
				iconSize = GUI.skin.label.fontSize;
				if (properties.IsEvent)
				{
					unicode = "\u25C7";
				}
				else if (properties.HasConditions)
				{
					unicode = "\u25CB";
				}
				else
				{
					yOffset = -2.0f;
					iconSize = 20;
					unicode = "\u25a1";
				}
			}
			if (properties.NodeType == NodeType.Minor)
			{
				iconSize = Mathf.CeilToInt(0.65f * iconSize);
			}
			else
			{
				iconSize += 2;
			}
			if (style.fontSize != 0 && style.fontSize != GUI.skin.label.fontSize)
			{
				float percent = (float)style.fontSize / GUI.skin.label.fontSize;
				iconSize = Mathf.RoundToInt(percent * iconSize);
			}
		}
	}
}
