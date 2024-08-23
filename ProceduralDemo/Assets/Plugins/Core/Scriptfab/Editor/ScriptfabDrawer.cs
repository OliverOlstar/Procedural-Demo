using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Core
{
	[CustomPropertyDrawer(typeof(ScriptfabBase), true)]
	public class ScriptfabDrawer : PropertyDrawer
	{
		public static readonly GUIContent[] TOOLBAR_CONTENT =
		{
			new("L", "Convert to local value"),
			new("G", "Convert to global Scriptable Object")
		};

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty local = property.FindPropertyRelative("m_LocalValue");
			SerializedProperty fab = property.FindPropertyRelative("m_FabValue");
			SerializedProperty useLocal = property.FindPropertyRelative("m_UseLocal");

			Rect button = position;
			button.width = 33;
			position.width -= button.width;
			button.x += position.width;

			button.height = EditorGUIUtility.singleLineHeight;

			int fontSize = GUI.skin.button.fontSize;
			GUI.skin.GetStyle("ButtonMid").fontSize = 10;
			GUI.skin.GetStyle("ButtonMid").fontStyle = FontStyle.Bold;
			GUI.skin.GetStyle("ButtonRight").fontSize = 10;
			GUI.skin.GetStyle("ButtonRight").fontStyle = FontStyle.Bold;
			GUI.skin.GetStyle("ButtonLeft").fontSize = 10;
			GUI.skin.GetStyle("ButtonLeft").fontStyle = FontStyle.Bold;
			int selected = useLocal.boolValue ? 0 : 1;
			int newSelected = GUI.Toolbar(
				button,
				selected,
				TOOLBAR_CONTENT,
				GUI.skin.GetStyle("Button"));
			GUI.skin.GetStyle("ButtonMid").fontSize = fontSize;
			GUI.skin.GetStyle("ButtonMid").fontStyle = FontStyle.Normal;
			GUI.skin.GetStyle("ButtonRight").fontSize = fontSize;
			GUI.skin.GetStyle("ButtonRight").fontStyle = FontStyle.Normal;
			GUI.skin.GetStyle("ButtonLeft").fontSize = fontSize;
			GUI.skin.GetStyle("ButtonLeft").fontStyle = FontStyle.Normal;

			if (newSelected != selected)
			{
				if (useLocal.boolValue)
				{
					if (local.managedReferenceValue != null)
					{
						Event e = Event.current;
						GenericMenu menu = new();
						Vector2 p = GUIUtility.GUIToScreenPoint(e.mousePosition);
						Context context = new()
						{
							Position = p,
							LocalObject = local.managedReferenceValue,
							Property = property,
						};
						menu.AddItem(new GUIContent("Choose existing"), false, OnChooseExisting, context);
						menu.AddItem(new GUIContent("Create from local"), false, OnCreateFromLocal, context);
						menu.ShowAsContext();
					}
				}
				else
				{
					if (fab.objectReferenceValue != null)
					{
						SerializedObject so = new(fab.objectReferenceValue);
						SerializedProperty value = so.FindProperty("m_Type");
						if (value.managedReferenceValue != null)
						{
							local.managedReferenceValue = value.managedReferenceValue;
						}
					}
					TryFixLocalNullValue(local);
					useLocal.boolValue = true;
				}
			}

			if (useLocal.boolValue)
			{
				fab.objectReferenceValue = null;
				TryFixLocalNullValue(local);
				EditorGUI.PropertyField(position, local, label, true);
			}
			else
			{
				local.managedReferenceValue = null;
				EditorGUI.PropertyField(position, fab, label, true);
			}
		}

		private class Context
		{
			public Vector2 Position;
			public object LocalObject;
			public SerializedProperty Property;
		}

		private void OnChooseExisting(object obj)
		{
			Context context = obj as Context;
			SerializedProperty local = context.Property.FindPropertyRelative("m_LocalValue");
			local.managedReferenceValue = null;
			SerializedProperty useLocal = context.Property.FindPropertyRelative("m_UseLocal");
			useLocal.boolValue = false;
			context.Property.serializedObject.ApplyModifiedProperties();
		}

		private void OnCreateFromLocal(object obj)
		{
			Context context = obj as Context;
			SerializedProperty local = context.Property.FindPropertyRelative("m_LocalValue");
			SerializedProperty fab = context.Property.FindPropertyRelative("m_FabValue");
			SerializedProperty useLocal = context.Property.FindPropertyRelative("m_UseLocal");

			System.Type soType = GetScriptibleObjectType();
			ScriptableObject createdObject = ScriptableObject.CreateInstance(soType);
			SerializedObject sCreatedObject = new(createdObject);
			sCreatedObject.FindProperty("m_Type").managedReferenceValue = local.managedReferenceValue;
			sCreatedObject.ApplyModifiedProperties();
			string path = AssetPickerUtilityMenu.GetCreatedObjectPath(createdObject);
			AssetPickerUtilityMenu.HandleCreatedObject(createdObject, path);
			AssetPickerRenameWindow.Open(createdObject, createdObject.GetType(), context.Position);

			fab.objectReferenceValue = createdObject;
			local.managedReferenceValue = null;
			useLocal.boolValue = false;
			context.Property.serializedObject.ApplyModifiedProperties();
		}

		private System.Type[] GetScriptfabGenericArguments()
		{
			System.Type t = PropertyDrawerUtil.GetUnderlyingType(fieldInfo);
			while (t.BaseType != typeof(ScriptfabBase))
			{
				t = t.BaseType;
			}
			return t.GetGenericArguments();
		}
		public System.Type GetScriptibleObjectType()
		{
			System.Type[] args = GetScriptfabGenericArguments();
			return args[0]; // First generic argument should be the type of scriptable object
		}
		public System.Type GetValueType()
		{
			System.Type[] args = GetScriptfabGenericArguments();
			return args[1]; // Second generic argument should be the type of fab value
		}

		private void TryFixLocalNullValue(SerializedProperty local)
		{
			if (local.managedReferenceValue != null)
			{
				return;
			}
			if (fieldInfo.FieldType.GetCustomAttribute<ScriptfabStyle.NeverNullLocalValue>(true) == null)
			{
				return;
			}
			System.Type valueType = GetValueType();
			local.managedReferenceValue = System.Activator.CreateInstance(valueType);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty local = property.FindPropertyRelative("m_LocalValue");
			SerializedProperty fab = property.FindPropertyRelative("m_FabValue");
			SerializedProperty useLocal = property.FindPropertyRelative("m_UseLocal");

			if (useLocal.boolValue)
			{
				return EditorGUI.GetPropertyHeight(local, label, true);
			}
			else
			{
				return EditorGUI.GetPropertyHeight(fab, label, true);
			}
		}
	}
}
