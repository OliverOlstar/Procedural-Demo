using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace ODev.Update
{
    public class UpdateManager : MonoBehaviourSingletonAuto<UpdateManager>
    {
        private static List<Updateable> s_Updatables = new();
        private static List<Updateable> s_EarlyUpdatables = new();
        private static List<Updateable> s_LateUpdatables = new();
        private static List<Updateable> s_FixedUpdatables = new();
        private readonly List<Updateable> m_Updateables = new(); // Resuse instead of making new every frame

#if UNITY_EDITOR
        public static IEnumerable<Updateable> GetAllUpdateables()
        {
            foreach (Updateable updateable in s_EarlyUpdatables)
            {
                yield return updateable;
            }
            foreach (Updateable updateable in s_Updatables)
            {
                yield return updateable;
            }
            foreach (Updateable updateable in s_LateUpdatables)
            {
                yield return updateable;
            }
            foreach (Updateable updateable in s_FixedUpdatables)
            {
                yield return updateable;
            }
        }
#endif

        

        internal static void RegisterUpdate(in Updateable pUpdatable)
        {
            TryCreate();

            ref List<Updateable> items = ref GetUpdatables(pUpdatable.Type);
            int index;
            for (index = 0; index < items.Count; index++)
            {
                if (items[index].Priority > pUpdatable.Priority)
                {
                    break;
                }
            }
            items.Insert(index, pUpdatable);
        }

        internal static void DeregisterUpdate(in Updateable pUpdatable)
        {
            if (!GetUpdatables(pUpdatable.Type).Remove(pUpdatable))
            {
                LogError($"Failed to remove {pUpdatable}.");
            }
        }

        private static ref List<Updateable> GetUpdatables(Type pType)
        {
            switch (pType)
            {
                case Type.Early:
                    return ref s_EarlyUpdatables;
                case Type.Late:
                    return ref s_LateUpdatables;
                case Type.Fixed:
                    return ref s_FixedUpdatables;
                default:
                    return ref s_Updatables;
            }
        }

        private void Update()
        {
            UpdateInternal(s_EarlyUpdatables, Time.deltaTime, "MonoUtil.EarlyUpdate()");
            UpdateInternal(s_Updatables, Time.deltaTime, "MonoUtil.Update()");
        }

        private void LateUpdate()
        {
            UpdateInternal(s_LateUpdatables, Time.deltaTime, "MonoUtil.LateUpdate()");
        }

        private void FixedUpdate()
        {
            UpdateInternal(s_FixedUpdatables, Time.fixedDeltaTime, "MonoUtil.FixedUpdate()");
        }

        private void UpdateInternal(List<Updateable> pUpdatables, in float pDeltaTime, string pProfilerName)
        {
            Profiler.BeginSample(pProfilerName);
            m_Updateables.AddRange(pUpdatables); // Copy over incase it changes
            foreach (Updateable updatable in m_Updateables)
            {
                updatable.TryUpdate(pDeltaTime);
            }
            m_Updateables.Clear();
            Profiler.EndSample();
        }
    }
}
