using UnityEditor;

namespace ODev
{
	public abstract class EditorPrefsValue<T>
	{
		public static implicit operator T(EditorPrefsValue<T> value)
		{
			return value.m_Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		protected abstract T Get();
		protected abstract void Set(T value);

		public string m_Key;
		protected T m_DefaultValue;
		protected bool m_HasValue = false;
		protected T m_Value;

		public string Key => m_Key;
		public T DefaultValue => m_DefaultValue;
		public bool HasValue => m_HasValue;
		public T Value
		{
			get => m_Value;
			set
			{
				m_HasValue = true;
				m_Value = value;
				Set(m_Value);
			}
		}

		public EditorPrefsValue(string key)
			: this(key, default(T))
		{
		}

		public EditorPrefsValue(string key, T defaultValue)
		{
			m_Key = key;
			m_DefaultValue = defaultValue;
			m_HasValue = EditorPrefs.HasKey(m_Key);
			m_Value = m_HasValue ? Get() : m_DefaultValue;
		}
	}
}
