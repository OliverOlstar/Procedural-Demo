using UnityEditor;
using UnityEngine;

public static class AssetPickerUtilityMenu
{
	public class Context
	{
		public Vector2 Position;
		public System.Type Type;
		public Object Object;
		public System.Action<SerializedProperty, string> OnSelected;
		public SerializedProperty Property;
	}

	public static void TryAttachMenu(
		ref Rect attachToRect,
		SerializedProperty property,
		System.Type objectType,
		string selectedPath = null,
		System.Action<SerializedProperty, string> onSelected = null)
	{
		if (!typeof(ScriptableObject).IsAssignableFrom(objectType))
		{
			return;
		}

		attachToRect.width -= UberPickerGUI.POINTER_BUTTON_WIDTH;
		Rect r = new(attachToRect.x + attachToRect.width, attachToRect.y, UberPickerGUI.HAMBURGER_BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);
		GUIStyle hamburgerLabel = new(GUI.skin.label);
		GUI.Label(r, UberPickerGUI.HAMBURGER_UNICODE, UberPickerGUI.HAMBURGER_STYLE);

		Event e = Event.current;
		if (e.type == EventType.MouseDown &&
			e.button == 0 &&
			r.Contains(e.mousePosition))
		{
			GenericMenu menu = new();
			Vector2 p = GUIUtility.GUIToScreenPoint(e.mousePosition);
			if (!string.IsNullOrEmpty(selectedPath))
			{
				Object obj = AssetDatabase.LoadAssetAtPath(selectedPath, objectType);
				if (obj != null)
				{
					Context context = new()
					{
						Type = objectType,
						Object = obj,
						Property = property,
						OnSelected = onSelected,
						Position = p,
					};
					menu.AddItem(new GUIContent("Rename"), false, OnRename, context);
					menu.AddItem(new GUIContent($"Duplicate '{GetObjectName(obj)}'"), false, OnDuplicate, context);
				}
			}
			foreach (System.Type type in OCore.TypeUtility.GetMatchingTypes(objectType, IsTypeMatching))
			{
				Context context = new()
				{
					Type = type,
					Property = property,
					OnSelected = onSelected,
					Position = p,
				};
				menu.AddItem(new GUIContent("New " + type.Name), false, OnCreate, context);
			}
			menu.ShowAsContext();
		}
	}

	/// <summary>Accessing OCore.SOCacheName.name causes exceptions, utility function to avoid that</summary>
	public static string GetObjectName(Object obj) => obj is OCore.SOCacheName cachedName ? cachedName.Name : obj.name;

	private static bool IsTypeMatching(System.Type type, System.Type baseType)
	{
		return baseType.IsAssignableFrom(type) &&
			!type.IsGenericTypeDefinition &&
			!type.IsAbstract &&
			!type.IsDefined(typeof(System.ObsoleteAttribute), false);
	}

	public static void OnRename(object obj)
	{
		Context context = obj as Context;
		EditorGUIUtility.PingObject(context.Object);
		AssetPickerRenameWindow.Open(context.Object, context.Type, context.Position);
	}

	public static void OnDuplicate(object obj)
	{
		Context context = obj as Context;
		Object objectToDuplicate = context.Object;
		Object createdObject = Object.Instantiate(objectToDuplicate);
		string createdObjectPath = GetCreatedObjectPath(createdObject, exampleObjectHint: objectToDuplicate);
		HandleCreatedObjectInternal(createdObject, createdObjectPath, context);
	}

	public static void OnCreate(object obj)
	{
		Context context = obj as Context;
		Object createdObject = ScriptableObject.CreateInstance(context.Type);
		string createdObjectPath = GetCreatedObjectPath(createdObject, objectTypeHint: context.Type);
		HandleCreatedObjectInternal(createdObject, createdObjectPath, context);
	}

	public static string GetCreatedObjectPath(
		Object createdObject,
		Object exampleObjectHint = null,
		System.Type objectTypeHint = null)
	{
		// Use hints to try to find an example path
		string examplePath = null;
		if (exampleObjectHint != null)
		{
			examplePath = AssetDatabase.GetAssetPath(exampleObjectHint);
		}
		else
		{
			if (objectTypeHint == null)
			{
				objectTypeHint = createdObject.GetType();
			}
			string[] paths = OCore.AssetDatabaseUtil.Find(objectTypeHint);
			if (paths.Length != 0)
			{
				examplePath = paths[0];
			}
		}

		string createdObjectName = GetObjectName(createdObject);
		if (string.IsNullOrEmpty(createdObjectName))
		{
			createdObjectName = $"New {createdObject.GetType().Name}";
		}

		string createdObjectPath;
		if (!string.IsNullOrEmpty(examplePath))
		{
			createdObjectPath = $"{System.IO.Path.GetDirectoryName(examplePath)}/{createdObjectName}{System.IO.Path.GetExtension(examplePath)}";
		}
		else // No example was found so try our best to choose a decent path
		{
			string folderName = createdObject.GetType().Name;
			if (folderName.StartsWith("SO")) // Often we use the prefix "SO" in the name of ScriptableObject class but it makes for ugly paths
			{
				folderName = folderName.Substring(2);
			}
			createdObjectPath = $"Assets/ScriptableObjects/{folderName}";
			System.IO.Directory.CreateDirectory(createdObjectPath);
			createdObjectPath += $"/{createdObjectName}.asset";
		}
		return createdObjectPath;
	}

	public static void HandleCreatedObject(Object createdObject, string createdObjectPath)
	{
		createdObjectPath = AssetDatabase.GenerateUniqueAssetPath(createdObjectPath);
		AssetDatabase.CreateAsset(createdObject, createdObjectPath);
		EditorGUIUtility.PingObject(createdObject);
	}

	private static void HandleCreatedObjectInternal(Object createdObject, string createdObjectPath, Context context)
	{
		HandleCreatedObject(createdObject, createdObjectPath);
		AssetPickerRenameWindow.Open(createdObject, context.Type, context.Position);
		context.OnSelected?.Invoke(context.Property, createdObjectPath);
	}
}
