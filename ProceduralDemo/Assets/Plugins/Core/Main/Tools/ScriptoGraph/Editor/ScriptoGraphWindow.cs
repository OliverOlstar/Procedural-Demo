
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphEditor;

public class ScriptoGraphWindow : EditorWindow
{
	private static bool IsObjectValid(Object obj)
	{
		switch (obj)
		{
			case ScriptableObject so:
				return !so.GetType().IsDefined(typeof(ScriptoGraph.HideAttribute), true);
			case GameObject go:
				return go.TryGetComponent(out ScriptoGraph.IScriptoGraphBehaviour behaviour);
			default:
				return false;
		}
	}

	private static bool TryOpenWindow(Object obj)
	{
		if (!IsObjectValid(obj))
		{
			return false;
		}
		ScriptoGraphWindow window = GetWindow<ScriptoGraphWindow>("Script-O-Graph");
		window.Initialize(obj);
		window.Show();
		return true;
	}

	[MenuItem("Assets/Open in Script-O-Graph", true, 51)]
	private static bool ValidateOpen() => IsObjectValid(Selection.activeObject);

	[MenuItem("Assets/Open in Script-O-Graph", false, 51)]
	private static bool Open()
	{
		return TryOpenWindow(Selection.activeObject);
	}

	[UnityEditor.Callbacks.OnOpenAsset(100)] // Smaller numbers seem to be higher priority, pick a low priority so we don't override other custom functionality
	public static bool OnOpenAsset(int instanceID, int line)
	{
		Object obj = EditorUtility.InstanceIDToObject(instanceID);
		return TryOpenWindow(obj);
	}

	private static readonly float m_Width = 424.0f;
	private static readonly float m_Height = 24.0f;
	private static readonly float m_HorizSpace = 32.0f;

	private static readonly float EDGE = 2.0f;
	private static readonly float BORDER = 4.0f;
	private static readonly float INDENT = 15.0f;
	private static readonly float TAB_HEIGHT = 12.0f;
	private static readonly float m_VertSpace = 8.0f + BORDER + TAB_HEIGHT;

	private static readonly Color HEADER_COLOR = new Color32(85, 85, 85, 255);
	private static readonly Color HEADER_SELECTED_COLOR = new Color32(85, 85, 85, 255);
	private static readonly Color PANEL_HIGHLIGHT_COLOR = new Color32(68, 68, 68, 255);
	private static readonly Color PANEL_COLOR = new Color32(50, 50, 50, 255);
	private static readonly Color BG_COLOR = new Color32(25, 25, 25, 255);
	private static readonly Color TYPE_TEXT_COLOR = new Color32(164, 164, 164, 255);

	private HashSet<Object> m_Objects = new();
	private Vector2 m_Offset = Vector2.zero;
	private Object m_Obj = null;
	private EditorWindowInput m_Input = null;

	ScriptoGraphWindow()
	{
		m_Input = new EditorWindowInput();
		m_Input.SubscribeMouseDrag(EditorWindowInput.MOUSE_MIDDLE, OnMiddleMouseDrag);
		m_Input.SubscribeKeyDown(KeyCode.LeftAlt, OnAltDown);
		m_Input.SubscribeKeyUp(KeyCode.LeftAlt, OnAltUp);
	}

	public void Initialize(Object obj)
	{
		if (obj is GameObject go && go.TryGetComponent(out ScriptoGraph.IScriptoGraphBehaviour behaviour))
		{
			obj = behaviour as Object;
		}
		m_Obj = obj;
		m_Offset = Vector2.zero;
	}

	private void OnInspectorUpdate()
	{
		Repaint();
	}

	bool m_AltDown = false;
	private void OnAltDown(Event guiEvent)
	{
		m_AltDown = true;
	}
	private void OnAltUp(Event guiEvent)
	{
		m_AltDown = false;
	}

	private void OnMiddleMouseDrag(Event guiEvent)
	{
		m_Offset += guiEvent.delta;
	}

	private void OnGUI()
	{
		Rect bgRect = position;
		bgRect.position = Vector2.zero;
		EditorGUI.DrawRect(bgRect, BG_COLOR);
		if (m_Obj == null)
		{
			return;
		}
		//GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, 0.5f * Vector3.one);
		m_Objects.Clear();
		Build(m_Obj, string.Empty, m_Offset.x + BORDER, m_Offset.y + m_VertSpace, null);
		//GUI.matrix = Matrix4x4.identity;
		m_Input.Update(Event.current);
	}

