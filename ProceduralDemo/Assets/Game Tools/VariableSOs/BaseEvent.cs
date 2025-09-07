using System;
using UnityEngine;

namespace Events
{
    public abstract class BaseEvent<T> : ScriptableObject
    {
        public T Value { get; private set; }
        private event Action<T> _action;

        public void Register(Action<T> listener)
        {
            _action += listener;
        }

        public void UnRegister(Action<T> listener)
        {
            _action -= listener;
        }
        
        public void UnRegisterAll()
        {
            _action = delegate { };
        }

        public void Fire(T data)
        {
            Value = data;
            _action?.Invoke(data);
        }
    }
    
    [CreateAssetMenu(menuName = "Events/EmptyEvent", fileName = "EmptyEvent", order = 0)]
    public sealed class BaseEvent : ScriptableObject
    {
        private event Action _action;

        public void Register(Action listener)
        {
            _action += listener;
        }

        public void UnRegister(Action listener)
        {
            _action -= listener;
        }
        
        public void Fire()
        {
            _action?.Invoke();
        }
    }
}