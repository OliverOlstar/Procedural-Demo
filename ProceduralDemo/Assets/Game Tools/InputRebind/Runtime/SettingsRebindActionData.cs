using System;
using System.Collections.Generic;
using ODev.VariableSOs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Data.UI.Controls
{
	[Serializable]
	public class SettingsRebindActionData
	{
		[SerializeField] private string _locName;
		public bool IsHidden;
		public bool AddUISpaceAbove = false;

		[Header("Input")]
		public InputActionReference Action;
		public List<InputActionReference> HiddenDuplicateActions;
		public bool IsHoldAction;

		[InputBinding(nameof(Action))] public string ModifierBindingId;
		[InputBinding(nameof(Action))] public string BindingId;
		[InputBinding(nameof(Action))] public string AltModifierBindingId;
		[InputBinding(nameof(Action))] public string AltBindingId;

		[Space] public ValidatorSO FeatureFlagValidator;

		public Action OnChanged = delegate { };

		public string GetLocalizedName() => /*LocalizationUtility.GetLocalizedText(*/_locName/*)*/;
		public bool HasAltBinding() => !string.IsNullOrEmpty(AltBindingId);
	}
}