	private string ExpandedKey(string id) { return "ScriptoExp " + m_Obj.name + " " + id; }

	private void SetMinimized(string id, bool minimized)
	{
		if (minimized)
		{
			EditorPrefs.DeleteKey(ExpandedKey(id));
		}
		else
		{
			EditorPrefs.SetBool(ExpandedKey(id), true);
		}
	}

	private bool GetMinimized(string id)
	{
		return !EditorPrefs.GetBool(ExpandedKey(id), false);
	}

	private void FindReferences(
		SerializedObject serObj, 
		SerializedProperty property, 
		Vector2 position, 
		string baseID, 
		List<System.Tuple<Vector2, Object, string>> refList)
	{
		//Debug.LogWarning("+ FindReferences " + property.propertyPath + " " + property.depth + " " +
		//	property.propertyType + (property.isArray ? " " + property.arrayElementType + "[]" : ""));
		switch (property.propertyType)
		{
			case SerializedPropertyType.Generic:
				if (property.isArray)
				{
					switch (property.arrayElementType)
					{
						case "bool":
						case "char":
						case "int":
						case "float":
						case "string":
							break;
						default:
							for (int i = 0; i < property.arraySize; i++)
							{
								//Debug.Log("+ Array " + property.propertyPath + " " + i);
								SerializedProperty prop = property.GetArrayElementAtIndex(i);
								FindReferences(serObj, prop, position, baseID, refList);
								//Debug.Log("+ Array " + property.propertyPath + " " + i);
							}
							break;
					}
				}
				else
				{
					SerializedProperty iterator = serObj.FindProperty(property.propertyPath);
					iterator.Next(true);
					int iteratorDepth = iterator.depth;
					do
					{
						//Debug.Log("+ Iterate " + property.propertyPath + " " + property.depth);
						FindReferences(serObj, iterator, position, baseID, refList);
						//Debug.Log("- Iterate " + property.propertyPath + " " + property.depth);
					}
					while (iterator.Next(false) && iterator.depth == iteratorDepth);
				}
				break;
			case SerializedPropertyType.ObjectReference:
				if (property.objectReferenceValue != null)
				{
					System.Type type = property.objectReferenceValue.GetType();
					bool defined = type.IsDefined(typeof(ScriptoGraph.HideAttribute), true);
					if (!defined)
					{
						//Debug.LogError("!!ADD!! " + property.propertyPath + " " + property.objectReferenceValue.name);
						string childID = baseID + property.propertyPath;
						refList.Add(new System.Tuple<Vector2, Object, string>(position, property.objectReferenceValue, childID));
					}
				}
				break;
		}
		//Debug.LogWarning("- FindReferences " + property.propertyPath + " " + property.depth);
	}

