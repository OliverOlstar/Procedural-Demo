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
        [SerializeField] protected T _defaultValue;
        [SerializeField] private T _value;
        public T Value => _value;
        public virtual T DefaultValue => _defaultValue;

        public event Action<T> ValueChanged = delegate { };

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
            SetValue(_defaultValue);
        }

        public virtual void SetValue(T value)
        {
            _value = value;
            ValueChanged?.Invoke(value);
        }

        [Button]
        private void CallValueChanged()
        {
            ValueChanged?.Invoke(_value);
        }
    }
}