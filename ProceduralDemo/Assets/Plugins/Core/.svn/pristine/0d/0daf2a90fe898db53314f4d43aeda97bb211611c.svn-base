using System;

namespace CoreEditor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class EditorSettingsAttribute : Attribute
	{
		public string Path { get; private set; }

		public EditorSettingsAttribute(string path)
		{
			Path = path;
		}
	}
}
