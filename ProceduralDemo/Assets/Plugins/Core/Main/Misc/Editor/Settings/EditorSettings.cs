using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CoreEditor
{
	public abstract class EditorSettings : ScriptableObject
	{
	}

	public abstract class EditorSettings<T> : EditorSettings
		where T : EditorSettings
	{
		private static T s_Asset;

		public static T Asset
		{
			get
			{
				if (s_Asset == null)
				{
					EditorSettingsAttribute attribute = typeof(T).GetCustomAttribute<EditorSettingsAttribute>();
					if (attribute == null)
					{
						throw new Exception($"{nameof(T)} does not have an [{nameof(EditorSettingsAttribute)}]");
					}
					s_Asset = AssetDatabase.LoadAssetAtPath<T>(attribute.Path);
					if (s_Asset == null)
					{
						s_Asset = CreateInstance<T>();
						string directory = Path.GetDirectoryName(attribute.Path).Replace("\\", "/");
						if (!Directory.Exists(directory))
						{
							Directory.CreateDirectory(directory);
						}
						AssetDatabase.CreateAsset(s_Asset, attribute.Path);
						AssetDatabase.ImportAsset(attribute.Path);
					}

				}
				return s_Asset;
			}
		}
	}
}
