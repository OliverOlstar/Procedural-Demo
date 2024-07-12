using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class SerializedInterface
{
	[SerializeField]
	private Object m_Target = null;

	protected Object Target => m_Target;

	public abstract Type GetInterfaceType();

	public static implicit operator bool(SerializedInterface serializedInterface)
	{
		if (serializedInterface == null)
		{
			return false;
		}
		return serializedInterface.m_Target;
	}
}

[Serializable]
public abstract class SerializedInterface<T> : SerializedInterface, ISerializationCallbackReceiver
	where T : class
{
	public T AsInterface { get; private set; }

	protected SerializedInterface()
	{
		if (!typeof(T).IsInterface)
		{
			Debug.LogErrorFormat($"Type {GetType().Name} must have an interface type as its generic type. Has {typeof(T).Name}");
		}
	}

	public sealed override Type GetInterfaceType()
	{
		return typeof(T);
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		AsInterface = Target as T;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}
}