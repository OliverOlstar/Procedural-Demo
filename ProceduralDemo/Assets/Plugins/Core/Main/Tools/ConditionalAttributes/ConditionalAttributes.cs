using UnityEngine;

namespace Core
{
	public enum ConditionalAttributeStyle
	{
		None = 0,
		FlattenInvisible,
		FlattenBoxed
	}

	public abstract class ConditionalBaseAttribute : PropertyAttribute
	{
		public ConditionalAttributeStyle m_FlattenStyle = ConditionalAttributeStyle.None;
		public bool m_Indent = true;

		public ConditionalBaseAttribute(ConditionalAttributeStyle flatten, bool indent)
		{
			m_FlattenStyle = flatten;
			m_Indent = indent;
		}
	}

	public class ConditionalAttribute : ConditionalBaseAttribute
	{
		public string m_MemberVariableName = string.Empty;
		public object[] m_MemberVariableValue = null;

		public ConditionalAttribute(string name, object arg) : base(ConditionalAttributeStyle.None, true)
		{
			m_MemberVariableName = name;
			m_MemberVariableValue = new object[] { arg };
		}

		public ConditionalAttribute(
			string name,
			object arg,
			bool indent = true,
			bool flattenClass = false) : base(flattenClass ? ConditionalAttributeStyle.FlattenBoxed : ConditionalAttributeStyle.None, indent)
		{
			m_MemberVariableName = name;
			m_MemberVariableValue = new object[] { arg };
		}

		public ConditionalAttribute(
			string name,
			object arg,
			ConditionalAttributeStyle flatten = ConditionalAttributeStyle.None,
			bool indent = true) : base(flatten, indent)
		{
			m_MemberVariableName = name;
			m_MemberVariableValue = new object[] { arg };
		}

		public ConditionalAttribute(
			string name,
			params object[] arg) : base(ConditionalAttributeStyle.None, true)
		{
			m_MemberVariableName = name;
			m_MemberVariableValue = arg;
		}

		public ConditionalAttribute(
			string name,
			ConditionalAttributeStyle flatten,
			bool indent,
			params object[] arg) : base(flatten, indent)
		{
			m_MemberVariableName = name;
			m_MemberVariableValue = arg;
		}
	}

	public class HideIf : ConditionalBaseAttribute
	{
		public string m_MethodName = string.Empty;

		public HideIf(string methodName, ConditionalAttributeStyle flatten = ConditionalAttributeStyle.None, bool indent = true) :
			base(flatten, indent)
		{
			m_MethodName = methodName;
		}
	}

	public class ShowIf : ConditionalBaseAttribute
	{
		public string m_MethodName = string.Empty;

		public ShowIf(string methodName, ConditionalAttributeStyle flatten = ConditionalAttributeStyle.None, bool indent = true) :
			base(flatten, indent)
		{
			m_MethodName = methodName;
		}
	}
}
