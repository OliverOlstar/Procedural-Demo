using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Data.UI.Controls
{
	[CreateAssetMenu(fileName = "SettingsRebindDatabase", menuName = "Input/Settings/RebindDatabase")]
	public class SettingsRebindDatabase : ScriptableObject
	{
		[Serializable]
		public class GroupCollection
		{
			public SettingsRebindGroup[] Groups;
		}

		public readonly Dictionary<string, string> BindingPathReplacers = new()
		{
			{ "<Keyboard>/leftCtrl", "<Keyboard>/ctrl" },
			{ "<Keyboard>/rightCtrl", "<Keyboard>/ctrl" },
			{ "<Keyboard>/leftAlt", "<Keyboard>/alt" },
			{ "<Keyboard>/rightAlt", "<Keyboard>/alt" },
			{ "<Keyboard>/leftShift", "<Keyboard>/shift" },
			{ "<Keyboard>/rightShift", "<Keyboard>/shift" },
		};

		[Header("Display")]
		[SerializeField]
		private InputBinding.DisplayStringOptions _displayStringOptions;
		[SerializeField]
		private InputBinding.DisplayStringOptions _displayFullStringOptions;

		[Header("Groups")]
		[SerializeField]
		private SettingsRebindGroup[] _groups;
		[SerializeField]
		private GroupCollection[] _conflictingGroups;

		[Header("Binding Paths")]
		[SerializeField]
		private InputActionReference[] _modifierInputs = Array.Empty<InputActionReference>();
		[SerializeField]
		private string[] _cancelBindingPaths = new string[] { "<Keyboard>/escape", "<Mouse>/leftButton" };
		[SerializeField]
		private string[] _clearBindingPaths = new string[] { "<Keyboard>/delete" };

		public InputBinding.DisplayStringOptions DisplayStringOptions => _displayStringOptions;
		public InputBinding.DisplayStringOptions DisplayFullStringOptions => _displayFullStringOptions;
		public SettingsRebindGroup[] Groups => _groups;
		public string[] CancelBindingPaths => _cancelBindingPaths;
		public string[] ClearBindingPaths => _clearBindingPaths;
		public GroupCollection[] ConflictingGroups => _conflictingGroups;

		public IEnumerable<InputAction> GetAllModifierInputs()
		{
			foreach (var inputAction in _modifierInputs)
			{
				yield return inputAction.action;
			}
		}

		public IEnumerable<InputActionReference> GetAllModifierReferences()
		{
			foreach (var inputAction in _modifierInputs)
			{
				yield return inputAction;
			}
		}

		public IEnumerable<string> GetAllModifierBindingPaths()
		{
			foreach (var inputAction in _modifierInputs)
			{
				foreach (var binding in inputAction.action.bindings)
				{
					yield return binding.path;
				}
			}
		}
	}
}