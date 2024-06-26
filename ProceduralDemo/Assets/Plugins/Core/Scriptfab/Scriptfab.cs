using UnityEngine;

namespace Core
{
	public abstract class ScriptfabBase // For attaching property drawer
	{

	}

	public abstract class ScriptfabGeneric<TSOValue, TValue> : ScriptfabBase
		where TSOValue : SOScriptfabGeneric<TValue>
		where TValue : class
	{
		[SerializeField]
		protected bool m_UseLocal = true;

		public abstract bool TryGetValue(out TValue value);
		public override string ToString() => 
			$"{GetType().Name}({(TryGetValue(out TValue value) ? value.GetType().ToString() : "NULL")}, {(m_UseLocal ? "L" : "G")})";
	}

	public abstract class Scriptfab<TSOValue, TValue> : ScriptfabGeneric<TSOValue, TValue>
		where TSOValue : SOScriptfab<TValue>
		where TValue : class
	{
		[SerializeReference, SerializedReferenceDrawer]
		private TValue m_LocalValue = null;

		[SerializeField, UberPicker.AssetNonNull]
		private TSOValue m_FabValue = null;

		public Scriptfab(bool defaultToUseLocal, TValue defaultLocalValue = null)
		{
			m_UseLocal = defaultToUseLocal;
			m_LocalValue = defaultLocalValue;
		}

		public bool HasValue
		{
			get
			{
				TValue value = m_UseLocal ? m_LocalValue :
					m_FabValue != null ? m_FabValue.Value :
					null;
				return value != null;
			}
		}

		public TValue Value
		{
			get
			{
				TValue value = m_UseLocal ? m_LocalValue :
					m_FabValue != null ? m_FabValue.Value :
					null;
				if (value == null)
				{
					throw new System.NullReferenceException($"{GetType().Name}.Value {(m_UseLocal ? "Local" : "Global")} value is null");
				}
				return value;
			}
		}

		public override bool TryGetValue(out TValue value)
		{
			value = Value;
			return value != null;
		}
	}

	public abstract class ScriptfabNullable<TSOValue, TValue> : ScriptfabGeneric<TSOValue, TValue>
		where TSOValue : SOScriptfab<TValue>
		where TValue : class
	{
		[SerializeReference, SerializedReferenceDrawer(nullEntryName:"None")]
		private TValue m_LocalValue = null;

		[SerializeField, UberPicker.Asset]
		private TSOValue m_FabValue = null;

		public ScriptfabNullable(bool defaultToUseLocal, TValue defaultLocalValue = null)
		{
			m_UseLocal = defaultToUseLocal;
			m_LocalValue = defaultLocalValue;
		}

		public bool HasValue => TryGetValue(out _);

		public override bool TryGetValue(out TValue value)
		{
			value = m_UseLocal ? m_LocalValue :
				m_FabValue != null ? m_FabValue.Value :
				null;
			return value != null;
		}

		public ScriptfabNullable(bool defaultToUseLocal)
		{
			m_UseLocal = defaultToUseLocal;
		}
	}
}
