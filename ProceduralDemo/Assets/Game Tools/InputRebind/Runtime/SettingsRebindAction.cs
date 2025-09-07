using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Data.UI.Controls
{
	public class SettingsRebindAction
	{
		private readonly SettingsRebindActionData _data;
		private readonly SettingsRebindGroup _group;
		private readonly SettingsRebindDatabase _database;
		private readonly bool _isAlt;
		private SettingsRebindAction _siblingRebindAction;

		private Guid _bindingId;
		private int _bindingIndex;

		private Guid _modiferBindingId;
		private int _modiferBindingIndex;

		private readonly List<object> _disableCount = new();

		public Action<SettingsRebindAction> OnChanged = delegate { };

		public InputAction Action => _data.Action.action;
		public InputActionReference ActionReference => _data.Action;
		public SettingsRebindActionData Data => _data;
		public SettingsRebindGroup Group => _group;
		public SettingsRebindAction SiblingRebindAction => _siblingRebindAction;
		public bool HasModifier => _modiferBindingIndex != -1;
		public bool IsAlt => _isAlt;

		public InputBinding Binding => Action.bindings[_bindingIndex];
		public InputBinding ModifierBinding => Action.bindings[_modiferBindingIndex];

		public bool IsUnbound() => string.IsNullOrEmpty(Binding.effectivePath);
		public bool IsModifierUnbound() => !HasModifier || string.IsNullOrEmpty(ModifierBinding.effectivePath) || ModifierBinding.effectivePath == Binding.effectivePath;

		public SettingsRebindAction(SettingsRebindActionData data, SettingsRebindGroup group, SettingsRebindDatabase database, bool isAlt)
		{
			_data = data;
			_group = group;
			_database = database;
			_isAlt = isAlt;

			if (isAlt)
			{
				InitalizeBindings(data.AltBindingId, data.AltModifierBindingId);
			}
			else
			{
				InitalizeBindings(data.BindingId, data.ModifierBindingId);
			}
		}

		public IEnumerable<InputAction> GetDuplicateActions()
		{
			foreach (var action in _data.HiddenDuplicateActions)
			{
				yield return action.action;
			}
		}

		public void SetSiblingRebind(SettingsRebindAction siblingRebindAction)
		{
			_siblingRebindAction = siblingRebindAction;
		}

		public InputActionRebindingExtensions.RebindingOperation PerformInteractiveRebinding()
		{
			return Action.PerformInteractiveRebinding(_bindingIndex);
		}

		public string GetBindingString() => GetBindingStringInternal(_database.DisplayStringOptions);
		public string GetBindingLongString() => GetBindingStringInternal(_database.DisplayFullStringOptions);

		public bool HasOverrides()
		{
			return BindingHasOverrides(Action.bindings[_bindingIndex]) ||
				(HasModifier && BindingHasOverrides(Action.bindings[_modiferBindingIndex]));
		}

		public void ClearOverrides(out string previousBindingPath, out string previousModifierBindingPath)
		{
			previousBindingPath = Action.bindings[_bindingIndex].effectivePath;
			Action.RemoveBindingOverride(_bindingIndex);
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.RemoveBindingOverride(_bindingIndex);
			}

			if (!HasModifier)
			{
				previousModifierBindingPath = string.Empty;
				return;
			}
			previousModifierBindingPath = Action.bindings[_modiferBindingIndex].effectivePath;
			Action.RemoveBindingOverride(_modiferBindingIndex);
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.RemoveBindingOverride(_modiferBindingIndex);
			}
		}

		public void ApplyOverrideUnbound()
		{
			ApplyOverride(string.Empty);
			if (HasModifier)
			{
				ApplyModifierOverride(string.Empty);
			}
		}

		public void ApplyOverride(string bindingPath)
		{
			Action.ApplyBindingOverride(_bindingIndex, bindingPath);
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.ApplyBindingOverride(_bindingIndex, bindingPath);
			}
		}
		public void ApplyOverride(InputBinding binding)
		{
			Action.ApplyBindingOverride(_bindingIndex, binding);
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.ApplyBindingOverride(_bindingIndex, binding);
			}
		}

		public void ApplyOverrideToDuplicateAction()
		{
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.ApplyBindingOverride(_bindingIndex, Binding);
			}
		}

		public void ApplyModifierOverride(string bindingPath)
		{
			Action.ApplyBindingOverride(_modiferBindingIndex, bindingPath);
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.ApplyBindingOverride(_modiferBindingIndex, bindingPath);
			}
		}
		public void ApplyModifierOverride(InputBinding binding)
		{
			Action.ApplyBindingOverride(_modiferBindingIndex, binding);
			foreach (var duplicateAction in GetDuplicateActions())
			{
				duplicateAction.ApplyBindingOverride(_modiferBindingIndex, binding);
			}
		}

		private string GetBindingStringInternal(InputBinding.DisplayStringOptions options)
		{
			string bindingString = Action.GetBindingDisplayString(_bindingIndex, out _, out _, options);
			if (IsModifierUnbound())
			{
				return bindingString;
			}

			string modifierBindingString = Action.GetBindingDisplayString(_modiferBindingIndex, out _, out _, options);
			return modifierBindingString + "+" + bindingString;
		}

		private static bool BindingHasOverrides(InputBinding binding)
		{
			return binding.hasOverrides && binding.overridePath != binding.path;
		}

		private void InitalizeBindings(string bindingId, string modifierBindingId)
		{
			_bindingId = new Guid(bindingId);
			_bindingIndex = Action.bindings.IndexOf(x => x.id == _bindingId);

			if (string.IsNullOrEmpty(modifierBindingId))
			{
				_modiferBindingId = default;
				_modiferBindingIndex = -1;
			}
			else
			{
				_modiferBindingId = new Guid(modifierBindingId);
				_modiferBindingIndex = Action.bindings.IndexOf(x => x.id == _modiferBindingId);
			}
		}

		public void Disable(object handle)
		{
			Assert.IsNotNull(handle);
			if (_disableCount.Contains(handle))
			{
				return;
			}
			_disableCount.Add(handle);

			Action.Disable();
			foreach (var inputAction in GetDuplicateActions())
			{
				inputAction.Disable();
			}
		}

		public void RemoveDisable(object handle)
		{
			Assert.IsNotNull(handle);
			if (!_disableCount.Contains(handle))
			{
				return;
			}
			_disableCount.Remove(handle);
			if (_disableCount.Count > 0)
			{
				return;
			}
			if (!Action.actionMap.enabled)
			{
				return;
			}

			Action.Enable();
			foreach (var inputAction in GetDuplicateActions())
			{
				inputAction.Enable();
			}
		}
	}
}