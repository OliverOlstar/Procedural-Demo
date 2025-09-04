using System.Collections.Generic;
using System.Linq;
using ODev.Debug;

namespace ODev.Data
{
	public class DebugOptionData // TODO Come up with a better way of having this exist without blocking the regular debugOptions but with keeping this class findable
	{
		public class DataID : DebugOption.StringWithDropdown
		{
			public DataID(
				string group,
				string name,
				IReadOnlyList<DataBin> db,
				DefaultSetting defaultSetting = DefaultSetting.Off,
				ReleaseSetting releaseSetting = ReleaseSetting.AlwaysOff,
				ShowIfType showIfType = ShowIfType.Always,
				string tooltip = null) :
					base(group, name, GetIDs(db), defaultSetting, releaseSetting, showIfType, tooltip)
			{ }

			private static string[] GetIDs(IReadOnlyList<DataBin> db) => db == null ? new string[0] : db.Select(d => d.ID).ToArray();
		}
	}
}