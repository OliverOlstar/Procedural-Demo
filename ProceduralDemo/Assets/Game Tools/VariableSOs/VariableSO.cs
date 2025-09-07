using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ODev.VariableSOs
{
    public abstract class VariableSO : ScriptableObject
    {
        public abstract void ResetToDefault();
    }

    public class VariableSO<T> : VariableSO
    {
        [SerializeField] protected T m_DefaultValue;
        [SerializeField, HideInEditorMode] protected T m_Value;

        public event Action<T> OnValueChanged = delegate { };

        public virtual T Value => m_Value;
        public virtual T DefaultValue => m_DefaultValue;

        protected virtual void OnDisable()
        {
            // this.Log($"Set {name} to defaultValue {_defaultValue} on Disable");
            ResetToDefault();
        }

        protected virtual void OnEnable()
        {
            // this.Log($"Set {name} to defaultValue {_defaultValue} on Enable");
            ResetToDefault();
        }

		public override void ResetToDefault()
        {
            SetValue(m_DefaultValue);
        }

        public virtual void SetValue(T value)
        {
            m_Value = value;
            OnValueChanged?.Invoke(value);
        }

        [Button]
        private void CallValueChanged()
        {
            OnValueChanged?.Invoke(m_Value);
        }
    }
}