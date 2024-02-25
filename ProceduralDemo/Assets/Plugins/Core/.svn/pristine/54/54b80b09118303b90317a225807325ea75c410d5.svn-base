using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public abstract class SOScriptfabSimple<TValue> : SOScriptfabGeneric<TValue>
		where TValue : class, new()
	{
#if ODIN_INSPECTOR
		[Sirenix.OdinInspector.DrawWithUnity]
#endif
		[SerializeReference, UnityEngine.Serialization.FormerlySerializedAs("m_Type")]
		private TValue m_Value = new();
		public override TValue Value => m_Value;
	}

	namespace ScriptfabStyle
	{
		/// <summary>Requires value type to have a non abstract base class with a parameterless constructor</summary>
		public class NeverNullLocalValue : System.Attribute { }
	}

	/// <summary>Use ScriptfabSimple when value type is not an interface, abstract class, or parent of a class hierarchy.
	/// This is especially useful when the value type has a custom PropertyDrawer.</summary>
	[ScriptfabStyle.NeverNullLocalValue]
	public abstract class ScriptfabSimple<TSOValue, TValue> : ScriptfabGeneric<TSOValue, TValue>
		where TSOValue : SOScriptfabSimple<TValue>
		where TValue : class, new()
	{
		[SerializeReference]
		private TValue m_LocalValue = null;

		[SerializeField, UberPicker.AssetNonNull]
		private TSOValue m_FabValue = null;

		public ScriptfabSimple(bool defaultToUseLocal)
		{
			m_UseLocal = defaultToUseLocal;
			m_LocalValue = defaultToUseLocal ? new() : null;
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
}
