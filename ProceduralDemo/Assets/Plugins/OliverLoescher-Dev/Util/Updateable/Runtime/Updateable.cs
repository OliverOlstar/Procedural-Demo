using System;
using ODev.Util;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODev.Updateables
{
    [Serializable]
    public class Updateable : IComparable<Updateable>
    {
        [SerializeField, DisableInPlayMode]
        private UpdateableType m_Type;
        [SerializeField, DisableInPlayMode]
        private UpdateablePriority m_Priority;
        [SerializeField, DisableInPlayMode]
        private float m_IntervalSeconds;

        private Action<float> m_Action;
        private Func<bool> m_Predicate;
        private float m_TimeElapse = 0.0f;

        public UpdateableType Type => m_Type;
        public UpdateablePriority Priority => m_Priority;
        public float IntervalSeconds => m_IntervalSeconds;
        public bool IsRegistered => m_Action != null;
        public float TimeSinceLastUpdate => m_TimeElapse;

        public Updateable(UpdateableType type, UpdateablePriority priority, float intervalSeconds = 0.0f)
        {
            m_Action = null;
            m_Predicate = null;
            m_TimeElapse = 0.0f;

            m_Type = type;
            m_Priority = priority;
            m_IntervalSeconds = intervalSeconds;
        }

        public void SetProperties(UpdateableType type, UpdateablePriority priority, float intervalSeconds = 0.0f)
        {
            if (IsRegistered)
            {
                this.LogError("Tried setting properties when already registered");
                return;
            }
            m_Type = type;
            m_Priority = priority;
            m_IntervalSeconds = intervalSeconds;
        }

        public void SetInterval(float intervalSeconds)
        {
            m_IntervalSeconds = intervalSeconds;
        }

        public void Register(Action<float> pAciton, Func<bool> pPredicate = null)
        {
            Assert.IsNotNull(pAciton, "Action is null");
            bool registered = IsRegistered;
            if (registered)
            {
                m_Action = null;
            }
            m_Action = pAciton;
            m_Predicate = pPredicate;
            m_TimeElapse = 0.0f;
            if (!registered)
            {
                UpdateableManager.Instance.Register(this);
            }
        }

        public void UnRegister()
        {
            if (!IsRegistered)
            {
                return;
            }
			if (UpdateableManager.Exists)
			{
				UpdateableManager.Instance.Unregister(this);
			}
			m_Action = null;
            m_Predicate = null;
        }

        internal void TryUpdate(float deltaTime)
        {
            if (m_Action == null)
            {
                this.LogError("Action is null, this should never happen. Please fix!");
                UpdateableManager.Instance.Unregister(this);
                m_Predicate = null;
                return;
            }
            // if (!m_Action.Target)
            // {
            //     this.LogWarning($"{nameof(m_Action)} is not alive, unregistering");
            //     Deregister();
            //     return;
            // }

            m_TimeElapse += deltaTime;
            if (m_TimeElapse < m_IntervalSeconds)
            {
                return;
            }
            if (m_Predicate == null || m_Predicate.Invoke())
            {
                m_Action.Invoke(m_TimeElapse);
            }
            m_TimeElapse = 0.0f;
        }

        int IComparable<Updateable>.CompareTo(Updateable other)
        {
            return other.Priority.CompareTo(Priority);
        }

        public override string ToString()
        {
            if (!IsRegistered)
            {
                return $"{nameof(Updateable)}(Action: NULL, Type: {m_Type}, Priority: {m_Priority}, Interval: {m_IntervalSeconds})";
            }
            return $"{nameof(Updateable)}(Action: {m_Action.Method.Name}, Type: {m_Type}, Priority: {m_Priority}, Interval: {m_IntervalSeconds})";
        }
    }
}
