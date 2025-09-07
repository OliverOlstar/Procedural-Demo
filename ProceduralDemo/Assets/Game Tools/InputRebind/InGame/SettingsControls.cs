using Data.UI.Controls;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Presentation.UI.Controls
{
    public class SettingsControls : MonoBehaviour
    {
        [SerializeField] private SettingsRebindDatabase _database;
        [SerializeField] private SettingsControlsPopulator _populator;
        [SerializeField] private InputActionAsset _defaultInputActions;
        [SerializeField] private Button _resetAllButton;

        [Header("Events")]
        // [SerializeField] private ShowMenuModalDialogEvent _showMenuModalDialogEvent;
        [SerializeField] private SettingsRebindEvent _settingRebindStartEvent;
        [SerializeField] private SettingsRebindEvent _settingRebindEndEvent;

        private InputActionMap _uiInputActionMap;

        private void Start()
        {
            _settingRebindStartEvent.Register(OnRebindStart);
            _settingRebindEndEvent.Register(OnRebindEnd);
            _resetAllButton.onClick.AddListener(ResetAllBindings);

            if (_defaultInputActions != null && _uiInputActionMap == null)
            {
                _uiInputActionMap = _defaultInputActions.FindActionMap("UI");
            }

            InputSystem.onActionChange += OnActionChange;
            _populator.Populate();
        }

        private void OnDestroy()
        {
            _settingRebindStartEvent.UnRegister(OnRebindStart);
            _settingRebindEndEvent.UnRegister(OnRebindEnd);
            _resetAllButton.onClick.RemoveListener(ResetAllBindings);

            InputSystem.onActionChange -= OnActionChange;
        }

        private void OnRebindStart(SettingsRebindAction _)
        {
            _uiInputActionMap?.Disable();
        }

        private void OnRebindEnd(SettingsRebindAction _)
        {
            _uiInputActionMap?.Enable();
        }

		private void ResetAllBindings()
		{
			// MenuModalDialogDto dto = new("ModalWarning.Title", "ModalWarning.ResetBindings", Sizes.S, ResetAllBindingsInternal, true)
			// {
			//     OverrideSuccessButtonTextKey = "ModalWarning.ResetBindingsConfirmButton"
			// };
			// _showMenuModalDialogEvent.Fire(dto);

			ResetAllBindingsInternal();
		}

        private void ResetAllBindingsInternal()
        {
            _defaultInputActions.RemoveAllBindingOverrides();

            foreach (var group in _database.Groups)
                foreach (var rebindData in group.RebindActionDatas)
                {
                    rebindData.OnChanged.Invoke();
                }
            foreach (var rebindUI in _populator.SettingsRebindActionUIs)
            {
                rebindUI.UpdateBindingDisplay();
            }
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
            {
                return;
            }

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            foreach (var rebindActionUI in _populator.SettingsRebindActionUIs)
            {
                var referencedAction = rebindActionUI.Action;
                if (referencedAction == null)
                {
                    continue;
                }

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                {
                    rebindActionUI.UpdateBindingDisplay();
                }
            }
        }
    }
}