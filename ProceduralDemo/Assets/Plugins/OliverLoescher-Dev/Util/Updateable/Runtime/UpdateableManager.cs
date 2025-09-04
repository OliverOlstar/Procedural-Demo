using System.Collections.Generic;
using ODev.Util;
using UnityEngine;
using UnityEngine.Profiling;

namespace ODev.Updateables
{
    public class UpdateableManager : MonoBehaviourSingletonAuto<UpdateableManager>
    {
        private readonly List<Updateable> _updateables = new();
        private readonly List<Updateable> _earlyUpdateables = new();
        private readonly List<Updateable> _lateUpdateables = new();
        private readonly List<Updateable> _fixedUpdateables = new();

        private readonly string _defaultProfileName = $"{nameof(UpdateableManager)}.{nameof(Update)}()";
        private readonly string _earlyProfileName = $"{nameof(UpdateableManager)}.Early{nameof(Update)}()";
        private readonly string _lateProfileName = $"{nameof(UpdateableManager)}.{nameof(LateUpdate)}()";
        private readonly string _fixedProfileName = $"{nameof(UpdateableManager)}.{nameof(FixedUpdate)}()";

        private readonly List<Updateable> _updateablesToAdd = new();
        private readonly List<Updateable> _updateablesToRemove = new();
        private UpdateableType? _isUpdatingType = null;

        public static UpdateableManager Create()
        {
            var instance = new GameObject(nameof(UpdateableManager));
            DontDestroyOnLoad(instance);
            return instance.AddComponent<UpdateableManager>();
        }

#if UNITY_EDITOR
        public IEnumerable<Updateable> EditorGetAllUpdateables()
        {
            foreach (Updateable updateable in _earlyUpdateables)
            {
                yield return updateable;
            }
            foreach (Updateable updateable in _updateables)
            {
                yield return updateable;
            }
            foreach (Updateable updateable in _lateUpdateables)
            {
                yield return updateable;
            }
            foreach (Updateable updateable in _fixedUpdateables)
            {
                yield return updateable;
            }
        }
#endif
        private List<Updateable> GetUpdateables(UpdateableType type)
        {
            return type switch
            {
                UpdateableType.Early => _earlyUpdateables,
                UpdateableType.Default => _updateables,
                UpdateableType.Late => _lateUpdateables,
                UpdateableType.Fixed => _fixedUpdateables,
                _ => throw new System.NotImplementedException(),
            };
        }

        private void AddUpdateable(Updateable updateable, List<Updateable> updateables)
        {
            int index = Math.BinaryFindNearest(updateable, updateables);
            updateables.Insert(index, updateable);
        }

        private void RemoveUpdateable(Updateable updateable, List<Updateable> updateables)
        {
            if (!updateables.Remove(updateable))
            {
                typeof(UpdateableManager).LogError($"Failed to remove {updateable}.");
            }
        }

        #region API
        public void Register(Updateable updateable)
        {
            if (_isUpdatingType.HasValue && _isUpdatingType == updateable.Type)
            {
                _updateablesToAdd.Add(updateable);
                _updateablesToRemove.Remove(updateable); // Ensre it's not also removed
                return;
            }
            AddUpdateable(updateable, GetUpdateables(updateable.Type));
        }

        public void Unregister(Updateable updateable)
        {
            if (_isUpdatingType.HasValue && _isUpdatingType == updateable.Type)
            {
                _updateablesToRemove.Add(updateable);
                _updateablesToAdd.Remove(updateable); // Ensre it's not also added
                return;
            }
            RemoveUpdateable(updateable, GetUpdateables(updateable.Type));
        }
        #endregion API

        #region Update

        private void Update()
        {
            UpdateInternal(_earlyUpdateables, UpdateableType.Early, Time.deltaTime, _earlyProfileName);
            UpdateInternal(_updateables, UpdateableType.Default, Time.deltaTime, _defaultProfileName);
        }

        private void LateUpdate()
        {
            UpdateInternal(_lateUpdateables, UpdateableType.Late, Time.deltaTime, _lateProfileName);
        }

        private void FixedUpdate()
        {
            UpdateInternal(_fixedUpdateables, UpdateableType.Fixed, Time.fixedDeltaTime, _fixedProfileName);
        }

        private void UpdateInternal(List<Updateable> updateables, UpdateableType type, in float deltaTime, string profilerName)
        {
            Profiler.BeginSample(profilerName);
            ProcessUpdateables(updateables, type, deltaTime);
            ResolvePendingRegisters(updateables);
            Profiler.EndSample();
        }

        private void ProcessUpdateables(List<Updateable> updateables, UpdateableType type, float deltaTime)
        {
            _isUpdatingType = type;
            foreach (Updateable updateable in updateables)
            {
                updateable.TryUpdate(deltaTime);
            }
            _isUpdatingType = null;
        }

        private void ResolvePendingRegisters(List<Updateable> updateables)
        {
            if (_updateablesToAdd.Count > 0)
            {
                foreach (Updateable updateable in _updateablesToAdd)
                {
                    AddUpdateable(updateable, updateables);
                }
                _updateablesToAdd.Clear();
            }

            if (_updateablesToRemove.Count > 0)
            {
                foreach (Updateable updateable in _updateablesToRemove)
                {
                    RemoveUpdateable(updateable, updateables);
                }
                _updateablesToRemove.Clear();
            }
        }
        #endregion Update
    }
}
