
using System;
using UnityEngine;

namespace Core
{
	public class SuffixAttribute : PropertyAttribute
	{
		public string m_Suffix = string.Empty;
		public SuffixAttribute(string suffix)
		{
			m_Suffix = suffix;
		}
	}

	public class TooltipAttribute : PropertyAttribute
	{
		public string m_Tooltip = null;

		public TooltipAttribute(string toolTip)
		{
			m_Tooltip = toolTip;
		}

	}

	public class PercentAttribute : TooltipAttribute
	{
		public bool m_Clamp = true;
		public PercentAttribute(bool clamp = true, string toolTip = null) : base(toolTip)
		{
			m_Clamp = clamp;
		}
	}

	public class LightDirAttribute : PropertyAttribute { }

	public class FramesAttribute : TooltipAttribute
	{
		public FramesAttribute(string toolTip = null) : base(toolTip)
		{

		}
	}

	public class Frames60Attribute : TooltipAttribute
	{
		public Frames60Attribute(string toolTip = null) : base(toolTip)
		{

		}
	}

	public class DegreeAttribute : TooltipAttribute
	{
		public DegreeAttribute(string toolTip = null) : base(toolTip)
		{

		}
	}

	public class RangedAttribute : PropertyAttribute
	{
		public float m_Min = 0.0f;
		public float m_Max = 1.0f;

		public RangedAttribute()
		{
			m_Min = 0.0f;
			m_Max = 1.0f;
		}
		public RangedAttribute(float min, float max)
		{
			m_Min = min;
			m_Max = max;
		}
		public RangedAttribute(int min, int max)
		{
			m_Min = min;
			m_Max = max;
		}
	}

	public class MinAttribute : PropertyAttribute
	{
		public float m_Min = 0.0f;

		public MinAttribute()
		{
			m_Min = 0.0f;
		}
		public MinAttribute(float min)
		{
			m_Min = min;
		}
		public MinAttribute(int min)
		{
			m_Min = min;
		}
	}

	public class MaxAttribute : PropertyAttribute
	{
		public float m_Max = 1.0f;

		public MaxAttribute()
		{
			m_Max = 1.0f;
		}
		public MaxAttribute(float max)
		{
			m_Max = max;
		}
		public MaxAttribute(int max)
		{
			m_Max = max;
		}
	}
	
	public class StringAutoCompleteAttribute : PropertyAttribute
	{
		static readonly string[] DEFAULT = new string[] { };

		public string[] mSuggestions = null;
		public string mGetSuggestionsFunction = null;

		public StringAutoCompleteAttribute(string[] options)
		{
			mSuggestions = options != null ? options : DEFAULT;
		}

		public StringAutoCompleteAttribute(string getOptionsFunction)
		{
			mGetSuggestionsFunction = getOptionsFunction;
		}
	}

	public class StringDropdownAttribute : PropertyAttribute
	{
		static readonly string[] DEFAULT = new string[] { };

		public string[] mOptions = null;
		public string mGetOptionsFunction = null;

		public StringDropdownAttribute(string[] options)
		{
			mOptions = options != null ? options : DEFAULT;
		}

		public StringDropdownAttribute(string getOptionsFunction)
		{
			mGetOptionsFunction = getOptionsFunction;
		}
	}

	public class EnumListAttribute : PropertyAttribute
	{
		public Type mEnumType = null;
		public EnumListAttribute(Type enumType)
		{
			mEnumType = enumType;
		}
	}

	public class EnumMaskAttribute : PropertyAttribute
	{
		private Type m_Type = null;
		public Type Type => m_Type;

		public EnumMaskAttribute(Type type = null)
		{
			m_Type = type;
		}
	}

	public class EnumIncludeAttribute : PropertyAttribute
	{
		protected int[] m_IncludeIndexes = null;
		public int[] IncludeIndexes => m_IncludeIndexes;

		public EnumIncludeAttribute(params int[] includeIndexes)
		{
			m_IncludeIndexes = includeIndexes;
		}
	}

	public class EnumExcludeAttribute : PropertyAttribute
	{
		protected int[] m_ExcludeIndexes = null;
		public int[] ExcludeIndexes => m_ExcludeIndexes;

		public EnumExcludeAttribute(params int[] excludeIndexes)
		{
			m_ExcludeIndexes = excludeIndexes;
		}
	}

	public class EnumExcludeFirstAttribute : EnumExcludeAttribute
	{
		public EnumExcludeFirstAttribute() : base(0) { }
	}

	public class EnumExcludeLastAttribute : PropertyAttribute
	{
		public EnumExcludeLastAttribute() { }
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	public class HelpBoxAttribute : PropertyAttribute
	{
		public enum MessageType
		{
			None = 0,
			Info = 1,
			Warning = 2,
			Error = 3
		}

		public string m_MessageText = null;
		public MessageType m_MessageType = MessageType.None;

		public HelpBoxAttribute(string message, MessageType type = MessageType.Info)
		{
			m_MessageText = message;
			m_MessageType = type;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyAttribute : PropertyAttribute
	{
		public bool m_Flatten = false;

		public ReadOnlyAttribute(bool flatten = false)
		{
			m_Flatten = flatten;
		}
	}

	public class FlattenAttribute : PropertyAttribute
	{
		public bool m_Title = true;
		public bool m_Box = true;
		public bool m_Indent = true;

		public FlattenAttribute(
			bool title = true,
			bool box = true,
			bool indent = true)
		{
			m_Title = title;
			m_Box = box;
			m_Indent = indent;
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class FilePickerAttribute : PropertyAttribute
	{
		public bool m_ShowFolders = false;
		public bool m_ShowFiles = false;
		public bool m_GotoButton = false;

		public FilePickerAttribute(bool showFolders, bool showFiles, bool gotoButton = true)
		{
			m_ShowFolders = showFolders;
			m_ShowFiles = showFiles;
			m_GotoButton = gotoButton;
		}
	}
}