	private float Build(Object obj, string id, float x, float y, bool? recursiveMinimize)
	{
		if (obj is GameObject go && go.TryGetComponent(out ScriptoGraph.IScriptoGraphBehaviour sgb))
		{
			obj = sgb as MonoBehaviour;
		}

		id += obj.name;
		bool loop = m_Objects.Contains(obj);
		if (!loop)
		{
			m_Objects.Add(obj);
		}
		SerializedObject serObj = new(obj);
		SerializedProperty property = serObj.GetIterator();
		property.NextVisible(true); // Skip the first visible property as this is always the script reference
		MonoScript mono = property.propertyType == SerializedPropertyType.ObjectReference ? property.objectReferenceValue as MonoScript : null;
		int propDepth = property.depth;

		bool hasProperites = 
			!(obj is GameObject) &&
			property.NextVisible(false) && 
			!obj.GetType().IsDefined(typeof(ScriptoGraph.HideChildrenAttribute), true);

		if (recursiveMinimize.HasValue)
		{
			SetMinimized(id, recursiveMinimize.Value);
		}
		bool min = GetMinimized(id);
		GUIContent headerContent = new(obj.name + (loop ? " (Loop)" : ""));
		GUIStyle headerStyle = new(GUI.skin.label);
		//headerStyle.fontStyle = FontStyle.Bold;
		float minHeaderWidth = headerStyle.CalcSize(headerContent).x + INDENT + UberPickerGUI.POINTER_BUTTON_WIDTH;
		Rect headerPos = new(x, y, min ? minHeaderWidth : m_Width, m_Height);
		bool selected = false;
		foreach (Object selObj in Selection.objects)
		{
			if (selObj == obj)
			{
				selected = true;
				break;
			}
		}

		GUIContent tabContent = new(obj.GetType().Name);
		GUIStyle tabStyle = new(GUI.skin.label);
		tabStyle.fontSize = 10;
		tabStyle.normal.textColor = TYPE_TEXT_COLOR;
		tabStyle.alignment = TextAnchor.MiddleCenter;
		Rect tabPos = new(headerPos.x, headerPos.y - TAB_HEIGHT, tabStyle.CalcSize(tabContent).x + 4.0f, TAB_HEIGHT);
		EditorGUI.DrawRect(tabPos, selected ? HEADER_SELECTED_COLOR : HEADER_COLOR);

		if (GUI.Button(tabPos, tabContent, tabStyle) && mono != null)
		{
			AssetDatabase.OpenAsset(mono);
		}

		EditorGUI.DrawRect(headerPos, selected ? HEADER_SELECTED_COLOR : HEADER_COLOR);
		Rect headerLabelPos = headerPos;
		if (UberPickerGUI.AttachAssetSelectButton(ref headerLabelPos))
		{
			Selection.activeObject = obj;
		}
		headerLabelPos.y -= 2.0f; // Align text with pointer button
		if (hasProperites)
		{
			headerLabelPos.x += EDGE;
			headerLabelPos.width -= EDGE;
			bool foldout = EditorGUI.Foldout(headerLabelPos, !min, headerContent, true);
			if (foldout == min)
			{
				min = !min;
				SetMinimized(id, min);
				if (m_AltDown)
				{
					recursiveMinimize = min;
				}
			}
		}
		else
		{
			headerLabelPos.x += INDENT;
			headerLabelPos.width -= INDENT;
			EditorGUI.LabelField(headerLabelPos, headerContent);
		}

		Rect pos = headerPos;
		pos.y += pos.height;

		if (hasProperites && !min)
		{
			Rect bgRect = pos;
			bgRect.height = 0.0f;
			SerializedProperty prop = serObj.GetIterator();
			prop.NextVisible(true); // Skip the first visible property as this is always the script reference
			while (prop.NextVisible(false) && prop.depth == propDepth)
			{
				bgRect.height += EDGE + EditorGUI.GetPropertyHeight(prop, true);
			}
			bgRect.height += BORDER;
			EditorGUI.DrawRect(bgRect, PANEL_COLOR);
		}

		List<System.Tuple<Vector2, Object, string>> refList = new();
		if (hasProperites)
		{
			do
			{
				bool isRef = false;
				if (!loop)
				{
					Vector2 p = min ? headerPos.position : pos.position;
					int count = refList.Count;
					FindReferences(serObj, property, p, id, refList);
					isRef = isRef ? true : refList.Count > count;
				}
				if (!min)
				{
					pos.y += EDGE;
					pos.height = EditorGUI.GetPropertyHeight(property, true);
					if (isRef)
					{
						Rect highlightRect = pos;
						highlightRect.x += INDENT - EDGE;
						highlightRect.width -= INDENT - EDGE;
						EditorGUI.DrawRect(highlightRect, PANEL_HIGHLIGHT_COLOR);

					}
					Rect window = new(0.0f, 0.0f, this.position.width, this.position.height);
					if (window.Overlaps(pos)) // For performance only draw controls that are visible in the window
					{
						Rect indentPos = pos;
						indentPos.x += INDENT;
						indentPos.width -= INDENT + BORDER;
						EditorGUI.PropertyField(indentPos, property, true);
					}
					pos.y += pos.height;
				}
			}
			while (property.NextVisible(false) && property.depth == propDepth);
		}

		x += pos.width + m_HorizSpace;
		float childY = y;
		foreach (System.Tuple<Vector2, Object, string> refItem in refList)
		{
			childY = Mathf.Max(childY, refItem.Item1.y);
			float h = Build(refItem.Item2, refItem.Item3, x, childY, recursiveMinimize);

			Vector2 p1 = refItem.Item1 + pos.width * Vector2.right + 0.5f * EditorGUIUtility.singleLineHeight * Vector2.up;
			Vector2 p2 = new(p1.x + m_HorizSpace, childY + 0.5f * EditorGUIUtility.singleLineHeight);
			Lines.DrawLine(p1, p2, HEADER_COLOR);

			childY += h; // Mathf.Max(childHeight + h, (refItem.Item1.y - y));
		}

		if (serObj.hasModifiedProperties)
		{
			serObj.ApplyModifiedProperties();
		}

		return Mathf.Max((pos.y - y) + m_VertSpace, childY - y);
	}
}
