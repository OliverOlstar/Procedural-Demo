using System;
using UnityEngine;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Assembly-CSharp")]
[Serializable]
public class SerializedType : ISerializationCallbackReceiver
{
	[SerializeField]
	private string m_Type = string.Empty;

	private Type m_Value;

	public Type Value 
	{
		get
		{
			if (m_Value == null)
			{
				m_Value = Type.GetType(m_Type);
			}
			return m_Value;
		}
	}

	public static implicit operator Type(SerializedType type)
	{
		return type.Value;
	}

	public static implicit operator SerializedType(Type type)
	{
		if (type == null)
		{
			return null;
		}
		return new SerializedType()
		{
			m_Value = type,
			m_Type = type.AssemblyQualifiedName,
		};
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_Value = Type.GetType(m_Type);
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}
}