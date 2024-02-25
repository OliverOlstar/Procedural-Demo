using UnityEditor;
using UnityEngine;

public class ActNodeDrawer
{
	private GUIStyle m_BaseStyle = null;
	private GUIStyle m_NodeLabelStyle = null;
	private GUIStyle m_RootNodeLabelStyle = null;
	private GUIStyle m_NodeIconStyle = null;
	private GUIStyle m_ReferencedChildLabelStyle = null;
	private GUIStyle m_ReferencedChildIconStyle = null;
	private GUIStyle m_SequenceIndicatorStyle = null;
	private GUIStyle m_SelectedNodeLabelStyle = null;
	private GUIStyle m_NodeFoldoutStyle = null;
	private GUIStyle m_SelectedNodeFoldoutStyle = null;


	private Color ROOT_HEADER_COLOR => new Color32(116, 116, 116, 255);
	private Color ROOT_TEXT_COLOR => new Color32(243, 245, 248, 255);
	private Color ROOT_HEADER_DIVIDER_COLOR => new Color32(66, 66, 66, 255);

	private Color NODE_FOLDOUT_WIDGET_COLOR => new Color32(155, 155, 155, 255); // Lighter than ROOT_HEADER_COLOR, darker than NODE_TEXT_COLOR
	private Color NODE_FOLDOUT_WIDGET_SELECTED_COLOR => NODE_FOLDOUT_WIDGET_COLOR;

	private Color SIDEBAR_COLOR => new Color32(45, 45, 45, 255);

	private Color HIGHLIGHTED_COLOR => new Color32(44, 93, 135, 255);
	private Color HIGHLIGHTED_SIDEBAR_COLOR => new Color32(52, 83, 125, 255);

	private Color REF_COLOR => new Color32(77, 77, 77, 255);
	private Color REF_SIDEBAR_COLOR => new Color32(66, 66, 66, 255);

	private Color NODE_TEXT_SELECTED_COLOR = new Color32(243, 245, 248, 255);
	private Color NODE_TEXT_COLOR = new Color32(194, 194, 194, 255);

	private Color REF_CHILD_COLOR = new Color32(124, 124, 124, 255);

	public void Initialize()
	{
		if (m_BaseStyle != null
			&& m_NodeLabelStyle != null
			&& m_RootNodeLabelStyle != null
			&& m_SelectedNodeLabelStyle != null
			&& m_NodeIconStyle != null
			&& m_NodeFoldoutStyle != null
			&& m_SelectedNodeFoldoutStyle != null)
		{
			return;
		}

		m_BaseStyle = new GUIStyle(GUIStyle.none);
		m_BaseStyle.alignment = TextAnchor.MiddleLeft;
		m_BaseStyle.fixedHeight = 16;
		m_BaseStyle.contentOffset = Vector2.zero;
		m_BaseStyle.richText = true;

		m_NodeLabelStyle = new GUIStyle(m_BaseStyle);
		m_NodeLabelStyle.normal.textColor = NODE_TEXT_COLOR;

		m_RootNodeLabelStyle = new GUIStyle(m_BaseStyle);
		m_RootNodeLabelStyle.normal.textColor = ROOT_TEXT_COLOR;

		m_NodeIconStyle = new GUIStyle(m_NodeLabelStyle);
		m_NodeIconStyle.alignment = TextAnchor.MiddleCenter;

		m_ReferencedChildLabelStyle = new GUIStyle(m_NodeLabelStyle);
		m_ReferencedChildLabelStyle.normal.textColor = REF_CHILD_COLOR;

		m_ReferencedChildIconStyle = new GUIStyle(m_NodeIconStyle);
		m_ReferencedChildIconStyle.normal.textColor = REF_CHILD_COLOR;

		m_SelectedNodeLabelStyle = new GUIStyle(m_NodeLabelStyle);
		m_SelectedNodeLabelStyle.normal.textColor = NODE_TEXT_SELECTED_COLOR;

		m_SequenceIndicatorStyle = new GUIStyle(m_NodeIconStyle);

		m_NodeFoldoutStyle = new GUIStyle(m_BaseStyle);
		m_NodeFoldoutStyle.fontSize = 10;
		m_NodeFoldoutStyle.normal.textColor = NODE_FOLDOUT_WIDGET_COLOR;

		m_SelectedNodeFoldoutStyle = new GUIStyle(m_NodeFoldoutStyle);
		m_SelectedNodeFoldoutStyle.normal.textColor = NODE_FOLDOUT_WIDGET_SELECTED_COLOR;
	}

