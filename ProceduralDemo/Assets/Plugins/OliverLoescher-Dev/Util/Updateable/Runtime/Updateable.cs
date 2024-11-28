using ODev.Util;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ODev.Update
{
    [Serializable]
    public class Updateable
    {
        [SerializeField, DisableInPlayMode]
        private Type m_Type;
        [SerializeField, DisableInPlayMode]
        private Priority m_Priority;
        [SerializeField, DisableInPlayMode]
        private float m_IntervalSeconds;

        private Action<float> m_Action;
        private Func<bool> m_CanUpdate;
        private float m_TimeElapsed;

        public Action<float> Action => m_Action;
        public Type Type => m_Type;
        public Priority Priority => m_Priority;
        public float IntervalSeconds => m_IntervalSeconds;
        public bool IsRegistered => m_Action != null;
        public float TimeSinceLastUpdate => m_TimeElapsed;

        public Updateable(Type pType, Priority pPriority, float pIntervalSeconds = 0.0f)
        {
            m_Action = null;
            m_CanUpdate = null;
            m_TimeElapsed = 0;

            m_Type = pType;
            m_Priority = pPriority;
            m_IntervalSeconds = pIntervalSeconds;
        }

        public void SetProperties(Type pType, Priority pPriority, float pIntervalSeconds = 0.0f)
        {
            if (m_Action != null)
            {
                this.LogError("Tried setting properties when already registered");
                return;
            }
            m_Type = pType;
            m_Priority = pPriority;
            m_IntervalSeconds = pIntervalSeconds;
        }

        public void Register(Action<float> pAciton, Func<bool> pCanUpdate = null)
        {
            if (pAciton == null)
            {
                this.DevException("Was passed a null action");
                return;
            }
            if (IsRegistered)
            {
                if (m_Action.Method == pAciton.Method)
                {
                    this.LogWarning("Was passed the same method which was already registered, skipping");
                    return;
                }
                Deregister(); // Remove old action before registering the new one
            }
            m_Action = pAciton;
            m_CanUpdate = pCanUpdate;
            m_TimeElapsed = 0.0f;
            UpdateManager.RegisterUpdate(this);
        }

        public void Deregister()
        {
            if (m_Action == null)
            {
                this.LogWarning("Tried deregistering when not registered");
                return;
            }
            UpdateManager.DeregisterUpdate(this);
            m_Action = null;
        }

        internal void TryUpdate(float pDeltaTime)
        {
            m_TimeElapsed += pDeltaTime;
            if (m_TimeElapsed < m_IntervalSeconds)
            {
                return;
            }
            if (m_CanUpdate != null && !m_CanUpdate.Invoke())
            {
                return;
            }
            m_Action.Invoke(m_TimeElapsed);
            m_TimeElapsed = 0.0f;
        }

        public override string ToString()
        {
            if (m_Action == null)
            {
                return $"Updateable(Action: NULL, Type: {m_Type}, Priority: {m_Priority}, Interval: {m_IntervalSeconds})";
            }
            return $"Updateable(Action: {m_Action.Target} - {m_Action.Method.Name}, Type: {m_Type}, Priority: {m_Priority}, Interval: {m_IntervalSeconds})";
        }
    }
}
