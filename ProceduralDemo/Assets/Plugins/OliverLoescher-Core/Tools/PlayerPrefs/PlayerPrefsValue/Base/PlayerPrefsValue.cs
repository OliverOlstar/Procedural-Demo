
public abstract class PlayerPrefsValue<T>
{
	public static implicit operator T(PlayerPrefsValue<T> value)
	{
		return value.m_Value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	protected abstract T Get(bool isGobal);
	protected abstract void Set(T value, bool isGobal);

	public string m_Key;
	public bool m_IsGlobalPref = false;
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
			Set(m_Value, m_IsGlobalPref);
		}
	}

	public PlayerPrefsValue(string key, bool isGlobalPref = false)
		: this(key, default(T), isGlobalPref)
	{
	}

	public PlayerPrefsValue(string key, T defaultValue, bool isGlobalPref = false)
	{
		m_Key = key;
		m_IsGlobalPref = isGlobalPref;
		m_DefaultValue = defaultValue;
		m_HasValue = m_IsGlobalPref ? PlayerPrefs.HasGlobalKey(m_Key) : PlayerPrefs.HasKey(m_Key);
		m_Value = m_HasValue ? Get(m_IsGlobalPref) : m_DefaultValue;
	}
}