	public bool OnGUI(
		ActTree tree,
		Act.Node node,
		bool isCollapsed,
		int selectedNodeID,
		bool isReferencedChild,
		out Act.Node referencedNode,
		out bool foldoutToggled,
		out Rect controlRect)
	{
		controlRect = EditorGUILayout.GetControlRect(false, m_BaseStyle.fixedHeight, m_BaseStyle);
		bool isSelected = !isReferencedChild && node.GetID() == selectedNodeID;
		bool isRootNode = node.GetID() == Act.Node.ROOT_ID;
		bool isLeaf = true;
		bool comesFromSelected = false;
		bool goesToSelected = false;
		foreach (Act.NodeLink link in tree.GetAllNodeLinks())
		{
			if (link.GetParentID() == node.GetID())
			{
				isLeaf = false;
				break;
			}
		}
		if (!isReferencedChild && selectedNodeID != Act.Node.INVALID_ID)
		{
			foreach (Act.NodeSequence sequence in tree.GetAllOpportunities())
			{
				if (sequence.GetFromID() == node.GetID() && sequence.GetToID() == selectedNodeID)
				{
					goesToSelected = true;
				}
				if (sequence.GetFromID() == selectedNodeID && sequence.GetToID() == node.GetID())
				{
					comesFromSelected = true;
				}
			}
		}

		if (isRootNode)
		{
			controlRect.position = controlRect.position + Vector2.down;
		}

		Rect highlightedRect = new Rect(controlRect);
		highlightedRect.xMin -= 4.0f;
		highlightedRect.xMax += 4.0f;
		if (isRootNode)
		{
			highlightedRect.yMax += 1.0f;
		}

		Rect sidebarRect = new Rect(highlightedRect);
		sidebarRect.xMax = sidebarRect.xMin + 21.0f;

		Rect sequenceIconRect = new Rect(sidebarRect);
		sequenceIconRect.position = sequenceIconRect.position + Vector2.right * 3.0f + Vector2.down;

		Rect headerRect = new Rect(highlightedRect);
		headerRect.yMax += 1.0f;

		Rect foldoutRect = new Rect(controlRect);
		foldoutRect.position = isCollapsed ? foldoutRect.position : foldoutRect.position + Vector2.up;
		foldoutRect.xMin = 14.0f * EditorGUI.indentLevel + 18.0f;
		foldoutRect.xMax = foldoutRect.xMin + m_NodeFoldoutStyle.fontSize;

		Rect nodeIconRect = new Rect(controlRect);
		nodeIconRect.xMin = foldoutRect.xMax;
		nodeIconRect.xMax = nodeIconRect.xMin + 19.0f;

		Rect labelRect = new Rect(controlRect);
		labelRect.xMin = nodeIconRect.xMax;

		// Highlighting.
		if (isRootNode)
		{
			EditorGUI.DrawRect(headerRect, ROOT_HEADER_DIVIDER_COLOR);
			EditorGUI.DrawRect(highlightedRect, ROOT_HEADER_COLOR);
		}
		if (isSelected)
		{
			EditorGUI.DrawRect(highlightedRect, HIGHLIGHTED_COLOR);
			if (!isRootNode)
			{
				EditorGUI.DrawRect(sidebarRect, HIGHLIGHTED_SIDEBAR_COLOR);
			}
		}
		else if (comesFromSelected || goesToSelected)
		{
			EditorGUI.DrawRect(highlightedRect, REF_COLOR);
			if (!isRootNode)
			{
				EditorGUI.DrawRect(sidebarRect, REF_SIDEBAR_COLOR);
			}
		}
		else if (!isRootNode)
		{
			EditorGUI.DrawRect(sidebarRect, SIDEBAR_COLOR);
		}

		// Sequence indication.
		if (isSelected && goesToSelected)
		{
			m_SequenceIndicatorStyle.fontSize = 14;
			GUI.Label(sequenceIconRect, "\u21BB", m_SequenceIndicatorStyle);
		}
		else if (comesFromSelected && goesToSelected)
		{
			m_SequenceIndicatorStyle.fontSize = 12;
			GUI.Label(sequenceIconRect, "\u2194", m_SequenceIndicatorStyle);
		}
		else if (comesFromSelected)
		{
			m_SequenceIndicatorStyle.fontSize = 12;
			GUI.Label(sequenceIconRect, "\u2192", m_SequenceIndicatorStyle);
		}
		else if (goesToSelected)
		{
			m_SequenceIndicatorStyle.fontSize = 12;
			GUI.Label(sequenceIconRect, "\u2190", m_SequenceIndicatorStyle);
		}

		// Foldout button.
		if (!isLeaf && 
			!isReferencedChild && 
			GUI.Button(
				foldoutRect,
				isCollapsed ? "\u25BA" : "\u25BC",
				isSelected ? m_SelectedNodeFoldoutStyle : m_NodeFoldoutStyle))
		{
			foldoutToggled = true;
			isCollapsed = !isCollapsed;
		}
		else
		{
			foldoutToggled = false;
		}

		GUIStyle labelStyle = null;
		GUIStyle iconStyle = null;
		if (isReferencedChild)
		{
			labelStyle = m_ReferencedChildLabelStyle;
			iconStyle = m_ReferencedChildIconStyle;
		}
		else if (isSelected)
		{
			labelStyle = m_SelectedNodeLabelStyle;
			iconStyle = m_NodeIconStyle;
		}
		else if (isRootNode)
		{
			labelStyle = m_RootNodeLabelStyle;
			iconStyle = m_NodeIconStyle;
		}
		else
		{
			labelStyle = m_NodeLabelStyle;
			iconStyle = m_NodeIconStyle;
		}

		// Node icon.
		GetNodeInformation(tree, node, out bool isMaster, out bool hasTrack, out referencedNode, out string icon, out int iconSize);
		iconStyle.fontSize = iconSize;
		GUI.Label(nodeIconRect, icon, iconStyle);

		// Node label.
		labelStyle.fontStyle = isRootNode ? FontStyle.Bold : FontStyle.Normal;
		GUI.Label(labelRect, isRootNode ? tree.name : node.GetName(), labelStyle);

		return isCollapsed;
	}

