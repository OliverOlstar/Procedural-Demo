using System;

[AttributeUsage(AttributeTargets.Field)]
public abstract class TypeConstraintAttribute : Attribute
{
	public abstract bool IsTypeValid(Type type);
}

[AttributeUsage(AttributeTargets.Field)]
public class DerivedTypeConstraintAttribute : TypeConstraintAttribute
{
	private Type m_BaseType;

	public DerivedTypeConstraintAttribute(Type baseType)
	{
		m_BaseType = baseType;
	}

	public override bool IsTypeValid(Type type)
	{
		return !type.IsAbstract && type.IsSubclassOf(m_BaseType);
	}
}

[AttributeUsage(AttributeTargets.Field)]
public class InterfaceTypeConstraintAttribute : TypeConstraintAttribute
{
	private Type m_InterfaceType;
	private bool m_IncludeInterfaceType;

	public InterfaceTypeConstraintAttribute(Type interfaceType)
		: this(interfaceType, false)
	{
	}

	public InterfaceTypeConstraintAttribute(Type interfaceType, bool includeInterfaceType)
	{
		m_InterfaceType = interfaceType;
		m_IncludeInterfaceType = includeInterfaceType;
	}

	public override bool IsTypeValid(Type type)
	{
		if (type == m_InterfaceType && !m_IncludeInterfaceType)
		{
			return false;
		}
		return m_InterfaceType.IsAssignableFrom(type);
	}
}