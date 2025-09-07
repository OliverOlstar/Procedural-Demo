using Events;
using UnityEngine;

namespace Data.UI.Controls
{
	[CreateAssetMenu(menuName = "Events/Settings/RebindEvent", fileName = "SettingsRebindEvent", order = 0)]
	public class SettingsRebindEvent : BaseEvent<SettingsRebindAction> { }
}