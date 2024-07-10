using OCore;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(SerializedInterface), true)]
public class SerializedInterfaceDrawer : PropertyDrawer
{
	private const float NOTIFICATION_DURATION = 5f;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty target = property.FindPropertyRelative("m_Target");
		SerializedInterface serializedInterface = property.GetField<SerializedInterface>();
		if (serializedInterface != null && label != GUIContent.none)
		{
			label = new GUIContent($"{label.text} ({serializedInterface.GetInterfaceType().Name})");
		}
		Object pickedObject = EditorGUI.ObjectField(position, label, target.objectReferenceValue, typeof(Object), true);
		if (pickedObject == target.objectReferenceValue)
		{
			return;
		}
		if (!pickedObject)
		{
			target.objectReferenceValue = null;
			return;
		}
		if (pickedObject is GameObject go)
		{
			if (go.TryGetComponent(serializedInterface.GetInterfaceType(), out Component component))
			{
				target.objectReferenceValue = component;
			}
			else
			{
				GUIContent content = new($"GameObject {go.name} does not have any\nComponents implementing {serializedInterface.GetInterfaceType().Name}");
				EditorWindow.focusedWindow.ShowNotification(content, NOTIFICATION_DURATION);
			}
		}
		else if (pickedObject.GetType().Is(serializedInterface.GetInterfaceType()))
		{
			target.objectReferenceValue = pickedObject;
		}
		else
		{
			GUIContent content = new($"{pickedObject.GetType().Name} does not\nimplement {serializedInterface.GetInterfaceType().Name}");
			EditorWindow.focusedWindow.ShowNotification(content, NOTIFICATION_DURATION);
		}
	}
}