	public static void GetNodeInformation(
		ActTree tree, 
		Act.Node node, 
		out bool isMaster, 
		out bool hasTrack, 
		out Act.Node referencedNode, 
		out string unicode,
		out int iconSize)
	{
		isMaster = false;
		hasTrack = false;
		foreach (ActTrack track in tree.GetTracks())
		{
			if (track == null) // Apparently this can happen?
			{
				continue;
			}
			if (track.GetNodeID() != node.GetID() && track.GetNodeID() != node.GetPointerID())
			{
				continue;
			}
			hasTrack = true;
			if (track.IsMaster())
			{
				isMaster = true;
				break;
			}
		}
		referencedNode = null;
		if (node.GetPointerID() != Act.Node.INVALID_ID)
		{
			foreach (Act.Node otherNode in tree.GetAllNodes())
			{
				if (node.GetPointerID() == otherNode.GetID())
				{
					referencedNode = otherNode;
					break;
				}
			}
		}
		if (referencedNode != null)
		{
			iconSize = 18;
			unicode = "\u260A";
		}
		else if (isMaster)
		{
			iconSize = GUI.skin.label.fontSize;
			unicode = "\u25CF";
		}
		else if (hasTrack)
		{
			iconSize = 22;
			unicode = "\u25C9";
		}
		else
		{
			iconSize = GUI.skin.label.fontSize;
			unicode = "\u25CB";
		}
	}
}
