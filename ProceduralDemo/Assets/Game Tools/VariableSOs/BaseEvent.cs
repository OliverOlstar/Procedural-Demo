using System;
using UnityEngine;

namespace Events
{
    public abstract class BaseEvent<T> : ScriptableObject
    {
        public T Value { get; private set; }

        private event Action<T> m_Action = delegate { };

        public void Register(Action<T> listener)
        {
            m_Action += listener;
        }

        public void UnRegister(Action<T> listener)
        {
            m_Action -= listener;
        }
        
        public void UnRegisterAll()
        {
            m_Action = delegate { };
        }

        public void Fire(T data)
        {
            Value = data;
            m_Action.Invoke(data);
        }
    }
    
    [CreateAssetMenu(menuName = "Events/EmptyEvent", fileName = "EmptyEvent", order = 0)]
    public sealed class BaseEvent : ScriptableObject
    {
        private event Action m_Action;

        public void Register(Action listener)
        {
            m_Action += listener;
        }

        public void UnRegister(Action listener)
        {
            m_Action -= listener;
        }
        
        public void Fire()
        {
            m_Action?.Invoke();
        }
    }
}