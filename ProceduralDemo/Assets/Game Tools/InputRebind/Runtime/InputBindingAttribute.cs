using System;
using UnityEngine;

namespace Data.UI.Controls
{
	[AttributeUsage(AttributeTargets.Field)]
	public class InputBindingAttribute : PropertyAttribute
	{
		public string ActionPropertyId;

		public InputBindingAttribute(string actionPropertyId)
		{
			ActionPropertyId = actionPropertyId;
		}
	}
}