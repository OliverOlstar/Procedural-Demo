using System.Collections.Generic;
using Data.UI.Controls;
using TMPro;
using UnityEngine;

namespace Presentation.UI.Controls
{
    public class SettingsControlsPopulator : MonoBehaviour
    {
        [SerializeField] private SettingsRebindRuntimeInfo _rebindInfo;
        [SerializeField] private TMP_Text _rebindGroupPrefab;
        [SerializeField] private SettingsRebindActionUI _rebindActionPrefab;
        [SerializeField] private GameObject _spacerPrefab;
        [SerializeField] private Transform _resetButton;

        private readonly List<SettingsRebindActionUI> _settingsRebindActionUIs = new();

        public IEnumerable<SettingsRebindActionUI> SettingsRebindActionUIs => _settingsRebindActionUIs;

        public void Populate()
        {
            _rebindGroupPrefab.gameObject.SetActive(true);
            _rebindActionPrefab.gameObject.SetActive(true);
            _spacerPrefab.SetActive(true);

            SettingsRebindGroup previousGroup = null;
            SettingsRebindAction rebindActionSibling = null;
            foreach (var rebindAction in _rebindInfo.AllRebindActions)
            {
                if (rebindAction.Data.IsHidden || rebindAction == rebindActionSibling)
                {
                    continue;
                }

                if (previousGroup != rebindAction.Group)
                {
                    var rebindGroup = Instantiate(_rebindGroupPrefab, _rebindGroupPrefab.transform.parent);
                    rebindGroup.SetText(rebindAction.Group.GetLocalizedName());
                    previousGroup = rebindAction.Group;
                }

                if (rebindAction.Data.AddUISpaceAbove)
                {
                    Instantiate(_spacerPrefab, _spacerPrefab.transform.parent);
                }

                var rebindActionUI = Instantiate(_rebindActionPrefab, _rebindActionPrefab.transform.parent);
                rebindActionSibling = rebindAction.SiblingRebindAction;
                rebindActionUI.Initialize(rebindAction, rebindActionSibling);
                _settingsRebindActionUIs.Add(rebindActionUI);
            }

            _resetButton.SetAsLastSibling();

            _spacerPrefab.SetActive(false);
            _rebindGroupPrefab.gameObject.SetActive(false);
            _rebindActionPrefab.gameObject.SetActive(false);
        }
    }
}
