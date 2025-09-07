using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.UI.Controls
{
	[CreateAssetMenu(fileName = "SettingsRebindGroup", menuName = "Input/Settings/RebindGroup")]
	public class SettingsRebindGroup : ScriptableObject
	{
		[SerializeField]
		private string _groupLocName;
		[Space, SerializeField, FormerlySerializedAs("_rebindables")]
		private SettingsRebindActionData[] _rebindActionDatas;

		public IReadOnlyList<SettingsRebindActionData> RebindActionDatas => _rebindActionDatas;
		public string GetLocalizedName() => /*LocalizationUtility.GetLocalizedText(*/_groupLocName/*)*/;
	}
}