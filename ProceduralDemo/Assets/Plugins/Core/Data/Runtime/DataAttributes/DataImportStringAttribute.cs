
using System.Collections.Generic;
using UnityEngine;

namespace Data.Import.String
{
	public class ToLowerAttribute : StringAttribute
	{
		protected override string ImportString(string columnName, System.Type columnType, string rawValue, ref string importError)
		{
			return rawValue.ToLower();
		}
	}

	public class ToUpperAttribute : StringAttribute
	{
		protected override string ImportString(string columnName, System.Type columnType, string rawValue, ref string importError)
		{
			return rawValue.ToUpper();
		}
	}

	public class DateTimeAttribute : StringAttribute
	{
		protected override string ImportString(string columnName, System.Type columnType, string rawValue, ref string importError)
		{
			if (!System.DateTime.TryParse(rawValue, out System.DateTime dt))
			{
				importError = "Invalid Date Time format";
				return string.Empty;
			}
			return Core.Chrono.DateTimeToString(dt); // Call ToString() with a specific format as different machines will have different default formats
		}
	}
}
