using System.Collections.Generic;
using Data.UI.Controls;
using ODev.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Presentation.UI.Controls
{
    public class SettingsRebindActionUI : MonoBehaviour
    {
        private const string OverlayText_LocKey = "RebindSettings.RebindOverlay";

        [Header("UI")]
        [SerializeField] private GameObject _rebindOverlay;
        [SerializeField] private TMP_Text _rebindText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private SettingsRebindDatabase _database;
        [SerializeField] private SettingsRebindRuntimeInfo _rebindInfo;

        [Space]
        [SerializeField] private TMP_Text _rebindActionLabel;
        [SerializeField] private TMP_Text _bindingText;
        [SerializeField] private Button _rebindButton;

        [Space]
        [SerializeField] private string _hasDuplicateTitleLocKey;
        [SerializeField] private string _hasDuplicateBodyLocKey;
        [SerializeField] private string _acceptDuplicateLocKey;
        [SerializeField] private string _declineDuplicateLocKey;

        [Header("Alternative Rebind")]
        [SerializeField] private GameObject _altRebindContainer;
        [SerializeField] private TMP_Text _altBindingText;
        [SerializeField] private Button _altRebindButton;

        [Header("Events")]
        // [SerializeField] private ShowMenuModalDialogEvent _showMenuModalDialogEvent;
        [SerializeField] private SettingsRebindEvent _settingRebindStartEvent;
        [SerializeField] private SettingsRebindEvent _settingRebindEndEvent;

        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private SettingsRebindAction _rebindAction;
        private SettingsRebindAction _altRebindAction;

        public InputAction Action => _rebindAction.Action;

        private void Start()
        {
            _rebindButton.onClick.AddListener(StartInteractiveRebind);
            _altRebindButton.onClick.AddListener(StartInteractiveRebindAlt);
            _resetButton.onClick.AddListener(ResetToDefault);
        }

        private void OnDestroy()
        {
            _rebindButton.onClick.RemoveListener(StartInteractiveRebind);
            _altRebindButton.onClick.RemoveListener(StartInteractiveRebindAlt);
            _resetButton.onClick.RemoveListener(ResetToDefault);
        }

        protected void OnDisable()
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;
        }

        public void Initialize(SettingsRebindAction rebindAction, SettingsRebindAction altRebindAction = null)
        {
            _rebindAction = rebindAction;
            _altRebindAction = altRebindAction;
            _rebindActionLabel.SetText(rebindAction.Data.GetLocalizedName());
            UpdateBindingDisplay();
        }

        public void UpdateBindingDisplay()
        {
            _bindingText.SetText (_rebindAction.GetBindingString());

            bool hasAltRebind = _altRebindAction != null;
            _altBindingText.SetText (hasAltRebind ? _altRebindAction.GetBindingString() : string.Empty);
            _altRebindContainer.SetActive(hasAltRebind);

            bool hasOverrides = _rebindAction.HasOverrides() || (hasAltRebind && _altRebindAction.HasOverrides());
            _resetButton.gameObject.SetActive(hasOverrides);
        }

        public void ResetToDefault()
        {
            _rebindAction.ClearOverrides(out string previousBindingPath, out string previousModifierBindingPath);
            ResolveDuplicates(_rebindAction, previousBindingPath, previousModifierBindingPath);

            if (_altRebindAction != null)
            {
                _altRebindAction.ClearOverrides(out previousBindingPath, out previousModifierBindingPath);
                ResolveDuplicates(_rebindAction, previousBindingPath, previousModifierBindingPath);
            }

            UpdateBindingDisplay();
            _rebindAction.OnChanged.Invoke(_rebindAction);
            _rebindAction.Data.OnChanged.Invoke();
        }

        public void StartInteractiveRebind(SettingsRebindAction rebindAction)
        {
            PerformInteractiveRebind(rebindAction, false);
        }
        public void StartInteractiveRebind() => StartInteractiveRebind(_rebindAction);
        public void StartInteractiveRebindAlt() => StartInteractiveRebind(_altRebindAction);

        private void PerformInteractiveRebind(SettingsRebindAction rebindAction, bool didAssignModifer = false)
        {
            _settingRebindStartEvent.Fire(rebindAction);

            bool wasEnabled = rebindAction.Action.enabled;
            rebindAction.Action.Disable();
            foreach (var duplicateAction in rebindAction.GetDuplicateActions())
            {
                duplicateAction.Disable();
            }

            string previousBinding = rebindAction.Binding.effectivePath;
            string previousModifierBinding = rebindAction.HasModifier ? rebindAction.ModifierBinding.effectivePath : string.Empty;

            OnInteractiveRebindStart();
            PreformInteractiveRebindInternal(rebindAction, wasEnabled, previousBinding, previousModifierBinding, didAssignModifer);
        }

        private void PreformInteractiveRebindInternal(SettingsRebindAction rebindAction, bool wasEnabled, string previousBinding, string previousModifierBinding, bool didAssignModifer = false)
        {
            if (_rebindOperation != null && !_rebindOperation.completed)
            {
                _rebindOperation.Cancel();
            }
            _rebindOperation?.Dispose();

            _rebindOperation = rebindAction.PerformInteractiveRebinding()
                .WithCancelingThrough(_database.CancelBindingPaths[0])
                .OnCancel(_ => OnInteractiveRebindCancel(rebindAction, wasEnabled, previousModifierBinding))
                .OnComplete(_ => OnInteractiveRebindComplete(rebindAction, wasEnabled, didAssignModifer, previousBinding, previousModifierBinding));

            _rebindOperation.Start();
        }

        private void OnInteractiveRebindStart()
        {
            if (_rebindOverlay != null)
            {
                _rebindOverlay.SetActive(true);
            }
            if (_rebindText != null)
            {
                _rebindText.SetText (/*LocalizationUtility.GetLocalizedText(*/OverlayText_LocKey/*)*/);
            }
        }

        private void OnInteractiveRebindCancel(SettingsRebindAction rebindAction, bool wasEnabled, string previousModifierBinding)
        {
            if (rebindAction.HasModifier)
            {
                rebindAction.ApplyModifierOverride(previousModifierBinding);
            }

            _rebindOverlay.SetActive(false);
            UpdateBindingDisplay();
            OnInteractiveRebindFinished(rebindAction, wasEnabled);
        }

        private void OnInteractiveRebindComplete(SettingsRebindAction rebindAction, bool doEnable, bool didAssignModifer, string previousBinding, string previousModifierBinding)
        {
            for (int i = 1; i < _database.CancelBindingPaths.Length; i++)
            {
                if (_database.CancelBindingPaths[i] != rebindAction.Binding.effectivePath)
                {
                    continue;
                }
                rebindAction.ApplyOverride(previousBinding);
                OnInteractiveRebindCancel(rebindAction, doEnable, previousModifierBinding);
                return;
            }

            if (ShouldClearNewBinding(rebindAction.Binding))
            {
                this.Log("Clear New Input Binding");
                rebindAction.ApplyOverrideUnbound();
            }
            else if (_database.BindingPathReplacers.TryGetValue(rebindAction.Binding.effectivePath, out string replacementBindingPath))
            {
                rebindAction.ApplyOverride(replacementBindingPath);
            }

            if (IsNewBindingAModifier(rebindAction.Binding))
            {
                if (!rebindAction.HasModifier)
                {
                    rebindAction.ApplyOverride(previousBinding);
                    PreformInteractiveRebindInternal(rebindAction, doEnable, previousBinding, previousModifierBinding);
                }
                else
                {
                    rebindAction.ApplyModifierOverride(rebindAction.Binding);
                    rebindAction.ApplyOverride(string.Empty);
                    PreformInteractiveRebindInternal(rebindAction, doEnable, previousBinding, previousModifierBinding, true);
                }
                return;
            }

            if (!didAssignModifer && rebindAction.HasModifier)
            {
                rebindAction.ApplyModifierOverride(rebindAction.Binding);
            }

            _rebindOverlay.SetActive(false);
            UpdateBindingDisplay();
            OnInteractiveRebindFinished(rebindAction, doEnable);

            ResolveDuplicates(rebindAction, previousBinding, previousModifierBinding);
        }

        /// <returns>Is Resolving Duplicates</returns>
        private bool ResolveDuplicates(SettingsRebindAction rebindAction, string previousBinding, string previousModifierBinding)
        {
            if (!HasDuplicateBindings(rebindAction, out var overlappingBindings))
            {
                return false;
            }

            this.DevException(new System.NotImplementedException("Modal isn't implemented :/"));
            // MenuModalDialogDto dto = new(_hasDuplicateTitleLocKey, _hasDuplicateBodyLocKey, Sizes.M,
            //     () => OnAcceptDuplicate(overlappingBindings), true,
            //     () => OnDeclineDuplicate(rebindAction, previousBinding, previousModifierBinding))
            // {
            //     OverrideSuccessButtonTextKey = _acceptDuplicateLocKey,
            //     OverrideCancelButtonTextKey = _declineDuplicateLocKey
            // };

            // dto.Text = string.Format(dto.Text, rebindAction.Data.GetLocalizedName(), overlappingBindings[0].Data.GetLocalizedName());

            // _showMenuModalDialogEvent.Fire(dto);
            OnAcceptDuplicate(overlappingBindings);

            return true;
        }

        private void OnAcceptDuplicate(List<SettingsRebindAction> overlappingRebinds)
        {
            foreach (var rebindAction in overlappingRebinds)
            {
                rebindAction.ApplyOverrideUnbound();
                rebindAction.OnChanged.Invoke(rebindAction);
                rebindAction.Data.OnChanged.Invoke();
            }
        }

        private void OnDeclineDuplicate(SettingsRebindAction rebindAction, string previousBinding, string modifierPreviousBinding)
        {
            rebindAction.ApplyOverride(previousBinding);
            if (rebindAction.HasModifier)
            {
                rebindAction.ApplyModifierOverride(modifierPreviousBinding);
                rebindAction.OnChanged.Invoke(rebindAction);
                rebindAction.Data.OnChanged.Invoke();
            }
        }

        private void OnInteractiveRebindFinished(SettingsRebindAction rebindAction, bool wasEnabled)
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;

            if (wasEnabled)
            {
                rebindAction.Action.Enable();
                foreach (var duplicateAction in rebindAction.GetDuplicateActions())
                {
                    duplicateAction.Enable();
                }
            }
            rebindAction.ApplyOverrideToDuplicateAction();
            rebindAction.OnChanged.Invoke(rebindAction);
            rebindAction.Data.OnChanged.Invoke();
            _settingRebindEndEvent.Fire(rebindAction);
        }

        // private bool HasDuplicateCompositePartsBindings(InputAction action, int bindingIndex)
        // {
        //     var newBinding = action.bindings[bindingIndex];
        //     for (int i = 0; i < bindingIndex; i++)
        //     {
        //         if (action.bindings[i].effectivePath == newBinding.overridePath)
        //         {
        //             this.Log($"Duplicate binding found (composite part): {newBinding.effectivePath}");
        //             return true;
        //         }
        //     }
        //     return false;
        // }

        private bool HasDuplicateBindings(SettingsRebindAction rebindAction, out List<SettingsRebindAction> overlappingRebinds)
        {
            overlappingRebinds = new();
            var newBinding = rebindAction.Binding;

            if (string.IsNullOrEmpty(newBinding.effectivePath))
            {
                return false;
            }

            var modifierBindingPath = rebindAction.IsModifierUnbound() ? string.Empty : rebindAction.ModifierBinding.effectivePath;

            if (!_rebindInfo.TryGetConflictGroups(rebindAction.Group, out var conflictGroups))
            {
                return false;
            }

            foreach (var otherGroup in conflictGroups)
            {
                if (!_rebindInfo.TryGetRebindActions(otherGroup, out var otherRebindActions))
                {
                    continue;
                }

                foreach (var otherRebindAction in otherRebindActions)
                {
                    if (rebindAction.Action.id == otherRebindAction.Action.id ||
                        rebindAction.Data.IsHoldAction != otherRebindAction.Data.IsHoldAction ||
                        otherRebindAction.Binding.effectivePath != newBinding.effectivePath)
                    {
                        continue;
                    }
                    var otherModifierBindingPath = otherRebindAction.IsModifierUnbound() ? string.Empty : otherRebindAction.ModifierBinding.effectivePath;
                    if (otherModifierBindingPath != modifierBindingPath)
                    {
                        continue;
                    }

                    this.Log($"Duplicate binding found: {newBinding.effectivePath}");
                    overlappingRebinds.Add(otherRebindAction);
                }
            }

            return overlappingRebinds.Count > 0;
        }

        private bool ShouldClearNewBinding(InputBinding binding)
        {
            foreach (var path in _database.ClearBindingPaths)
            {
                if (binding.effectivePath == path)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsNewBindingAModifier(InputBinding binding)
        {
            foreach (var path in _database.GetAllModifierBindingPaths())
            {
                if (binding.effectivePath == path)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
