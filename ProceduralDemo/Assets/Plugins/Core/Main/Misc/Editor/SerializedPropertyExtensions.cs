using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Core
{
	public static class SerializedPropertyExtensions
	{
		public static T GetField<T>(this SerializedProperty self)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			return GetField<T>(self, flags);
		}

		public static T GetField<T>(this SerializedProperty self, BindingFlags flags)
		{
			string path = self.propertyPath.Replace(".Array.data", string.Empty);
			string[] tokens = path.Split('.');
			Regex regex = new("(\\[[0-9]\\])");
			object target = self.serializedObject.targetObject;
			for (int i = 0; i < tokens.Length; ++i)
			{
				string token = tokens[i];
				if (token.Contains("["))
				{
					Match match = regex.Match(token);
					string value = match.Groups[0].Value;
					string fieldName = token.Replace(value, "");
					int index = int.Parse(value.Replace("[", "").Replace("]", ""));
					FieldInfo fieldInfo = target.GetType().GetField(fieldName, flags);
					if (fieldInfo != null)
					{
						IEnumerable array = fieldInfo.GetValue(target) as IEnumerable;
						int count = 0;
						foreach (object obj in array)
						{
							if (count == index)
							{
								target = obj;
								break;
							}
							count++;
						}
					}
				}
				else
				{
					FieldInfo fieldInfo = target.GetType().GetField(token, flags);
					target = fieldInfo.GetValue(target);
				}
			}
			try
			{
				return (T)target;
			}
			catch
			{
				return default;
			}
		}
	}
}