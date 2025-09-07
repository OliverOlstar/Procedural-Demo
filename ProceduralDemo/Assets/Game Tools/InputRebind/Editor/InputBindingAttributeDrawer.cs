using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Linq;

namespace Data.UI.Controls.Editor
{
	[CustomPropertyDrawer(typeof(InputBindingAttribute))]
	public class InputBindingAttributeDrawer : PropertyDrawer
	{
		private SerializedProperty _actionProperty;

		private GUIContent[] _bindingOptions;
		private string[] _bindingOptionValues;
		private int _selectedBindingOption;
		private SerializedProperty _currentReference;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType != SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, $"Use {nameof(InputBindingAttribute)} attribute with string fields only");
				return;
			}

			var bindingAttribute = (InputBindingAttribute)attribute;
			var actionPropertyPath = property.propertyPath.Replace(property.name, bindingAttribute.ActionPropertyId);
			_actionProperty = property.serializedObject.FindProperty(actionPropertyPath);

			if (_actionProperty.objectReferenceValue == null)
			{
				EditorGUI.LabelField(position, label.text, $"Please assign \"{_actionProperty.displayName}\" first");
				return;
			}

			if (property != _currentReference)
			{
				RefreshBindingOptions(property.stringValue);
				_currentReference = property;
			}

			var newSelectedBinding = EditorGUI.Popup(position, label, _selectedBindingOption, _bindingOptions);
			if (newSelectedBinding != _selectedBindingOption)
			{
				var bindingId = _bindingOptionValues[newSelectedBinding];
				property.stringValue = bindingId;
				_selectedBindingOption = newSelectedBinding;

				property.serializedObject.ApplyModifiedProperties();
				property.serializedObject.Update();
			}
		}

		private void RefreshBindingOptions(string currentValue)
		{
			var action = ((InputActionReference)_actionProperty.objectReferenceValue).action;

			if (action == null)
			{
				_bindingOptions = new GUIContent[0];
				_bindingOptionValues = new string[0];
				_selectedBindingOption = -1;
				return;
			}

			var bindings = action.bindings;
			var bindingCount = bindings.Count;

			_bindingOptions = new GUIContent[bindingCount + 1];
			_bindingOptions[0] = new GUIContent("None");
			_bindingOptionValues = new string[bindingCount + 1];
			_bindingOptionValues[0] = string.Empty;
			_selectedBindingOption = -1;

			var currentBindingId = currentValue;
			for (var i = 0; i < bindingCount; ++i)
			{
				var binding = bindings[i];
				var bindingId = binding.id.ToString();
				var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

				// If we don't have a binding groups (control schemes), show the device that if there are, for example,
				// there are two bindings with the display string "A", the user can see that one is for the keyboard
				// and the other for the gamepad.
				var displayOptions = InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
				if (!haveBindingGroups)
				{
					displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;
				}

				// Create display string.
				var displayString = action.GetBindingDisplayString(i, displayOptions);

				// If binding is part of a composite, include the part name.
				if (binding.isPartOfComposite)
				{
					displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";
				}

				// Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
				// by instead using a backlash.
				displayString = displayString.Replace('/', '\\');

				// If the binding is part of control schemes, mention them.
				if (haveBindingGroups)
				{
					var asset = action.actionMap?.asset;
					if (asset != null)
					{
						var controlSchemes = string.Join(", ",
							binding.groups.Split(InputBinding.Separator)
								.Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

						displayString = $"{displayString} ({controlSchemes})";
					}
				}

				_bindingOptions[i + 1] = new GUIContent(string.IsNullOrEmpty(displayString) ? $"{i} Unbound" : $"{i} {displayString}");
				_bindingOptionValues[i + 1] = bindingId;

				if (currentBindingId == bindingId)
				{
					_selectedBindingOption = i + 1;
				}
			}
		}
	}
}