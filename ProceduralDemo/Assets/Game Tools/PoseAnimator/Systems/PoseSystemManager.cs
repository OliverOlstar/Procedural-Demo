using System;
using System.Collections.Generic;
using ODev.Updateables;
using ODev.Util;

namespace ODev.PoseAnimator
{
    public class PoseSystemManager : MonoBehaviourSingleton<PoseSystemManager>
    {
        // TODO: Offset each system to different frames preventing overlap
        // TODO: Remove Complete in place of a queue system for changes to the arrays
        private const float UPDATE_INTERVAL = 1.0f / 24.0f;
        private readonly Updateable m_Updateable = new(UpdateableType.Fixed, UpdateablePriority.PoseAnimator, UPDATE_INTERVAL);
        private readonly Updateable m_UpdateableComplete = new(UpdateableType.Fixed, UpdateablePriority.PoseAnimator + 1, UPDATE_INTERVAL);
        private readonly Dictionary<int, PoseSystem> m_Systems = new();

		private void OnEnable()
        {
            m_Updateable.Register(Tick);
            m_UpdateableComplete.Register(TickComplete);
        }

        private void OnDisable()
        {
            m_Updateable.UnRegister();
            m_UpdateableComplete.UnRegister();
        }

        protected override void OnDestroy()
        {
            foreach (var system in m_Systems.Values)
            {
                system.Dispose();
            }

            base.OnDestroy();
        }

        private void Tick(float pDeltaTime)
        {
            foreach (var system in m_Systems.Values)
            {
                system.Tick(pDeltaTime);
            }
        }

        private void TickComplete(float pDeltaTime)
        {
            foreach (var system in m_Systems.Values)
            {
                system.TickComplete(pDeltaTime);
            }
        }

        public PoseSystem GetOrCreatePoseSystem(SOPoseSkeleton pSkeleton, SOPoseAnimatorConfig pConfig)
        {
            var handle = HashCode.Combine(pSkeleton.GetHashCode(), pConfig.GetHashCode());

            if (!m_Systems.TryGetValue(handle, out var system))
            {
                this.Log($"New {nameof(PoseSystem)} created for {pSkeleton.name}+{pConfig.name} ({handle})");
                system = new PoseSystem(pSkeleton, pConfig);
                m_Systems.Add(handle, system);
            }
            return system;
        }
    }
}
