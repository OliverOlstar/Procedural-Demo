using System.Collections.Generic;
using ODev.Util;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Data.UI.Controls
{
    [CreateAssetMenu(fileName = "SettingsRebindRuntimeInfo", menuName = "Input/Settings/SettingsRebindRuntimeInfo")]
    public class SettingsRebindRuntimeInfo : ScriptableObject
    {
        [SerializeField] private SettingsRebindDatabase _database;

        private readonly List<SettingsRebindAction> _rebindActions = new();
        private readonly Dictionary<InputActionReference, List<SettingsRebindActionData>> _rebindDatasByInputActions = new();
        private readonly Dictionary<InputActionReference, List<SettingsRebindAction>> _rebindActionsByInputActions = new();
        private readonly Dictionary<SettingsRebindGroup, List<SettingsRebindAction>> _rebindActionsByGroup = new();
        private readonly Dictionary<SettingsRebindGroup, List<SettingsRebindGroup>> _conflictingGroupByGroup = new();

        public IEnumerable<SettingsRebindAction> AllRebindActions => _rebindActions;

        public void Initialize()
        {
            _rebindActions.Clear();
            _rebindDatasByInputActions.Clear();
            _rebindActionsByInputActions.Clear();
            _rebindActionsByGroup.Clear();
            _conflictingGroupByGroup.Clear();

            CreateRebindActions();
            PopulateConflictDicationary();
        }

        public bool TryGetBindingString(InputActionReference inputAction, out string bindingString, bool getLongVersion = false, string inBetweens = "")
        {
            bindingString = "";

            if (!TryGetRebindActions(inputAction, out var rebindActions))
            {
                this.LogError($"There are no {nameof(SettingsRebindAction)} for my {nameof(SettingsRebindActionData)}, there should always be atleast 1?");
                return false;
            }

            bool first = true;
            bool useAlt = false;

            foreach (var rebindAction in rebindActions)
            {
                if (rebindAction.IsUnbound())
                {
                    useAlt = true;
                    continue;
                }

                if (rebindAction.IsAlt && !useAlt)
                {
                    continue;
                }

                useAlt = false;

                if (!first)
                {
                    bindingString += inBetweens;
                }

                first = false;

                if (getLongVersion)
                    bindingString += rebindAction.GetBindingLongString();
                else
                    bindingString += rebindAction.GetBindingString();
            }

            return !string.IsNullOrEmpty(bindingString);
        }

        public bool TryGetRebindActionData(InputActionReference inputAction, out IReadOnlyList<SettingsRebindActionData> rebindActionData)
        {
            if (!_rebindDatasByInputActions.TryGetValue(inputAction, out var rebindActionDatasForInput))
            {
                rebindActionData = null;
                return false;
            }

            rebindActionData = rebindActionDatasForInput;
            return true;
        }

        public bool TryGetRebindActions(InputActionReference inputAction, out IReadOnlyList<SettingsRebindAction> rebindActions)
        {
            if (!_rebindActionsByInputActions.TryGetValue(inputAction, out var rebindActionsForInput))
            {
                rebindActions = null;
                return false;
            }

            rebindActions = rebindActionsForInput;
            return true;
        }

        public bool TryGetConflictGroups(SettingsRebindGroup group, out IReadOnlyList<SettingsRebindGroup> conflictGroups)
        {
            if (!_conflictingGroupByGroup.TryGetValue(group, out var foundConflictGroups))
            {
                conflictGroups = null;
                return false;
            }

            conflictGroups = foundConflictGroups;
            return true;
        }

        public bool TryGetRebindActions(SettingsRebindGroup group, out IReadOnlyList<SettingsRebindAction> rebindActions)
        {
            if (!_rebindActionsByGroup.TryGetValue(group, out var rebindActionsInGroup))
            {
                rebindActions = null;
                return false;
            }

            rebindActions = rebindActionsInGroup;
            return true;
        }

        private void CreateRebindActions()
        {
            foreach (var group in _database.Groups)
                foreach (var rebindActionData in group.RebindActionDatas)
                {
                    if (!_rebindDatasByInputActions.TryGetValue(rebindActionData.Action, out var rebindActionDatasForInput))
                    {
                        rebindActionDatasForInput = new();
                        _rebindDatasByInputActions.Add(rebindActionData.Action, rebindActionDatasForInput);
                        foreach (var duplicateAction in rebindActionData.HiddenDuplicateActions)
                        {
                            _rebindDatasByInputActions.Add(duplicateAction, rebindActionDatasForInput);
                        }
                    }

                    if (!rebindActionDatasForInput.Contains(rebindActionData))
                    {
                        rebindActionDatasForInput.Add(rebindActionData);
                    }

                    if (rebindActionData.FeatureFlagValidator != null && !rebindActionData.FeatureFlagValidator.IsValid())
                    {
                        continue;
                    }

                    if (!_rebindActionsByGroup.TryGetValue(group, out var rebindActionsInGroup))
                    {
                        rebindActionsInGroup = new();
                        _rebindActionsByGroup.Add(group, rebindActionsInGroup);
                    }

                    if (!_rebindActionsByInputActions.TryGetValue(rebindActionData.Action, out var rebindActionsForInput))
                    {
                        rebindActionsForInput = new();
                        _rebindActionsByInputActions.Add(rebindActionData.Action, rebindActionsForInput);
                        foreach (var duplicateAction in rebindActionData.HiddenDuplicateActions)
                        {
                            _rebindActionsByInputActions.Add(duplicateAction, rebindActionsForInput);
                        }
                    }

                    SettingsRebindAction rebindAction = new(rebindActionData, group, _database, false);
                    _rebindActions.Add(rebindAction);
                    rebindActionsInGroup.Add(rebindAction);
                    rebindActionsForInput.Add(rebindAction);

                    if (!rebindActionData.HasAltBinding())
                    {
                        continue;
                    }

                    SettingsRebindAction altRebindAction = new(rebindActionData, group, _database, true);
                    _rebindActions.Add(altRebindAction);
                    rebindActionsInGroup.Add(altRebindAction);
                    rebindActionsForInput.Add(altRebindAction);

                    rebindAction.SetSiblingRebind(altRebindAction);
                    altRebindAction.SetSiblingRebind(rebindAction);
                }
        }

        private void PopulateConflictDicationary()
        {
            foreach (var group in _database.Groups)
            {
                _conflictingGroupByGroup.Add(group, new List<SettingsRebindGroup> { group });
            }

            foreach (var groupCollection in _database.ConflictingGroups)
                for (int i = 0; i < groupCollection.Groups.Length; i++)
                {
                    SettingsRebindGroup group = groupCollection.Groups[i];

                    if (!_conflictingGroupByGroup.TryGetValue(group, out var groupList))
                    {
                        groupList = new() { group };
                        _conflictingGroupByGroup.Add(group, groupList);
                    }

                    for (int z = 0; z < groupCollection.Groups.Length; z++)
                    {
                        if (z == i)
                        {
                            continue;
                        }

                        var otherGroup = groupCollection.Groups[z];
                        if (!groupList.Contains(otherGroup))
                        {
                            groupList.Add(otherGroup);
                        }
                    }
                }
        }
    }
}