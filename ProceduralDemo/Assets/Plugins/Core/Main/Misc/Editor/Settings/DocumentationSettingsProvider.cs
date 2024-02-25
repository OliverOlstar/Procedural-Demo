using Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class DocumentationSettingsProvider
{
	private const string SETTINGS_PATH = "Project/ECG/Documentation";
	private const float LABEL_WIDTH = 130f;
	private const float BUTTON_WIDTH = 50f;

	private static readonly SortedDictionary<string, string> m_Links;

	static DocumentationSettingsProvider()
	{
		m_Links = new SortedDictionary<string, string>();
		foreach (Type type in TypeUtility.GetAllTypes())
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				DocumentationLinkAttribute attribute = field.GetCustomAttribute<DocumentationLinkAttribute>();
				if (attribute == null)
				{
					continue;
				}
				if (!field.FieldType.Is<string>() || !field.IsStatic || !field.IsLiteral)
				{
					Debug.LogError($"{nameof(DocumentationLinkAttribute)} can only be applied to const strings");
					continue;
				}
				string value = (string)field.GetValue(null);
				if (string.IsNullOrEmpty(value))
				{
					continue;
				}
				m_Links[attribute.DisplayName] = value;
			}
		}
	}

	[SettingsProvider]
	private static SettingsProvider Get()
	{
		DocumentationSettingsProvider provider = new DocumentationSettingsProvider();
		return new SettingsProvider(SETTINGS_PATH, SettingsScope.Project)
		{
			label = "Documentation",
			guiHandler = provider.OnGUI,
			keywords = new string[] { "documentation", "doc", "docs" },
		};
	}

	private void OnGUI(string searchContext)
	{
		EditorGUILayout.HelpBox("Use the [DocumentationLink] attribute to add links here.", MessageType.Info, true);
		EditorGUILayout.Space();
		foreach (KeyValuePair<string, string> kvp in m_Links)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(kvp.Key, EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
				if (GUILayout.Button(kvp.Value, EditorStyles.linkLabel))
				{
					Application.OpenURL(kvp.Value);
				}
			}
		}
	}
}
