using Data.UI.Controls;
using ODev.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Presentation.UI
{
    public class ButtonShortCutUI : MonoBehaviour
    {
        [SerializeField] private InputActionReference _inputAction;
        [SerializeField] private SettingsRebindRuntimeInfo _rebindInfo;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _shortcutIcon;
        [SerializeField] private TMP_Text _shortcutText;

        private SettingsRebindActionData _rebindActionData;
        private bool _initialized = false;

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            if (_initialized)
                return;
            
            if (_inputAction == null || _rebindInfo == null)
            {
                _shortcutIcon.SetActive(false);
                return;
            }
            if (_button != null)
            {
                _inputAction.action.performed += ActionPerformed;
            }

            if (!_rebindInfo.TryGetRebindActionData(_inputAction, out var rebindActionDatas))
            {
                _shortcutIcon.SetActive(false);
                this.LogError("You're using a shortcut that doesn't have a rebind?");
                return;
            }
            _rebindActionData = rebindActionDatas[0];
            _rebindActionData.OnChanged += OnInputChanged;
            OnInputChanged();
            _initialized = true;
        }

        private void OnInputChanged()
        {
            if (!_rebindInfo.TryGetRebindActions(_inputAction, out var rebindActions))
            {
                _shortcutIcon.SetActive(false);
                this.LogError($"There are no {nameof(SettingsRebindAction)} for my {nameof(SettingsRebindActionData)}, there should always be atleast 1?");
                return;
            }

            foreach (var rebindAction in rebindActions)
            {
                if (rebindAction.IsUnbound())
                {
                    continue;
                }
                _shortcutText.SetText(rebindAction.GetBindingString());
                _shortcutIcon.SetActive(true);
                return;
            }

            _shortcutIcon.SetActive(false);
        }

        private void OnDestroy()
        {
			if (_button != null && _inputAction != null)
			{
				_inputAction.action.performed -= ActionPerformed;
			}
			if (_rebindActionData != null)
            {
                _rebindActionData.OnChanged -= OnInputChanged;
            }
        }

        private void ActionPerformed(InputAction.CallbackContext obj)
        {
            if (_button.interactable)
            {
                _button.onClick.Invoke();
            }
        }
    }
}